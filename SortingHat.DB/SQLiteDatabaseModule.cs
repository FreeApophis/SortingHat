using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.DB.Access;
using Module = Autofac.Module;

namespace SortingHat.DB
{
    public class SqliteDatabaseModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RevisionMigrator>().As<IRevisionMigrator>().InstancePerLifetimeScope();

            // Register Databases
            builder.Register(CreateProjectDatabase()).As<SQLiteProjectDatabase>().As<IProjectDatabase>().InstancePerLifetimeScope();
            builder.Register(CreateMainDatabase()).As<SQLiteMainDatabase>().As<IMainDatabase>().InstancePerLifetimeScope();

            // Register DataAccess
            builder.RegisterType<SQLiteFile>().As<IFile>().InstancePerLifetimeScope();
            builder.RegisterType<SQLiteTag>().As<ITag>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<SQLiteSettings>().As<ISettings>().InstancePerLifetimeScope();
            builder.RegisterType<SQLiteProjects>().As<IProjects>().InstancePerLifetimeScope();
        }

        private Func<IComponentContext, SQLiteMainDatabase> CreateMainDatabase()
        {
            return context =>
            {
                var db = new SQLiteMainDatabase(context.Resolve<DatabaseSettings>());

                return RunDatabaseSetup(context.Resolve<IRevisionMigrator>(), db);
            };
        }

        private static Func<IComponentContext, IEnumerable<Parameter>, SQLiteProjectDatabase> CreateProjectDatabase()
        {
            return (context, parameters) =>
            {
                var db = new SQLiteProjectDatabase(
                    context.Resolve<DatabaseSettings>(),
                    context.Resolve<ISettings>());

                return RunDatabaseSetup(context.Resolve<IRevisionMigrator>(), db);
            };
        }

        private static TDatabase RunDatabaseSetup<TDatabase>(IRevisionMigrator migrator, TDatabase db)
            where TDatabase : SQLiteDatabase
        {
            migrator.Migrate(db);

            return db;
        }
    }
}
