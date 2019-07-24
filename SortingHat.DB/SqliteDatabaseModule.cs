using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using SortingHat.API;
using SortingHat.API.DI;
using Module = Autofac.Module;

namespace SortingHat.DB
{
    public class SqliteDatabaseModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(CreateProjectDatabase()).As<SQLiteProjectDatabase>().As<IProjectDatabase>().InstancePerLifetimeScope();
            builder.Register(CreateMainDatabase()).As<SQLiteMainDatabase>().As<IMainDatabase>().InstancePerLifetimeScope();

            builder.RegisterType<SQLiteFile>().As<IFile>().InstancePerLifetimeScope();
            builder.RegisterType<SQLiteTag>().As<ITag>().InstancePerLifetimeScope();
            builder.RegisterType<SQLiteSettings>().As<ISettings>().InstancePerLifetimeScope();
        }

        private Func<IComponentContext, SQLiteMainDatabase> CreateMainDatabase()
        {
            return context =>
            {

                var db = new SQLiteMainDatabase(
                    () => context.Resolve<ISettings>(), 
                    projectName => context.Resolve<IProjectDatabase>(new NamedParameter("projectName", projectName)),
                    context.Resolve<DatabaseSettings>()
                    );

                RunDatabaseSetup(db);

                return db;
            };
        }

        private static Func<IComponentContext, IEnumerable<Parameter>, SQLiteProjectDatabase> CreateProjectDatabase()
        {
            return (context, parameters) =>
            {
                var db = new SQLiteProjectDatabase(
                    context.Resolve<IFile>, 
                    context.Resolve<ITag>, 
                    context.Resolve<DatabaseSettings>(),
                    parameters.Named<string>("projectName"));

                RunDatabaseSetup(db);

                return db;
            };
        }

        private static void RunDatabaseSetup(SQLiteDatabase db)
        {
            var migrator = new RevisionMigrator(db);

            migrator.Initialize();
            migrator.Migrate();
        }
    }
}
