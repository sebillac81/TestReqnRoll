using IntegrationTests.TestInfrastructure;
using NUnit.Framework;

namespace IntegrationTests
{
    public abstract class BaseIntegrationTest
    {
        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            await TestContainerManager.StartPulsarContainerAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await TestContainerManager.StopPulsarContainerAsync();
        }
    }
}
