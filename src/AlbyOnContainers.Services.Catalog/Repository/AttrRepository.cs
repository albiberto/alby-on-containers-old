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
    public class AttrRepository : RepositoryBase<AttrAggregate>
    {
        readonly RepositoryBase<AttrDesc> _repository;

        public AttrRepository(LuciferContext context) : base(context)
        {
            _repository = new RepositoryBase<AttrDesc>(context);
        }

        public Task<List<AttrDesc>> GetDescriptionsAsync(IEnumerable<Guid> ids) =>
            _context.AttrDescs!
                .AsNoTracking()
                .Where(description => ids.Contains(description.ProductId))
                .ToListAsync();

        public Task<Dictionary<Guid, AttrAggregate>> GetAttributesAsync(IEnumerable<Guid> ids) =>
            _context.Attrs!
                .AsNoTracking()
                .Where(attribute => ids.Contains(attribute.Id!.Value))
                .ToDictionaryAsync(e => e.Id!.Value);

        public async Task<AttrDesc> FindDescriptionAsync(Guid id) => await _repository.FindAsync(id);

        public async Task<IEnumerable<AttrDesc>> GetAllDescriptionsAsync(Predicate<AttrDesc>? selector = default) => await _repository.GetAllAsync(selector);
    }
}