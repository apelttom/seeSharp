using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace StreamProcessingService
{
    class TApeltauerStreamService : IStreamService
    {
        public double CalculateAverage(IEnumerable<IList<double>> dataStreams)
        {
            double dataStreamsSum = 0;
            int dataStreamsItemsCount = 0;
            foreach (var stream in dataStreams)
            {
                int streamItemsCounter = 0;
                for (; ; streamItemsCounter++)
                {
                    try
                    {
                        dataStreamsSum = dataStreamsSum + stream[streamItemsCounter];
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        streamItemsCounter = streamItemsCounter - 1;
                    }

                    finally
                    {
                        dataStreamsItemsCount = dataStreamsItemsCount + streamItemsCounter;
                    }
                }
            }
            return (dataStreamsSum / dataStreamsItemsCount);
        }

        public double CalculateAverage<T>(IList<T> data, int parallelismDegree, Func<T, double> valueExtractor)
        {
            throw new NotImplementedException();
        }

        public Task<double> CalculateAverageAsync(IEnumerable<Stream> dataStreams)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<double> JoinAndSort(Stream stream1, Stream stream2)
        {
            throw new NotImplementedException();
        }
    }
}
