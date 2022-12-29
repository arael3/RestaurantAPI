using System.Runtime.Serialization;

namespace RestaurantAPI.Exceptions
{
    [Serializable]
    internal class ForbidException : Exception
    {
        public ForbidException()
        {
        }

        public ForbidException(string? message) : base(message)
        {

        }

        public ForbidException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ForbidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}