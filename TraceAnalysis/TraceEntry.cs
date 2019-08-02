using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TraceAnalysis
{
    public class TraceEntry : INotifyPropertyChanged
    {
        public DateTime OccurredAt { get; set; }
        public TraceEntryTypes TraceEntryType { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public TraceEntry Parent { get; set; }
        public List<TraceEntry> Children { get; set; }

        public string InProcedure { get; set; }
        public string InInternalProcedure { get; set; }
        public int AtLine { get; set; }

        public bool Initialising { get; set; }

        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if ((!Initialising)
                    && _isExpanded
                    && Parent != null)
                {
                    Parent.IsExpanded = true;
                }
            }
        }

        private bool _isUITriggered = false;
        public bool IsUITriggered
        {
            get { return _isUITriggered; }
            set
            {
                if (value != _isUITriggered)
                {
                    _isUITriggered = value;
                    this.OnPropertyChanged("IsUITriggered");
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        private bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set
            {
                if (value != _isHighlighted)
                {
                    _isHighlighted = value;
                    this.OnPropertyChanged("IsHighlighted");
                }
            }
        }

        public TraceEntry(DateTime occurredAt, string content)
        {
            this.OccurredAt = occurredAt;
            this.Initialising = false;
            this.TraceEntryType = TraceEntryTypes._4GLTrace;

            Children = new List<TraceEntry>();

            Content = content;
            if (Content == null
                || Content == "")
            {
                return;
            }

            this.Description = string.Format("{0} {1}", this.OccurredAt, this.Content);

            //triggered by UI
            if (content.Contains("USER-INTERFACE-TRIGGER")
                || content.Contains("[PERS TRIG]") 
                || content.Contains(".ctList.ItemClick \"")
                || content.Contains(".ctList.Click [")
                || content.Contains(".ctList.KeyDown \"")
                || content.Contains(".ctPush.Click [")
                || content.Contains(".ctToolBar.ItemClick \"")
                || content.Contains(".ctToolBar.ItemOver [")
                
                )
            {
                this.IsUITriggered = true;
            }

            string desc = "";

            int ipos = Content.LastIndexOf('[');
            if (ipos > 0)
            {
                desc = Content.Substring(ipos).Replace("[", "").Replace("]", "");
            }

            if (desc == null
                || desc == "")
            {
                return;
            }

            //look for a -
            int idash = desc.IndexOf(" - ");
            if (idash <= 0)
            {
                this.InProcedure = desc.Substring(0, desc.Length);
                return;
            }

            this.InInternalProcedure = desc.Substring(0, idash);

            int iAt = desc.IndexOf('@');

            if (iAt >= 0
                && desc.Length > idash + 3 + 1)
            {
                this.InProcedure = desc.Substring(idash + 3, iAt - (idash + 4));
                int atLine = 0;
                if (int.TryParse(desc.Substring(iAt + 2), out atLine))
                {
                    this.AtLine = atLine;
                }
                else
                {
                    this.AtLine = 0;
                }
            }
            else
            {
                this.InProcedure = "UNKNOWN";
                this.AtLine = 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
