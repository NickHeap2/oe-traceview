using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceQueryStatistics : TraceEntry
    {
        public TraceQueryStatistics(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            
        }
    }
}
