using Carter;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Contracts;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Products;

public class GetProductById
{
    public record Query(Guid ProductId) : IRequest<Result<ProductResponse>>;

    internal sealed class Handler : IRequestHandler<Query, Result<ProductResponse>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<ProductResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.AsNoTracking()
                .Where(p => p.Id == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            var productsResponse = product.Adapt<ProductResponse>();

            return productsResponse;
        }
    }
}

public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetProductById.Query(id);

            var response = await sender.Send(query);

            if (response.IsFailure)
                return Results.NotFound(response.Error);

            return Results.Ok(response.Value);
        })
        .WithTags("Products");
    }
}