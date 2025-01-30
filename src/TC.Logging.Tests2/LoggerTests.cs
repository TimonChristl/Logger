namespace TC.Logging.Tests2
{
    [TestClass]
    public class LoggerTests
    {
        [TestMethod]
        public void Verify_HexDump_DoesNotThrowForEmptyArrays()
        {
            var logger = new Logger();
            logger.HexDump(Severity.Info, "", []);
        }
    }
}
