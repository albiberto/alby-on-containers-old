using System.Linq;
using System.Threading.Tasks;
using AlbyOnContainers.Tools.Catalog.Model;
using GraphQL;
using GraphQL.Client.Http;
using Microsoft.AspNetCore.Components;

namespace AlbyOnContainers.Tools.Catalog.Pages.Products
{
    public class ProductsComponent : ComponentBase
    {
        [Inject] GraphQLHttpClient Client { get; init; }

        protected ProductType[] _products;

        protected override async Task OnInitializedAsync()
        {
            var request = new GraphQLRequest {Query = @"{ products { id, name }}"};
            var response = await Client.SendQueryAsync<ProductsType>(request);

            _products = response.Data.Products.ToArray();
        }
    }
}