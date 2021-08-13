using System;

namespace Demetra.Aggregates.AttrDescr
{
    public record AddAttrDescrInput(string Name, string? Description, Guid AttrId, Guid ProductId);
}