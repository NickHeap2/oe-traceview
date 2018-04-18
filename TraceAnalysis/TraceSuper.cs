using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceSuper : TraceEntry
    {
        public string FullClassName { get; set; }
        public string ClassName { get; set; }

        public TraceSuper(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            string[] parts = content.Split(' ');
            this.FullClassName = parts[1];
            this.ClassName = this.FullClassName.Split('.').Last();
        }
    }
}
