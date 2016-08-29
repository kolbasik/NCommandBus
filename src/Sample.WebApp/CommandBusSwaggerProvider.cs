using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using kolbasik.NCommandBus.Abstractions;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Sample.WebApp
{
    public sealed class CommandBusSwaggerProvider : ISwaggerProvider
    {
        private readonly ISwaggerProvider defaultSwaggerProvider;

        public CommandBusSwaggerProvider(ISwaggerProvider defaultSwaggerProvider, string apiVersion, IEnumerable<Type> definitions, SchemaRegistry schemaRegistry = null)
        {
            this.defaultSwaggerProvider = defaultSwaggerProvider;
            this.ApiVersion = apiVersion;
            this.Definitions = definitions;
            this.SchemaRegistry = schemaRegistry ?? CreateDefaultSchemaRegistry();
            this.PathResolver = CreateDefaultPathResolver(apiVersion);
        }

        public string ApiVersion { get; }
        public IEnumerable<Type> Definitions { get; set; }
        public SchemaRegistry SchemaRegistry { get; }
        public Func<Type, Type, string> PathResolver { get; set; }

        public SwaggerDocument GetSwagger(string rootUrl, string apiVersion)
        {
            // NOTE: https://github.com/swagger-api/swagger-spec/blob/master/versions/2.0.md
            var swaggerDocument = defaultSwaggerProvider.GetSwagger(rootUrl, apiVersion);
            if (string.Equals(ApiVersion, apiVersion, StringComparison.OrdinalIgnoreCase))
            {
                var commandHandlerTypes = Definitions.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));
                foreach (var commandHandlerType in commandHandlerTypes)
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