using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using ReadFit.FileModel;
using System.Deployment.Application;

namespace ReadFit
{
    class WriteKmlViewModel : ObservableObject, IPageViewModel, IDataErrorInfo
    {
        public string Name
        {
            get { return "Write KML"; }
        }

        public ObservableCollection<string> MyColorNames { get; set; }
        public Dictionary<string, double> mld { get; set; }
        public Dictionary<string, Color> mycolordict { get; set; }

        MsgBoxService msgBoxobj;

        public IXmlFunctions xmldatalayer { get; set; }

        public WriteKmlViewModel()
        {
            MessageBus.Instance.Subscribe<ReturnInitial>(handleInitialData);
            MessageBus.Instance.Subscribe<MyFlag>(handleFlagMessage);
            MessageBus.Instance.Subscribe<ID>(handleIDMessage);

            msgBoxobj = new MsgBoxService();

            xmldatalayer = GetLayer.giveMeAXmlLayer();

            string path = string.Empty;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                path = ApplicationDeployment.CurrentDeployment.DataDirectory + @"\utilitiesDocument.xml";
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "utilitiesDocument.xml";
            }

            myXdoc = xmldatalayer.ReadInitialWkml(path);

            //myXdoc2 = new Uri(path, UriKind.Absolute);    //uri of file path

            XElement mytype = XElement.Load(path);  //linqtoxml

            var query1 = (from x in mytype.Descendants("File")
                          select new
                          {
                              ridetype = x.Element("Type").Value
                          });

            TypeOfFile = new ObservableCollection<string>();

            foreach (var q in query1)
            {
                TypeOfFile.Add(q.ridetype);
            }

            var query2 = (from x in mytype.Descendants("Linew")
                          select new
                          {
                              mylw = x.Element("Width").Value
                          });

            WidthOfLine = new ObservableCollection<string>();

            foreach (var q in query2)
            {
                WidthOfLine.Add(q.mylw);
            }

            var query3 = (from x in mytype.Descendants("Line")
                          select new
                          {
                              mylo = x.Element("Opacity").Value
                          });

            OpacityOfLine = new ObservableCollection<string>();

            foreach (var q in query3)
            {
                OpacityOfLine.Add(q.mylo);
            }

            var query4 = (from x in mytype.Descendants("Split")
                          select new
                          {
                              myspl = x.Element("Name").Value
                          });

            LSplits = new ObservableCollection<string>();

            foreach (var q in query4)
            {
                LSplits.Add(q.myspl);
            }

            if ((string)_MyFileType == "Time Slider")
            {
                WriteSP = false;
                SplitDistanceEnabled = false;
                WriteSpEnabled = false;
            }
            else if ((string)_MyFileType == "Simple Path")
            {
                SplitDistanceEnabled = false;
                WriteSpEnabled = true;
            }
            else
            {
                SplitDistanceEnabled = true;
                WriteSpEnabled = true;
            }

            MySplit = Properties.Settings.Default.KmlSplitDistance;
            MyFileType = Properties.Settings.Default.KmlFileType;
            MyOpacity = (Properties.Settings.Default.KmlOpacity).ToString();
            MyLineWidth = (Properties.Settings.Default.KmlLineWidth).ToString();
            MyColor = (Properties.Settings.Default.KmlColor != null) ? Properties.Settings.Default.KmlColor : "Red";
            UserName = Properties.Settings.Default.KmlUserName;
            WriteSP = Properties.Settings.Default.KmlWriteSp;
            WriteStart = Properties.Settings.Default.KmlWriteStart;
            WriteEnd = Properties.Settings.Default.KmlWriteEnd;
            MapStopTimes = Properties.Settings.Default.MapStopTime;

            int mci = MyColorNames.IndexOf(MyColor);
            myIndex = mci;

            OnPropertyChanged("MyColor");
            OnPropertyChanged("myIndex");

            if (string.IsNullOrEmpty(Properties.Settings.Default.KMLInitialDirectory))
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                InitialDirectory = Properties.Settings.Default.KMLInitialDirectory;
            }

            ColorVisible = false;

            //InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //InitialDirectory = @"C:\Users\mark\Documents\KML\";
        }

        private int _selIndex;
        public int selIndex
        {
            get { return _selIndex; }
            set
            {
                if (value != _selIndex)
                {
                    _selIndex = value;
                    OnPropertyChanged("selIndex");
                }
            }
        }
      
        private void handleIDMessage(ID msg)
        {
            UserId = msg.UserId;
            UserSport = msg.Sport;
        }

        private void handleInitialData(ReturnInitial init)  //get the initial data for this viewmodel
        {
            MyColorNames = init.MyColorNames;
            mld = init.Mld;
            mycolordict = init.MyColorDict;

            MySplitDistance = mld[Properties.Settings.Default.KmlSplitDistance];
        }

        private void handleFlagMessage(MyFlag flg)
        {
            switch (flg.FlagName)
            {
                case "IsReady":
                    IsReady = flg.FlagState;    //controls visibility of the write kml button, and the filename visibility
                    break;
            }

            if (IsReady)
            {
                
                MyKmlFileName = UserId.Substring(0, 10) + UserSport + ".kml";
                MyKmlSPFileName = UserId.Substring(0, 10) + UserSport + "SP.kml";
            }
            else
            {
                MyKmlFileName = string.Empty;       //xml history file has not been read yet
                MyKmlSPFileName = string.Empty;
            }
        }

        private XDocument _myXdoc;
        public XDocument myXdoc
        {
            get { return _myXdoc; }
            set
            {
                if (value != _myXdoc)
                {
                    _myXdoc = value;
                    OnPropertyChanged("myXdoc");
                }
            }
        }

        private ObservableCollection<string> _TypeOfFile;
        public ObservableCollection<string> TypeOfFile
        {
            get { return _TypeOfFile; }
            set
            {
                if (value != _TypeOfFile)
                {
                    _TypeOfFile = value;
                    OnPropertyChanged("TypeOfFile");
                }
            }
        }

        private ObservableCollection<string> _WidthOfLine;
        public ObservableCollection<string> WidthOfLine
        {
            get { return _WidthOfLine; }
            set
            {
                if (value != _WidthOfLine)
                {
                    _WidthOfLine = value;
                    OnPropertyChanged("WidthOfLine");
                }
            }
        }

        private ObservableCollection<string> _OpacityOfLine;
        public ObservableCollection<string> OpacityOfLine
        {
            get { return _OpacityOfLine; }
            set
            {
                if (value != _OpacityOfLine)
                {
                    _OpacityOfLine = value;
                    OnPropertyChanged("OpacityOfLine");
                }
            }
        }

        private ObservableCollection<string> _LSplits;
        public ObservableCollection<string> LSplits
        {
            get { return _LSplits; }
            set
            {
                if (value != _LSplits)
                {
                    _LSplits = value;
                    OnPropertyChanged("LSplits");
                }
            }
        }

        private int _myIndex;
        public int myIndex
        {
            get { return _myIndex; }
            set
            {
                if (value != _myIndex)
                {
                    _myIndex = value;
                    OnPropertyChanged("myIndex");
                }
            }
        }

        private ICommand writeCommand;
        public ICommand WriteCommand
        {
            get
            {
                if (writeCommand == null)
                {
                    writeCommand = new RelayCommand(p => writeFile((string)p));
                }

                return writeCommand;
            }
        }

        private void writeFile(string name)
        {
            string errormsg = string.Empty;

            bool returnResult = false;

            Properties.Settings.Default.KmlUserName = UserName;
            Properties.Settings.Default.Save();

            string myFilePath = Path.GetDirectoryName(name);    //get just the path - todo save the new path in properties?
            InitialDirectory = myFilePath;

            string enteredFileName = Path.GetFileName(name);

            if (!MyKmlFileName.Equals(enteredFileName, StringComparison.OrdinalIgnoreCase))
            {
                msgBoxobj.ShowNotification("KML File Name Has Been Changed");   //todo ask if new name is wanted
            }

            MyPassedData myPassedData = new MyPassedData();

            myPassedData.myUserId = UserId;
            myPassedData.mySport = UserSport;
            myPassedData.myKmlFile = MyKmlFileName;
            myPassedData.myFQKmlFile = System.IO.Path.Combine(myFilePath, MyKmlFileName);     //fully qualified file name
            myPassedData.mySPKmlFile = MyKmlSPFileName;
            myPassedData.myFQSPKmlFile = System.IO.Path.Combine(myFilePath, MyKmlSPFileName); //fully qualified smart phone file name
            myPassedData.myLineWidth = Properties.Settings.Default.KmlLineWidth;
            myPassedData.Mld = mld;
            myPassedData.Mycolordict = mycolordict;
            myPassedData.Mysplitdistance = MySplitDistance;

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = false });

            Task.Factory.StartNew(() =>
                {
                    returnResult = xmldatalayer.WriteFile(myPassedData);

                }).ContinueWith(t => onComplete(t, returnResult), TaskContinuationOptions.None);
        }

        private void onComplete(Task t, bool flag)
        {
            if (t.IsFaulted)
            {
                msgBoxobj.ShowNotification("Fault on write: " + t.Exception.Message);
            }
            else if (!flag)
            {
                msgBoxobj.ShowNotification("KML write returned an error");
            }
            else
            {
                msgBoxobj.ShowNotification("KML Write was successful");
            }

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = true });
        }

        private string _InitialDirectory;
        public string InitialDirectory
        {
            get { return _InitialDirectory; }
            set
            {
                if (value != _InitialDirectory)
                {
                    _InitialDirectory = value;
                    OnPropertyChanged("InitialDirectory");
                    Properties.Settings.Default.KMLInitialDirectory = value;
                }
            }
        }

        private string _UserId;
        public string UserId
        {
            get { return _UserId; }
            set
            {
                if (value != _UserId)
                {
                    _UserId = value;
                    OnPropertyChanged("UserId");
                }
            }
        }

        private string _UserSport;
        public string UserSport
        {
            get { return _UserSport; }
            set
            {
                if (value != _UserSport)
                {
                    _UserSport = value;
                    OnPropertyChanged("UserSport");
                }
            }
        }


        private string _MyKmlFileName;
        public string MyKmlFileName
        {
            get { return _MyKmlFileName; }
            set
            {
                if (value != _MyKmlFileName)
                {
                    _MyKmlFileName = value;
                    OnPropertyChanged("MyKmlFileName");
                }
            }
        }

        private string _MyKmlSPFileName;
        public string MyKmlSPFileName
        {
            get { return _MyKmlSPFileName; }
            set
            {
                if (value != _MyKmlSPFileName)
                {
                    _MyKmlSPFileName = value;
                    OnPropertyChanged("MyKmlSPFileName");
                }
            }
        }

        private double _MySplitDistance;     //Split distance in feet/meters
        public double MySplitDistance
        {
            get { return _MySplitDistance; }
            set
            {
                if (value != _MySplitDistance)
                {
                    _MySplitDistance = value;
                    OnPropertyChanged("MySplitDistance");
                }
            }
        }

        private bool _MapStopTimes;      //if you want the stop times mapped
        public bool MapStopTimes
        {
            get { return _MapStopTimes; }
            set
            {
                if (value != _MapStopTimes)
                {
                    _MapStopTimes = value;
                    OnPropertyChanged("MapStopTimes");
                    Properties.Settings.Default.MapStopTime = value;
                }
            }
        }

        private bool _WriteSP;       //the smart phone checkbox
        public bool WriteSP
        {
            get { return _WriteSP; }
            set
            {
                if (value != _WriteSP)
                {
                    _WriteSP = value;
                    OnPropertyChanged("WriteSP");
                    Properties.Settings.Default.KmlWriteSp = value;
                }
            }
        }

        private bool _WriteSpEnabled;
        public bool WriteSpEnabled
        {
            get { return _WriteSpEnabled; }
            set
            {
                if (value != _WriteSpEnabled)
                {
                    _WriteSpEnabled = value;
                    OnPropertyChanged("WriteSpEnabled");
                }
            }
        }

        private bool _SplitDistanceEnabled;      //enable/disable the splitdistance combobox
        public bool SplitDistanceEnabled
        {
            get { return _SplitDistanceEnabled; }
            set
            {
                if (value != _SplitDistanceEnabled)
                {
                    _SplitDistanceEnabled = value;
                    OnPropertyChanged("SplitDistanceEnabled");
                }
            }
        }

        private bool _WriteStart;    //write the start placemark on the kml file
        public bool WriteStart
        {
            get { return _WriteStart; }
            set
            {
                if (value != _WriteStart)
                {
                    _WriteStart = value;
                    OnPropertyChanged("WriteStart");
                    Properties.Settings.Default.KmlWriteStart = value;
                }
            }
        }

        private bool _WriteEnd;      //write the end placemark in the kml file
        public bool WriteEnd
        {
            get { return _WriteEnd; }
            set
            {
                if (value != _WriteEnd)
                {
                    _WriteEnd = value;
                    OnPropertyChanged("WriteEnd");
                    Properties.Settings.Default.KmlWriteEnd = value;
                }
            }
        }

        private bool _IsReady;
        public bool IsReady
        {
            get { return _IsReady; }
            set
            {
                if (value != _IsReady)
                {
                    _IsReady = value;
                    OnPropertyChanged("IsReady");
                }
            }
        }

        private object _MyFileType;      //Kml File Type (default: Splits)
        public object MyFileType
        {
            get { return _MyFileType; }
            set
            {
                if (value != _MyFileType)
                {
                    _MyFileType = value;
                    OnPropertyChanged("MyFileType");
                    Properties.Settings.Default.KmlFileType = (string)_MyFileType;
                    switch ((string)MyFileType)
                    {
                        case "Time Slider":
                            WriteSP = false;
                            SplitDistanceEnabled = false;
                            WriteSpEnabled = false;
                            break;
                        case "Simple Path":
                            SplitDistanceEnabled = false;
                            WriteSpEnabled = true;
                            break;
                        default:
                            SplitDistanceEnabled = true;
                            WriteSpEnabled = true;
                            break;
                    }
                }
            }
        }

        private object _MySplit;     //Kml Split Distance (default: 1 Mile)
        public object MySplit
        {
            get { return _MySplit; }
            set
            {
                if (value != _MySplit)
                {
                    _MySplit = value;
                    OnPropertyChanged("MySplit");
                    MySplitDistance = mld[(string)MySplit];
                    Properties.Settings.Default.KmlSplitDistance = (string)MySplit;
                }
            }
        }

        private object _MyOpacity;       //Kml Line Opacity  (default: 70)
        public object MyOpacity
        {
            get { return _MyOpacity; }
            set
            {
                if (value != _MyOpacity)
                {
                    _MyOpacity = value;
                    OnPropertyChanged("MyOpacity");
                    Properties.Settings.Default.KmlOpacity = Convert.ToInt32((string)MyOpacity);
                }
            }
        }

        private object _MyLineWidth;     //Kml Line Width  (default: 5)
        public object MyLineWidth
        {
            get { return _MyLineWidth; }
            set
            {
                if (value != _MyLineWidth)
                {
                    _MyLineWidth = value;
                    OnPropertyChanged("MyLineWidth");
                    Properties.Settings.Default.KmlLineWidth = Convert.ToInt32((string)MyLineWidth);
                }
            }
        }

        private string _MyColor;     //Kml Line color  (default Red)
        public string MyColor
        {
            get { return _MyColor; }
            set
            {
                if (value != _MyColor)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _MyColor = value;
                        OnPropertyChanged("MyColor");

                        ColorVisible = false;
                        Properties.Settings.Default.KmlColor = MyColor;
                    }
                }
            }
        }

        private bool _ColorVisible;
        public bool ColorVisible
        {
            get { return _ColorVisible; }
            set
            {
                if (value != _ColorVisible)
                {
                    _ColorVisible = value;
                    OnPropertyChanged("ColorVisible");

                    if (_ColorVisible)
                    {
                        int mytempindex = myIndex;
                        string mytempcolor = Properties.Settings.Default.KmlColor;

                        myIndex = -1;
                        MyColor = null;
                        OnPropertyChanged("myIndex");
                        OnPropertyChanged("MyColor");

                        MyColor = mytempcolor;
                        myIndex = mytempindex;
                        OnPropertyChanged("MyColor");
                        OnPropertyChanged("myIndex");
                    }
                }
            }
        }

        private string _UserName;
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != _UserName)
                {
                    _UserName = value;
                    OnPropertyChanged("UserName");
                }
            }
        }

        public string Error
        {
            get { return null; }
        }

        public string this[string columnname]
        {
            get
            {
                if (columnname == "UserName")
                {
                    if (string.IsNullOrEmpty(UserName))
                    {
                        return "Must enter a name";
                    }

                    if (UserName == "Enter Name")
                    {
                        return "Please enter a name";
                    }
                }
                return null;
            }
        }

        private string _FileFilters;
        public string FileFilters
        {
            get
            {
                if (_FileFilters == null)
                {
                    //fileFilters = "All files (*.*)|*.*|Text files (*.txt)|*.txt|Xml files (*.xml)|*.xml";
                    _FileFilters = "Kml files (.kml)|*.kml";
                }
                return _FileFilters;
            }
        }

        public int FilterIndex
        {
            get
            {
                return 2;
            }
        }
    }
}
