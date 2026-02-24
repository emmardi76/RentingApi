using System;

namespace Renting.Domain.Exceptions
{
    public sealed class RentalNotFoundException : Exception
    {
        public Guid RentalId { get; }

        public RentalNotFoundException(Guid rentalId)
            : base($"Rental with ID '{rentalId}' was not found.")
        {
            RentalId = rentalId;
        }
    }
}