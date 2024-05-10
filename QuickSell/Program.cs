using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuickSell.Extensions;
using QuickSell.Persistence;

var builder = WebApplication.CreateBuilder(args);
{
    var assembly = typeof(Program).Assembly;

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<QuickSellDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
    builder.Services.AddValidatorsFromAssembly(assembly);

    builder.Services.AddCarter();
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.ApplyMigrations();
    }

    app.MapCarter();

    app.UseHttpsRedirection();
    app.Run();
}
