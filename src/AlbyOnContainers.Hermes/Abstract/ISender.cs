using System.Threading.Tasks;

namespace Hermes.Abstract
{
    public interface ISender<in T>
    {
        Task SendAsync(T message);
    }
}