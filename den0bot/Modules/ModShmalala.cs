// den0bot (c) StanR 2019 - MIT License
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using den0bot.Util;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	class ModShmalala : IModule, IReceiveAllMessages
	{
		// Based on https://github.com/IrcDotNet/IrcDotNet/tree/master/samples/IrcDotNet.Samples.MarkovTextBot
		private class MarkovChainNode
		{
			private readonly List<MarkovChainNode> links;
			public ReadOnlyCollection<MarkovChainNode> Links { get; }
			public string Word { get; set; }

			public MarkovChainNode()
			{
				links = new List<MarkovChainNode>();
				Links = new ReadOnlyCollection<MarkovChainNode>(links);
			}

			public void AddLink(MarkovChainNode toNode)
			{
				links.Add(toNode);
			}
		}

		private class MarkovChain
		{
			private readonly List<MarkovChainNode> nodes = new List<MarkovChainNode>();
			public ReadOnlyCollection<MarkovChainNode> Nodes { get; }

			private const string file_path = "./markov.json";

			public MarkovChain()
			{
				// unpack chain if it exists
				if (System.IO.File.Exists(file_path))
				{
					var packedChain = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(file_path));
					foreach (var word in packedChain)
					{
						// first add all nodes without links
						nodes.Add(new MarkovChainNode
						{
							Word = word.Key
						});
					}

					foreach (var word in packedChain)
					{
						// then add all links between nodes
						var links = word.Value;
						foreach (var link in links)
						{
							var node = nodes.Find(x => x.Word == link);
							if (node != null)
								nodes.Find(x => x.Word == word.Key)?.AddLink(node);
						}
					}
				}

				Nodes = new ReadOnlyCollection<MarkovChainNode>(nodes);
			}

			public IEnumerable<string> GenerateSequence()
			{
				var curNode = GetNode(default(string));
				while (true)
				{
					if (curNode.Links.Count == 0)
						break;

					curNode = curNode.Links[RNG.NextNoMemory(0, curNode.Links.Count)];
					if (curNode.Word == null)
						break;

					yield return curNode.Word;
				}
			}

			public void Train(string fromValue, string toValue)
			{
				var fromNode = GetNode(fromValue);
				var toNode = GetNode(toValue);
				fromNode.AddLink(toNode);
			}

			private MarkovChainNode GetNode(string value)
			{
				MarkovChainNode node = null;
				if (string.IsNullOrEmpty(value))
				{
					node = nodes[RNG.NextNoMemory(0, nodes.Count)];
				}
				else
				{
					node = nodes.SingleOrDefault(n => n.Word == value);
				}
				if (node == null)
				{
					node = new MarkovChainNode { Word = value };
					nodes.Add(node);
				}
				return node;
			}

			public void SaveToFile()
			{
				Dictionary<string, List<string>> packedChain = new Dictionary<string, List<string>>();
				foreach (MarkovChainNode node in Nodes)
				{
					if (node.Word != null)
					{
						packedChain.Add(node.Word, node.Links.Select(x => x.Word).ToList());
					}
				}
				
				System.IO.File.WriteAllText(file_path, JsonConvert.SerializeObject(packedChain, Formatting.Indented));
			}
		}

		private readonly char[] sentenceSeparators = { '.', '!', '?', ',', '(', ')', '\n' };
		private readonly Regex cleanWordRegex = new Regex(@"[()\[\]{}'""`~\\\/]|(http|ftp|https):\/\/([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:\/~+#-]*[\w@?^=%&\/~+#-])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private int numTrainingMessagesReceived;
		private int numTrainingWordsReceived;

		private readonly MarkovChain markovChain = new MarkovChain();

		public ModShmalala()
		{
			AddCommands(new [] 
			{
				new Command
				{
					Name = "talk",
					Action = SendRandomMessage
				},
				new Command
				{
					Name = "talkstats",
					Action = msg => $"Messages: {numTrainingMessagesReceived}, words: {numTrainingWordsReceived}"
				},
				new Command
				{
					Name = "talkdump",
					Action = msg =>
					{
						markovChain.SaveToFile();
						return "k cool";
					},
					IsOwnerOnly = true
				}
			});
		}

		private string SendRandomMessage(Message msg)
		{
			if (markovChain.Nodes.Count == 0)
				return "Bot has not yet been trained.";

			var textBuilder = new StringBuilder();

			// Use Markov chain to generate random message, composed of one or more sentences.
			for (int i = 0; i < RNG.NextNoMemory(1, 4); i++)
				textBuilder.Append(GenerateRandomSentence());

			return textBuilder.ToString();
		}

		private string GenerateRandomSentence()
		{
			// Generate sentence by using Markov chain to produce sequence of random words.
			// Note: There must be at least three words in sentence.
			int trials = 0;
			string[] words;
			do
			{
				words = markovChain.GenerateSequence().ToArray();
			}
			while (words.Length < 3 && trials++ < 10);

			// uppercase first char
			words[0] = words[0].Substring(0, 1).ToUpper() + words[0].Remove(0, 1);

			return string.Join(" ", words) + $"{sentenceSeparators[RNG.NextNoMemory(0, sentenceSeparators.Length)]} ";
		}

		public void ReceiveMessage(Message message)
		{
			var text = message.Text.ToLower();
			if (text.StartsWith(Localization.Get("shmalala_trigger", message.Chat.Id)))
			{
				API.SendMessage(GenerateRandomSentence(), message.Chat);
				return;
			}

			// Train Markov generator from received message text.
			// Assume it is composed of one or more coherent sentences that are themselves are composed of words.
			var sentences = text.ToLower().Split(sentenceSeparators);
			foreach (var s in sentences)
			{
				string lastWord = null;
				foreach (var w in s.Split(' ').Select(w => cleanWordRegex.Replace(w, string.Empty)))
				{
					if (string.IsNullOrEmpty(w))
						continue;

					markovChain.Train(lastWord, w);
					lastWord = w;
					numTrainingWordsReceived++;
				}
				markovChain.Train(lastWord, null);
			}

			numTrainingMessagesReceived++;

			// save whole chain every 10 messages
			if (numTrainingMessagesReceived % 10 == 0)
				markovChain.SaveToFile();
		}
	}
}
