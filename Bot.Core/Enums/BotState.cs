namespace Bot.Core.Enums
{
    public enum ChatState
    {
        Common,
        Overrun,
    }
    public enum PrivateChatState : int
    {
        Ready = 2,
        SubFSMWorking = 1,
        Busy = 3
    }
}
