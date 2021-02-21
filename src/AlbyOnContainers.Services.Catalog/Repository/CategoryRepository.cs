using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Infrastructure;
using Catalog.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Repository
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CategoryRepository : RepositoryBase<Category>
    {
        public CategoryRepository(ApplicationContext context) : base(context)
        {
        }

        public Task<Dictionary<Guid, Category>> Filter(IEnumerable<Guid> ids) =>
            _context?.Categories
                ?.Where(micro => ids.Contains(micro.Id!.Value))
                .ToDictionaryAsync(e => e.Id!.Value)
            ?? Task.FromResult(new Dictionary<Guid, Category>());
    }
}