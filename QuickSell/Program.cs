using Microsoft.EntityFrameworkCore;
using QuickSell.Extensions;
using QuickSell.Persistence;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<QuickSellDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
}

var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.ApplyMigrations();
    }

    app.UseHttpsRedirection();
    app.Run();
}
