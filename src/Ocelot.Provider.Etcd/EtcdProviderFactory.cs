namespace Ocelot.Provider.Etcd
{
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceDiscovery;

    public static class EtcdProviderFactory
    {
        public static ServiceDiscoveryFinderDelegate Get = (provider, config, name) =>
        {
            var factory = provider.GetService<IOcelotLoggerFactory>();

            var consulFactory = provider.GetService<IEtcdClientFactory>();

            var consulRegistryConfiguration = new EtcdRegistryConfiguration(config.Host, config.Port, name);

            var consulServiceDiscoveryProvider = new Etcd(consulRegistryConfiguration, factory, consulFactory);

            if (config.Type?.ToLower() == "polletcd")
            {
                return new PollEtcd(config.PollingInterval, factory, consulServiceDiscoveryProvider);
            }

            return consulServiceDiscoveryProvider;
        };
    }
}