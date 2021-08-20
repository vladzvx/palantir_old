namespace Common.Services.Interfaces
{
    public interface ICommonWriter
    {
        public void PutData(object data);

        public int GetQueueCount();
    }
}
