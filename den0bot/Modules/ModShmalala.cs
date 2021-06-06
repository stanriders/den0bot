//#define GPT
#define GPT_SBERBANK
#define GPT_TALKONLY
// den0bot (c) StanR 2021 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using den0bot.Types;
using den0bot.Types.Answers;
using den0bot.Util;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace den0bot.Modules
{
	internal class ModShmalala : IModule, IReceiveAllMessages, IReceiveShutdown
	{
		// Based on https://github.com/IrcDotNet/IrcDotNet/tree/master/samples/IrcDotNet.Samples.MarkovTextBot
		private class MarkovChain
		{
			public class MarkovChainNode : IEquatable<MarkovChainNode>
			{
				public List<MarkovChainNode> Links { get; } = new();
				public string Word { get; }

				public MarkovChainNode(string w)
				{
					Word = w;
				}

				public void AddLink(MarkovChainNode toNode)
				{
					Links.Add(toNode);
				}

				public bool Equals(MarkovChainNode other)
				{
					if (ReferenceEquals(null, other)) return false;
					if (ReferenceEquals(this, other)) return true;
					return Equals(Links, other.Links) && Word == other.Word;
				}

				public override bool Equals(object obj)
				{
					if (ReferenceEquals(null, obj)) return false;
					if (ReferenceEquals(this, obj)) return true;
					if (obj.GetType() != this.GetType()) return false;
					return Equals((MarkovChainNode) obj);
				}

				public override int GetHashCode()
				{
					return HashCode.Combine(Links, Word);
				}
			}

			public List<MarkovChainNode> Nodes { get; } = new();

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
						Nodes.Add(new MarkovChainNode(word.Key));
					}

					// if chain is getting really big it might take a while to unpack
					ThreadPool.QueueUserWorkItem(_ =>
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
					node = new MarkovChainNode(value);
					Nodes.Add(node);
				}
				return node;
			}

			private MarkovChainNode GetExistingNode(string value)
			{
				MarkovChainNode node = null;
				if (!string.IsNullOrEmpty(value))
				{
					node = Nodes.SingleOrDefault(n => n.Word == value && n.Links.Distinct().Count() > 2);
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

			public static void DeleteFile()
			{
				if (System.IO.File.Exists(file_path))
					System.IO.File.Delete(file_path);
			}
		}

		private readonly char[] sentenceSeparators = { '.', '.', '.', '!', '!', '?', '(', ')', '\n' };
		private readonly Regex cleanWordRegex = 
			new(@"[()\[\]{}'""`~\\\/\-*\d]|(http|ftp|https):\/\/([\w_-]+(?:(?:\.[\w_-]+)+))([\w.,@?^=%&:\/~+#-]*[\w@?^=%&\/~+#-])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private int numTrainingMessagesReceived;

		private const int min_nodes = 100;

		private MarkovChain markovChain = new();

		public ModShmalala()
		{
			AddCommands(new [] 
			{
				new Command
				{
					Name = "talk",
					Reply = true,
					Action = SendRandomMessage
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
						return new TextCommandAnswer(Localization.FormatGet("shmalala_stats", msg.Chat.Id, markovChain.Nodes.Count, linkAmount));
					}
				},
				new Command
				{
					Name = "talkdump",
					Action = msg =>
					{
						markovChain.SaveToFile();
						return new TextCommandAnswer("k cool");
					},
					IsOwnerOnly = true
				},
				new Command
				{
					Name = "talkwipe",
					Action = msg =>
					{
						MarkovChain.DeleteFile();
						markovChain = new MarkovChain();
						return new TextCommandAnswer("Done!");
					},
					IsAdminOnly = true
				}
			});
		}

		private ICommandAnswer SendRandomMessage(Message msg)
		{
#if GPT || GPT_TALKONLY
			if (msg.Text.Length > 5)
			{
				var input = msg.Text[6..];
				if (!string.IsNullOrEmpty(input))
				{
					var response = GetGptResponse(input).Result;
					if (!string.IsNullOrEmpty(response))
					{
						return new TextCommandAnswer($"{input} {response}");
					}
				}
			}

			return null;
#else

			if (!markovChain.Ready || markovChain.Nodes.Count <= min_nodes)
				return Localization.GetAnswer("shmalala_notready", msg.Chat.Id);

			var textBuilder = new StringBuilder();

			// Use Markov chain to generate random message, composed of one or more sentences.
			for (int i = 0; i < RNG.NextNoMemory(1, 4); i++)
				textBuilder.Append(GenerateRandomSentence(default));

			return new TextCommandAnswer(textBuilder.ToString());
#endif
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
			if (string.IsNullOrEmpty(message.Text) || message.Chat.Type == ChatType.Private || !markovChain.Ready)
				return;

			var text = message.Text.ToLower();
			text = cleanWordRegex.Replace(text, string.Empty);
			var trigger = Localization.Get("shmalala_trigger", message.Chat.Id);
			if (text.StartsWith(trigger))
			{
#if GPT
				var response = await GetGptResponse(text[trigger.Length..]);
				if (!string.IsNullOrEmpty(response))
				{
					await API.SendMessage(response, message.Chat.Id, replyToId: message.MessageId);
				}
#else
				if (markovChain.Nodes.Count <= min_nodes)
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
						await API.SendMessage(completeMessage, message.Chat.Id, replyToId: message.MessageId);
					}
			}
#endif
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

		public void Shutdown()
		{
			markovChain.SaveToFile();
		}
#if GPT_YANDEX
		private static async Task<string> GetGptResponse(string input)
		{
			var response = await Web.PostJson("https://yandex.ru/lab/api/gpt3/text2", JsonConvert.SerializeObject(new
			{
				intro = 0,
				query = input
			}));

			dynamic json = JsonConvert.DeserializeObject(response);
			if (json != null)
			{
				if (json.error == 0)
					return json.text.ToString();
				
				Log.Warning($"GPT error: {json.error?.ToString()}");
			}
			else
			{
				Log.Warning($"GPT error: {response}");
			}

			return string.Empty;
		}
#elif GPT_SBERBANK
		private static async Task<string> GetGptResponse(string input)
		{
			var response = await Web.PostJson("https://api.aicloud.sbercloud.ru/public/v1/public_inference/gpt3/predict", JsonConvert.SerializeObject(new
			{
				text = input
			}));

			dynamic json = JsonConvert.DeserializeObject(response);
			if (json != null)
			{
				return json.predictions.ToString();
			}

			Log.Warning($"GPT error: {response}");
			return string.Empty;
		}
#endif
	}
}
