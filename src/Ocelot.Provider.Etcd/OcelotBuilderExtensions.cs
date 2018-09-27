namespace Ocelot.Provider.Etcd
{
    using Configuration.Repository;
    using DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using ServiceDiscovery;

    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddConsul(this IOcelotBuilder builder)
        {
            builder.Services.AddSingleton<ServiceDiscoveryFinderDelegate>(EtcdProviderFactory.Get);
            builder.Services.AddSingleton<IEtcdClientFactory, EtcdClientFactory>();
            return builder;
        }

        public static IOcelotBuilder AddConfigStoredInConsul(this IOcelotBuilder builder)
        {
            builder.Services.AddSingleton<OcelotMiddlewareConfigurationDelegate>(EtcdMiddlewareConfigurationProvider.Get);
            builder.Services.AddHostedService<FileConfigurationPoller>();
            builder.Services.AddSingleton<IFileConfigurationRepository, EtcdFileConfigurationRepository>();
            return builder;
        }
    }
}