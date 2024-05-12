using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickSell.Contracts;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Products;

public class UpdateProduct
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Stock { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(p => p.Id).NotNull().NotEmpty().WithMessage("The field '{PropertyName} can not be null or empty.");
            RuleFor(c => c.Name).NotEmpty().WithMessage("The filed '{PropertyName}' cannot be empty");
            RuleFor(c => c.Description).NotEmpty().WithMessage("The filed '{PropertyName}' cannot be empty");
            RuleFor(c => c.Price).Must(arg => arg > 0)
                .WithMessage("The field '{PropertyName}' must have a value greater than 0");
            RuleFor(c => c.Stock).Must(arg => arg > 0)
                .WithMessage("The field '{PropertyName}' must have a value greater than 0");
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly QuickSellDbContext _dbContext;

        public Handler(QuickSellDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validator = new Validator();
            var validationResults = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResults.IsValid)
                return Result.Failure<Unit>(new Error(
                    "UpdateProduct.Validation",
                    validationResults.ToString()));

            var productToUpdate = request.Adapt<Domain.Product>();

            _dbContext.Update(productToUpdate);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/products", async (UpdateProductRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateProduct.Command>();

            var response = await sender.Send(command);

            if (response.IsFailure)
                return Results.BadRequest(response.Error);

            return Results.Ok();
        })
        .WithTags("Products");
    }
}