/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Hosting;
using Player.Vm.Api.Data;
using Player.Vm.Api.Infrastructure.Options;

namespace Player.Vm.Api.Infrastructure.Extensions
{
    public static class DatabaseExtensions
    {
        public static IWebHost InitializeDatabase(this IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var databaseOptions = services.GetService<DatabaseOptions>();
                    var vmCtx = services.GetRequiredService<VmContext>();

                    if (vmCtx != null)
                    {
                        if (databaseOptions.DevModeRecreate)
                            vmCtx.Database.EnsureDeleted();

                        // Do not run migrations on Sqlite, only devModeRecreate allowed
                        if (!vmCtx.Database.IsSqlite())
                        {
                            vmCtx.Database.Migrate();
                        }

                        if (databaseOptions.DevModeRecreate)
                        {
                            vmCtx.Database.EnsureCreated();
                        }
                    }

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while initializing the database.");

                    // exit on database connection error on startup so app can be restarted to try again
                    throw;
                }
            }

            return webHost;
        }

        public static DbContextOptionsBuilder UseConfiguredDatabase(
            this DbContextOptionsBuilder builder,
            IConfiguration config
        )
        {
            string dbProvider = config.GetValue<string>("Database:Provider", "Sqlite").Trim();
            var connectionString = config.GetConnectionString(dbProvider);

            switch (dbProvider)
            {
                case "Sqlite":
                    builder.UseSqlite(connectionString);
                    break;

                case "SqlServer":
                    builder.UseSqlServer(connectionString);
                    break;

                case "PostgreSQL":
                    builder.UseNpgsql(connectionString);
                    break;

            }
            return builder;
        }
    }
}
