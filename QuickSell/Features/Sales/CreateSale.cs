using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Contracts;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Sales;

public class CreateSale
{
    public class Command : IRequest<Result<Guid>>
    {
        public ICollection<Domain.SaleItem> SaleItems { get; set; } = [];
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.SaleItems)
                .Must(si => si.Count > 0)
                .WithMessage("The filed '{PropertyName}' must contain at least 1 product.");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Guid>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new Validator();
            var validationResults = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResults.IsValid)
                return Result.Failure<Guid>(new Error(
                    "CreateSale.Validation",
                    validationResults.ToString()));

            var saleItemsEntity = request.SaleItems.Adapt<List<Domain.SaleItem>>();
            var saleProductList = new List<Domain.Product>();
            var productsSold = 0;
            var subTotal = 0.0;

            foreach (var saleItem in saleItemsEntity)
            {
                var productData = await _dbContext.Products
                    .Where(p => p.Id == saleItem.ProductId)
                    .FirstOrDefaultAsync(cancellationToken);


                if (productData is null)
                    return Result.Failure<Guid>(new Error(
                        "CreateSale.ProductNotFound",
                        $"The product with the ID '{saleItem.ProductId}' was not found."));

                if (!IsEnoughProductStockAvailable(productData, saleItem.Quantity))
                    return Result.Failure<Guid>(new Error(
                        "CreateSale.InsufficientStock",
                        $"Not enough available stock for the product with ID '{saleItem.ProductId}' ."));

                productsSold += saleItem.Quantity;
                subTotal += saleItem.Quantity * productData.Price;

                productData.Stock -= saleItem.Quantity;
                saleProductList.Add(productData);
            }

            var tax = subTotal * 0.15;
            var total = subTotal * 1.15;

            var sale = new Domain.Sale
            {
                SubTotal = subTotal,
                Tax = tax,
                Total = total,
                ProductsInSale = productsSold,
                SaleItems = saleItemsEntity
            };

            _dbContext.Products.UpdateRange(saleProductList);
            _dbContext.Sales.Add(sale);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return sale.Id;
        }

        bool IsEnoughProductStockAvailable(Domain.Product product, int requiredSaleStock) =>  requiredSaleStock <= product.Stock;
    }
}


public class CreateSaleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/sales", async (CreateSaleRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateSale.Command>();

            var response = await sender.Send(command);

            if (response.IsFailure)
                return Results.BadRequest(response.Error);

            return Results.Ok(response.Value);
        })
        .WithTags("Sales");
    }
}