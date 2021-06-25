using System.Collections.Generic;
using Demetra.Model;

namespace Demetra.Products
{
    public class AddProductPayload : ProductPayloadBase
    {
        public AddProductPayload(Product product) : base(product)
        {
        }

        public AddProductPayload(IReadOnlyList<UserError> errors) : base(errors)
        {
        }
    }
}