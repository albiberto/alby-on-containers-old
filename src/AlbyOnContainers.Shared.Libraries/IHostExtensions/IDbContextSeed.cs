using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Libraries.IHostExtensions
{
    public interface IDbContextSeed<TContext> where TContext: DbContext?
    {
        public Task SeedAsync();
    }
}