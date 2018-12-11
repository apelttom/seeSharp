using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StreamProcessingService.Tests
{
    [TestClass]
    public class StreamServiceTests
    {
        private const double EqualityThreshold = 1e-8;

        [TestMethod]
        public void TestMethod1()
        {
            // Arrange

            const double expectedResult = 3.5285714285d;

            var dataStreams = new List<IList<double>>
            {
                new List<double> {12, 2.2, 1.1},
                new List<double> {3.4, 5, 1, 0}
            };

            var service = StreamServiceFactory.CreateService();

            // Act

            var result = service.CalculateAverage(dataStreams);

            // Assert

            Assert.IsTrue(CompareDouble(expectedResult, result));
        }

        [TestMethod]
        public void TestMethod2()
        {
            // Arrange

            const double expectedResult = 3.5285714285d;

            var service = StreamServiceFactory.CreateService();

            var data1 = new MemoryStream(Encoding.UTF8.GetBytes("12;2.2\nppp\n\n\n1.1\n3.4;;"));
            var data2 = new MemoryStream(Encoding.UTF8.GetBytes("5;1\n\n;;0;"));

            var dataStreams = new List<Stream> {data1, data2};

            // Act

            var result = service.CalculateAverageAsync(dataStreams).Result;

            // Assert

            Assert.IsTrue(CompareDouble(expectedResult, result));
        }

        [TestMethod]
        public void TestMethod3()
        {
            // Arrange

            const double expectedResult = 3.5285714285d;

            var service = StreamServiceFactory.CreateService();

            var data = new List<double> {12, 2.2, 1.1, 3.4, 5, 1, 0};

            // Act

            var result = service.CalculateAverage(data, 4, d => d);

            // Assert

            Assert.IsTrue(CompareDouble(expectedResult, result));
        }

        [TestMethod]
        public void TestMethod4()
        {
            // Arrange

            var service = StreamServiceFactory.CreateService();

            var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(" 1 \t \npp\n3\n5\n7 \n9"));
            var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(" 2 \t\t\npp\n4\n6\n8 \n10"));

            // Act

            var result = service.JoinAndSort(stream1, stream2).ToList();

            // Assert

            foreach (var i in Enumerable.Range(1, 10))
            {
                Assert.AreEqual(i, result[i - 1]);
            }
        }

        private bool CompareDouble(double expectedResult, double result)
        {
            var limit = EqualityThreshold * expectedResult;
            return Math.Abs(expectedResult - result) < limit;
        }
    }
}