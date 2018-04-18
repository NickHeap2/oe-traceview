using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceFunc : TraceEntry
    {
        public string Function { get; set; }
        public string Parameters { get; set; }
        public string ReturnValue { get; set; }
        public bool HasReturn { get; set; }

        public TraceFunc(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            HasReturn = false;

            string[] parts = content.Split(' ');
            this.Function = parts[1];

            int istart = content.IndexOf('"');
            if (istart > 0)
            {
                istart++;
                int iend = content.LastIndexOf('"');
                if (istart >= iend)
                {
                    this.Parameters = content.Substring(istart);
                }
                else
                {
                    this.Parameters = content.Substring(istart, iend - istart);
                }
            }
        }

    }
}
