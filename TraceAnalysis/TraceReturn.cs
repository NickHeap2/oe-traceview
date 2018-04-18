using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceReturn : TraceEntry
    {
        public string ReturnValue { get; set; }
        public bool IsUnlinkedReturn { get; set; }
        public bool IsMainBlockReturn { get; set; }
        public bool IsConstructorReturn { get; set; }
        public bool IsStaticReturn { get; set; }
        public bool IsPropertyReturn { get; set; }
        public string PropertyName { get; set; }

        public string ReturnFrom { get; set; }

        public TraceReturn(DateTime occurredAt, string content)
            : base(occurredAt, content)
        {
            this.IsUnlinkedReturn = false;
            this.IsMainBlockReturn = false;
            this.IsConstructorReturn = false;
            this.ReturnFrom = "";

            string[] parts = content.Split(' ');
            if (parts.Length > 2)
            {
                if (parts[2] == "CREATE-TRIGGER"
                    || parts[2] == "WRITE-TRIGGER"
                    || parts[2] == "DELETE-TRIGGER")
                {
                    this.IsUnlinkedReturn = true;
                }
                else if (parts.Length > 3
                         && parts[2] == "Main"
                         && parts[3] == "Block")
                {
                    this.ReturnFrom = "Main Block";
                    this.IsMainBlockReturn = true;
                }
                else
                {
                    if (parts[2] == "PUBLISH")
                    {
                        this.ReturnFrom = parts[3];
                    }
                    else
                    {
                        this.ReturnFrom = parts[2];
                    }
                }
            }

            int istart = content.IndexOf('"');
            if (istart > 0)
            {
                istart++;
                int iend = content.LastIndexOf('"');
                if (iend < istart)
                {
                    iend = content.Length;
                }
                this.ReturnValue = content.Substring(istart, iend - istart);
            }

            istart = content.IndexOf('[');
            if (istart > 0)
            {
                istart++;
                int iend = content.LastIndexOf(']');
                if (iend < istart)
                {
                    iend = content.Length;
                }
                this.InInternalProcedure = content.Substring(istart, iend - istart);
            }

            // do we have a class?
            if (this.InInternalProcedure != null
                && this.InInternalProcedure.Contains('.'))
            {
                string[] classParts = this.InInternalProcedure.Split('.');
                // is this a constructor ?
                if (classParts[classParts.Length - 1] == this.ReturnFrom)
                {
                    this.IsConstructorReturn = true;
                }
            }

            //handle db triggers that don't have a run
            if (this.ReturnFrom == "Main Block"
                && this.InInternalProcedure != null
                && this.InInternalProcedure.StartsWith(@"triggers/"))
            {
                this.IsUnlinkedReturn = true;
            }
            //cope with SYSTEM-TRIGGER that doesn't have a run
            else if (this.ReturnFrom == "SYSTEM-TRIGGER")
            {
                this.IsUnlinkedReturn = true;
            }
            else if (this.ReturnFrom.StartsWith("propGet_")
                     || this.ReturnFrom.StartsWith("propSet_"))
            {
                this.IsPropertyReturn = true;
                this.PropertyName = this.ReturnFrom.Replace("propGet_", "").Replace("propSet_", "");
            }
        }
    }
}
