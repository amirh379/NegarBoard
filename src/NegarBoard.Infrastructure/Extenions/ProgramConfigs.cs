using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NegarBoard.Application.Contracts;
using NegarBoard.Application.Services;
using System.Data;

namespace NegarBoard.Infrastructure.Extenions;

public static class ProgramConfigs
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDatabaseService, DatabaseService>();
        services.AddScoped<IDbConnection>(sp =>
            new SqlConnection(configuration.GetConnectionString("NegarBoard")));
    }
}
