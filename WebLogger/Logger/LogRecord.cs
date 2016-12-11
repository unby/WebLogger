using System;

namespace WebLogger
{
    [Serializable]
    public class LogRecord
    {
        public LogRecord(LevelRecord levelRecord, string textRecord, string attributeMessage, string screenPath, LogSession logSession)
        {
            LevelRecord = levelRecord;
            TextRecord = textRecord;
            AttributeMessage = attributeMessage;
            ScreenPath = screenPath;
            LogSession = logSession;
            Time=DateTime.Now;
        }

        // private LogRecord(){Time=DateTime.Now;}
        public LevelRecord LevelRecord { get; set; }
        public DateTime Time { get; private set; }
        public String TextRecord { get; set; }
        public String AttributeMessage { get; set; }
        public String ScreenPath { get; set; }
        public LogSession LogSession { get; private set; }
    }
}