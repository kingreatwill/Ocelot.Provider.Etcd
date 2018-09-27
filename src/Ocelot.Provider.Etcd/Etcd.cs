namespace Ocelot.Provider.Etcd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dotnet_etcd;
    using global::Consul;
    using Infrastructure.Extensions;
    using Logging;
    using ServiceDiscovery.Providers;
    using Values;

    public class Etcd : IServiceDiscoveryProvider
    {
        private readonly EtcdRegistryConfiguration _config;
        private readonly IOcelotLogger _logger;
        private readonly EtcdClient _etcdClient;
        private const string VersionPrefix = "version-";

        public Etcd(EtcdRegistryConfiguration config, IOcelotLoggerFactory factory, IEtcdClientFactory clientFactory)
        {
            _logger = factory.CreateLogger<Etcd>();
            _config = config;
            _etcdClient = clientFactory.Get(_config);
        }

        public async Task<List<Service>> Get()
        {
            var queryResult = await _etcdClient.GetAsync($"{_config.KeyOfServiceInEtcd}/Services");

            // var queryResult = await _etcdClient.Health.Service(_config.KeyOfServiceInEtcd, string.Empty, true);

            var services = new List<Service>();

            foreach (var serviceEntry in queryResult.)
            {
                if (IsValid(serviceEntry))
                {
                    services.Add(BuildService(serviceEntry));
                }
                else
                {
                    _logger.LogWarning($"Unable to use service Address: {serviceEntry.Service.Address} and Port: {serviceEntry.Service.Port} as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0");
                }
            }

            return services.ToList();
        }

        private Service BuildService(ServiceEntry serviceEntry)
        {
            return new Service(
                serviceEntry.Service.Service,
                new ServiceHostAndPort(serviceEntry.Service.Address, serviceEntry.Service.Port),
                serviceEntry.Service.ID,
                GetVersionFromStrings(serviceEntry.Service.Tags),
                serviceEntry.Service.Tags ?? Enumerable.Empty<string>());
        }

        private bool IsValid(ServiceEntry serviceEntry)
        {
            if (string.IsNullOrEmpty(serviceEntry.Service.Address) || serviceEntry.Service.Address.Contains("http://") || serviceEntry.Service.Address.Contains("https://") || serviceEntry.Service.Port <= 0)
            {
                return false;
            }

            return true;
        }

        private string GetVersionFromStrings(IEnumerable<string> strings)
        {
            return strings
                ?.FirstOrDefault(x => x.StartsWith(VersionPrefix, StringComparison.Ordinal))
                .TrimStart(VersionPrefix);
        }
    }
}