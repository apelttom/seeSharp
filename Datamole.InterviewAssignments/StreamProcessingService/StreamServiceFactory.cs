using System;

namespace StreamProcessingService
{
    public static class StreamServiceFactory
    {
        public static IStreamService CreateService()
        {
            // Todo:

            dynamic tapeltauerStreamServiceObj = new TApeltauerStreamService();
            return tapeltauerStreamServiceObj;

            //throw new NotImplementedException();
        }
    }
}
