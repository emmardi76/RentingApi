using System;

namespace Renting.Domain.Exceptions
{
    public sealed class CustomerHasActiveRentalException : Exception
    {
        public string CustomerId { get; }

        public CustomerHasActiveRentalException(string customerId)
            : base($"Customer '{customerId}' already has an active rental.")
        {
            CustomerId = customerId;
        }
    }
}