using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceAnalysis
{
    public enum TraceEntryTypes
    {
        Configuration = 0,
        _4GLTrace,
        _4GLMESSAGE,
        QryInfo,
        FileId,
        Connection,
        Unknown,
        Application,
        Information,
        StartupProcedure
    }
}
