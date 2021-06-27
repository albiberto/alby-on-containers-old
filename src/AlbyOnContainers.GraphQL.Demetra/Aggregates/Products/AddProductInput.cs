using System;

namespace Demetra.Aggregates.Products
{
    public record AddProductInput(string Name, string? Description, Guid CategoryId);
}