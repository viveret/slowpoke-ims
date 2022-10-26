namespace slowpoke.core.Services.Node
{
    public class IteratorResults
    {
        public int noExceptionCount { get; set; }
        public int exceptionCount { get; set; }
        public int timeoutCount { get; set; }
        public int softFailCount { get; set; }
        public int successCount { get; set; }

        public IteratorResults()
        {
            
        }
    }
}