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
					var packedChain = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(file_path));
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
						var links = word.Value.Split(';').ToList();
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
				var node = nodes.SingleOrDefault(n => n.Word == value);
				if (node == null)
				{
					node = new MarkovChainNode { Word = value };
					nodes.Add(node);
				}
				return node;
			}

			public void SaveToFile()
			{
				Dictionary<string, string> packedChain = new Dictionary<string, string>();
				foreach (MarkovChainNode node in Nodes)
				{
					if (node.Word != null)
					{
						if (node.Links.Count > 0)
						{
							StringBuilder builder = new StringBuilder();
							foreach (var link in node.Links)
							{
								builder.Append(link.Word);
								if (node.Links.Count > 1)
									builder.Append(';');
							}

							packedChain.Add(node.Word, builder.ToString());
						}
						else
						{
							packedChain.Add(node.Word, "");
						}
					}
				}

				System.IO.File.WriteAllText(file_path, JsonConvert.SerializeObject(packedChain));
			}
		}

		private readonly char[] sentenceSeparators = { '.', '!', '?', ',', ';', ':', '(', ')' };
		private readonly Regex cleanWordRegex = new Regex(@"[()\[\]{}'""`~]");

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

			return string.Join(" ", words) + ". ";
		}

		public void ReceiveMessage(Message message)
		{
			// Train Markov generator from received message text.
			// Assume it is composed of one or more coherent sentences that are themselves are composed of words.
			var sentences = message.Text.Split(sentenceSeparators);
			foreach (var s in sentences)
			{
				string lastWord = null;
				foreach (var w in s.Split(' ').Select(w => cleanWordRegex.Replace(w, string.Empty)))
				{
					if (w.Length == 0)
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
