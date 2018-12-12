using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
            List<double> extractedNumbers = new List<double>();
            Thread[] threadPool = new Thread[parallelismDegree];
            int threadPoolPointer = 0;
            foreach (var dataObject in data)
            {
                if (threadPool[threadPoolPointer] == null)
                {
                    threadPool[threadPoolPointer] = new Thread(lambda => extractNumberFromUserObject(dataObject, extractedNumbers, valueExtractor));
                    threadPool[threadPoolPointer].Start();
                    threadPoolPointer = (threadPoolPointer + 1) % parallelismDegree;
                } else
                {
                    while (threadPool[threadPoolPointer].IsAlive)
                    {
                        threadPoolPointer = (threadPoolPointer + 1) % parallelismDegree;
                    }
                    // we found a thread that has finished already, we can start a new one
                    threadPool[threadPoolPointer] = new Thread(lambda => extractNumberFromUserObject(dataObject, extractedNumbers, valueExtractor));
                    threadPool[threadPoolPointer].Start();
                    threadPoolPointer = (threadPoolPointer + 1) % parallelismDegree;
                }
            }
            // wait for the threads to finish
            for (threadPoolPointer = 0; threadPoolPointer < parallelismDegree; threadPoolPointer++)
            {
                threadPool[threadPoolPointer].Join();
            }
            return extractedNumbers.Average();
        }

        static void extractNumberFromUserObject<T>(T dataArgument, List<double> extractedNumCollection, Func<T, double> extractor)
        {
            extractedNumCollection.Add(extractor(dataArgument));
        }

        public async Task<double> CalculateAverageAsync(IEnumerable<Stream> dataStreams)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            // Maximum number of bytes we can fit into RAM and still be OK with it
            int RAMupperBoundBytes = 8;
            byte[] buffer = new byte[RAMupperBoundBytes];
            // Can calculate average by itself
            List<double> extractedNumbers = new List<double>();
            foreach (var stream in dataStreams)
            {
                // Since we will be throwing away some of the bytes, we will need our own position pointer
                int relevantStreamPosition = 0;
                // Do not end until there are data in the data stream
                while (await stream.ReadAsync(buffer, 0, RAMupperBoundBytes) != 0)
                {
                    /**
                     * We do not have much to grap on. Only given rule is that there are some semicolons
                     * and that there are line feeds (for simplicity just one type of line ending).
                     * 
                     * We will have to read maximum amount of bytes we can and then throw away a few bytes from the ending
                     * Why? because if we are not checking the stream data byte by byte, there can be a situation when
                     * we have read only a few digits from a number or only a few bytes when actually we are missing the rest.
                     * 
                     * To be 100% sure that we will not miss or missinterpret any number, we have to process data from a fixed point
                     * to a fixed point and there are no other artefacts in the data stream that would not change than semicolon and line feed
                     */

                    // decode byte array into string and throw away uncomplete characters (not all bytes have been read) on the end
                    string decodedString = utf8.GetString(buffer);
                    // now we have to determine which character will be our fixed point. Generally line feed is better, because we
                    // suppose that there will be less line feeds than semicolons
                    if(decodedString.IndexOf("\n") != -1)
                    {
                        // we want to include the fixed point character in order to read stream after it next time
                        decodedString = decodedString.Substring(0, decodedString.LastIndexOf("\n") + 1);
                    } else
                    {
                        if (decodedString.IndexOf(";") != -1)
                        {
                            // we want to include the fixed point character in order to read stream after it next time
                            decodedString = decodedString.Substring(0, decodedString.LastIndexOf(";") + 1 );
                        }
                    }
                    // now we have to calculate how nuch bytes did we use from the reading, so we can continue from relevant point
                    relevantStreamPosition = relevantStreamPosition + utf8.GetByteCount(decodedString);
                    stream.Position = relevantStreamPosition;
                    // This part handles spliting and extracting the numbers
                    string[] lines = decodedString.Split(new[] { "\n" }, StringSplitOptions.None);
                    foreach (string line in lines)
                    {
                        string trim = Regex.Replace(line, @"s", "");
                        string[] encodedValues = trim.Split(new[] { ";" }, StringSplitOptions.None);
                        foreach (string encodedValue in encodedValues)
                        {
                            try
                            {
                                // if character is not a number, List will take care of it by raising an exception
                                // since we are not bound by computation performance, we can afford it
                                extractedNumbers.Add(double.Parse(encodedValue, System.Globalization.CultureInfo.InvariantCulture));
                            }
                            catch (FormatException e)
                            {
                                //Do nothing. It is not a number, so do not add into collection of numbers.
                            }
                        }
                    }
                    // clear the buffer
                    buffer = new byte[RAMupperBoundBytes];
                }
            }
            // automatically calculate average for us
            return extractedNumbers.Average();
        }

        public IEnumerable<double> JoinAndSort(Stream stream1, Stream stream2)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            // Maximum number of bytes we can fit into RAM and still be OK with it
            // we expect that we can fit at least one line from each stream into RAM
            int RAMupperBoundBytes = 16;
            byte[] buffer1 = new byte[RAMupperBoundBytes/2];
            byte[] buffer2 = new byte[RAMupperBoundBytes/2];
            List<double> extractedNumbers = new List<double>();
            // Since we will be throwing away some of the bytes, we will need our own position pointer
            int relevantStreamPosition1 = 0;
            int relevantStreamPosition2 = 0;
            // Do not end until there are data in the data streams
            while ((stream1.Read(buffer1, 0, RAMupperBoundBytes/2) != 0) && (stream2.Read(buffer2, 0, RAMupperBoundBytes/2) != 0))
            {
                string decodedStringStream1 = utf8.GetString(buffer1);
                string decodedStringStream2 = utf8.GetString(buffer2);

                // --------------- FIRST STREAM PROCESSING -------------------------
                if (decodedStringStream1.IndexOf("\n") != -1)
                {
                    decodedStringStream1 = decodedStringStream1.Substring(0, decodedStringStream1.IndexOf("\n") + 1);
                }
                relevantStreamPosition1 = relevantStreamPosition1 + utf8.GetByteCount(decodedStringStream1);
                stream1.Position = relevantStreamPosition1;
                string trimStream1 = Regex.Replace(decodedStringStream1, @"s", "");
                trimStream1 = Regex.Replace(trimStream1, "\n", "");
                try
                {
                    // if character is not a number, List will take care of it by raising an exception
                    // since we are not bound by computation performance, we can afford it
                    extractedNumbers.Add(double.Parse(trimStream1, System.Globalization.CultureInfo.InvariantCulture));
                }
                catch (FormatException e)
                {
                    //Do nothing. It is not a number, so do not add into collection of numbers.
                }

                // --------------- SECOND STREAM PROCESSING -------------------------
                if (decodedStringStream2.IndexOf("\n") != -1)
                {
                    decodedStringStream2 = decodedStringStream2.Substring(0, decodedStringStream2.IndexOf("\n") + 1);
                }
                relevantStreamPosition2 = relevantStreamPosition2 + utf8.GetByteCount(decodedStringStream2);
                stream2.Position = relevantStreamPosition2;
                string trimStream2 = Regex.Replace(decodedStringStream2, @"s", "");
                trimStream2 = Regex.Replace(trimStream2, "\n", "");
                try
                {
                    // if character is not a number, List will take care of it by raising an exception
                    // since we are not bound by computation performance, we can afford it
                    extractedNumbers.Add(double.Parse(trimStream2, System.Globalization.CultureInfo.InvariantCulture));
                }
                catch (FormatException e)
                {
                    //Do nothing. It is not a number, so do not add into collection of numbers.
                }
                // clear the buffers
                buffer1 = new byte[RAMupperBoundBytes / 2];
                buffer2 = new byte[RAMupperBoundBytes / 2];
            }
            extractedNumbers.Sort();
            return extractedNumbers;
        }
    }
}
