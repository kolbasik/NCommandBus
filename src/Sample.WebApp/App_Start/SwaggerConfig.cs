using System;
using System.Collections.Generic;
using System.Linq;
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
            // https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md
            configuration.EnableSwagger(
                c =>
                {
                    c.MultipleApiVersions(
                        (apiDesc, targetApiVersion) => true,
                        vc =>
                        {
                            vc.Version("RPC", "Sample.WebApp RPC");
                            vc.Version("v1", "Sample.WebApp");
                        });

                    c.IgnoreObsoleteActions();
                    c.IgnoreObsoleteProperties();
                    c.UseFullTypeNameInSchemaIds();
                    c.DescribeAllEnumsAsStrings();

                    //c.SchemaFilter<ApplySchemaVendorExtensions>();
                    //c.DocumentFilter<ApplyDocumentVendorExtensions>();
                    //c.OperationFilter<AddDefaultResponse>();

                    c.CustomProvider(defaultProvider => new CommandBusSwaggerProvider(defaultProvider));
                }).EnableSwaggerUi(c => c.EnableDiscoveryUrlSelector());
        }

        private sealed class CommandBusSwaggerProvider : ISwaggerProvider
        {
            private readonly ISwaggerProvider defaultSwaggerProvider;
            private readonly SchemaRegistry schemaRegistry;

            public CommandBusSwaggerProvider(ISwaggerProvider defaultSwaggerProvider)
            {
                this.defaultSwaggerProvider = defaultSwaggerProvider;
                this.schemaRegistry = new SchemaRegistry(
                        new JsonSerializerSettings(),
                        new Dictionary<Type, Func<Schema>>(),
                        new List<ISchemaFilter>(),
                        new IModelFilter[0],
                        true,
                        type => type.Name,
                        true,
                        false,
                        true);
            }

            public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
            {
                var swaggerDocument = defaultSwaggerProvider.GetSwagger(rootUrl, apiVersion);
                if ("RPC".Equals(apiVersion, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var commandHandlerType in Global.CommandHandlerTypes)
                    {
                        var commandType = commandHandlerType.GenericTypeArguments[0];
                        var resultType = commandHandlerType.GenericTypeArguments[1];

                        var path = $"/CommandBus.ashx?commandType={HttpUtility.UrlEncode(GetTypeName(commandType))}&resultType={HttpUtility.UrlEncode(GetTypeName(resultType))}";
                        swaggerDocument.paths.Add(
                            path,
                            new PathItem
                            {
                                post =
                                    new Operation
                                    {
                                        tags = new List<string> { commandType.Name },
                                        operationId = commandType.FullName.Replace(".", "_"),
                                        consumes = new List<string> { "application/json" },
                                        produces = new List<string> { "application/json" },
                                        parameters = new List<Parameter> { new Parameter { name = "command", @in = "body", required = true, schema = schemaRegistry.GetOrRegister(commandType) } },
                                        responses = new Dictionary<string, Response>()
                                        {
                                            { "200", new Response { description = "OK", schema = schemaRegistry.GetOrRegister(resultType) } }
                                        }
                                    }
                            });
                    }
                    swaggerDocument.definitions = schemaRegistry.Definitions;
                }
                return swaggerDocument;
            }

            private static string GetTypeName(Type type)
            {
                var assemblyQualifiedName = type.AssemblyQualifiedName;
                var count = assemblyQualifiedName.IndexOf(',', assemblyQualifiedName.IndexOf(',') + 1);
                return assemblyQualifiedName.Substring(0, count);
            }
        }
    }
}