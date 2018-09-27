namespace Ocelot.Provider.Etcd
{
    using System;
    using dotnet_etcd;
    using global::Consul;

    public class EtcdClientFactory : IEtcdClientFactory
    {
        public EtcdClient Get(EtcdRegistryConfiguration config)
        {
            return new EtcdClient(config.Host, config.Port);
        }
    }
}