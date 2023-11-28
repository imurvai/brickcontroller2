using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace BrickController2.Helpers;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder ConfigureContainer(this MauiAppBuilder builder, Action<ContainerBuilder> configure)
    {
        builder.ConfigureContainer(new AutofacServiceProviderFactory(), configure);
        return builder;
    }
}
