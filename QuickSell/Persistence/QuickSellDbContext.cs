using Microsoft.EntityFrameworkCore;

namespace QuickSell.Persistence;

public class QuickSellDbContext(DbContextOptions options) : DbContext(options)
{

}