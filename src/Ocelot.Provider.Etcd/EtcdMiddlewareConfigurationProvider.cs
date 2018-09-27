namespace Ocelot.Provider.Etcd
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration.Creator;
    using Configuration.File;
    using Configuration.Repository;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Middleware;
    using Responses;

    public static class EtcdMiddlewareConfigurationProvider
    {
        public static OcelotMiddlewareConfigurationDelegate Get = async builder =>
        {
            var fileConfigRepo = builder.ApplicationServices.GetService<IFileConfigurationRepository>();
            var fileConfig = builder.ApplicationServices.GetService<IOptionsMonitor<FileConfiguration>>();
            var internalConfigCreator = builder.ApplicationServices.GetService<IInternalConfigurationCreator>();
            var internalConfigRepo = builder.ApplicationServices.GetService<IInternalConfigurationRepository>();

            if (UsingEtcd(fileConfigRepo))
            {
                await SetFileConfigInEtcd(builder, fileConfigRepo, fileConfig, internalConfigCreator, internalConfigRepo);
            }
        };

        private static bool UsingEtcd(IFileConfigurationRepository fileConfigRepo)
        {
            return fileConfigRepo.GetType() == typeof(EtcdFileConfigurationRepository);
        }

        private static async Task SetFileConfigInEtcd(IApplicationBuilder builder,
            IFileConfigurationRepository fileConfigRepo, IOptionsMonitor<FileConfiguration> fileConfig,
            IInternalConfigurationCreator internalConfigCreator, IInternalConfigurationRepository internalConfigRepo)
        {
            // get the config from Etcd.
            var fileConfigFromEtcd = await fileConfigRepo.Get();

            if (IsError(fileConfigFromEtcd))
            {
                ThrowToStopOcelotStarting(fileConfigFromEtcd);
            }
            else if (ConfigNotStoredInEtcd(fileConfigFromEtcd))
            {
                //there was no config in etcd set the file in config in etcd
                await fileConfigRepo.Set(fileConfig.CurrentValue);
            }
            else
            {
                // create the internal config from etcd data
                var internalConfig = await internalConfigCreator.Create(fileConfigFromEtcd.Data);

                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
                else
                {
                    // add the internal config to the internal repo
                    var response = internalConfigRepo.AddOrReplace(internalConfig.Data);

                    if (IsError(response))
                    {
                        ThrowToStopOcelotStarting(response);
                    }
                }

                if (IsError(internalConfig))
                {
                    ThrowToStopOcelotStarting(internalConfig);
                }
            }
        }

        private static void ThrowToStopOcelotStarting(Response config)
        {
            throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", config.Errors.Select(x => x.ToString()))}");
        }

        private static bool IsError(Response response)
        {
            return response == null || response.IsError;
        }

        private static bool ConfigNotStoredInEtcd(Response<FileConfiguration> fileConfigFromEtcd)
        {
            return fileConfigFromEtcd.Data == null;
        }
    }
}