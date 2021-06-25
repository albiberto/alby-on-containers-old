using System.Collections.Generic;
using Demetra.Common;
using Demetra.Model;

namespace Demetra.Products
{
    public class ProductPayloadBase: Payload
    {
        protected ProductPayloadBase(Product product)
        {
            Product = product;
        }

        protected ProductPayloadBase(IReadOnlyList<UserError> errors)
            : base(errors)
        {
        }

        public Product? Product { get; }
    }
}