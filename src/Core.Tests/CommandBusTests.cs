﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FakeItEasy;
using kolbasik.NCommandBus.Core;
using Ploeh.AutoFixture;
using Xunit;

namespace Core.Tests
{
    public class CommandBusTests
    {
        private readonly CommandBus commandBus;
        private readonly Fixture fixture;

        public CommandBusTests()
        {
            fixture = new Fixture();
            commandBus = A.Fake<CommandBus>();
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
                    Assert.ThrowsAsync<CommandBusValidationException>(
                        () => commandBus.Send<TestResult, TestCommand>(command)).ConfigureAwait(false);

            // assert
            Assert.NotNull(actual);
            Assert.Equal(command, actual.Command);
            Assert.Equal(1, actual.ValidationResults.Count);
            Assert.Contains(nameof(command.Unique), actual.ValidationResults[0].ErrorMessage);
        }

        [Fact]
        public async Task Send_should_call_the_execute_method_to_get_the_result()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            var expected = fixture.Create<TestResult>();

            A.CallTo(commandBus).Where(x => x.Method.Name == "Execute")
                .WithReturnType<Task<TestResult>>()
                .Returns(expected);

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

            A.CallTo(commandBus).Where(x => x.Method.Name == "Execute")
                .WithReturnType<Task<TestResult>>()
                .Returns(commandResult);

            var actual = new List<int>();
            A.CallTo(() => commandObserver.PreExecute(A<CommandContext>.Ignored))
                .Invokes(x => actual.Add(1)).Returns(Task.FromResult(1));
            A.CallTo(() => commandValidator.Validate(A<CommandContext>.Ignored))
                .Invokes(x => actual.Add(2)).Returns(Task.FromResult(1));
            A.CallTo(() => commandObserver.PostExecute(A<CommandContext>.Ignored))
                .Invokes(x => actual.Add(3)).Returns(Task.FromResult(1));

            var expected = new[] {1, 2, 3};

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