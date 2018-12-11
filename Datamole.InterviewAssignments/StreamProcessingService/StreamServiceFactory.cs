using System;

namespace StreamProcessingService
{
    public static class StreamServiceFactory
    {
        public static IStreamService CreateService()
        {
            // Todo:

            return new TApeltauerStreamService();

            throw new NotImplementedException();
        }
    }
}
