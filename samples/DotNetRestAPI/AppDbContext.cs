using Microsoft.EntityFrameworkCore;

namespace DotNetRestAPI;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
}
