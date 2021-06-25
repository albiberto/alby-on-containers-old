namespace Demetra.Model
{
    public class AddProductPayload
    {
        public AddProductPayload(Product product)
        {
            Product = product;
        }

        public Product Product { get; }
    }
}