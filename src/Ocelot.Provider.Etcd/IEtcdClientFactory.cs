namespace Ocelot.Provider.Etcd
{
    using dotnet_etcd;

    public interface IEtcdClientFactory
    {
        EtcdClient Get(EtcdRegistryConfiguration config);
    }
}