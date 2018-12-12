using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                for (int streamItemsCounter = 0;  ; streamItemsCounter++)
                {
                    try
                    {
                        dataStreamsSum = dataStreamsSum + stream[streamItemsCounter];
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        dataStreamsItemsCount = dataStreamsItemsCount + streamItemsCounter;
                        break;
                    }
                }
            }
            double dataStreamsAvg = dataStreamsSum / dataStreamsItemsCount;

            return dataStreamsAvg;
        }

        public double CalculateAverage<T>(IList<T> data, int parallelismDegree, Func<T, double> valueExtractor)
        {
            throw new NotImplementedException();
        }

        public async Task<double> CalculateAverageAsync(IEnumerable<Stream> dataStreams)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            // Suppose we can fit at least few lines into the RAM
            int RAMupperBoundBytes = 8;
            byte[] buffer = new byte[RAMupperBoundBytes];
            List<double> extractedNumbers = new List<double>();
            foreach (var stream in dataStreams)
            {
                int relevantStreamPosition = 0;
                while (await stream.ReadAsync(buffer, 0, RAMupperBoundBytes) != 0)
                {
                    string decodedString = utf8.GetString(buffer);
                    if(decodedString.IndexOf("\n") != -1)
                    {
                        decodedString = decodedString.Substring(0, decodedString.LastIndexOf("\n") + 1);
                    } else
                    {
                        if (decodedString.IndexOf(";") != -1)
                        {
                            decodedString = decodedString.Substring(0, decodedString.LastIndexOf(";") + 1 );
                        }
                    }
                    relevantStreamPosition = relevantStreamPosition + utf8.GetByteCount(decodedString);
                    stream.Position = relevantStreamPosition;
                    string[] lines = decodedString.Split(new[] { "\n" }, StringSplitOptions.None);
                    foreach (string line in lines)
                    {
                        string trim = Regex.Replace(line, @"s", "");
                        string[] encodedValues = trim.Split(new[] { ";" }, StringSplitOptions.None);
                        foreach (string encodedValue in encodedValues)
                        {
                            try
                            {
                                extractedNumbers.Add(double.Parse(encodedValue, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            catch (FormatException e)
                            {
                                //Do nothing. It is not a number, so do not add into collection of numbers.
                            }
                        }
                    }
                    buffer = new byte[RAMupperBoundBytes];
                }
            }
            //Console.WriteLine(string.Join(";", extractedNumbers));
            return extractedNumbers.Average();
        }

        public IEnumerable<double> JoinAndSort(Stream stream1, Stream stream2)
        {
            throw new NotImplementedException();
        }
    }
}
