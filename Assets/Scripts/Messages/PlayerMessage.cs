namespace Messages
{
    public class PlayerMessage: IMessage
    {
        public string Name;
        public int ConnectionId;

        public short MessageId => 3;
    }
}