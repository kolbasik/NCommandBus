using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kolbasik.NCommandBus.Abstractions;
using Sample.Commands;

namespace Sample.Handles
{
    public sealed class Shop : ICommandHandler<ShopApi.CreateOrder>, IQueryHandler<ShopApi.GetCustomerOrders, ShopApi.GetCustomerOrdersResult>
    {
        private readonly ConcurrentDictionary<string, Customer> customers;

        public Shop()
        {
            customers = new ConcurrentDictionary<string, Customer>();
        }

        public Task Handle(ShopApi.CreateOrder command, CancellationToken cancellationToken)
        {
            var customer = customers.GetOrAdd(command.CustomerEmail, email => new Customer(email));
            customer.CreateOrder(command.ProductName, command.Quantity);
            return Task.FromResult(true);
        }

        public Task<ShopApi.GetCustomerOrdersResult> Handle(ShopApi.GetCustomerOrders query, CancellationToken cancellationToken)
        {
            var customer = customers.GetOrAdd(query.CustomerEmail, email => new Customer(email));
            var result = new ShopApi.GetCustomerOrdersResult();
            result.CustomerEmail = customer.Email;
            result.Orders =
                customer.Orders
                    .Select(x => new ShopApi.CustomerOrder(x.ProductName, x.Quantity, x.CreatedAt))
                    .ToArray();
            return Task.FromResult(result);
        }

        private sealed class Customer
        {
            public Customer(string email)
            {
                Email = email;
                Orders = new List<Order>();
            }

            public string Email { get;}
            public List<Order> Orders { get; }

            public void CreateOrder(string productName, uint quantity)
            {
                Orders.Add(new Order(productName, quantity, DateTimeOffset.UtcNow));
            }
        }

        private sealed class Order
        {
            public Order(string productName, uint quantity, DateTimeOffset createdAt)
            {
                ProductName = productName;
                Quantity = quantity;
                CreatedAt = createdAt;
            }

            public string ProductName { get; set; }
            public uint Quantity { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }
    }
}