using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FakeItEasy.Configuration;
using Ploeh.AutoFixture;
using Xunit;

namespace kolbasik.NCommandBus.Http.Tests
{
    public class HttpCommandInvokerTests
    {
        private readonly Fixture fixture;
        private readonly HttpMessageHandler httpMessageHandler;
        private readonly HttpMessageInvoker httpMessageInvoker;

        public HttpCommandInvokerTests()
        {
            fixture = new Fixture();
            httpMessageHandler = A.Fake<HttpMessageHandler>();
            httpMessageInvoker = new HttpMessageInvoker(
                fixture.Create<Uri>(),
                httpMessageHandler,
                new MediaTypeFormatterCollection());
        }

        [Fact]
        public async Task Invoke_should_serialize_the_command_and_include_metadata()
        {
            // arrange
            var command = fixture.Create<TestCommand>();

            ArgumentCollection actual = null;
            A.CallTo(httpMessageHandler).Where(x => x.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .Invokes(x => actual = x.Arguments)
                .Returns(new HttpResponseMessage(HttpStatusCode.OK));

            // act
            await httpMessageInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.NotNull(actual);
            var httpRequestMessage = Assert.IsType<HttpRequestMessage>(actual[0]);
            var httpRequestContent = Assert.IsType<ObjectContent<TestCommand>>(httpRequestMessage.Content);

            Assert.Equal(command, httpRequestContent.Value);

            Assert.Contains(@"X-RPC-CommandType", httpRequestContent.Headers.Select(x => x.Key));
            Assert.Contains(typeof(TestCommand).Name, httpRequestContent.Headers.GetValues(@"X-RPC-CommandType").Single());
            Assert.Contains(@"X-RPC-ResultType", httpRequestContent.Headers.Select(x => x.Key));
            Assert.Contains(typeof(TestResult).Name, httpRequestContent.Headers.GetValues(@"X-RPC-ResultType").Single());
        }

        [Fact]
        public async Task Invoke_should_return_null_if_the_response_is_empty()
        {
            // arrange
            var command = fixture.Create<TestCommand>();

            // act
            var actual = await httpMessageInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task Invoke_should_deserialize_the_response_if_not_empty()
        {
            // arrange
            var expected = fixture.Create<TestResult>();
            var command = fixture.Create<TestCommand>();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("X-RPC-ResultType", typeof(TestResult).FullName);
            var json = await new ObjectContent<TestResult>(expected, httpMessageInvoker.MediaTypeFormatter).ReadAsStringAsync().ConfigureAwait(false);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            A.CallTo(httpMessageHandler).Where(x => x.Method.Name == "SendAsync")
                .WithReturnType<Task<HttpResponseMessage>>()
                .Returns(response);

            // act
            var actual = await httpMessageInvoker.Invoke<TestResult, TestCommand>(command, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.NotNull(actual);
            Assert.NotSame(expected, actual);
            Assert.Equal(expected, actual);
        }

        [DataContract]
        public sealed class TestCommand
        {
            [DataMember]
            public Guid Unique { get; set; }
        }

        [DataContract]
        public sealed class TestResult
        {
            [DataMember]
            public Guid Unique { get; set; }

            private bool Equals(TestResult other)
            {
                return Unique.Equals(other.Unique);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is TestResult && Equals((TestResult) obj);
            }

            public override int GetHashCode()
            {
                return Unique.GetHashCode();
            }
        }
    }
}
