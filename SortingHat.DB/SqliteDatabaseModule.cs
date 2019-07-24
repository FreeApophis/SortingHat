using Autofac;
using SortingHat.API.DI;

namespace SortingHat.DB
{
    public class SqliteDatabaseModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SQLiteProjectDatabase>().As<IProjectDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<SQLiteMainDatabase>().As<IMainDatabase>().AsSelf().SingleInstance();
            builder.RegisterType<SQLiteFile>().As<IFile>().SingleInstance();
            builder.RegisterType<SQLiteTag>().As<ITag>().SingleInstance();
        }
    }
}
