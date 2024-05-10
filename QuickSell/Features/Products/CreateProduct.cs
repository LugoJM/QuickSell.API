using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using QuickSell.Contracts;
using QuickSell.Persistence;
using QuickSell.Shared;

namespace QuickSell.Features.Products;

public class CreateProduct
{
    public class Command : IRequest<Result<Guid>>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; } = 0;
        public int Stock { get; set; } = 0;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The filed '{PropertyName}' cannot be empty");
            RuleFor(c => c.Description).NotEmpty().WithMessage("The filed '{PropertyName}' cannot be empty");
            RuleFor(c => c.Price).Must(arg => arg > 0)
                .WithMessage("The field '{PropertyName}' must have a value greater than 0");
            RuleFor(c => c.Stock).Must(arg => arg > 0)
                .WithMessage("The field '{PropertyName}' must have a value greater than 0");
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
                    "CreateProduct.Validation",
                    validationResults.ToString()));

            var productEntity = request.Adapt<Domain.Product>();

            _dbContext.Add(productEntity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return productEntity.Id;
        }
    }
}

public class CreateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/products", async (CreateProductRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateProduct.Command>();

            var response = await sender.Send(command);

            if (response.IsFailure)
                return Results.BadRequest(response.Error);

            return Results.Ok(response.Value);
        })
        .WithTags("Products");
    }
}