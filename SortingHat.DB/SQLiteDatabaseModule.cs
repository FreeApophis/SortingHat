using Autofac;
using SortingHat.API;
using SortingHat.API.DI;
using SortingHat.DB.Access;
using Module = Autofac.Module;

namespace SortingHat.DB;

public class SqliteDatabaseModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<RevisionMigrator>().As<IRevisionMigrator>().InstancePerLifetimeScope();
        builder.RegisterType<TagNameGrammarMigration>().As<ICodeMigration>().SingleInstance();
        builder.RegisterType<TagNameKeyMigration>().As<ICodeMigration>().SingleInstance();

        // the databases open and migrate on their first use
        builder.Register(context => new SQLiteProjectDatabase(context.Resolve<DatabaseSettings>(), context.Resolve<ISettings>(), context.Resolve<IRevisionMigrator>()))
            .As<SQLiteProjectDatabase>().As<IProjectDatabase>().InstancePerLifetimeScope();
        builder.Register(context => new SQLiteMainDatabase(context.Resolve<DatabaseSettings>(), context.Resolve<IRevisionMigrator>()))
            .As<SQLiteMainDatabase>().As<IMainDatabase>().InstancePerLifetimeScope();

        // Register DataAccess
        builder.RegisterType<SQLiteFile>().As<IFile>().InstancePerLifetimeScope();
        builder.RegisterType<SQLiteTag>().As<ITag>().AsSelf().InstancePerLifetimeScope();
        builder.RegisterType<SQLiteSettings>().As<ISettings>().InstancePerLifetimeScope();
        builder.RegisterType<SQLiteProjects>().As<IProjects>().InstancePerLifetimeScope();
    }
}
