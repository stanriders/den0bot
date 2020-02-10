// den0bot (c) StanR 2020 - MIT License
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using den0bot.Util;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
	public class ModShmalala : IModule, IReceiveAllMessages
	{
		// Based on https://github.com/IrcDotNet/IrcDotNet/tree/master/samples/IrcDotNet.Samples.MarkovTextBot
		private class MarkovChain
		{
			public class MarkovChainNode
			{
				public List<MarkovChainNode> Links { get; } = new List<MarkovChainNode>();
				public string Word { get; set; }

				public void AddLink(MarkovChainNode toNode)
				{
					Links.Add(toNode);
				}
			}

			public List<MarkovChainNode> Nodes { get; } = new List<MarkovChainNode>();

			public bool Ready { get; private set; } = false;

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
						Nodes.Add(new MarkovChainNode
						{
							Word = word.Key
						});
					}

					// if chain is getting really big it might take a while to unpack
					ThreadPool.QueueUserWorkItem((a) =>
					{
						Parallel.ForEach(packedChain, (word) =>
						{
							// then add all links between nodes
							var links = word.Value;
							foreach (var link in links)
							{
								var node = Nodes.Find(x => x.Word == link);
								if (node != null)
									Nodes.Find(x => x.Word == word.Key)?.AddLink(node);
							}
						});

						Ready = true;
					});
				}
				else
				{
					Ready = true;
				}
			}

			public IEnumerable<string> GenerateSequence(string startNode)
			{
				var curNode = GetExistingNode(startNode);
				int wordAmt = 0; // make responses 8 words max so it could make a bit more sense
				int wordMax = RNG.NextNoMemory(3, 9);
				while (wordAmt < wordMax)
				{
					if (curNode.Links.Count == 0)
						break;

					curNode = curNode.Links[RNG.NextNoMemory(0, curNode.Links.Count)];
					if (curNode.Word == null)
						break;

					wordAmt++;
					yield return curNode.Word;
				}
			}

			public void Train(string fromValue, string toValue)
			{
				if (!string.IsNullOrEmpty(fromValue))
				{
					var fromNode = GetNode(fromValue);
					if (!string.IsNullOrEmpty(toValue))
					{
						var toNode = GetNode(toValue);
						if (toNode != null)
							fromNode.AddLink(toNode);
					}
				}
			}

			private MarkovChainNode GetNode(string value)
			{
				MarkovChainNode node = Nodes.SingleOrDefault(n => n.Word == value);
				if (node == null)
				{
					node = new MarkovChainNode { Word = value };
					Nodes.Add(node);
				}
				return node;
			}

			private MarkovChainNode GetExistingNode(string value)
			{
				MarkovChainNode node = null;
				if (!string.IsNullOrEmpty(value))
				{
					node = Nodes.SingleOrDefault(n => n.Word == value);
				}

				if (node == null)
					node = Nodes[RNG.NextNoMemory(0, Nodes.Count)];

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

			public void DeleteFile()
			{
				if (System.IO.File.Exists(file_path))
					System.IO.File.Delete(file_path);
			}
		}

		private bool useTranslation = true;

		private readonly char[] sentenceSeparators = { '.', '.', '.', '!', '!', '?', '(', ')', '\n' };
		private readonly Regex cleanWordRegex = 
			new Regex(@"[()\[\]{}'""`~\\\/\-*\d]|(http|ftp|https):\/\/([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:\/~+#-]*[\w@?^=%&\/~+#-])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private int numTrainingMessagesReceived;

		private MarkovChain markovChain = new MarkovChain();

		public ModShmalala()
		{
			AddCommands(new [] 
			{
				new Command
				{
					Name = "talk",
					Reply = true,
					ActionAsync = SendRandomMessage
				},
				new Command
				{
					Name = "talkstats",
					Action = msg =>
					{
						int linkAmount = 0;
						foreach (var node in markovChain.Nodes)
						{
							linkAmount += node.Links.Count;
						}
						return Localization.FormatGet("shmalala_stats", msg.Chat.Id, markovChain.Nodes.Count, linkAmount);
					}
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
				},
				new Command
				{
					Name = "talkwipe",
					Action = msg =>
					{
						markovChain.DeleteFile();
						markovChain = new MarkovChain();
						return "Done!";
					},
					IsAdminOnly = true
				}
			});
		}

		private async Task<string> SendRandomMessage(Message msg)
		{
			if (!markovChain.Ready || markovChain.Nodes.Count == 0)
				return Localization.Get("shmalala_notready", msg.Chat.Id);

			var textBuilder = new StringBuilder();

			// Use Markov chain to generate random message, composed of one or more sentences.
			for (int i = 0; i < RNG.NextNoMemory(1, 4); i++)
				textBuilder.Append(GenerateRandomSentence(default));

			var completeMessage = textBuilder.ToString();

			if (useTranslation)
			{
				var ruenTranslate = await Translate(completeMessage, "en");
				if (!string.IsNullOrEmpty(ruenTranslate))
				{
					var enruTranslate = await Translate(ruenTranslate, "ru");
					if (!string.IsNullOrEmpty(enruTranslate))
					{
						return enruTranslate;
					}
				}
			}

			return completeMessage;
		}

		private async Task<string> Translate(string text, string lang)
		{
			var translate = await Web.DownloadString(
				$"https://translate.yandex.net/api/v1.5/tr.json/translate?key={Config.Params.YandexTranslateToken}&text={text}&lang={lang}");

			if (!string.IsNullOrEmpty(translate))
			{
				dynamic json = JsonConvert.DeserializeObject(translate);
				if (json.code == 200)
				{
					return json.text[0];
				}
			}

			return string.Empty;
		}

		private string GenerateRandomSentence(string startNode)
		{
			// Generate sentence by using Markov chain to produce sequence of random words.
			// Note: There must be at least three words in sentence.
			int trials = 0;
			string[] words;
			do
			{
				words = markovChain.GenerateSequence(startNode).ToArray();
				if (trials++ > 10)
					break;
			}
			while (words.Length < 3);

			if (words.Length <= 0)
				return string.Empty;

			// uppercase first char
			words[0] = words[0].Substring(0, 1).ToUpper() + words[0].Remove(0, 1);

			return string.Join(" ", words) + $"{sentenceSeparators[RNG.NextNoMemory(0, sentenceSeparators.Length)]} ";
		}

		public async Task ReceiveMessage(Message message)
		{
			if (!string.IsNullOrEmpty(message.Text))
			{
				if (markovChain.Ready)
				{
					var text = message.Text.ToLower();
					text = cleanWordRegex.Replace(text, string.Empty);
					if (text.StartsWith(Localization.Get("shmalala_trigger", message.Chat.Id)))
					{
						if (markovChain.Nodes.Count == 0)
							return;

						// use random word from message to start our response from
						var words = text.Split(' ');
						if (words.Length > 1)
						{
							var textBuilder = new StringBuilder();

							// Use Markov chain to generate random message, composed of one or more sentences.
							for (int i = 0; i < RNG.NextNoMemory(1, 4); i++)
								textBuilder.Append(GenerateRandomSentence(words[RNG.NextNoMemory(1, words.Length)]));

							var completeMessage = textBuilder.ToString();
							if (!string.IsNullOrEmpty(completeMessage))
							{
								var transSuccess = false;

								if (useTranslation && RNG.NextNoMemory(1, 3) == 2)
								{
									var ruenTranslate = await Translate(completeMessage, "en");
									if (!string.IsNullOrEmpty(ruenTranslate))
									{
										var enruTranslate = await Translate(ruenTranslate, "ru");
										if (!string.IsNullOrEmpty(enruTranslate))
										{
											await API.SendMessage(enruTranslate, message.Chat.Id, replyID: message.MessageId);
											transSuccess = true;
										}
									}
								}

								if (!transSuccess)
									await API.SendMessage(completeMessage, message.Chat.Id, replyID: message.MessageId);
							}
						}

						return;
					}

					// Train Markov generator from received message text.
					// Assume it is composed of one or more coherent sentences that are themselves are composed of words.
					var sentences = text.ToLower().Split(sentenceSeparators);
					foreach (var s in sentences)
					{
						string lastWord = null;
						foreach (var w in s.Split(' '))
						{
							if (string.IsNullOrEmpty(w))
								continue;

							markovChain.Train(lastWord, w);
							lastWord = w;
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
	}
}
