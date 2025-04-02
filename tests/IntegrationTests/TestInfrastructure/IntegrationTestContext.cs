using DotPulsar.Abstractions;
using DotPulsar;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using System.Data;
using Dapper;
using Org.BouncyCastle.Crypto.Operators;

namespace IntegrationTests.TestInfrastructure
{
    public static class IntegrationTestContext
    {
        private static IContainer _pulsarContainer;
        private static PostgreSqlContainer _postgreSqlContainer;
        private static IDbConnection _dbConnection;
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
        public static IDbConnection DbConnection { get { return _dbConnection; } }
        public static async Task BuildPulsarContainerAsync()
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
        }

        public static async Task BuildPostgreSqlContainerAsync()
        {
            if (_postgreSqlContainer == null)
            {
                _postgreSqlContainer = new PostgreSqlBuilder()
                  .WithImage("postgres:latest")
                  .WithDatabase("testdb")
                  .WithUsername("postgres")
                  .WithPassword("postgres")
                  .WithCleanUp(true)
                  .Build();

                await _postgreSqlContainer.StartAsync();
                _dbConnection = new Npgsql.NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
            }
        }

        public static async Task DisposePulsarResourcesAsync()
        {
            if (_client != null)
                await _client.DisposeAsync();

            if (_pulsarContainer != null)
            {
                await _pulsarContainer.StopAsync();
            }

            Console.WriteLine("container stopped");
        }

        public static async Task DisposePostgreSqlResourcesAsync()
        {
            _dbConnection?.Dispose();

            if (_postgreSqlContainer != null)
                await _postgreSqlContainer.DisposeAsync();
        }
        public static string GetPulsarBrokerUrl() => "pulsar://localhost:6650";
    }
}
