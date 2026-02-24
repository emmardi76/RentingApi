using System;

namespace Renting.Domain.Exceptions
{
    public sealed class RentalAlreadyReturnedException : Exception
    {
        public Guid RentalId { get; }

        public RentalAlreadyReturnedException(Guid rentalId)
            : base($"Rental with ID '{rentalId}' has already been returned.")
        {
            RentalId = rentalId;
        }
    }
}