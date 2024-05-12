using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Products;

public class DeleteProduct
{
    public record Command(Guid ProductId) : IRequest<Result<Unit>>;

    internal sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var productToDelete = await _dbContext.Products
                .Where(p => p.Id == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (productToDelete is null)
                return Result.Failure<Unit>(new Error(
                    "DeleteProduct.NotFound",
                    $"Product with ID '{request.ProductId}' was not found."));

            _dbContext.Remove(productToDelete);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

public class DeleteProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteProduct.Command(id);

            var response = await sender.Send(command);

            if (response.IsFailure)
                return Results.NotFound(response.Error);

            return Results.Ok();
        })
        .WithTags("Products");
    }
}