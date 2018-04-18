using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TraceAnalysis;

namespace TraceView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<TraceHeader> headers = new List<TraceHeader>();
        public List<TraceHeader> Headers
        {
            get
            {
                return headers;
            }
            set
            {
                if (headers != value)
                {
                    headers = value;
                    NotifyPropertyChanged("Headers");
                }
            }
        }
        private List<TraceEntry> treeRoot;
        public List<TraceEntry> TreeRoot
        {
            get
            {
                return treeRoot;
            }
            set
            {
                if (treeRoot != value)
                {
                    treeRoot = value;
                    NotifyPropertyChanged("TreeRoot");
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            TreeRoot = new List<TraceEntry>();
            TraceEntry te = new TraceEntry(DateTime.MinValue, "Root");
            te.Description = "Root";
            TreeRoot.Add(te);

            if (Application.Current.Properties.Count == 1)
            {
                Console.WriteLine();
            }

            tbTraceFilename.Text = @"D:\workspaces\oe-traceview\input.log";

            //treeview.DataContext = this;
        }

        public void SetFileName(string filename)
        {
            tbTraceFilename.Text = filename;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tbTraceFilename.Text))
            {
                MessageBox.Show(string.Format("File {0} not found!", tbTraceFilename.Text), "File not found");
                return;
            }

            Console.WriteLine("Building tree");
            TraceAnal traceAnal = new TraceAnal();
            try
            {
                traceAnal.Analyse(tbTraceFilename.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Headers = traceAnal.TraceHeaders;
            //this.TreeRoot = traceAnal.TraceEntryTree;
            this.TreeRoot = new List<TraceEntry>();
            this.TreeRoot.Add(traceAnal.TraceEntryTree);
            treeview.ItemsSource = this.TreeRoot;
            Console.WriteLine("Tree built.");

        }

        private void btnExpandAll_Click(object sender, RoutedEventArgs e)
        {
            treeview.BeginInit();
            ExpandNode(TreeRoot[0]);
            treeview.EndInit();
        }

        private void ExpandNode(TraceEntry traceEntry)
        {
            //Debug.WriteLine("Expanding {0}", traceEntry.AtLine);

            traceEntry.Initialising = true;
            traceEntry.IsExpanded = true;
            traceEntry.Initialising = false;
            foreach (var child in traceEntry.Children)
            {
                ExpandNode(child);
            }
            //Debug.WriteLine("   Expanded {0}", traceEntry.AtLine);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
