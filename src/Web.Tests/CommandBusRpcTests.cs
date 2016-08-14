using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using FakeItEasy;
using kolbasik.NCommandBus.Abstractions;
using kolbasik.NCommandBus.Core;
using kolbasik.NCommandBus.Host;
using kolbasik.NCommandBus.Web;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Xunit;

namespace Web.Tests
{
    public class CommandBusRpcTests
    {
        private readonly CommandBusRpc commandBusRpc;
        private readonly Fixture fixture;
        private readonly IServiceProvider serviceProvider;

        public CommandBusRpcTests()
        {
            fixture = new Fixture();
            serviceProvider = A.Fake<IServiceProvider>();
            commandBusRpc = new CommandBusRpc(new CommandBus(new HostCommandInvoker(serviceProvider)));
        }

        [Fact]
        public async Task ProcessRequest_should_response_the_BadRequest_if_metadata_is_not_found()
        {
            // arrange
            var httpContext = A.Fake<HttpContextBase>();

            // act
            await commandBusRpc.ProcessRequest(httpContext);

            // assert
            Assert.Equal((int) HttpStatusCode.BadRequest, httpContext.Response.StatusCode);
        }

        [Fact]
        public async Task Test()
        {
            // arrange
            var command = fixture.Create<TestCommand>();
            var result = fixture.Create<TestResult>();

            var httpContext = A.Fake<HttpContextBase>();
            A.CallTo(() => httpContext.Request).Returns(A.Fake<HttpRequestBase>());
            A.CallTo(() => httpContext.Request.Params).Returns(new NameValueCollection());
            A.CallTo(() => httpContext.Request.InputStream).Returns(new MemoryStream());
            A.CallTo(() => httpContext.Response).Returns(A.Fake<HttpResponseBase>());
            httpContext.Request.Params["X-RPC-CommandType"] = typeof (TestCommand).AssemblyQualifiedName;
            httpContext.Request.Params["X-RPC-ResultType"] = typeof (TestResult).AssemblyQualifiedName;

            var writer = new StreamWriter(httpContext.Request.InputStream);
            writer.Write(JsonConvert.SerializeObject(command));
            writer.Flush();
            httpContext.Request.InputStream.Position = 0;

            var commandHandler = A.Fake<ICommandHandler<TestCommand, TestResult>>();
            A.CallTo(() => commandHandler.Handle(A<CommandContext<TestCommand>>._)).Returns(result);
            A.CallTo(() => serviceProvider.GetService(typeof (ICommandHandler<TestCommand, TestResult>)))
                .Returns(commandHandler);

            // act
            await commandBusRpc.ProcessRequest(httpContext).ConfigureAwait(false);

            // assert
            Assert.Equal((int) HttpStatusCode.OK, httpContext.Response.StatusCode);
            Assert.Equal("application/json", httpContext.Response.ContentType);
            A.CallTo(httpContext.Response)
                .Where(x => x.Method.Name == "Write" && x.GetArgument<string>(0).Contains(result.Unique.ToString()))
                .MustHaveHappened();
        }

        public class TestCommand
        {
            public Guid Unique { get; set; }
        }

        public class TestResult
        {
            public Guid Unique { get; set; }
        }
    }
}