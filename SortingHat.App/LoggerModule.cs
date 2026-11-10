using System;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using Serilog;

namespace SortingHat.App;

/// <summary>
/// Gives every class that takes a Serilog <see cref="ILogger"/> one that names the class, such as
/// <c>SortingHat.Plugin.MoveOnTag.MoveOnTagHandler</c>, so neither hat nor a plugin has to call ForContext.
/// It works on the registrations of the builder it is registered with: the container and the scope of the plugins each get it.
/// </summary>
public sealed class LoggerModule : Module
{
    protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration)
        => registration.PipelineBuilding += (_, pipeline) => pipeline.Use(new ContextLogger());

    private sealed class ContextLogger : IResolveMiddleware
    {
        public PipelinePhase Phase => PipelinePhase.ParameterSelection;

        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            context.ChangeParameters(context.Parameters.Append(new ResolvedParameter(
                (parameter, _) => parameter.ParameterType == typeof(ILogger),
                (parameter, components) => components.Resolve<ILogger>().ForContext(parameter.Member.DeclaringType ?? typeof(ILogger)))));

            next(context);
        }
    }
}
