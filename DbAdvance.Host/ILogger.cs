namespace DbAdvance.Host
{
    public interface ILogger
    {
        void Log(string message, params object[] parameters);
    }
}