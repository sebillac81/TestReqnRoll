using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using DotPulsar.Internal;
using IntegrationTests.TestInfrastructure;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Steps
{
    [Binding]
    internal class PulsarSteps
    {
        private IProducer<System.Buffers.ReadOnlySequence<byte>> _producer;
        private IConsumer<System.Buffers.ReadOnlySequence<byte>> _consumer;
        private string _messageSent;
        private string _messageReceived;

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            await IntegrationTestContext.BuildPulsarContainerAsync();
        }


        [Given("The Pulsar client is created")]
        public void PulsarClientIsCreated()
        {

            Assert.IsNotNull(IntegrationTestContext.Client, "The Pulsar client was not initialized");
        }

        [When(@"The producer send a message ""([^""]*)""")]
        public async Task WhenMessageIsSent(string message)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            _messageReceived = message;
            _producer = IntegrationTestContext.Client.NewProducer()
                .Topic("public/default/my-topic")
            .Create();

            await _producer.Send(Encoding.UTF8.GetBytes(message));
        }

        [Then(@"The consumer receive a message ""([^""]*)""")]
        public async Task ThenTheMessageReceivedMustBe(string expectedMessage)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                try
                {
                    _consumer = IntegrationTestContext.Client.NewConsumer()
                        .Topic("public/default/my-topic")
                        .SubscriptionName("test-subscription")
                        .InitialPosition(DotPulsar.SubscriptionInitialPosition.Earliest)
                        .Create();

                    var message = await _consumer.Receive(cts.Token);
                    _messageReceived = Encoding.UTF8.GetString(message.Data);
                    Assert.That(_messageReceived, Is.EqualTo(expectedMessage));
                }
                catch (OperationCanceledException)
                {

                    Assert.Fail("no message");
                }
            }

        }

        [AfterScenario]
        public async Task Cleanup()
        {
            if(_producer != null)
                await _producer.DisposeAsync();
            if(_consumer != null)
                await _consumer.DisposeAsync();

            await IntegrationTestContext.DisposeResourcesAsync();
        }

    }
}
