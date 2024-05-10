using Carter;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Contracts;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Products;

public class GetProducts
{
    public record Query : IRequest<Result<List<ProductResponse>>>;

    internal sealed class Handler : IRequestHandler<Query, Result<List<ProductResponse>>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<List<ProductResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var products = await _dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);

            var productsResponseList = products.Adapt<List<ProductResponse>>();

            return productsResponseList;
        }
    }
}

public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products", async (ISender sender) =>
        {
            var query = new GetProducts.Query();

            var response = await sender.Send(query);

            return Results.Ok(response.Value);
        })
        .WithTags("Products");
    }
}