using DotPulsar.Abstractions;
using DotPulsar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;

namespace IntegrationTests.TestInfrastructure
{
    public static class IntegrationTestContext
    {
        private static IContainer _pulsarContainer;
        private static IPulsarClient _client;
        public static IContainer PulsarContainer {  get { return _pulsarContainer; } }
        public static IPulsarClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = PulsarClient.Builder()
                        .ServiceUrl(new Uri(GetPulsarBrokerUrl()))
                        .Build();
                    Console.WriteLine("Client created");
                }
                return _client;
            }

            private set
            {
                _client = value;
            }
        }
        public static async Task<IContainer> BuildPulsarContainerAsync()
        {
            if( _pulsarContainer == null)
            {
                _pulsarContainer = new ContainerBuilder()
                .WithImage("apachepulsar/pulsar:latest")
                .WithPortBinding(6650, 6650)
                .WithPortBinding(8080, 8080)
                .WithName("pulsarTestSeba")
                .WithCommand("bin/pulsar", "standalone")
                .WithEnvironment("PULSAR_MEM", "-Xms512m -Xmx512m")
                .Build();

                await _pulsarContainer.StartAsync();
            }

            return _pulsarContainer;
        }

        public static async Task DisposeResourcesAsync()
        {
            if (_client != null)
                await _client.DisposeAsync();

            if (_pulsarContainer != null)
            {
                await _pulsarContainer.StopAsync();
            }
            Console.WriteLine("container stopped");
        }
        public static string GetPulsarBrokerUrl() => "pulsar://localhost:6650";
    }
}
