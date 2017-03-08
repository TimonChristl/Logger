using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TC.Logging.Tests
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Verify_HexDump_DoesNotThrowForEmptyArrays()
        {
            var logger = new Logger();
            logger.HexDump(Severity.Info, "", new byte[0]);
        }
    }
}
