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

            var etcdFactory = provider.GetService<IEtcdClientFactory>();

            var etcdRegistryConfiguration = new EtcdRegistryConfiguration(config.Host, config.Port, name);

            var etcdServiceDiscoveryProvider = new Etcd(etcdRegistryConfiguration, factory, etcdFactory);

            if (config.Type?.ToLower() == "polletcd")
            {
                return new PollEtcd(config.PollingInterval, factory, etcdServiceDiscoveryProvider);
            }

            return etcdServiceDiscoveryProvider;
        };
    }
}