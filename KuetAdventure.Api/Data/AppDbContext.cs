using KuetAdventure.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KuetAdventure.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MembershipApplication> MembershipApplications => Set<MembershipApplication>();
}
