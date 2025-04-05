namespace CSPLib
{
    public interface IMessageReporter
    {
        void Warning(string text);
        void Error(string text);
        void Info(string text);
    }
}
