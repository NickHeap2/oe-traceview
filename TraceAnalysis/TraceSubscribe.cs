using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceSubscribe : TraceEntry
    {
        public string EventName { get; set; }
        public string TargetName { get; set; }
        public TraceSubscribe(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            string[] parts = content.Split(' ');
            this.EventName = parts[1];
            this.TargetName = parts[2];
        }
    }
}
