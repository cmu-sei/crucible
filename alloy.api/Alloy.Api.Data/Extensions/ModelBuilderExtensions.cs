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
using System;
using System.Linq;
using System.Reflection;

namespace Alloy.Api.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Finds all IEntityTypeConfiguration classes and applies them to the ModelBuilder
        /// </summary>
        /// <param name="builder">The ModelBuilder</param>
        public static void ApplyConfigurations(this ModelBuilder builder)
        {
            var implementedConfigTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract
                    && !t.IsGenericTypeDefinition
                    && t.GetTypeInfo().ImplementedInterfaces.Any(i =>
                        i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)));

            foreach (var configType in implementedConfigTypes)
            {
                dynamic config = Activator.CreateInstance(configType);
                builder.ApplyConfiguration(config);
            }
        }

        /// <summary>
        /// If using PostgreSQL, add uuid generation extension and set all Guid Identity properties to use it
        /// Without this, the client has to provide the UUID, which doesn't matter too much for EF, but can be annoying when making manual changes to the db.
        /// </summary>
        /// <param name="builder">The ModelBuilder</param>        
        public static void AddPostgresUUIDGeneration(this ModelBuilder builder)
        {
            builder.HasPostgresExtension("uuid-ossp");

            foreach (var property in builder.Model
                .GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                        .Where(p => p.ClrType == typeof(Guid))
                            .Select(p => builder.Entity(p.DeclaringEntityType.ClrType).Property(p.Name))
                                .Where(p => p.Metadata.ValueGenerated == Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd &&
                                            p.Metadata.IsPrimaryKey())
            )
            {
                property.HasDefaultValueSql("uuid_generate_v4()");
            }
        }

        /// <summary>
        /// Translates all table and column names to snake case to better fit PostgreSQL conventions
        /// </summary>
        /// <param name="builder">The ModelBuilder</param>
        public static void UsePostgresCasing(this ModelBuilder builder)
        {
            var mapper = new Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator();

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // modify column names
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(mapper.TranslateMemberName(property.GetColumnName()));
                }

                // modify table name
                entity.SetTableName(mapper.TranslateMemberName(entity.GetTableName()));
            }
        }
    }
}

