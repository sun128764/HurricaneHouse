using NUnit.Framework;
using System.Collections.Generic;
namespace NUnitTestProject1
{
    using DataCollectDaemon;
    public class Tests
    {
        
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void Test1()
        {
            var readList = Tools.ReadSensorList("ssss");
            Assert.Pass();
        }
    }
}