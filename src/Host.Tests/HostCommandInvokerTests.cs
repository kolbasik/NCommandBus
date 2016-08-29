﻿using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using kolbasik.NCommandBus.Abstractions;
using Ploeh.AutoFixture;
using Xunit;

namespace kolbasik.NCommandBus.Host.Tests
{
    public sealed class HostCommandInvokerTests
    {
        private readonly Fixture fixture;
        private readonly ServiceContainer serviceContainer;
        private readonly InProcessMessageInvoker hostMessageInvoker;

        public HostCommandInvokerTests()
        {
            fixture = new Fixture();
            serviceContainer = new ServiceContainer();
            hostMessageInvoker = new InProcessMessageInvoker(serviceContainer);
        }

        [Fact]
        public async Task Send_should_call_the_handle_method_of_the_command_handler()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            var expected = fixture.Create<TestResult>();

            var commandHandler = A.Fake<IQueryHandler<TestCommand, TestResult>>();
            A.CallTo(() => commandHandler.Handle(command, CancellationToken.None)).Returns(expected);

            serviceContainer.AddService(typeof(IQueryHandler<TestCommand, TestResult>), commandHandler);

            // act
            var actual = await hostMessageInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None).ConfigureAwait(false);

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
                        () => hostMessageInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None)).ConfigureAwait(false);

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