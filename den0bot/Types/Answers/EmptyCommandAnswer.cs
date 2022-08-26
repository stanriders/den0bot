// den0bot (c) StanR 2022 - MIT License
namespace den0bot.Types.Answers;

/// <summary>
/// Sometimes we want to reply to the message in the module without returning anything to the main processing loop
/// To make sure command is processed correctly we send an empty answer
/// </summary>
public class EmptyCommandAnswer : ICommandAnswer
{
}