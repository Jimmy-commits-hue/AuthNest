namespace AuthApiBackend.Exceptions.ExceptionTypes
{
    public class DailyMaximumAttemptsReachedException : IOException
    {
        public DailyMaximumAttemptsReachedException(string message) : base(message) { }
    }
}
