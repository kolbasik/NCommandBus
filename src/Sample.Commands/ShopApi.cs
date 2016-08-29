using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Sample.Commands
{
    public static class ShopApi
    {
        [DataContract, Serializable]
        public class CreateOrder
        {
            [Required]
            public string CustomerEmail { get; set; }
            [Required]
            public string ProductName { get; set; }
            [Required]
            public uint Quantity { get; set; }
        }

        [DataContract, Serializable]
        public class GetCustomerOrders
        {
            public GetCustomerOrders(string customerEmail)
            {
                CustomerEmail = customerEmail;
            }

            public string CustomerEmail { get; }
        }

        [DataContract, Serializable]
        public class GetCustomerOrdersResult
        {
            public string CustomerEmail { get; set; }
            public CustomerOrder[] Orders { get; set; }
        }

        [DataContract, Serializable]
        public class CustomerOrder
        {
            public CustomerOrder(string productName, uint quantity, DateTimeOffset createdAt)
            {
                ProductName = productName;
                Quantity = quantity;
                CreatedAt = createdAt;
            }

            public string ProductName { get; }
            public uint Quantity { get; }
            public DateTimeOffset CreatedAt { get; }
        }
    }
}