using System;

namespace Demetra.Aggregates.Categories
{
    public record AddCategoryInput(string Name, string? Description, Guid? ParentId);
}