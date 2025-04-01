using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotPulsar;
using DotPulsar.Abstractions;


namespace IntegrationTests.TestInfrastructure
{
    internal static class TestContainerManager
    {
        private static IContainer _pulsarContainer;
        private static IPulsarClient _client;
        public static IContainer Container {  get { return _pulsarContainer; } }
        public static IPulsarClient Client
        {
            get {
                if (_client == null)
                {
                    _client = PulsarClient.Builder()
                        .ServiceUrl(new Uri(GetPulsarBrokerUrl()))
                        .Build();
                    Console.WriteLine("Client created");
                }
                return _client;
            }

            private set { 
                _client = value;
            }
        }

        public static async Task StartPulsarContainerAsync()
        {
            _pulsarContainer = new ContainerBuilder()
             .WithImage("apachepulsar/pulsar:latest")
             .WithPortBinding(6650, 6650)
             .WithPortBinding(8080, 8080)
             .WithName("pulsarTestSeba")
             .WithCommand("bin/pulsar", "standalone")
             .WithEnvironment("PULSAR_MEM","-Xms512m -Xmx512m")
             .Build();

            await _pulsarContainer.StartAsync();
            Console.WriteLine("container created");
        }
        public static async Task StopPulsarContainerAsync()
        {
            if (_client != null)
                await _client.DisposeAsync();

            if(_pulsarContainer != null)
            {
                await _pulsarContainer.StopAsync();
            }
            Console.WriteLine("container stopped");
        }

        public static string GetPulsarBrokerUrl() => "pulsar://localhost:6650";
    }
}
