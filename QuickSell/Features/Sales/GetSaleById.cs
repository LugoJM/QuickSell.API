using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Sales;

public class GetSaleById
{
    public record Query(Guid SaleId) : IRequest<Result<Domain.Sale>>;


    internal sealed class Handler : IRequestHandler<Query, Result<Domain.Sale>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<Domain.Sale>> Handle(Query request, CancellationToken cancellationToken)
        {
            var sale = await _dbContext.Sales.AsNoTracking()
                .Where(s => s.Id == request.SaleId)
                .FirstOrDefaultAsync(cancellationToken);

            if (sale is null)
                return Result.Failure<Domain.Sale>(new Error(
                    "GetSaleById.SaleNotFound",
                    $"The Sale with the ID '{request.SaleId}' was not found."));

            var saleItems = await _dbContext.SaleItems.AsNoTracking()
                .Where(si => si.SaleId == sale.Id)
                .ToListAsync(cancellationToken);

            foreach (var saleItem in saleItems)
            {
                var product = await _dbContext.Products.AsNoTracking()
                    .Where(p => p.Id == saleItem.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);

                saleItem.Product = product!;
            }

            sale.SaleItems = saleItems;

            return sale;
        }
    }
}

public class GetSaleByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/sales/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetSaleById.Query(id);

            var response = await sender.Send(query);

            if (response.IsFailure)
                return Results.NotFound(response.Error);

            return Results.Ok(response.Value);
        })
        .WithTags("Sales");
    }
}