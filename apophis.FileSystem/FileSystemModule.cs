using Autofac;

namespace apophis.FileSystem
{
    public class FileSystemModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SystemFileCopy>().As<ICopyFile>();
            builder.RegisterType<SystemFileMove>().As<IMoveFile>();
            builder.RegisterType<SystemFileExists>().As<IExistsFile>();
            builder.RegisterType<SystemDirectoryExists>().As<IExistsDirectory>();
        }
    }
}
