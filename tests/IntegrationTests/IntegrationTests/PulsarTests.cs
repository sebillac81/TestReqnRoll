
using DotPulsar;
using DotPulsar.Extensions;
using IntegrationTests.TestInfrastructure;
using System.Text;

namespace IntegrationTests.IntegrationTests
{
    [TestFixture]
    public class PulsarTests: BaseIntegrationTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task ShouldProduceAndConsumeMessages()
        {
            await Task.Delay(TimeSpan.FromSeconds(30));

            var producer = TestContainerManager.Client.NewProducer()
                .Topic("public/default/my-topic")
                .Create();

            await producer.Send(Encoding.UTF8.GetBytes("Hello Pulsar!"));

            var consumer = TestContainerManager.Client.NewConsumer()
                .SubscriptionName("my-subscription")
                .Topics("public/default/my-topic")
                .InitialPosition( SubscriptionInitialPosition.Earliest)
                .Create();

            var message = await consumer.Receive();
            Assert.That(Encoding.UTF8.GetString(message.Data), Is.EqualTo("Hello Pulsar!"));
        }
    }
}