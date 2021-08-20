namespace Common
{
    public partial class Order
    {
        public readonly static Order empty = new Order() { Type = OrderType.Empty };
        private readonly object locker = new object();
        public bool TryGet()
        {
            lock (locker)
            {
                if (stat == Status.Created)
                {
                    stat = Status.Getted;
                    //order = this;
                    return true;
                }
                else
                {
                    //order = empty;
                    return false;
                }
            }
        }
        public enum Status
        {
            Created,
            Getted,
            Executed
        }
        //private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        private Status stat;
        public Status status
        {
            get
            {
                lock (locker)
                {
                    Status result = stat;
                    return result;
                }
            }
            set
            {
                lock (locker)
                {
                    stat = value;
                }
            }
        }
    }
}
