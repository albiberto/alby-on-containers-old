﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;

namespace Libraries.IHostExtensions
{
    // ReSharper disable once InconsistentNaming
    public static class Extensions
    {
        public static async Task MigrateAsync<TContext>(this IHost host, int retries = 10) where TContext : DbContext
        {
            var inK8S = host.IsInKubernetes();

            using var scope = host.Services.CreateScope();
            var provider = scope.ServiceProvider;
            
            var context = provider.GetRequiredService<IDbContextFactory<TContext>>();
            var seeder = provider.GetService<IDbContextSeed<TContext>>();
            var logger = provider.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);
                
                if (inK8S)
                {
                    await InvokeSeederAsync(seeder, context.CreateDbContext());
                }
                else
                {
                    var retry = Policy.Handle<NpgsqlException>()
                        .WaitAndRetryAsync(retries,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            (exception, _, r, _) => logger.LogWarning(exception, "[{prefix}] Exception {ExceptionType} with message {Message} detected on attempt {retry} of {retries}", nameof(TContext), exception.GetType().Name,
                                exception.Message, r, retries)
                        );

                    //if the sql server container is not created on run docker compose this
                    //migration can't fail for network related exception. The retry options for DbContext only 
                    //apply to transient exceptions
                    // Note that this is NOT applied when running some orchestrators (let the orchestrator to recreate the failing service)
                    await retry.ExecuteAsync(async () => await InvokeSeederAsync(seeder, context.CreateDbContext()));
                }

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
                if (inK8S) throw; // Rethrow under k8s because we rely on k8s to re-run the pod
            }
        }

        static bool IsInKubernetes(this IHost webHost)
        {
            var cfg = webHost.Services.GetService<IConfiguration>();
            var orchestratorType = cfg.GetValue<string>("OrchestratorType");
            return orchestratorType?.ToUpper() == "K8S";
        }

        static async Task InvokeSeederAsync<TContext>(IDbContextSeed<TContext>? seeder, TContext? context) where TContext : DbContext
        {
            if(context?.Database != null) await context.Database.MigrateAsync();
            if(seeder != default) await seeder.SeedAsync();
        }
    }
}