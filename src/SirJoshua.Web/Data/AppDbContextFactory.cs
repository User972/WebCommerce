using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SirJoshua.Web.Data;

/// <summary>
/// Design-time factory used only by the EF Core CLI (<c>dotnet ef migrations …</c>).
///
/// Its sole purpose is to let the tooling build the model and scaffold migrations WITHOUT executing
/// <c>Program.cs</c> — which runs <c>db.Database.Migrate()</c> against the real database at startup.
/// The connection string here is a deliberate throwaway localhost value: <c>migrations add</c> never
/// opens a connection, so this guarantees the tooling can never reach the production database.
/// Runtime configuration (the real connection string) is bound in <c>Program.cs</c> as usual.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time_only;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}
