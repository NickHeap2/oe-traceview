using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceFuncPropSet : TraceFunc
    {
        public string PropertyName { get; set; }

        public TraceFuncPropSet(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {

            this.PropertyName = this.Function.Replace("propSet_", "");
            
        }

    }
}
