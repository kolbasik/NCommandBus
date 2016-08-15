using System.Web.Http;
using Sample.Core;
using Sample.WebApp;
using Swashbuckle.Application;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "RegisterGlobalConfiguration")]

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

                    c.CustomProvider(defaultProvider => new CommandBusSwaggerProvider(defaultProvider, "CommandBus", SampleDependencyResolver.Instance.Definitions));

                }).EnableSwaggerUi(c => c.EnableDiscoveryUrlSelector());
        }
    }
}