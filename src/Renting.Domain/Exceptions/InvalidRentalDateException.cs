using System;

namespace Renting.Domain.Exceptions
{
    public sealed class InvalidRentalDateException : Exception
    {
        public InvalidRentalDateException(string message)
            : base(message)
        {
        }
    }
}