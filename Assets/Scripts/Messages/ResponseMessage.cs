namespace Messages
{
    public class ResponseMessage: IMessage
    {
        public bool Success;
        public int ErrorCode;

        public short MessageId => 2;
    }
}