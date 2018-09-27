namespace Ocelot.Provider.Etcd
{
    using global::Consul;

    public interface IConsulClientFactory
    {
        IConsulClient Get(ConsulRegistryConfiguration config);
    }
}
