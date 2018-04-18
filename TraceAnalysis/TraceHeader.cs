using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceHeader
    {
        public DateTime OccurredAt { get; set; }
        public TimeSpan Duration { get; set; }
        public string PValue { get; set; }
        public string TValue { get; set; }
        public int Level { get; set; }
        public int LineNumber { get; set; }
        public string Type { get; set; }
        public TraceEntryTypes TraceEntryType { get; set; }
        public TraceEntry TraceEntry { get; set; }
        public TraceHeader NextHeader { get; set; }
    }
}
