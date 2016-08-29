using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;

namespace Sample.Core
{
    public sealed class SampleDependencyResolver
    {
        public static readonly SampleDependencyResolver Instance = new SampleDependencyResolver();

        public SampleDependencyResolver()
        {
            ServiceContainer = new ServiceContainer();
            Definitions = new HashSet<Type>();
        }

        public ServiceContainer ServiceContainer { get; }
        public HashSet<Type> Definitions { get; }

        public SampleDependencyResolver RegisterTypes(Type contractType, params Assembly[] assemblies)
        {
            var serviceTypes = assemblies.SelectMany(x => x.GetTypes());
            if (contractType.IsGenericTypeDefinition)
            {
                foreach (var serviceType in serviceTypes)
                {
                    foreach (var serviceContract in serviceType.GetInterfaces())
                    {
                        if (serviceContract.GetGenericTypeDefinition() == contractType)
                        {
                            Register(serviceContract, serviceType);
                        }
                    }
                }
            }
            else
            {
                foreach (var serviceType in serviceTypes.Where(contractType.IsAssignableFrom))
                {
                    Register(contractType, serviceType);
                }
            }
            return this;
        }

        public SampleDependencyResolver Register(object instance)
        {
            var type = instance.GetType();
            Register(type, instance);
            foreach (var contractType in type.GetInterfaces())
            {
                Register(contractType, instance);
            }
            return this;
        }

        public SampleDependencyResolver Register(Type serviceType)
        {
            Register(serviceType, serviceType);
            foreach (var contractType in serviceType.GetInterfaces())
            {
                Register(contractType, serviceType);
            }
            return this;
        }

        public SampleDependencyResolver Register(Type contractType, object instance)
        {
            return Register(contractType, requiredType => instance);
        }

        public SampleDependencyResolver Register(Type contractType, Type serviceType)
        {
            return Register(contractType, requiredType => Activator.CreateInstance(serviceType));
        }

        public SampleDependencyResolver Register(Type contractType, Func<Type, object> factory)
        {
            if (!Definitions.Contains(contractType))
            {
                Definitions.Add(contractType);
                ServiceContainer.AddService(contractType, (container, requiredType) => factory(requiredType));
            }
            return this;
        }

        public TService Resolve<TService>() => (TService) ServiceContainer.GetService(typeof(TService));
    }
}