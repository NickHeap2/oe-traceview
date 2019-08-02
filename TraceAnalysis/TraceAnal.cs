using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis
{
    public class TraceAnal
    {
        public List<TraceHeader> TraceHeaders { get; set; }
        public TraceEntry TraceEntryTree { get; set; }

        //public Stack<string> ReturnPoints { get; set; }
        public Stack<ReturnPoint> ReturnPoints { get; set; }

        public int UnparsedLines { get; set; }
        public int ParsedLines { get; set; }
        public int InputLines { get; set; }

        private List<String> HideFunctions { get; set; }

        private List<String> HideProcedures { get; set; }

        TraceReturn lastReturn;
        int lastReturnLine = 0;
        bool haveStartupProcedure = false;

        public void Analyse(string filename)
        {
            HideFunctions = new List<string>();
            //HideFunctions.Add("getSPFtext");
            //HideFunctions.Add("");
            //HideFunctions.Add("");
            //HideFunctions.Add("");

            HideProcedures = new List<string>();
            //HideProcedures.Add("");
            //HideProcedures.Add("");

            TraceHeaders = new List<TraceHeader>();
            //add tree route
            TraceEntryTree = new TraceEntry(DateTime.MinValue, "Root");
            TraceEntryTree.Description = "Root";
            TraceEntry currentEntry = TraceEntryTree;

            //ReturnPoints = new Stack<string>();
            ReturnPoints = new Stack<ReturnPoint>();

            List<string> lines = new List<string>();
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader sr = new StreamReader(fs))
                {
                    string line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            InputLines = lines.Count;
            //int linenum = -1;
            //var q = from lines in File.ReadLines(filename)
            //        select lines;
            //foreach (string line in q)
            TraceHeader th = null;
            TraceHeader lastTH = null;

            for (int linenum = 0; linenum < InputLines; linenum++)
            {
                //linenum++;
                string line = lines[linenum];

                if (line[0] != '[')
                {
                    continue;
                }

                lastTH = th;
                th = new TraceHeader();
                th.LineNumber = linenum;

                string[] parts = line.Split(' ');
                if (parts.Count() >= 6)
                {
                    string[] dateTimeParts = parts[0].Replace("[", "").Replace("]", "").Split('@');

                    th.OccurredAt = DateTime.Parse(string.Format("{0} {1}", dateTimeParts[0], dateTimeParts[1]));
                    th.Duration = new TimeSpan(0);
                    th.PValue = parts[1];
                    th.TValue = parts[2];
                    th.Level = int.Parse(parts[3]);
                    th.Type = parts[4];

                    if (lastTH != null)
                    {
                        lastTH.NextHeader = th;
                        lastTH.Duration = th.OccurredAt - lastTH.OccurredAt;
                    }

                    TraceEntryTypes tet;
                    if (parts[5] == "4GLTRACE")
                    {
                        tet = TraceEntryTypes._4GLTrace;
                    }
                    else if (parts[5] == "4GLMESSAGE")
                    {
                        tet = TraceEntryTypes._4GLMESSAGE;
                    }
                    else if (parts[5] == "QRYINFO")
                    {
                        tet = TraceEntryTypes.QryInfo;
                    }
                    else if (parts[5] == "FILEID")
                    {
                        tet = TraceEntryTypes.FileId;
                    }
                    else if (parts[5] == "CONN")
                    {
                        tet = TraceEntryTypes.Connection;
                    }
                    else if (parts[5] == "APPL")
                    {
                        tet = TraceEntryTypes.Application;
                    }
                    else if (parts[5] == "--")
                    {
                        tet = TraceEntryTypes.Configuration;
                        string config = line.Substring(53).TrimStart(' ');
                        if (config.StartsWith("-- Logging level set to")
                            || config.StartsWith("WS -- Logging level set to"))
                        {
                            //a new session
                            ReturnPoints.Clear();
                            currentEntry = TraceEntryTree;

                            TraceSession traceSession = new TraceSession(th.OccurredAt, config);
                            //link parent and children
                            traceSession.Parent = currentEntry;
                            traceSession.TraceEntryType = tet;
                            currentEntry.Children.Add(traceSession);
                            currentEntry = traceSession;
                        }
                        else if (currentEntry is TraceSession)
                        {
                            //addition trace line
                            currentEntry.Content += string.Format("\n{0}", config);
                        }
                        else
                        {
                            tet = TraceEntryTypes.Information;
                        }
                    }
                    else
                    {
                        tet = TraceEntryTypes.Unknown;
                    }

                    th.TraceEntryType = tet;

                    if (th.TraceEntryType == TraceEntryTypes.Configuration
                        || th.TraceEntryType == TraceEntryTypes.FileId
                        || th.TraceEntryType == TraceEntryTypes.Connection
                        || th.TraceEntryType == TraceEntryTypes.Unknown)
                    {
                        ParsedLines++;
                        continue;
                    }

                    string content;
                    //string content = line.Substring(68);
                    if (tet is TraceEntryTypes.Information)
                    {
                        content = line.Substring(59).TrimStart(' ');
                    }
                    else
                    {
                        content = line.Substring(64).TrimStart(' ');
                    }
                    if (linenum >= 559)
                    {
                        ;
                    }

                    //end messages
                    if (currentEntry is TraceMessage
                        && th.TraceEntryType != TraceEntryTypes._4GLMESSAGE)
                    {
                        currentEntry = currentEntry.Parent;
                    }
                    if (currentEntry is ApplicationMessage
                        && th.TraceEntryType != TraceEntryTypes.Application)
                    {
                        currentEntry = currentEntry.Parent;
                    }
                    //end qryinfo
                    if ((currentEntry is TraceQueryPlan
                         || currentEntry is TraceQueryStatistics)
                        && th.TraceEntryType != TraceEntryTypes.QryInfo)
                    {
                        currentEntry = currentEntry.Parent;
                    }

                    if (th.TraceEntryType == TraceEntryTypes._4GLTrace)
                    {
                        string[] contentParts = content.Split(' ');


                        if (!haveStartupProcedure)
                        {
                            for(int i = 0; i < contentParts.Length; i++)
                            {
                                if (contentParts[i] == "[Main"
                                    && contentParts[i + 1] == "Block"
                                    && contentParts[i + 2] == "-"
                                    )
                                {
                                    TraceStartupProcedure traceStartupProcedure = new TraceStartupProcedure(th.OccurredAt, contentParts[i + 3]);
                                    traceStartupProcedure.TraceEntryType = TraceEntryTypes.StartupProcedure;

                                    var returnPointProcedure = traceStartupProcedure.InternalProcedure;
                                    if (returnPointProcedure == null)
                                    {
                                        returnPointProcedure = traceStartupProcedure.InInternalProcedure;
                                    }

                                    ReturnPoints.Push(new ReturnPoint(returnPointProcedure, traceStartupProcedure.Procedure, traceStartupProcedure));
                                    traceStartupProcedure.Parent = currentEntry;
                                    currentEntry.Children.Add(traceStartupProcedure);
                                    currentEntry = traceStartupProcedure;

                                    TraceHeader spth = new TraceHeader();
                                    spth.OccurredAt = th.OccurredAt;
                                    spth.Duration = th.Duration;
                                    spth.PValue = th.PValue;
                                    spth.TValue = th.TValue;
                                    spth.Level = th.Level;
                                    spth.Type = th.Type;
                                    spth.TraceEntry = traceStartupProcedure;
                                    spth.TraceEntryType = TraceEntryTypes.StartupProcedure;

                                    TraceHeaders.Add(spth);
                                }
                                haveStartupProcedure = true;
                            }
                        }

                        if (contentParts[0].Equals("Run", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceRun traceRun = new TraceRun(th.OccurredAt, content);

                            //push procedure on the stack for returns
                            if (traceRun.IsSuper)
                            {
                                //super will have same name as last returnpoint
                                ReturnPoint rp = ReturnPoints.Peek();
                                traceRun.InternalProcedure = rp.ReturnFrom;
                                traceRun.Procedure = rp.Context;

                                ReturnPoints.Push(new ReturnPoint(traceRun.InternalProcedure, traceRun.Procedure, traceRun));
                            }
                            else if (traceRun.InternalProcedure != null
                                && traceRun.InternalProcedure != "")
                            {
                                //ReturnPoints.Push(traceRun.InternalProcedure);
                                ReturnPoints.Push(new ReturnPoint(traceRun.InternalProcedure, traceRun.Procedure, traceRun));
                                if (!HideProcedures.Contains(traceRun.InternalProcedure))
                                {
                                    //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", traceRun.InternalProcedure, traceRun.Procedure);
                                }
                            }
                            else
                            {
                                //ReturnPoints.Push(traceRun.Procedure);
                                ReturnPoints.Push(new ReturnPoint(traceRun.Procedure, traceRun.Procedure, traceRun));
                                if (!HideProcedures.Contains(traceRun.Procedure))
                                {
                                    //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", traceRun.Procedure, traceRun.Procedure);
                                }
                            }
                            //if (currentEntry is TraceRun)
                            //{
                            //    TraceRun currentRun = currentEntry as TraceRun;
                            //    if (traceRun.InProcedure != currentRun.Procedure
                            //        || traceRun.InInternalProcedure != currentRun.InternalProcedure)
                            //    {
                            //        currentEntry = currentEntry.Parent;
                            //    }
                            //}
                            //if (currentEntry is TraceFunc)
                            //{
                            //    TraceFunc currentFunc = currentEntry as TraceFunc;
                            //    if (traceRun.InProcedure != currentFunc.Function)
                            //    {
                            //        currentEntry = currentEntry.Parent;
                            //    }
                            //}

                            if (traceRun.InternalProcedure == "Main Block")
                            {
                                traceRun = new TraceRunMain(th.OccurredAt, traceRun.Content); 
                            }


                            //link parent and children
                            traceRun.Parent = currentEntry;
                            currentEntry.Children.Add(traceRun);

                            if (traceRun.Procedure == "web/dispatcher.p")
                            {
                                //expand any rdt menu or desktop
                                ExpandParents(traceRun);
                            }

                            //new current
                            currentEntry = traceRun;

                            th.TraceEntry = traceRun;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("Return", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (currentEntry.TraceEntryType == TraceEntryTypes.StartupProcedure)
                            {
                                ;
                            }

                            if (linenum >= 57)
                            {
                                ;
                            }

                            TraceReturn traceReturn = new TraceReturn(th.OccurredAt, content);
                            if (!HideFunctions.Contains(traceReturn.ReturnFrom)
                                && !HideProcedures.Contains(traceReturn.ReturnFrom))
                            {
                                //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "}", traceReturn.ReturnFrom, traceReturn.InInternalProcedure);
                            }

                            if (!traceReturn.IsUnlinkedReturn
                                //|| ReturnPoints.Count == 0)
                                && ReturnPoints.Count > 0)
                            {
                                //if (linenum >= 532988)
                                //{
                                //    int i = 0;
                                //}

                                ReturnPoint returnPoint = ReturnPoints.Peek();

                                Debug.Assert(returnPoint.ReturnFrom != null);
                                Debug.Assert(traceReturn.ReturnFrom != null);

                                if (string.Compare(returnPoint.ReturnFrom, traceReturn.ReturnFrom, true) == 0
                                    || (returnPoint.ReturnFrom.Contains('.')
                                        && string.Compare(returnPoint.ReturnFrom.Split('.').Last(), traceReturn.ReturnFrom, true) == 0)
                                    )
                                {
                                    //remove head of stack
                                    ReturnPoint rp = ReturnPoints.Pop();
                                    if (rp.Creator is TraceFunc)
                                    {
                                        (rp.Creator as TraceFunc).ReturnValue = traceReturn.ReturnValue;
                                    }
                                    else if (rp.Creator is TraceInvoke)
                                    {
                                        (rp.Creator as TraceInvoke).ReturnValue = traceReturn.ReturnValue;
                                    }

                                    //link parent and children
                                    traceReturn.Parent = currentEntry;
                                    //new current is this parent
                                    currentEntry = currentEntry.Parent;
                                }
                                //catch unmatched returns
                                else
                                {

                                    bool doesEndpointExist = false;
                                    
                                    //does the return point exist on the stack?
                                    for (int i = ReturnPoints.Count - 1; i >= 0; i--)
                                    {
                                        if (string.Compare(ReturnPoints.ElementAt(i).ReturnFrom, traceReturn.ReturnFrom) == 0)
                                        {
                                            doesEndpointExist = true;
                                            break;
                                        }
                                    }
                                    // is this a double constructor return?
                                    // the same return content on the next line
                                    if (lastReturn != null
                                        && lastReturn.Content == traceReturn.Content
                                        && ParsedLines == (lastReturnLine + 1))
                                    {
                                        // indicate last return is not linked (think it is now)
                                        //lastReturn.IsUnlinkedReturn = true;
                                        // use parent of previous entry
                                        currentEntry = lastReturn.Parent;
                                    }

                                    // is this likely a static return
                                    if ((!doesEndpointExist)
                                        && traceReturn.IsConstructorReturn)
                                    {
                                        traceReturn.IsStaticReturn = true;
                                        traceReturn.IsUnlinkedReturn = true;
                                    }

                                    //did we find an endpoint?
                                    if (doesEndpointExist)
                                    {
                                        while (string.Compare(ReturnPoints.Peek().ReturnFrom, traceReturn.ReturnFrom, true) != 0)
                                        {
                                            ReturnPoints.Pop();
                                            //current entry has no return
                                            if (currentEntry is TraceRun)
                                            {
                                                (currentEntry as TraceRun).HasReturn = false;
                                            }
                                            //drop back a parent
                                            currentEntry = currentEntry.Parent;
                                        };

                                        ReturnPoints.Pop();
                                    }

                                    //link parent and children
                                    traceReturn.Parent = currentEntry;
                                    //new current is this parent
                                    if (!(traceReturn.IsStaticReturn
                                          && traceReturn.IsUnlinkedReturn))
                                    {
                                        currentEntry = currentEntry.Parent;
                                    }
                                    //Console.WriteLine("!!! NO MATCH {0}({1}) -- {2} ({3}) !!!", traceReturn.ReturnFrom, traceReturn.InInternalProcedure, ReturnPoints.Peek().ReturnFrom, ReturnPoints.Peek().Context);
                                }
                            }
                            //if (!traceReturn.IsUnlinkedReturn)

                            // save details to check for a double return
                            lastReturn = traceReturn;
                            lastReturnLine = ParsedLines;

                            th.TraceEntry = traceReturn;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("Func", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceFunc traceFunc;
                            if (contentParts[1].StartsWith("propGet_"))
                            {
                                traceFunc = new TraceFuncPropGet(th.OccurredAt, content);
                            }
                            else if (contentParts[1].StartsWith("propGet_"))
                            {
                                traceFunc = new TraceFuncPropSet(th.OccurredAt, content);
                            }
                            else
                            {
                                traceFunc = new TraceFunc(th.OccurredAt, content);
                            }

                            //ReturnPoints.Push(traceFunc.Function);
                            ReturnPoints.Push(new ReturnPoint(traceFunc.Function, traceFunc.Parameters, traceFunc));
                            
                            //link parent and children
                            traceFunc.Parent = currentEntry;
                            currentEntry.Children.Add(traceFunc);
                            //new current
                            currentEntry = traceFunc;

                            th.TraceEntry = traceFunc;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("PUBLISH", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TracePublish tracePublish = new TracePublish(th.OccurredAt, content);

                            //ReturnPoints.Push(tracePublish.EventName);
                            ReturnPoints.Push(new ReturnPoint(tracePublish.EventName, "PUBLISH", tracePublish));
                            //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", tracePublish.EventName, "PUBLISH");

                            //link parent and children
                            tracePublish.Parent = currentEntry;
                            currentEntry.Children.Add(tracePublish);
                            //new current
                            currentEntry = tracePublish;

                            th.TraceEntry = tracePublish;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("SUBSCRIBE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceSubscribe traceSubscribe = new TraceSubscribe(th.OccurredAt, content);
                            //link parent and children
                            traceSubscribe.Parent = currentEntry;
                            currentEntry.Children.Add(traceSubscribe);

                            th.TraceEntry = traceSubscribe;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("NEW", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceNew traceNew = new TraceNew(th.OccurredAt, content);

                            //ReturnPoints.Push(tracePublish.EventName);
                            ReturnPoints.Push(new ReturnPoint(traceNew.ClassName, "NEW", traceNew));
                            //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", traceNew.ClassName, "NEW");

                            //link parent and children
                            traceNew.Parent = currentEntry;
                            currentEntry.Children.Add(traceNew);
                            //new current
                            currentEntry = traceNew;

                            th.TraceEntry = traceNew;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("INVOKE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceInvoke traceInvoke;
                            if (contentParts[1].StartsWith("propGet_"))
                            {
                                traceInvoke = new TraceInvokePropGet(th.OccurredAt, content);
                            }
                            else if (contentParts[1].StartsWith("propGet_"))
                            {
                                traceInvoke = new TraceInvokePropSet(th.OccurredAt, content);
                            }
                            else
                            {
                                traceInvoke = new TraceInvoke(th.OccurredAt, content);
                            }

                            //ReturnPoints.Push(tracePublish.EventName);
                            ReturnPoints.Push(new ReturnPoint(traceInvoke.MethodName, "INVOKE", traceInvoke));
                            //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", traceInvoke.ClassName, "NEW");

                            //link parent and children
                            traceInvoke.Parent = currentEntry;
                            currentEntry.Children.Add(traceInvoke);
                            //new current
                            currentEntry = traceInvoke;

                            th.TraceEntry = traceInvoke;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("DELETE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceDelete traceDelete = new TraceDelete(th.OccurredAt, content);

                            //ReturnPoints.Push(tracePublish.EventName);
                            ReturnPoints.Push(new ReturnPoint(traceDelete.MethodName, "DELETE", traceDelete));
                            //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", traceInvoke.ClassName, "NEW");

                            //link parent and children
                            traceDelete.Parent = currentEntry;
                            currentEntry.Children.Add(traceDelete);
                            //new current
                            currentEntry = traceDelete;

                            th.TraceEntry = traceDelete;
                            ParsedLines++;
                        }
                        else if (contentParts[0].Equals("SUPER", StringComparison.CurrentCultureIgnoreCase))
                        {
                            TraceSuper traceSuper = new TraceSuper(th.OccurredAt, content);

                            //ReturnPoints.Push(tracePublish.EventName);
                            ReturnPoints.Push(new ReturnPoint(traceSuper.ClassName, "SUPER", traceSuper));
                            //Console.WriteLine("{0}{1}({2})", new string('\t', ReturnPoints.Count) + "{", traceInvoke.ClassName, "NEW");

                            //link parent and children
                            traceSuper.Parent = currentEntry;
                            currentEntry.Children.Add(traceSuper);
                            //new current
                            currentEntry = traceSuper;

                            th.TraceEntry = traceSuper;
                            ParsedLines++;
                        }
                        else
                        {
                            UnparsedLines++;
                        }
                    }
                    else if (th.TraceEntryType == TraceEntryTypes._4GLMESSAGE
                             || th.TraceEntryType == TraceEntryTypes.Information)
                    {
                        if (!(currentEntry is TraceMessage))
                        {
                            TraceMessage traceMessage = new TraceMessage(th.OccurredAt, content);
                            //link parent and children
                            traceMessage.Parent = currentEntry;
                            currentEntry.Children.Add(traceMessage);

                            currentEntry = traceMessage;

                            //expand any messages
                            //traceMessage.IsExpanded = true;

                            ExpandParents(traceMessage.Parent);
                            //ExpandNode(traceMessage);
                        }

                        TraceMessageLine traceMessageLine = new TraceMessageLine(th.OccurredAt, content);

                        //link parent and children
                        traceMessageLine.Parent = currentEntry;
                        currentEntry.Children.Add(traceMessageLine);

                        //get some content on parent
                        //if (traceMessageLine.Parent

                        th.TraceEntry = traceMessageLine;
                        ParsedLines++;
                    }
                    else if (th.TraceEntryType == TraceEntryTypes.Application)
                    {
                        if (!(currentEntry is ApplicationMessage))
                        {
                            ApplicationMessage applicationMessage = new ApplicationMessage(th.OccurredAt, content);
                            //link parent and children
                            applicationMessage.Parent = currentEntry;
                            currentEntry.Children.Add(applicationMessage);

                            currentEntry = applicationMessage;

                            //expand any messages
                            //traceMessage.IsExpanded = true;

                            ExpandParents(applicationMessage.Parent);
                            //ExpandNode(traceMessage);
                        }

                        ApplicationMessageLine applicationMessageLine = new ApplicationMessageLine(th.OccurredAt, content);

                        //link parent and children
                        applicationMessageLine.Parent = currentEntry;
                        currentEntry.Children.Add(applicationMessageLine);

                        //get some content on parent
                        //if (applicationMessageLine.Parent

                        th.TraceEntry = applicationMessageLine;
                        ParsedLines++;
                    }
                    else if (th.TraceEntryType == TraceEntryTypes.QryInfo)
                    {
                        if (content.StartsWith("Query Plan:"))
                        {
                            if (currentEntry is TraceQueryPlan
                                || currentEntry is TraceQueryStatistics)
                            {
                                currentEntry = currentEntry.Parent;
                            }

                            TraceQueryPlan traceQueryPlan = new TraceQueryPlan(th.OccurredAt, content);
                            //link parent and children
                            traceQueryPlan.Parent = currentEntry;
                            currentEntry.Children.Add(traceQueryPlan);

                            currentEntry = traceQueryPlan;
                            th.TraceEntry = traceQueryPlan;
                        }
                        else if (content.StartsWith("Query Statistics:"))
                        {
                            if (currentEntry is TraceQueryPlan
                                || currentEntry is TraceQueryStatistics)
                            {
                                currentEntry = currentEntry.Parent;
                            }

                            TraceQueryStatistics traceQueryStatistics = new TraceQueryStatistics(th.OccurredAt, content);
                            //link parent and children
                            traceQueryStatistics.Parent = currentEntry;
                            currentEntry.Children.Add(traceQueryStatistics);

                            currentEntry = traceQueryStatistics;
                            th.TraceEntry = traceQueryStatistics;
                        }
                        else
                        {
                            TraceQueryDetail traceQueryDetail = new TraceQueryDetail(th.OccurredAt, content);
                            //link parent and children
                            traceQueryDetail.Parent = currentEntry;
                            currentEntry.Children.Add(traceQueryDetail);

                            th.TraceEntry = traceQueryDetail;
                        }
                        ParsedLines++;
                    }
                }

                TraceHeaders.Add(th);
            }

            //RelinkTree(TraceEntryTree, 0);
        }

        //private void RelinkTree(TraceEntry traceEntry, int thisIndex)
        //{
        //    bool relinkChildren = false;

        //    //check this entry
        //    if (traceEntry is TraceRun)
        //    {
        //        if (!((TraceRun)traceEntry).HasReturn)
        //        {
        //            relinkChildren = true;
        //        }
        //    }
        //    //check this entry
        //    else if (traceEntry is TraceFunc)
        //    {
        //        if (!((TraceFunc)traceEntry).HasReturn)
        //        {
        //            relinkChildren = true;
        //        }
        //    }

        //    //relink children to parent
        //    if (relinkChildren)
        //    {
        //        for (int childIndex = 0; childIndex < traceEntry.Children.Count; childIndex++)
        //        {
        //            //RelinkTree(traceEntry.Children[childIndex], thisIndex + childIndex);
        //            //childrens parent is my parent
        //            traceEntry.Children[childIndex].Parent = traceEntry.Parent;
        //            //add children to my parent
        //            traceEntry.Parent.Children.Insert(thisIndex + childIndex, traceEntry.Children[childIndex]);
        //        }
        //    }

        //    //check children
        //    for (int childIndex = 0; childIndex < traceEntry.Children.Count; childIndex++)
        //    {
        //        RelinkTree(traceEntry.Children[childIndex], childIndex);
        //    }
        //}

        public void ExpandNode(TraceEntry traceEntry)
        {
            traceEntry.IsExpanded = true;
            foreach (var child in traceEntry.Children)
            {
                ExpandNode(child);
            }
        }

        public void CollapseNode(TraceEntry traceEntry)
        {
            traceEntry.IsExpanded = false;
            foreach (var child in traceEntry.Children)
            {
                CollapseNode(child);
            }
        }

        public void ExpandParents(TraceEntry traceEntry)
        {
            traceEntry.IsExpanded = true;
            if (traceEntry.Parent != null)
            {
                ExpandParents(traceEntry.Parent);
            }
        }

        public void SearchForText(TraceEntry traceEntry, string textToFind)
        {
            if (traceEntry.Content.IndexOf(textToFind, StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                traceEntry.IsHighlighted = true;
                ExpandParents(traceEntry.Parent);
            }
            else
            {
                traceEntry.IsHighlighted = false;
            }
            // call on children
            foreach (var child in traceEntry.Children)
            {
                SearchForText(child, textToFind);
            }
        }
    }
}
