namespace Ocelot.Provider.Etcd
{
    public class EtcdRegistryConfiguration
    {
        public EtcdRegistryConfiguration(string host, int port, string keyOfServiceInEtcd)
        {
            this.Host = string.IsNullOrEmpty(host) ? "localhost" : host;
            this.Port = port > 0 ? port : 2379;
            this.KeyOfServiceInEtcd = keyOfServiceInEtcd;

            // this.Token = token;
        }

        public string KeyOfServiceInEtcd { get; }

        public string Host { get; }

        public int Port { get; }

        // public string Token { get; }
    }
}