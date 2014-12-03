using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Threading;
using System.Threading.Tasks;
using ReadFit.FileModel;
using System.IO;
using System.Deployment.Application;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
//using OxyPlot.Wpf;

namespace ReadFit
{
    class ListDataVeiwModel : ObservableObject, IPageViewModel
    {
        public string Name
        {
            get { return "List Data"; }
        }

        MsgBoxService msgBoxobj;

        public IDataAccessLayer datalayer { get; set; }

        public ListDataVeiwModel()
        {
            datalayer = GetLayer.giveMeADataLayer();

            msgBoxobj = new MsgBoxService();

            MessageBus.Instance.Subscribe<MyFlag>(handleFlagMessage);

            DataService.Instance.mycrashFlag = false;

            NavValue = Visibility.Hidden;

            ViewReplaced = false;

            lastClickType = "Activity";

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            //bool tconn1 = datalayer.tableMaintenence(@"C:\Users\mark\Documents\dbase.bak");
            //bool tconn1 = datalayer.tableMaintenence(null);

            //getHTTPRequest gr = new getHTTPRequest();

            //bool test1 = datalayer.BackupDatabase(@"C:\Users\mark\Documents\dbase.bak");

            SelectedIndex = -1;

            DataService.Instance.TabHeaderStp = "Stop Time";

            RecordDisplay = "Records";

            tabSelectedIndex = 0;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                string mypath1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string mypath2 = Path.Combine(mypath1, "Wiskochil");

                if (Properties.Settings.Default.DataBasePath.IsNullOrEmpty())
                {
                    try
                    {
                        Directory.CreateDirectory(mypath2);
                    }
                    catch (IOException e)
                    {
                        msgBoxobj.ShowNotification("error in directory create: " + e.Message);
                    }

                    Properties.Settings.Default.DataBasePath = Path.Combine(mypath2, "FitDataBase.db");
                }

                if (!Directory.Exists(mypath2))
                {
                    try
                    {
                        Directory.CreateDirectory(mypath2);
                    }
                    catch (IOException e)
                    {
                        msgBoxobj.ShowNotification("error in directory create: " + e.Message);
                    }
                }
            }
            else
            {
                Properties.Settings.Default.DataBasePath = "FitDataBase.db";    //use a test database in Debug
            }

            //bool tconn1 = datalayer.tableMaintenence(@"C:\Users\mark\Documents\dbase.bak");

            if (File.Exists(Properties.Settings.Default.DataBasePath))
            {
                var testDb = datalayer.getQuery<string>(null, "GetTableNames");

                if (testDb.Count() > 0)
                {
                    var mytest1 = Task.Factory.StartNew(() => getmydata(0));

                    mytest1.ContinueWith(t =>
                    {
                        bool gpsflag = false;
                        int rowid = 0;

                        if (DataService.Instance.activityData != null)
                        {
                            if (DataService.Instance.activityData.Count > 0)
                            {
                                gpsflag = DataService.Instance.activityData[0].HasGpsData;
                                rowid = DataService.Instance.activityData[0].rowID;
                            }
                        }

                        if (!t.Result.IsFaulted)
                        {
                            bool isLoaded = Convert.ToBoolean(mytest1.Result.Result);

                            if (isLoaded)
                            {
                                recordCount = getRecCnt(tabSelectedIndex);

                                //gr.testElevationRead(gpsflag, rowid);     //get a GoogleElevation read for each lat,long point in ride

                                myLapDisplay = DataService.Instance.lapData.ToCollectionView();
                                myLapDisplay.Filter = null;

                                myActivityDisplay = DataService.Instance.activityData.ToCollectionView();
                                myActivityDisplay.Filter = null;

                                if (DataService.Instance.trackData != null)
                                {
                                    myPlotModel = threeChart(DataService.Instance.DistanceTimeFlag);
                                }
                            }
                        }
                        else
                        {
                            msgBoxobj.ShowNotification("Critical Error In DataBase Read: " + mytest1.Result.Exception.InnerException.Message);
                        }
                    });

                }
                else
                {
                    //msgBoxobj.ShowNotification("Initialization of database");
                    bool tconn = datalayer.tableMaintenence(null);
                }
            }
            else
            {
                //msgBoxobj.ShowNotification("File not found");
                
                //System.Data.SQLite.SQLiteConnection.CreateFile("FitDataBase.db");
                try
                {
                    System.Data.SQLite.SQLiteConnection.CreateFile(Properties.Settings.Default.DataBasePath);
                }
                catch (Exception ex)
                {
                    msgBoxobj.ShowNotification("error in database create " + ex.Message);
                }

                //if (File.Exists("FitDataBase.db"))
                if (File.Exists(Properties.Settings.Default.DataBasePath))
                {
                    //msgBoxobj.ShowNotification("Created file");
                    //msgBoxobj.ShowNotification("Initialization of database");
                    bool tconn = datalayer.tableMaintenence(null);
                }
                else
                {
                    msgBoxobj.ShowNotification("Failed to create database");
                }
            }

            //getTheData gtd = new getTheData();

            //TestThis tst = new TestThis();

            //string path = @"C:\Users\mark\Documents\TrainingPeaks\Device Agent\saved\phleb3\";

            //List<string> files = tst.SearchXmlFiles(path, "*.pwx");  //works

            //string path = @"C:\Users\mark\Documents\TrainingPeaks\Device Agent\saved\phleb3\phleb3_Garmin_Edge_205_305_2010_07_31_12_33_21.pwx";

            //ReadXmlFile rf = new ReadXmlFile();

            //Task.Factory.StartNew(() => Parallel.ForEach(files, q => { rf.readXml(q); }));

            //Parallel.ForEach(files, q => { rf.readXml(q); });

            //rf.readXml(path);


            //var mylocation = new GeoCoordinate(44.088052, -73.314059);
            //var yourLocation = new GeoCoordinate(44.089064, -73.302452);

            //double distance = mylocation.GetDistanceTo(yourLocation);     //distance in meters

            //object myt6 = InvokeMethod(new Func<string,int>(Method1), "My String One");

            //int myt7 = (int)InvokeMethod(new Func<string, string, int>(datalayer.getUserData<int>), "2013-08-10T09:58:34", "TrackCountByKey");

            //DataService.Instance.DistanceTimeFlag = false;
            DataService.Instance.DistanceTimeFlag = Properties.Settings.Default.DistanceTimeFlag;

            LapsByDate = false;

            HideCorrectionTab = false;
        }

        //public static object InvokeMethod(Delegate method, params object[] args)
        //{
        //    return method.DynamicInvoke(args);
        //}

        private static Task<string> getmydata(int index)    //event driven data load
        {
            var tcs = new TaskCompletionSource<string>();

            dataAccessClient dac = new dataAccessClient();

            //when method completes, message is published and we receive it here
            MessageBus.Instance.Subscribe<dataAccessEventArgs>(t =>
            {
                if (t.Cancelled)
                {
                    tcs.SetCanceled();
                    return;
                }

                if (t.Error != null)
                {
                    tcs.SetException(t.Error);
                    return;
                }

                tcs.SetResult(t.Result.ToString());
            });

            dac.getDataNow(index);  //FileModel class - starts the event

            return tcs.Task;
        }

        private void handleFlagMessage(MyFlag msg)
        {
            switch (msg.FlagName)
            {
                case "Correction":
                    //myCorrectionModel = getData(msg.FlagState);
                    break;

                case "Chart":
                    myPlotModel = threeChart(msg.FlagState);
                    OnPropertyChanged("myPlotModel");
                    break;

                case "LoadCollections":
                    if (msg.FlagState)
                    {
                        if (DataService.Instance.lapData.IsNullOrEmpty())
                        {
                            msgBoxobj.ShowNotification("Empty lap data");
                        }

                        myActivityDisplay = DataService.Instance.activityData.ToCollectionView();   //refresh the collectionviews
                        myLapDisplay = DataService.Instance.lapData.ToCollectionView();

                        myActivityDisplay.Refresh();
                        myLapDisplay.Refresh();
                    }
                    break;

                case "HideTab":
                    HideCorrectionTab = msg.FlagState;
                    break;

                case "UpdateRecordCount":
                    recordCount = getRecCnt(0);
                    break;
            }
        }

        private bool _HideCorrectionTab;
        public bool HideCorrectionTab
        {
            get { return _HideCorrectionTab; }
            set
            {
                if (value != _HideCorrectionTab)
                {
                    _HideCorrectionTab = value;
                    OnPropertyChanged("HideCorrectionTab");
                }
            }
        }

        //private PlotModel getData(bool msg)     //plot the Garmin elevation vs the Google elevation
        //{
        //    //int idx;
        //    double elv1 = 0.0;
        //    double elv2 = 0.0;
        //    double changeElv = 0.0;

        //    //for (idx = 0; idx < 3; idx++)
        //    //{
        //    //    elv1 += DataService.Instance.myElevations[idx].corrected;
        //    //    elv2 += DataService.Instance.trackData[idx].altitude;
        //    //}

        //    //changeElv = (Math.Abs((elv1 / 3.0) - (elv2 / 3.0))) * 1.00;

        //    if (DataService.Instance.myElevations.Count < 1)
        //    {
        //        return new PlotModel();     //if there is no gps data, then no elevations either
        //    }

        //    elv1 = DataService.Instance.myElevations[0].corrected;
        //    elv2 = DataService.Instance.trackData[0].altitude;

        //    changeElv = Math.Abs(elv1 - elv2);

        //    PlotModel myModel = new PlotModel();

        //    myModel.LegendOrientation = LegendOrientation.Horizontal;   //Legend position/placement
        //    myModel.LegendPlacement = LegendPlacement.Outside;
        //    myModel.LegendPosition = LegendPosition.BottomRight;

        //    DateTime current1 = new DateTime();

        //    OxyPlot.Axes.DateTimeAxis datetimeAxis1;
        //    OxyPlot.Axes.LinearAxis linearAxis1;
        //    OxyPlot.Axes.LinearAxis linearAxis2;

        //    current1 = DateTime.Parse(DataService.Instance.trackData[0].timeStamp);
        //    string axisTitle = String.Format("{0:dddd, MMMM d, yyyy}", current1);

        //    double conversionFactor = Properties.Settings.Default.IsMetric ? 1.0 : 3.28084;
        //    double scaleFactor = Properties.Settings.Default.IsMetric ? 1000.0 : 5280.0;

        //    myModel.Title = "Elevation Correction";

        //    linearAxis1 = new LinearAxis();
        //    linearAxis1.MajorGridlineStyle = LineStyle.Solid;
        //    linearAxis1.Title = Properties.Settings.Default.IsMetric ? "Elevation - Meters" : "Elevation - Feet";
        //    linearAxis1.FontWeight = OxyPlot.FontWeights.Bold;
        //    linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
        //    myModel.Axes.Add(linearAxis1);

        //    if (!msg)
        //    {
        //        datetimeAxis1 = new DateTimeAxis();
        //        datetimeAxis1.Position = AxisPosition.Bottom;
        //        datetimeAxis1.Title = axisTitle;
        //        datetimeAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
        //        datetimeAxis1.MajorGridlineStyle = LineStyle.Solid;
        //        //datetimeAxis1.MinorGridlineStyle = LineStyle.Dot;
        //        datetimeAxis1.FontWeight = OxyPlot.FontWeights.Bold;
        //        myModel.Axes.Add(datetimeAxis1);
        //    }
        //    else
        //    {
        //        linearAxis2 = new LinearAxis();
        //        linearAxis2.MajorGridlineStyle = LineStyle.Solid;
        //        linearAxis2.FontWeight = OxyPlot.FontWeights.Bold;
        //        linearAxis2.Position = AxisPosition.Bottom;
        //        linearAxis2.Title = Properties.Settings.Default.IsMetric ? "Distance - Km" : "Distance - Miles";
        //        linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
        //        myModel.Axes.Add(linearAxis2);
        //    }

        //    var lineseries1 = new LineSeries();
        //    lineseries1.Title = "Garmin Edge 800";          //for the legend
        //    lineseries1.Color = OxyColors.CornflowerBlue;

        //    var lineseries2 = new LineSeries();
        //    lineseries2.Title = "Google Elevations";        //for the legend
        //    lineseries2.Color = OxyColors.Red;

        //    foreach (var q in DataService.Instance.myElevations)
        //    {
        //        if (!msg)
        //        {
        //            current1 = DateTime.Parse(q.timeStamp);
        //            lineseries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), ((q.uncorrected - changeElv) * conversionFactor)));
        //            //lineseries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), (q.uncorrected * conversionFactor)));
        //            lineseries2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), (q.corrected * conversionFactor)));
        //        }
        //        else
        //        {
        //            lineseries1.Points.Add(new DataPoint((q.distance * conversionFactor) / scaleFactor, ((q.uncorrected - changeElv) * conversionFactor)));
        //            //lineseries1.Points.Add(new DataPoint((q.distance * conversionFactor), (q.uncorrected * conversionFactor)));
        //            lineseries2.Points.Add(new DataPoint((q.distance * conversionFactor) / scaleFactor, (q.corrected * conversionFactor)));
        //        }
        //    }

        //    myModel.Series.Add(lineseries1);
        //    myModel.Series.Add(lineseries2);
        //    //myModel.Series.Add(lineseries3);

        //    return myModel;
        //}

        private PlotModel _myPlotModel;
        public PlotModel myPlotModel
        {
            get { return _myPlotModel; }
            set
            {
                if (value != _myPlotModel)
                {
                    _myPlotModel = value;
                    OnPropertyChanged("myPlotModel");
                }
            }
        }

        private PlotModel _myCorrectionModel;
        public PlotModel myCorrectionModel
        {
            get { return _myCorrectionModel; }
            set
            {
                if (value != _myCorrectionModel)
                {
                    _myCorrectionModel = value;
                    OnPropertyChanged("myCorrectionModel");
                }
            }
        }

        private PlotModel threeChart(bool flg)
        {
            if (DataService.Instance.trackData.IsNullOrEmpty())
            {
                return new PlotModel();
            }

            double hrLimit = 0.0;

            bool hrFlag = false;

            if (Properties.Settings.Default.MaxHeartRateFlag)
            {
                if (Properties.Settings.Default.MaximumHeartRate > 0)
                {
                    hrFlag = true;
                }
            }

            //hrFlag = (Properties.Settings.Default.MaximumHeartRate > 0) ? true : false;

            if (hrFlag)
            {
                hrLimit = (double)Properties.Settings.Default.MaximumHeartRate * 0.85;
            }

            double conversionFactor = Properties.Settings.Default.IsMetric ? 1.0 : 3.28084;
            double scaleFactor = Properties.Settings.Default.IsMetric ? 1000.0 : 5280.0;
            double scaledDistance = 0.0;

            DateTime current1 = new DateTime();

            current1 = DateTime.Parse(DataService.Instance.trackData[0].timeStamp);
            string axisTitle = String.Format("{0:dddd, MMMM d, yyyy}", current1);

            PlotModel myPlotModel1 = new PlotModel();

            var twoColorLineSeries1 = new TwoColorLineSeries();         //heartrate
            var lineSeries1 = new LineSeries();

            if (hrFlag)
            {
                twoColorLineSeries1.Limit = hrLimit;                        //is the limit between the two colors
                twoColorLineSeries1.Color = OxyColors.Red;                  //this color for above the limit
                twoColorLineSeries1.Color2 = OxyColors.DarkBlue;            //this color for below the limit
                twoColorLineSeries1.StrokeThickness = 2;
            }
            else
            {
                lineSeries1.Color = OxyColors.DarkBlue;
                lineSeries1.StrokeThickness = 2;
            }

            var lineSeries2 = new LineSeries();                 //cadence
            lineSeries2.Color = OxyColors.ForestGreen;
            lineSeries2.StrokeThickness = 2;
            lineSeries2.DataFieldX = "Date";
            lineSeries2.DataFieldY = "Value";

            var lineSeries3 = new LineSeries();                 //speed
            lineSeries3.Color = OxyColors.PaleVioletRed;
            lineSeries3.StrokeThickness = 2;
            lineSeries3.DataFieldX = "Date";
            lineSeries3.DataFieldY = "Value";

            var lineSeries4 = new LineSeries();                 //altitude
            lineSeries4.Color = OxyColors.Violet;
            lineSeries4.StrokeThickness = 2;
            lineSeries4.DataFieldX = "Date";
            lineSeries4.DataFieldY = "Value";

            foreach (var q in DataService.Instance.trackData)
            {
                if (!flg)
                {
                    current1 = DateTime.Parse(q.timeStamp);

                    if (hrFlag)
                    {
                        twoColorLineSeries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), Convert.ToDouble(q.heartRate)));
                    }
                    else
                    {
                        lineSeries1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), Convert.ToDouble(q.heartRate)));
                    }
                    lineSeries2.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), Convert.ToDouble(q.cadence)));
                    lineSeries3.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), q.speed * (Properties.Settings.Default.IsMetric ? 1.0 : 2.23693629)));
                    lineSeries4.Points.Add(new DataPoint(DateTimeAxis.ToDouble(current1), q.altitude * (Properties.Settings.Default.IsMetric ? 1.0 : 3.28084)));
                }
                else
                {
                    scaledDistance = (q.distance * conversionFactor) / scaleFactor;

                    if (hrFlag)
                    {
                        twoColorLineSeries1.Points.Add(new DataPoint((q.distance * conversionFactor) / scaleFactor, Convert.ToDouble(q.heartRate)));
                    }
                    else
                    {
                        lineSeries1.Points.Add(new DataPoint((q.distance * conversionFactor) / scaleFactor, Convert.ToDouble(q.heartRate)));
                    }
                    lineSeries2.Points.Add(new DataPoint(scaledDistance, Convert.ToDouble(q.cadence)));
                    lineSeries3.Points.Add(new DataPoint(scaledDistance, q.speed * (Properties.Settings.Default.IsMetric ? 1.0 : 2.23693629)));
                    lineSeries4.Points.Add(new DataPoint(scaledDistance, q.altitude * (Properties.Settings.Default.IsMetric ? 1.0 : 3.28084)));
                }
            }

            var linearAxis1 = new LinearAxis();
            linearAxis1.EndPosition = 1;
            linearAxis1.StartPosition = 0.75;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.Title = "Heart Rate";
            linearAxis1.Unit = "BPM";
            linearAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.FontWeight = OxyPlot.FontWeights.Bold;
            linearAxis1.ExtraGridlines = new double[] { hrLimit };      //draws the gridline for the limit
            linearAxis1.ExtraGridlineColor = OxyPlot.OxyColors.Tomato;
            linearAxis1.ExtraGridlineStyle = LineStyle.Dot;
            linearAxis1.Key = "Heart Rate";
            twoColorLineSeries1.YAxisKey = "Heart Rate";

            myPlotModel1.Axes.Add(linearAxis1);

            if (hrFlag)
            {
                myPlotModel1.Series.Add(twoColorLineSeries1);
            }
            else
            {
                myPlotModel1.Series.Add(lineSeries1);
            }

            var linearAxis3 = new LinearAxis();
            linearAxis3.EndPosition = 0.75;
            linearAxis3.StartPosition = 0.50;
            linearAxis3.MajorGridlineStyle = LineStyle.Solid;
            linearAxis3.MinorGridlineStyle = LineStyle.Dot;
            linearAxis3.Title = "Speed";
            linearAxis3.Unit = (Properties.Settings.Default.IsMetric ? "M/S" : "MPH");
            linearAxis3.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis3.FontWeight = OxyPlot.FontWeights.Bold;
            linearAxis3.Key = "Speed";
            lineSeries3.YAxisKey = "Speed";

            myPlotModel1.Axes.Add(linearAxis3);
            myPlotModel1.Series.Add(lineSeries3);

            var linearAxis2 = new LinearAxis();
            linearAxis2.EndPosition = 0.50;
            linearAxis2.StartPosition = 0.25;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            linearAxis2.Title = "Cadence";
            linearAxis2.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.FontWeight = OxyPlot.FontWeights.Bold;
            linearAxis2.Key = "Cadence";
            lineSeries2.YAxisKey = "Cadence";

            myPlotModel1.Axes.Add(linearAxis2);
            myPlotModel1.Series.Add(lineSeries2);

            var linearAxis4 = new LinearAxis();
            linearAxis4.EndPosition = 0.25;
            linearAxis4.StartPosition = 0;
            linearAxis4.MajorGridlineStyle = LineStyle.Solid;
            linearAxis4.MinorGridlineStyle = LineStyle.Dot;
            linearAxis4.Title = "Altitude";
            linearAxis4.Unit = Properties.Settings.Default.IsMetric ? "Meters" : "Feet";
            linearAxis4.TitleFontWeight = OxyPlot.FontWeights.Bold;
            linearAxis4.FontWeight = OxyPlot.FontWeights.Bold;
            linearAxis4.Key = "Altitude";
            lineSeries4.YAxisKey = "Altitude";

            myPlotModel1.Axes.Add(linearAxis4);
            myPlotModel1.Series.Add(lineSeries4);

            if (!flg)
            {
                var datetimeAxis1 = new DateTimeAxis();
                datetimeAxis1.Position = AxisPosition.Bottom;
                datetimeAxis1.Title = axisTitle;
                datetimeAxis1.TitleFontWeight = OxyPlot.FontWeights.Bold;
                datetimeAxis1.MajorGridlineStyle = LineStyle.Solid;
                datetimeAxis1.MinorGridlineStyle = LineStyle.Dot;
                datetimeAxis1.FontWeight = OxyPlot.FontWeights.Bold;

                myPlotModel1.Axes.Add(datetimeAxis1);
            }
            else
            {
                var linearAxis5 = new LinearAxis();
                linearAxis5.Position = AxisPosition.Bottom;
                linearAxis5.Title = Properties.Settings.Default.IsMetric ? "Distance - Km" : "Distance - Miles";
                linearAxis5.TitleFontWeight = OxyPlot.FontWeights.Bold;
                linearAxis5.MajorGridlineStyle = LineStyle.Solid;
                linearAxis5.FontWeight = OxyPlot.FontWeights.Bold;

                myPlotModel1.Axes.Add(linearAxis5);
            }

            return myPlotModel1;
        }

        private CollectionView _myLapDisplay;
        public CollectionView myLapDisplay
        {
            get { return _myLapDisplay; }
            set
            {
                if (value != _myLapDisplay)
                {
                    _myLapDisplay = value;
                    OnPropertyChanged("myLapDisplay");
                }
            }
        }
      
        private CollectionView _myActivityDisplay;
        public CollectionView myActivityDisplay
        {
            get { return _myActivityDisplay; }
            set
            {
                if (value != _myActivityDisplay)
                {
                    _myActivityDisplay = value;
                    OnPropertyChanged("myActivityDisplay");
                }
            }
        }

        private bool LapsFilter(object item)
        {
            myLapRecord lap = item as myLapRecord;

            return lap.lpkey.Equals(FilterString, StringComparison.OrdinalIgnoreCase);
        }

        private bool StatsFilter(object item)
        {
            myActivityRecord act = item as myActivityRecord;

            return act.id.Contains(statFilterString);
        }

        private string _statFilterString;
        public string statFilterString
        {
            get { return _statFilterString; }
            set
            {
                if (value != _statFilterString)
                {
                    _statFilterString = value;
                    OnPropertyChanged("statFilterString");
                }
            }
        }

        private string _FilterString;
        public string FilterString
        {
            get { return _FilterString; }
            set
            {
                if (value != _FilterString)
                {
                    _FilterString = value;
                    OnPropertyChanged("FilterString");

                    if (myLapDisplay != null)
                    {
                        myLapDisplay.Refresh();
                    }
                }
            }
        }

        private bool _LapsByDate;
        public bool LapsByDate
        {
            get { return _LapsByDate; }
            set
            {
                if (value != _LapsByDate)
                {
                    _LapsByDate = value;
                    OnPropertyChanged("LapsByDate");
                }
            }
        }

        private Visibility _NavValue;
        public Visibility NavValue
        {
            get { return _NavValue; }
            set
            {
                if (value != _NavValue)
                {
                    _NavValue = value;
                    OnPropertyChanged("NavValue");
                }
            }
        }
      
        private ICommand goNext;
        public ICommand GoNext
        {
            get
            {
                if (goNext == null)
                {
                    goNext = new RelayCommand(p => findNext((string)p));
                }

                return goNext;
            }
        }

        private void findNext(string msg)
        {
            msgBoxobj.ShowNotification("Got Here -> " + msg);
        }

        private int _tabSelectedIndex;
        public int tabSelectedIndex
        {
            get { return _tabSelectedIndex; }
            set
            {
                if (value != _tabSelectedIndex)
                {
                    _tabSelectedIndex = value;
                    OnPropertyChanged("tabSelectedIndex");

                    recordCount = getRecCnt(value);     //get the record count of the selected tab

                    if (value == 5)
                    {
                        NavValue = Visibility.Visible;
                    }
                    else
                    {
                        NavValue = Visibility.Hidden;
                    }
                }
            }
        }

        private string _RecordDisplay;
        public string RecordDisplay
        {
            get { return _RecordDisplay; }
            set
            {
                if (value != _RecordDisplay)
                {
                    _RecordDisplay = value;
                    OnPropertyChanged("RecordDisplay");
                }
            }
        }

        //private ICommand resetPlot;
        //public ICommand ResetPlot
        //{
        //    get
        //    {
        //        if (resetPlot == null)
        //        {
        //            resetPlot = new RelayCommand(p => resetGraphs());
        //        }

        //        return resetPlot;
        //    }
        //}

        //private void resetGraphs()
        //{
        //    //myPlotModel.RefreshPlot(true);

        //    DataService.Instance.mycrashFlag = true;

        //    //var t100 = Task.Factory.StartNew(() => datalayer.getQuery<myFitRecord>("2013-08-10T09:58:34", "GetTrackData"));
        //    var t100 = Task.Factory.StartNew(() => datalayer.getQuery<myFitRecord>(137, "GetTrackData"));

        //    t100.ContinueWith((t) =>
        //    {
        //        AsyncObservableCollection<myFitRecord> mytrack = t.Result;

        //        msgBoxobj.ShowNotification("Count = " + mytrack.Count());

        //        DataService.Instance.mycrashFlag = false;

        //    }, CancellationToken.None,
        //        TaskContinuationOptions.NotOnFaulted,
        //        TaskScheduler.FromCurrentSynchronizationContext());

        //    t100.ContinueWith(t1 =>
        //    {
        //        var x = t1.Exception;
        //        //msgBoxobj.ShowNotification("onlyonfaulted = " + x.InnerException);
        //        msgBoxobj.ExtendedNotification(t1.Exception.InnerException.Message, "Task Factory", MessageBoxButton.OK, MessageBoxImage.Error);
        //        DataService.Instance.mycrashFlag = false;

        //    }, CancellationToken.None,
        //        TaskContinuationOptions.OnlyOnFaulted,
        //        TaskScheduler.FromCurrentSynchronizationContext());
        //}

        private ObservableCollection<myActivityRecord> selectedItems = new ObservableCollection<myActivityRecord>();    //for datagrid multiselection

        public ObservableCollection<myActivityRecord> SelectedItems
        {
            get { return selectedItems; }
        }

        public bool ViewReplaced { get; set; }

        private ICommand testSelect;
        public ICommand TestSelect
        {
            get
            {
                if (testSelect == null)
                {
                    testSelect = new RelayCommand(p => testdgselect());
                }
                return testSelect;
            }
        }

        private void testdgselect()     //to do: if laps by date, and more than one lap, add totals
        {
            TimeSpan mytemp;
            TimeSpan timr = TimeSpan.Zero;
            TimeSpan elap = TimeSpan.Zero;
            double totdist = 0.0;
            int totcal = 0;
            int totcycles = 0;

            if (ViewReplaced)
            {
                myLapDisplay = DataService.Instance.lapData.ToCollectionView();
                ViewReplaced = false;
            }

            if (LapsByDate)
            {
                switch (lastClickType)
                {
                    case "Activity":
                        if (SelectedActivityValue != null)
                        {
                            var act = SelectedActivityValue as myActivityRecord;
                            FilterString = act.timeStamp;
                        }
                        else
                        {
                            FilterString = DataService.Instance.activityData[0].timeStamp;
                        }
                        break;

                    case "Session":
                        if (SelectedSessionValue != null)
                        {
                            var ses = SelectedSessionValue as mySessionRecord;
                            FilterString = ses.myskey;
                        }
                        else
                        {
                            FilterString = DataService.Instance.sessionData[0].myskey;
                        }
                        break;
                }
            
                var cadenceCollction = (from x in DataService.Instance.trackData
                           where x.cadence > 0
                           select x.cadence).AsObservableCollection();      //select cadence for each trackdata record

                var cadenceCnt = cadenceCollction.Count();

                int sum1 = 0;
                foreach (var q in cadenceCollction)
                {
                    sum1 += q;
                }

                int averageCadence = sum1 / cadenceCnt;       //sum all the cadence records and divide by the count to get average cadence

                var hrtrateCollection = (from x in DataService.Instance.trackData
                                         select x.heartRate).AsObservableCollection();

                var hrtrateCnt = hrtrateCollection.Count();

                sum1 = 0;
                foreach (var q in hrtrateCollection)
                {
                    sum1 += q;
                }

                int averageHeartRate = sum1 / hrtrateCnt;

                //var tot = ((from x in DataService.Instance.trackData
                //            select x.cadence).Sum()) / DataService.Instance.trackData.Count();

                var filteredlaps = (from q in DataService.Instance.lapData
                                    where q.lpkey == FilterString
                                    select q).AsObservableCollection();

                ObservableCollection<mySessionRecord> filteredsession = (from q in DataService.Instance.sessionData
                                                                         where q.myskey == FilterString
                                                                         select q).AsObservableCollection();

                //mySessionRecord msr = (from q in DataService.Instance.sessionData
                //                       where q.myskey == FilterString
                //                       select q).FirstOrDefault();
                //var t0 = msr;

                if (filteredlaps.Count() > 1)
                {
                    ViewReplaced = true;

                    myLapRecord mlr = new myLapRecord();

                    if (filteredsession.IsNullOrEmpty())
                    {
                        foreach (var q in filteredlaps)     //get totals here
                        {
                            TimeSpan.TryParse(q.totalElaspedTime, out mytemp);
                            elap += mytemp;
                            TimeSpan.TryParse(q.totalTimerTime, out mytemp);
                            timr += mytemp;
                            totdist += q.totalDistance;
                            totcal += q.totalCalories;
                            totcycles += q.totalCycles;
                        }

                        var millesec = timr.TotalMilliseconds;
                        var totSecs = millesec / 1000.0;            //convert milliseconds to seconds
                        double averageSpeed = totdist / totSecs;    //meters per second

                        mlr.startTime = "Totals";
                        mlr.totalElaspedTime = elap.ToString(@"hh\:mm\:ss\.FF");
                        mlr.totalTimerTime = timr.ToString(@"hh\:mm\:ss\.FF");
                        mlr.totalDistance = totdist;
                        mlr.totalCalories = totcal;
                        mlr.maxCadence = filteredlaps.Max(x => x.maxCadence);
                        mlr.maxSpeed = filteredlaps.Max(x => x.maxSpeed);
                        mlr.maxHeartRate = filteredlaps.Max(x => x.maxHeartRate);
                        mlr.avgCadence = averageCadence;
                        mlr.totalCycles = totcycles;
                        mlr.avgSpeed = averageSpeed;
                        mlr.avgHeartRate = averageHeartRate;
                    }
                    else
                    {
                        mlr.startTime = "Totals:";
                        mlr.totalElaspedTime = filteredsession[0].totalElapsedTime;
                        mlr.totalTimerTime = filteredsession[0].totalTimerTime;
                        mlr.totalDistance = filteredsession[0].totalDistance;
                        mlr.totalCalories = filteredsession[0].totalCalories;
                        mlr.maxCadence = filteredsession[0].maxCadence;
                        mlr.maxSpeed = filteredsession[0].maxSpeed;
                        mlr.maxHeartRate = filteredsession[0].maxHeartRate;
                        mlr.avgCadence = filteredsession[0].avgCadence;
                        mlr.totalCycles = filteredsession[0].totalCycles;
                        mlr.avgSpeed = filteredsession[0].avgSpeed;
                        mlr.avgHeartRate = filteredsession[0].avgHeartRate;
                        //mlr.lapTrigger = filteredsession[0].trigger;
                        mlr.totalFatCalories = filteredsession[0].totalFatCalories;
                    }

                    ObservableCollection<myLapRecord> t7 = new ObservableCollection<myLapRecord>();

                    myLapDisplay.Filter = LapsFilter;
                    //myLapDisplay.Refresh();

                    t7 = myLapDisplay.toObs<myLapRecord>();
                    t7.Add(mlr);

                    myLapDisplay = t7.ToCollectionView();
                    OnPropertyChanged("myLapDisplay");
                    myLapDisplay.Refresh();
                }
                else
                {
                    myLapDisplay.Filter = LapsFilter;
                    myLapDisplay.Refresh();
                }
            }
            else
            {
                myLapDisplay.Filter = null;
                myLapDisplay.Refresh();
            }

            recordCount = getRecCnt(tabSelectedIndex);
        }

        private ICommand selectAllRows;
        public ICommand SelectAllRows
        {
            get
            {
                if (selectAllRows == null)
                {
                    selectAllRows = new RelayCommand(p => selectunselect());
                }

                return selectAllRows;
            }
        }

        private void selectunselect()
        {
            if (!DataService.Instance.activityData.IsNullOrEmpty())
            {
                foreach (var q in DataService.Instance.activityData)    //change for collectionview?
                {
                    q.myDeleteFlag = IsSelected;
                }

                canDelete = IsSelected;
            }

            //canDelete = IsSelected;
        }

        private ICommand myTestBinding;
        public ICommand MyTestBinding
        {
            get
            {
                if (myTestBinding == null)
                {
                    myTestBinding = new RelayCommand(p => checkTestBinding());
                }

                return myTestBinding;
            }
        }

        private void checkTestBinding()
        {
            canDelete = DataService.Instance.activityData.Any(x => x.myDeleteFlag == true);     //used for the canexecute

            IsSelected = DataService.Instance.activityData.All(x => x.myDeleteFlag == true);
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        private bool _canDelete;    //if any rows are checked - used for the canexecute of the delete button
        public bool canDelete
        {
            get { return _canDelete; }
            set
            {
                if (value != _canDelete)
                {
                    _canDelete = value;
                    OnPropertyChanged("canDelete");
                }
            }
        }

        private bool canExecuteCheckbox()
        {
            return canDelete;
        }

        private ICommand stuffToDelete;
        public ICommand StuffToDelete
        {
            get
            {
                if (stuffToDelete == null)
                {
                    stuffToDelete = new RelayCommand(p => deleteStuff(), p => canExecuteCheckbox());
                }

                return stuffToDelete;
            }
        }

        private void deleteStuff()
        {
            bool myFlag = false;
            string delmsg = string.Empty;

            var delnbr = (from x in DataService.Instance.activityData
                          where x.myDeleteFlag == true
                          select x.rowID).ToList();

            int cnt = delnbr.Count();

            if (IsSelected)
            {
                delmsg = "You are about to delete all " + DataService.Instance.activityData.Count() + " records";
            }
            else
            {
                delmsg = "You are about to delete " + cnt + ((cnt > 1) ? " items" : " item");
            }

            if (msgBoxobj.AskForConfirmation(delmsg))
            {
                MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = false });     //starts the circular progress bar

                Task.Factory.StartNew(() =>
                {
                    if (!datalayer.deleteRecords<bool>(delnbr))
                    {
                        myFlag = true;
                    }
                    
                }).ContinueWith(t =>
                    {
                        if (myFlag)
                        {
                            msgBoxobj.ShowNotification("Error in delete");
                        }

                        datalayer.loadObservableCollection(0);

                        //if (datalayer.loadObservableCollection(0))
                        //{
                            myActivityDisplay = DataService.Instance.activityData.ToCollectionView();
                            myLapDisplay = DataService.Instance.lapData.ToCollectionView();

                            myActivityDisplay.Refresh();
                            myLapDisplay.Refresh();

                            recordCount = getRecCnt(0);

                            if (!datalayer.compactDatabase())
                            {
                                msgBoxobj.ShowNotification("Compact Database Failed");
                            }
                        //}

                        canDelete = false;

                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = true });     //stops the circular progress bar

                    });
            }
        }

        private int _recordCount;
        public int recordCount
        {
            get { return _recordCount; }
            set
            {
                if (value != _recordCount)
                {
                    _recordCount = value;
                    OnPropertyChanged("recordCount");
                }
            }
        }

        private string _lastClickType;
        public string lastClickType
        {
            get { return _lastClickType; }
            set
            {
                if (value != _lastClickType)
                {
                    _lastClickType = value;
                    OnPropertyChanged("lastClickType");
                }
            }
        }

        private object _SelectedLapValue;
        public object SelectedLapValue
        {
            get { return _SelectedLapValue; }
            set
            {
                if (value != _SelectedLapValue)
                {
                    _SelectedLapValue = value;
                    OnPropertyChanged("SelectedLapValue");

                    var keyValue = value as myLapRecord;

                    if (keyValue != null)
                    {
                        MessageBus.Instance.Publish<ID>(new ID
                        {
                            UserId = keyValue.lpkey,
                            Sport = keyValue.sport
                        });

                        if (keyValue.lpkey != null)
                        {
                            var t6 = DataService.Instance.activityData.Where(x => x.timeStamp == keyValue.lpkey).FirstOrDefault();

                            int myrowid = datalayer.getUserData<int, string>(keyValue.lpkey, "GetRowId");

                            Task.Factory.StartNew(() => getTrackData(myrowid, t6.HasGpsData));   //getTrackData needs to run in a seperate thread
                        }
                    }
                }
            }
        }

        private object _SelectedActivityValue;
        public object SelectedActivityValue
        {
            get { return _SelectedActivityValue; }
            set
            {
                if (value != _SelectedActivityValue)
                {
                    if (value != null)
                    {
                        _SelectedActivityValue = value;
                        OnPropertyChanged("SelectedActivityValue");

                        lastClickType = "Activity";

                        var keyValue = value as myActivityRecord;

                        MessageBus.Instance.Publish<ID>(new ID
                        {
                            UserId = keyValue.timeStamp,
                            Sport = keyValue.sport
                        });

                        int myrowid = datalayer.getUserData<int,string>(keyValue.timeStamp, "GetRowId");

                        Task.Factory.StartNew(() => getTrackData(myrowid, keyValue.HasGpsData));     //getTrackData needs to run in a seperate thread

                        if (LapsByDate)
                        {
                            testdgselect();     //if you choose a new activity value, see if the laps need to be filtered
                        }
                    }
                }
            }
        }

        private object _SelectedSessionValue;
        public object SelectedSessionValue
        {
            get { return _SelectedSessionValue; }
            set
            {
                if (value != _SelectedSessionValue)
                {
                    _SelectedSessionValue = value;
                    OnPropertyChanged("SelectedSessionValue");

                    var keyValue = value as mySessionRecord;

                    MessageBus.Instance.Publish<ID>(new ID
                    {
                        UserId = keyValue.timeStamp,
                        Sport = keyValue.sport
                    });

                    lastClickType = "Session";

                    if (keyValue.myskey != null)
                    {
                        var t6 = DataService.Instance.activityData.Where(x => x.timeStamp == keyValue.myskey).FirstOrDefault();

                        int myrowid = datalayer.getUserData<int, string>(keyValue.myskey, "GetRowId");

                        Task.Factory.StartNew(() => getTrackData(myrowid, t6.HasGpsData));   //getTrackData needs to run in a seperate thread
                    }

                    if (LapsByDate)
                    {
                        testdgselect();     //if you choose a new activity value, see if the laps need to be filtered
                    }
                }
            }
        }
      

        private void getTrackData(int key, bool gpsflag)     //run in a seperate thread due to AsyncObservableCollection - see FileModel for class
        {
            DataService.Instance.trackData = datalayer.getQuery<myFitRecord>(key, "GetTrackData");

            //getHTTPRequest gr = new getHTTPRequest();

            //gr.testElevationRead(gpsflag, key);  //gpsflag is so I don't call Google Elevation when there is no GPS data`

            myPlotModel = threeChart(DataService.Instance.DistanceTimeFlag);

            DataService.Instance.STPTM = StopTimes.getStopTimes();

            if (DataService.Instance.STPTM.Count > 1)
            {
                DataService.Instance.TabHeaderStp = string.Format("Stop Time ({0})", (DataService.Instance.STPTM.Count - 1).ToString());
            }
            else
            {
                DataService.Instance.TabHeaderStp = "Stop Time";
            }

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsReady", FlagState = true });
        }

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set
            {
                if (value != _SelectedIndex)
                {
                    _SelectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        /// <summary>
        /// Return the number of records for a given tab
        /// </summary>
        /// <param name="selindx"></param>
        /// <returns></returns>
        private int getRecCnt(int selindx)
        {
            int mycnt = 0;
            int stcnt = 0;

            switch (selindx)
            {
                case 0:
                    if (DataService.Instance.activityData != null)
                    {
                        mycnt = DataService.Instance.activityData.Count();
                    }
                    break;

                case 1:
                    if (DataService.Instance.lapData != null)
                    {
                        //mycnt = myLapDisplay.Count;
                        mycnt = DataService.Instance.lapData.Count();
                    }
                    break;

                case 2:
                    if (DataService.Instance.trackData != null)
                    {
                        mycnt = DataService.Instance.trackData.Count();
                    }
                    break;

                case 3:
                    if (DataService.Instance.STPTM != null)
                    {
                        stcnt = DataService.Instance.STPTM.Count();
                        mycnt = (stcnt > 1) ? stcnt - 1 : 0;
                    }
                    break;

                default:
                    //msgBoxobj.ShowNotification("Error in Tab Selected Index: value = " + selindx);
                    break;
            }

            if (mycnt != 1)
            {
                RecordDisplay = "Records";
            }
            else
            {
                RecordDisplay = "Record";
            }

            return mycnt;
        }
    }
}
