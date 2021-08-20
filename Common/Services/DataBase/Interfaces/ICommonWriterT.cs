using System.Threading.Tasks;

namespace Common.Services.Interfaces
{
    public interface ICommonWriter<T> where T : class
    {
        public void PutData(T data);
        public Task ExecuteAdditionalAction(object data);
        public int GetQueueCount();
    }
}
