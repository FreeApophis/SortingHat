using System.Linq;
using apophis.CLI.Writer;
using Autofac;
using Moq;
using SortingHat.API.DI;
using SortingHat.CLI.Commands.Projects;
using Xunit;

namespace SortingHat.Test.Commands
{
    public class ProjectCommandsTest
    {
        private readonly Mock<IProjects> _projects = new Mock<IProjects>();
        private readonly MemoryConsoleWriter _writer = new MemoryConsoleWriter();

        public IContainer GetContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<CreateProjectCommand>().AsSelf();
            builder.Register(context => _writer).As<IConsoleWriter>();
            _projects.Setup(p => p.AddProject(It.IsAny<string>())).Returns(true);
            builder.Register(context => _projects.Object).As<IProjects>();

            return builder.Build();
        }

        [Fact]
        public void GivenTheCreateProjectCommandWithACorrectProjectNameTheProjectIsCreated()
        {
            var createProject = GetContainer().Resolve<CreateProjectCommand>();
            var options = new Mock<IOptions>();
            var projectName = "myProject";
            createProject.Execute(Enumerable.Repeat(projectName, 1), options.Object);

            // Called at least once
            _projects.Verify(p => p.AddProject(projectName), Times.Once);
            Assert.Equal("Project 'myProject' has been created, and we switched to the project.", _writer.Lines[0]);
        }

        [Fact]
        public void GivenTheCreateProjectCommandWithNoProjectNameThereIsAnError()
        {
            var createProject = GetContainer().Resolve<CreateProjectCommand>();
            var options = new Mock<IOptions>();
            createProject.Execute(Enumerable.Empty<string>(), options.Object);

            // Not called
            _projects.Verify(p => p.AddProject(It.IsAny<string>()), Times.Never);
            Assert.Equal("Not enough arguments given, please give a name for the project you want to create.", _writer.Lines[0]);
        }

        [Fact]
        public void GivenTheCreateProjectCommandWithMoreThanOneParametersIsAnError()
        {
            var createProject = GetContainer().Resolve<CreateProjectCommand>();
            var options = new Mock<IOptions>();
            createProject.Execute(Enumerable.Repeat("myProject", 2), options.Object);

            // Not called
            _projects.Verify(p => p.AddProject(It.IsAny<string>()), Times.Never);
            Assert.Equal("Too many arguments given, only one project at a time can be created.", _writer.Lines[0]);
        }

    }
}
