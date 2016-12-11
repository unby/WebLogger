using System;

namespace WebLogger
{
        public interface ILogStore: IDisposable
    {
        void SaveRecord(LogRecord record);
        void Initialize();
        void Finish();
    }
}