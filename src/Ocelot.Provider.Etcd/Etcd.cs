namespace Ocelot.Provider.Etcd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using dotnet_etcd;
    using Infrastructure.Extensions;
    using Logging;
    using Newtonsoft.Json;
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
            // /Ocelot/Services/srvname/srvid
            var queryResult = await _etcdClient.GetRangeAsync($"/Ocelot/Services/{_config.KeyOfServiceInEtcd}");

            var services = new List<Service>();

            foreach (var dic in queryResult)
            {
                var srvs = dic.Key.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (srvs.Length == 4)
                {
                    var serviceEntry = JsonConvert.DeserializeObject<ServiceEntry>(dic.Value);
                    serviceEntry.Name = srvs[2];
                    serviceEntry.Id = srvs[3];
                    if (IsValid(serviceEntry))
                    {
                        services.Add(BuildService(serviceEntry));
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to use service Address: {serviceEntry.Host} and Port: {serviceEntry.Port} as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0");
                    }
                }
            }

            return services.ToList();
        }

        private Service BuildService(ServiceEntry serviceEntry)
        {
            return new Service(
                serviceEntry.Name,
                new ServiceHostAndPort(serviceEntry.Host, serviceEntry.Port),
                serviceEntry.Id,
              string.IsNullOrWhiteSpace(serviceEntry.Version) ? GetVersionFromStrings(serviceEntry.Tags) : serviceEntry.Version,
                serviceEntry.Tags ?? Enumerable.Empty<string>());
        }

        private bool IsValid(ServiceEntry serviceEntry)
        {
            if (string.IsNullOrEmpty(serviceEntry.Host) || serviceEntry.Host.Contains("http://") || serviceEntry.Host.Contains("https://") || serviceEntry.Port <= 0)
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