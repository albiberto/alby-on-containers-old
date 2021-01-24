using System.Threading.Tasks;

namespace AlbyOnContainers.Hermes.Abstract
{
    public interface ISender<in T>
    {
        Task SendAsync(T message);
    }
}