using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebLogger
{

    [Serializable]
    public class LogSession : IDisposable
    {
        private readonly string _title;
        private readonly LevelType _levelType;

        public LogSession()
        {
            _title = "Anonimus";
            _levelType = LevelType.Low;
        }

        public LogSession(string title, LevelType levelType)
        {
            _title = title;
            _levelType = levelType;
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        ~LogSession()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
    }
}
