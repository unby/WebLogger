using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebLogger
{
    public static class LogManager
    {
    //    public static ILogStore ResolveLogStore { return}
    }
    [Flags]
    public enum StoreType
    {
        OracleDb=1,
        SingeFile=2,
        MultiFile=4
    }

   [Serializable]
    public enum LevelRecord
    {
        Debug,
        Info,
        Important,
        Error
    }

    [Serializable]
    public enum LevelType
    {
        Low,
        High,
        Prior,
        Critical,
        Soap,
        Monitor
    }
}
