using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class ReturnPoint
    {
        public string ReturnFrom { get; set; }
        public string Context { get; set; }
        public TraceEntry Creator { get; set; }

        public ReturnPoint (string returnFrom, string context, TraceEntry creator)
        {
            this.ReturnFrom = returnFrom;
            this.Context = context;
            this.Creator = creator;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.ReturnFrom, this.Context);
        }
    }
}
