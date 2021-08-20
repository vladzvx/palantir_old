namespace Bot.Core.Enums
{
    public enum BotState : int
    {
        Started = 0,
        ConfiguringDepth = 1,
        ConfiguringGroups = 4,
        ConfiguringChannel = 5,
        Ready = 2,
        Searching = 3
    }
}
