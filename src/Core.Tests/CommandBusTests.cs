﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using kolbasik.NCommandBus.Abstractions;
using Ploeh.AutoFixture;
using Xunit;

namespace kolbasik.NCommandBus.Core.Tests
{
    public class CommandBusTests
    {
        private readonly Fixture fixture;
        private readonly ICommandInvoker commandInvoker;
        private readonly CommandBus commandBus;

        public CommandBusTests()
        {
            fixture = new Fixture();
            commandInvoker = A.Fake<ICommandInvoker>();
            commandBus = new CommandBus(commandInvoker);
        }

        [Fact]
        public async Task Send_should_validate_the_command()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            command.Unique = null;

            // act
            var actual =
                await
                    Assert.ThrowsAsync<ValidationException>(
                        () => commandBus.Send<TestResult, TestCommand>(command)).ConfigureAwait(false);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(command, actual.Command);
            Assert.Equal(1, actual.ValidationResults.Count);
            Assert.Contains(nameof(command.Unique), actual.ValidationResults[0].ErrorMessage);
        }

        [Fact]
        public async Task Send_should_call_the_invoke_method_to_get_the_result()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            var expected = fixture.Create<TestResult>();

            A.CallTo(() => commandInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None)).Returns(expected);

            // act
            var actual = await commandBus.Send<TestResult, TestCommand>(command).ConfigureAwait(false);

            // assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task PipelineTest()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            var commandResult = fixture.Create<TestResult>();

            var commandValidator = A.Fake<ICommandValidator>();
            commandBus.CommandValidators.Add(commandValidator);

            var commandObserver = A.Fake<ICommandObserver>();
            commandBus.CommandObservers.Add(commandObserver);

            var actual = new List<string>();
            A.CallTo(() => commandValidator.Validate(A<CommandContext<TestCommand, TestResult>>.Ignored))
                .Invokes(x => actual.Add("Validate")).Returns(Task.FromResult(1));
            A.CallTo(() => commandObserver.PreInvoke(A<CommandContext<TestCommand, TestResult>>.Ignored))
                .Invokes(x => actual.Add("PreInvoke")).Returns(Task.FromResult(1));
            A.CallTo(() => commandInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None))
                .Invokes(x => actual.Add("Invoke")).Returns(commandResult);
            A.CallTo(() => commandObserver.PostInvoke(A<CommandContext<TestCommand, TestResult>>.Ignored))
                .Invokes(x => actual.Add("PostInvoke")).Returns(Task.FromResult(1));

            var expected = new[] {"Validate", "PreInvoke", "Invoke", "PostInvoke"};

            // act
            var result = await commandBus.Send<TestResult, TestCommand>(command).ConfigureAwait(false);

            // assert
            Assert.Equal(commandResult, result);
            Assert.Equal(expected, actual);
        }

        private sealed class TestCommand
        {
            [Required]
            public Guid? Unique { get; set; }
        }

        private sealed class TestResult
        {
        }
    }
}