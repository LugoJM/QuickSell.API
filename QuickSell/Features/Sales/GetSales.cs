using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Sales;

public class GetSales
{
    public record Query : IRequest<Result<List<Domain.Sale>>>;

    internal sealed class Handler : IRequestHandler<Query, Result<List<Domain.Sale>>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<List<Domain.Sale>>> Handle(Query request, CancellationToken cancellationToken)
        {

            var sales = await _dbContext.Sales
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return sales;
        }
    }
}

public class GetAllSalesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/sales", async (ISender sender) =>
        {
            var query = new GetSales.Query();

            var response = await sender.Send(query);

            return Results.Ok(response.Value);
        })
        .WithTags("Sales");
    }
}
