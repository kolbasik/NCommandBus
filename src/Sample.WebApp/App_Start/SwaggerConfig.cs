using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Sample.WebApp;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SwaggerConfig), "RegisterGlobalConfiguration")]

namespace Sample.WebApp
{
    public class SwaggerConfig
    {
        public static void RegisterGlobalConfiguration()
        {
            Register(GlobalConfiguration.Configuration);
        }

        public static void Register(HttpConfiguration configuration)
        {
            configuration.EnableSwagger(
                c =>
                {
                    c.MultipleApiVersions(
                        (apiDesc, targetApiVersion) => true,
                        vc =>
                        {
                            vc.Version("CommandBus", "Sample.WebApp CommandBus");
                            vc.Version("v1", "Sample.WebApp");
                        });

                    c.IgnoreObsoleteActions();
                    c.IgnoreObsoleteProperties();
                    c.UseFullTypeNameInSchemaIds();
                    c.DescribeAllEnumsAsStrings();

                    c.CustomProvider(defaultProvider => new CommandBusSwaggerProvider(defaultProvider, "CommandBus", CommandBusSwaggerProvider.CreateDefaultSchemaRegistry()));
                }).EnableSwaggerUi(c => c.EnableDiscoveryUrlSelector());
        }
    }

    public sealed class CommandBusSwaggerProvider : ISwaggerProvider
    {
        private readonly ISwaggerProvider defaultSwaggerProvider;

        public CommandBusSwaggerProvider(ISwaggerProvider defaultSwaggerProvider, string apiVersion, SchemaRegistry schemaRegistry)
        {
            this.defaultSwaggerProvider = defaultSwaggerProvider;
            this.ApiVersion = apiVersion;
            this.SchemaRegistry = schemaRegistry;
            this.PathResolver = CreateDefaultPathResolver(apiVersion);
        }

        public string ApiVersion { get; }
        public SchemaRegistry SchemaRegistry { get; }
        public Func<Type, Type, string> PathResolver { get; set; }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            // NOTE: https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md
            var swaggerDocument = defaultSwaggerProvider.GetSwagger(rootUrl, apiVersion);
            if (string.Equals(ApiVersion, apiVersion, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var commandHandlerType in Global.CommandHandlerTypes)
                {
                    var commandType = commandHandlerType.GenericTypeArguments[0];
                    var resultType = commandHandlerType.GenericTypeArguments[1];

                    swaggerDocument.paths.Add(
                        PathResolver.Invoke(commandType, resultType),
                        new PathItem
                        {
                            post =
                                new Operation
                                {
                                    tags = new List<string> { commandType.Name },
                                    operationId = commandType.Name,
                                    consumes = new List<string> { "application/json" },
                                    produces = new List<string> { "application/json" },
                                    parameters = new List<Parameter> { new Parameter { name = "command", @in = "body", required = true, schema = SchemaRegistry.GetOrRegister(commandType) } },
                                    responses = new Dictionary<string, Response>()
                                    {
                                        { "200", new Response { description = "OK", schema = SchemaRegistry.GetOrRegister(resultType) } }
                                    }
                                }
                        });
                }
                swaggerDocument.definitions = SchemaRegistry.Definitions;
            }
            return swaggerDocument;
        }

        public static SchemaRegistry CreateDefaultSchemaRegistry()
        {
            return new SchemaRegistry(
                new JsonSerializerSettings(),
                new Dictionary<Type, Func<Schema>>(),
                new ISchemaFilter[0],
                new IModelFilter[0],
                true,
                type => string.Concat(type.DeclaringType?.Name, type.Name),
                true,
                false,
                true);
        }

        public static Func<Type, Type, string> CreateDefaultPathResolver(string apiVersion)
        {
            return (commandType, resultType) => $"/{apiVersion}.ashx?commandType={HttpUtility.UrlEncode(GetTypeName(commandType))}&resultType={HttpUtility.UrlEncode(GetTypeName(resultType))}";
        }

        public static string GetTypeName(Type type)
        {
            var assemblyQualifiedName = type.AssemblyQualifiedName;
            var count = assemblyQualifiedName.IndexOf(',', assemblyQualifiedName.IndexOf(',') + 1);
            return assemblyQualifiedName.Substring(0, count);
        }
    }
}