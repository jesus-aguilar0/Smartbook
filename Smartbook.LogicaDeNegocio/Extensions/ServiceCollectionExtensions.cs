using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smartbook.Persistencia.Data;
using Smartbook.Persistencia.Repositories;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Smartbook.LogicaDeNegocio.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDbContext<SmartbookDbContext>(options =>
            options.UseMySql(connectionString, 
                new MySqlServerVersion(new Version(8, 0, 21)),
                mySqlOptions => mySqlOptions
                    .EnableStringComparisonTranslations()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

