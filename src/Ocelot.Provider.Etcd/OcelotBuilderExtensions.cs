namespace Ocelot.Provider.Etcd
{
    using Configuration.Repository;
    using DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using Middleware;
    using ServiceDiscovery;

    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddEtcd(this IOcelotBuilder builder)
        {
            builder.Services.AddSingleton<ServiceDiscoveryFinderDelegate>(EtcdProviderFactory.Get);
            builder.Services.AddSingleton<IEtcdClientFactory, EtcdClientFactory>();
            return builder;
        }

        public static IOcelotBuilder AddConfigStoredInEtcd(this IOcelotBuilder builder)
        {
            builder.Services.AddSingleton<OcelotMiddlewareConfigurationDelegate>(EtcdMiddlewareConfigurationProvider.Get);
            builder.Services.AddHostedService<FileConfigurationPoller>();
            builder.Services.AddSingleton<IFileConfigurationRepository, EtcdFileConfigurationRepository>();
            return builder;
        }
    }
}