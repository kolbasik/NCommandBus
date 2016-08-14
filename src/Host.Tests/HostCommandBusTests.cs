using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Host;
using Ploeh.AutoFixture;
using Xunit;

namespace Host.Tests
{
    public sealed class HostCommandBusTests
    {
        private readonly Fixture fixture;
        private readonly HostCommandInvoker hostCommandInvoker;
        private readonly IServiceProvider serviceProvider;

        public HostCommandBusTests()
        {
            fixture = new Fixture();
            serviceProvider = A.Fake<IServiceProvider>();
            hostCommandInvoker = new HostCommandInvoker(serviceProvider);
        }

        [Fact]
        public async Task Send_should_call_the_handle_method_of_the_command_handler()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            var expected = fixture.Create<TestResult>();

            var commandHandler = A.Fake<ICommandHandler<TestCommand, TestResult>>();
            A.CallTo(() => commandHandler.Handle(command)).Returns(expected);
            A.CallTo(() => serviceProvider.GetService(typeof (ICommandHandler<TestCommand, TestResult>))).Returns(commandHandler);

            // act
            var actual = await hostCommandInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Send_should_throw_an_exception_if_could_not_find_an_command_handler()
        {
            // arrange
            var command = fixture.Create<TestCommand>();

            // act
            var actual =
                await
                    Assert.ThrowsAsync<InvalidOperationException>(
                        () => hostCommandInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None)).ConfigureAwait(false);

            // assert
            Assert.NotNull(actual);
        }

        public sealed class TestCommand
        {
        }

        public sealed class TestResult
        {
        }
    }
}