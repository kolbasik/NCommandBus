﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;

namespace Sample.WebApp
{
    public sealed class ServiceLocator
    {
        public static readonly ServiceLocator Instance = new ServiceLocator();

        public ServiceLocator()
        {
            ServiceContainer = new ServiceContainer();
            Definitions = new HashSet<Type>();
        }

        public ServiceContainer ServiceContainer { get; }
        public HashSet<Type> Definitions { get; }

        public ServiceLocator RegisterAll(Type contractType, params Assembly[] assemblies)
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

        public ServiceLocator Register(Type contractType, Type serviceType)
        {
            Definitions.Add(contractType);
            ServiceContainer.AddService(contractType, (container, requiredType) => Activator.CreateInstance(serviceType));
            return this;
        }

        public ServiceLocator Register(Type contractType, object instance)
        {
            Definitions.Add(contractType);
            ServiceContainer.AddService(contractType, instance);
            return this;
        }

        public TService Resolve<TService>() => (TService) ServiceContainer.GetService(typeof(TService));
    }
}