using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace InsuranceClient.Models
{
    public class Customer : TableEntity
    {
        public Customer(string customerId, string insuranceType)
        {
            RowKey = customerId;
            PartitionKey = insuranceType;
        }

        public string FullName { get; set; }

        public string Email { get; set; }

        public double Amount { get; set; }

        public double Premium { get; set; }

        public DateTime AppDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ImageUrl { get; set; }
    }
}
