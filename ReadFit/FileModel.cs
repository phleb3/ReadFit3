using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Data.SQLite;
using System.Collections.Concurrent;

namespace ReadFit
{
    namespace FileModel
    {
        public interface IDataAccessLayer
        {
            AsyncObservableCollection<T> getQuery<T>(int? input, string whatData);       //Reads the database and returns an ObservableCollection

            //T getUserData<T>(string input, string whatData);            //reads the database and returns a value

            T getUserData<T, TInput>(TInput input, string whatData);

            //bool insertProcessedFileName(string fileName);

            //T deleteRecords<T>(string Id, string tableName);

            T deleteRecords<T>(IEnumerable<int> deleteRecords);

            Dictionary<int, IEnumerable<myFitRecord>> GetTracksByLap(int PrimaryKey);

            bool tableMaintenence(string source);

            //bool insertActivityRecords(List<FileId> fileId, string timeStamp, string whatSport);

            //bool insertLapRecords(List<myLapRecord> laps, string timeStamp);

            //bool insertTrackRecords(List<myFitRecord> trackpoints, string timeStamp);

            //bool insertSessionRecords(List<mySessionRecord> mySession, string timeStamp);

            bool loadObservableCollection(int index);

            bool insertElevationRecords(List<ElevationUpdate> elevations, string timeStamp, int rowid);

            string generateKey(string filen);

            bool compactDatabase();

            bool insertFitRecords(DecodeRecords fitValues);

            int insertTest(string what);

            bool BackupDatabase(string filename);
        }

        public interface IFitRead
        {
            void getTrackData(Dynastream.Fit.MesgEventArgs e, int i, int j, myFitRecord mfr);

            void getLapData(Dynastream.Fit.MesgEventArgs e, int i, int j, myLapRecord mlr);

            void getEventData(Dynastream.Fit.MesgEventArgs e, int i, int j, List<string> test);

            void getActData(Dynastream.Fit.MesgEventArgs e, int i, int j, myActRecord myact);

            void getSessionData(Dynastream.Fit.MesgEventArgs e, int i, int j, mySessionRecord msr);

            void getFileIdData(Dynastream.Fit.MesgEventArgs e, int i, int j, FileId mfid);
        }

        public interface IXmlFunctions
        {
            void ReadFile(string filename);

            bool WriteFile(MyPassedData mypass);

            XDocument ReadInitialWkml(string myFilePath);
        }

        public class GetLayer
        {
            public static IFitRead giveMeAReadLayer()
            {
                return new ReadUtility();
            }

            public static IDataAccessLayer giveMeADataLayer()
            {
                return new DALlite();
            }

            public static IXmlFunctions giveMeAXmlLayer()
            {
                return new XmlDataAccess();
            }
        }

        public class MyPassedData
        {
            public string myUserId { get; set; }
            public string mySport { get; set; }
            public string myKmlFile { get; set; }
            public string myFQKmlFile { get; set; }
            public string mySPKmlFile { get; set; }
            public string myFQSPKmlFile { get; set; }
            public int myLineWidth { get; set; }
            public Dictionary<string, double> Mld { get; set; }
            public Dictionary<string, Color> Mycolordict { get; set; }
            public double Mysplitdistance { get; set; }
        }

        public class ReturnInitial
        {
            public Dictionary<string, double> Mld { get; set; }
            public Dictionary<string, Color> MyColorDict { get; set; }
            public ObservableCollection<string> MyColorNames { get; set; }
        }

        [Serializable]
        public class ElevationUpdate    //from Google Elevation API or Database
        {
            public double altitude { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double resolution { get; set; }
        }

        public class ElevationCorrection
        {
            public double uncorrected { get; set; }
            public double corrected { get; set; }
            public double bridgecorrected { get; set; }
            public string timeStamp { get; set; }
            public double distance { get; set; }
        }

        public class Person
        {
            public string Name { get; set; }
        }

        public class MyViewName
        {
            public string Name { get; set; }
        }

        public class myFit
        {
            public string Field { get; set; }
        }

        public class DecodeRecords
        {
            public string myFitKey { get; set; }
            public List<FileId> myFitAct { get; set; }
            public List<myLapRecord> myFitLap { get; set; }
            public List<myFitRecord> myFitTrack { get; set; }
            public List<mySessionRecord> myFitSession { get; set; }
            public string myFitSport { get; set; }
        }

        public class myFitRecord
        {
            public string timeStamp { get; set; }   //Trackpoints
            public double latitude { get; set; }
            public double longitude { get; set; }
            public double distance { get; set; }
            public double altitude { get; set; }
            public double speed { get; set; }
            public int heartRate { get; set; }
            public int cadence { get; set; }
            public int temperature { get; set; }
            public int sequence { get; set; }
        }

        public class myLapRecord
        {
            public int primarKey { get; set; }
            public string timeStamp { get; set; }       //laps
            public string startTime { get; set; }
            public double startPosLat { get; set; }
            public double startPosLong { get; set; }
            public double endPosLat { get; set; }
            public double endPosLong { get; set; }
            public string totalElaspedTime { get; set; }
            public string totalTimerTime { get; set; }
            public double totalDistance { get; set; }
            public int totalCycles { get; set; }
            public int totalCalories { get; set; }
            public int totalFatCalories { get; set; }
            public double avgSpeed { get; set; }
            public double maxSpeed { get; set; }
            public double avgPower { get; set; }
            public double maxPower { get; set; }
            public double totalAscent { get; set; }
            public double totalDesecent { get; set; }
            public int avgHeartRate { get; set; }
            public int maxHeartRate { get; set; }
            public int avgCadence { get; set; }
            public int maxCadence { get; set; }
            public int intensity { get; set; }
            public string lapTrigger { get; set; }
            public string sport { get; set; }
            public string lpkey { get; set; }
        }

        public class myActRecord
        {
            public string timeStamp { get; set; }
            public string totalTimerTime { get; set; }
            public int numSessions { get; set; }
            public string myType { get; set; }
            public string myEvent { get; set; }
            public string myEventType { get; set; }
        }

        public class mySessionRecord
        {
            public string timeStamp { get; set; }
            public string startTime { get; set; }
            public double startPositionLat { get; set; }
            public double startPositionLong { get; set; }
            public string totalElapsedTime { get; set; }
            public string totalTimerTime { get; set; }
            public double totalDistance { get; set; }
            public int totalCycles { get; set; }
            public double necLat { get; set; }
            public double necLong { get; set; }
            public double swcLat { get; set; }
            public double swcLong { get; set; }
            public int messageIndex { get; set; }
            public int totalCalories { get; set; }
            public int totalFatCalories { get; set; }
            public double avgSpeed { get; set; }
            public double maxSpeed { get; set; }
            public double avgPower { get; set; }
            public double maxPower { get; set; }
            public double totalAscent { get; set; }
            public double totalDescent { get; set; }
            public int firstLapIndex { get; set; }
            public int numLaps { get; set; }
            public string myEvent { get; set; }
            public string myEventType { get; set; }
            public string sport { get; set; }
            public string subSport { get; set; }
            public int avgHeartRate { get; set; }
            public int maxHeartRate { get; set; }
            public int avgCadence { get; set; }
            public int maxCadence { get; set; }
            public string myEventGroup { get; set; }
            public string trigger { get; set; }
            public string myskey { get; set; }
            public int rowId { get; set; }
        }

        public class FileId
        {
            public string serialNumber { get; set; }
            public string timeCreated { get; set; }
            public int manufacturer { get; set; }
            public int product { get; set; }
            public int number { get; set; }
            public int myType { get; set; }
            public bool HasGpsData { get; set; }
        }

        public class LatLong
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        public class myActivityRecord : ObservableObject
        {
            private bool _myDeleteFlag;
            public bool myDeleteFlag
            {
                get { return _myDeleteFlag; }
                set
                {
                    if (value != _myDeleteFlag)
                    {
                        _myDeleteFlag = value;
                        OnPropertyChanged("myDeleteFlag");
                    }
                }
            }
            public string sport { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string unitId { get; set; }
            public string productId { get; set; }
            public bool HasGpsData { get; set; }
            public string timeStamp { get; set; }
            public int rowID { get; set; }
        }

        public class myStopTime
        {
            public string Start { get; set; }
            public string End { get; set; }
            public string Duration { get; set; }
            public double? LatitudeDegrees { get; set; }
            public double? LongitudeDegrees { get; set; }
        }

        public class MyFlag
        {
            public string FlagName { get; set; }
            public bool FlagState { get; set; }
        }

        public class ID
        {
            public string UserId { get; set; }
            public string Sport { get; set; }
        }

        public class DeleteEnable
        {
            public bool DeleteFlag { get; set; }
        }

        public class r : ObservableObject
        {
            private bool _mySelectFlag;
            public bool mySelectFlag
            {
                get { return _mySelectFlag; }
                set
                {
                    if (value != _mySelectFlag)
                    {
                        _mySelectFlag = value;
                        OnPropertyChanged("mySelectFlag");
                    }
                }
            }
            public string myFileName { get; set; }
        }

        public class LatLng
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public LatLng()
            {
            }

            public LatLng(double lat, double lng)
            {
                this.Latitude = lat;
                this.Longitude = lng;
            }
        }

        public enum DistanceUnit { Miles, Kilometers };

        //public static class NumericExtensions
        //{

        //    public static double ToRadians(this double val)
        //    {
        //        return (Math.PI / 180) * val;
        //    }
        //}

        enum KMLAltitudeMode : byte     //for KML
        {
            absolute,
            clampToGround,
            relativeToGround,
            clampToSeaFloor,
            relativeToSeaFloor
        }

        public class ExtendedMsg
        {
            public string EMsg { get; set; }
            public string ECaption { get; set; }
            public MessageBoxButton Ebutton { get; set; }
            public MessageBoxImage EImage { get; set; }
        }

        //
        // http://www.thomaslevesque.com/2009/04/17/wpf-binding-to-an-asynchronous-collection/
        //
        public class AsyncObservableCollection<T> : ObservableCollection<T>
        {
            private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

            public AsyncObservableCollection()
            {
            }

            public AsyncObservableCollection(IEnumerable<T> list) : base(list)
            {
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (SynchronizationContext.Current == _synchronizationContext)
                {
                    // Execute the CollectionChanged event on the current thread
                    RaiseCollectionChanged(e);
                }
                else
                {
                    // Post the CollectionChanged event on the creator thread
                    _synchronizationContext.Post(RaiseCollectionChanged, e);
                }
            }

            private void RaiseCollectionChanged(object param)
            {
                // We are in the creator thread, call the base implementation directly
                base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
            }

            protected override void OnPropertyChanged(PropertyChangedEventArgs e)
            {
                if (SynchronizationContext.Current == _synchronizationContext)
                {
                    // Execute the PropertyChanged event on the current thread
                    RaisePropertyChanged(e);
                }
                else
                {
                    // Post the PropertyChanged event on the creator thread
                    _synchronizationContext.Post(RaisePropertyChanged, e);
                }   
            }

            private void RaisePropertyChanged(object param)
            {
                // We are in the creator thread, call the base implementation directly
                base.OnPropertyChanged((PropertyChangedEventArgs)param);
            }
        }

        //
        // http://blog.stephencleary.com/2010/06/reporting-progress-from-tasks.html
        //
        public sealed class ProgressReporter
        {
            /// <summary> 
            /// The underlying scheduler for the UI's synchronization context. 
            /// </summary> 
            private readonly TaskScheduler scheduler;

            /// <summary> 
            /// Initializes a new instance of the <see cref="ProgressReporter"/> class.
            /// This should be run on a UI thread. 
            /// </summary> 
            public ProgressReporter()
            {
                this.scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }

            /// <summary> 
            /// Gets the task scheduler which executes tasks on the UI thread. 
            /// </summary> 
            public TaskScheduler Scheduler
            {
                get { return this.scheduler; }
            }

            /// <summary> 
            /// Reports the progress to the UI thread. This method should be called from the task.
            /// Note that the progress update is asynchronous with respect to the reporting Task.
            /// For a synchronous progress update, wait on the returned <see cref="Task"/>. 
            /// </summary> 
            /// <param name="action">The action to perform in the context of the UI thread.
            /// Note that this action is run asynchronously on the UI thread.</param> 
            /// <returns>The task queued to the UI thread.</returns> 
            public Task ReportProgressAsync(Action action)
            {
                return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, this.scheduler);
            }

            /// <summary> 
            /// Reports the progress to the UI thread, and waits for the UI thread to process
            /// the update before returning. This method should be called from the task. 
            /// </summary> 
            /// <param name="action">The action to perform in the context of the UI thread.</param> 
            public void ReportProgress(Action action)
            {
                this.ReportProgressAsync(action).Wait();
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task finishes execution,
            /// whether it finishes with success, failiure, or cancellation. 
            /// </summary> 
            /// <param name="task">The task to monitor for completion.</param> 
            /// <param name="action">The action to take when the task has completed, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle completion. This is normally ignored.</returns> 
            public Task RegisterContinuation(Task task, Action action)
            {
                return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.None, this.scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task finishes execution,
            /// whether it finishes with success, failiure, or cancellation. 
            /// </summary> 
            /// <typeparam name="TResult">The type of the task result.</typeparam> 
            /// <param name="task">The task to monitor for completion.</param> 
            /// <param name="action">The action to take when the task has completed, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle completion. This is normally ignored.</returns> 
            public Task RegisterContinuation<TResult>(Task<TResult> task, Action action)
            {
                return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.None, this.scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task successfully finishes execution. 
            /// </summary> 
            /// <param name="task">The task to monitor for successful completion.</param> 
            /// <param name="action">The action to take when the task has successfully completed, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle successful completion. This is normally ignored.</returns> 
            public Task RegisterSucceededHandler(Task task, Action action)
            {
                return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task successfully finishes execution
            /// and returns a result. 
            /// </summary> 
            /// <typeparam name="TResult">The type of the task result.</typeparam> 
            /// <param name="task">The task to monitor for successful completion.</param> 
            /// <param name="action">The action to take when the task has successfully completed, in the context of the UI thread.
            /// The argument to the action is the return value of the task.</param> 
            /// <returns>The continuation created to handle successful completion. This is normally ignored.</returns> 
            public Task RegisterSucceededHandler<TResult>(Task<TResult> task, Action<TResult> action)
            {
                return task.ContinueWith(t => action(t.Result), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, this.Scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task becomes faulted. 
            /// </summary> 
            /// <param name="task">The task to monitor for faulting.</param> 
            /// <param name="action">The action to take when the task has faulted, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle faulting. This is normally ignored.</returns> 
            public Task RegisterFaultedHandler(Task task, Action<Exception> action)
            {
                return task.ContinueWith(t => action(t.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, this.Scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task becomes faulted. 
            /// </summary> 
            /// <typeparam name="TResult">The type of the task result.</typeparam> 
            /// <param name="task">The task to monitor for faulting.</param> 
            /// <param name="action">The action to take when the task has faulted, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle faulting. This is normally ignored.</returns> 
            public Task RegisterFaultedHandler<TResult>(Task<TResult> task, Action<Exception> action)
            {
                return task.ContinueWith(t => action(t.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, this.Scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task is cancelled. 
            /// </summary> 
            /// <param name="task">The task to monitor for cancellation.</param> 
            /// <param name="action">The action to take when the task is cancelled, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle cancellation. This is normally ignored.</returns> 
            public Task RegisterCancelledHandler(Task task, Action action)
            {
                return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, this.Scheduler);
            }

            /// <summary> 
            /// Registers a UI thread handler for when the specified task is cancelled. 
            /// </summary> 
            /// <typeparam name="TResult">The type of the task result.</typeparam> 
            /// <param name="task">The task to monitor for cancellation.</param> 
            /// <param name="action">The action to take when the task is cancelled, in the context of the UI thread.</param> 
            /// <returns>The continuation created to handle cancellation. This is normally ignored.</returns> 
            public Task RegisterCancelledHandler<TResult>(Task<TResult> task, Action action)
            {
                return task.ContinueWith(_ => action(), CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, this.Scheduler);
            }
        }

        public sealed class DataService : ObservableObject
        {
            private static readonly DataService instance = new DataService();

            static DataService() { }

            private DataService() { }

            public static DataService Instance
            {
                get
                {
                    return instance;
                }
            }

            //public ObservableCollection<Person> myActivityCollection { get; set; }

            //private ObservableCollection<string> _DbTableNames;
            //public ObservableCollection<string> DbTableNames
            //{
            //    get { return _DbTableNames; }
            //    set
            //    {
            //        if (value != _DbTableNames)
            //        {
            //            _DbTableNames = value;
            //            OnPropertyChanged("DbTableNames");
            //        }
            //    }
            //}

            private AsyncObservableCollection<myActivityRecord> _activityData;
            public AsyncObservableCollection<myActivityRecord> activityData
            {
                get { return _activityData; }
                set
                {
                    if (value != _activityData)
                    {
                        _activityData = value;
                        OnPropertyChanged("activityData");
                    }
                }
            }

            private AsyncObservableCollection<myLapRecord> _lapData;
            public AsyncObservableCollection<myLapRecord> lapData
            {
                get { return _lapData; }
                set
                {
                    if (value != _lapData)
                    {
                        _lapData = value;
                        OnPropertyChanged("lapData");
                    }
                }
            }

            private AsyncObservableCollection<myFitRecord> _trackData;
            public AsyncObservableCollection<myFitRecord> trackData
            {
                get { return _trackData; }
                set
                {
                    if (value != _trackData)
                    {
                        _trackData = value;
                        OnPropertyChanged("trackData");
                    }
                }
            }

            private AsyncObservableCollection<mySessionRecord> _sessionData;
            public AsyncObservableCollection<mySessionRecord> sessionData
            {
                get { return _sessionData; }
                set
                {
                    if (value != _sessionData)
                    {
                        _sessionData = value;
                        OnPropertyChanged("sessionData");
                    }
                }
            }
      

            private List<myFit> _myFitFile1;
            public List<myFit> myFitFile1
            {
                get { return _myFitFile1; }
                set
                {
                    if (value != _myFitFile1)
                    {
                        _myFitFile1 = value;
                        OnPropertyChanged("myFitFile1");
                    }
                }
            }

            private int _TrackSeq;
            public int TrackSeq
            {
                get { return _TrackSeq; }
                set
                {
                    if (value != _TrackSeq)
                    {
                        _TrackSeq = value;
                        OnPropertyChanged("TrackSeq");
                    }
                }
            }

            private List<myStopTime> _STPTM;
            public List<myStopTime> STPTM
            {
                get { return _STPTM; }
                set
                {
                    if (value != _STPTM)
                    {
                        _STPTM = value;
                        OnPropertyChanged("STPTM");
                    }
                }
            }

            private string _TabHeaderStp;
            public string TabHeaderStp
            {
                get { return _TabHeaderStp; }
                set
                {
                    if (value != _TabHeaderStp)
                    {
                        _TabHeaderStp = value;
                        OnPropertyChanged("TabHeaderStp");
                    }
                }
            }

            //private AsyncObservableCollection<ElevationCorrection> _myElevations;
            //public AsyncObservableCollection<ElevationCorrection> myElevations
            //{
            //    get { return _myElevations; }
            //    set
            //    {
            //        if (value != _myElevations)
            //        {
            //            _myElevations = value;
            //            OnPropertyChanged("myElevations");
            //        }
            //    }
            //}

            private bool _DistanceTimeFlag;
            public bool DistanceTimeFlag
            {
                get { return _DistanceTimeFlag; }
                set
                {
                    if (value != _DistanceTimeFlag)
                    {
                        _DistanceTimeFlag = value;
                        OnPropertyChanged("DistanceTimeFlag");

                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = value });
                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Chart", FlagState = value });
                        Properties.Settings.Default.DistanceTimeFlag = value;
                    }
                }
            }

            private bool _mycrashFlag;
            public bool mycrashFlag
            {
                get { return _mycrashFlag; }
                set
                {
                    if (value != _mycrashFlag)
                    {
                        _mycrashFlag = value;
                        OnPropertyChanged("mycrashFlag");
                    }
                }
            }

            public string getConnectionString()
            {
                return @"Data Source=FitDataBase.db;Version=3;foreign keys=true";
            }

            public BlockingCollection<DecodeRecords> myFitQueue { get; set; }
        }

        //public static class ObservableExtensions    //convert from a list<T> to an observable collection<T>
        //{
        //    public static ObservableCollection<T> ToObservableCollection<T>(this List<T> items)
        //    {
        //        ObservableCollection<T> collection = new ObservableCollection<T>();

        //        foreach (var item in items)
        //        {
        //            collection.Add(item);
        //        }

        //        return collection;
        //    }

        //    public static ObservableCollection<T> AsObservableCollection<T>(this IEnumerable<T> en)
        //    {
        //        return new ObservableCollection<T>(en);
        //    }

        //    public static CollectionView ToCollectionView<T>(this IEnumerable<T> items)
        //    {
        //        return (CollectionView)CollectionViewSource.GetDefaultView(items);
        //    }

        //    public static AsyncObservableCollection<T> AsAsyncObservableCollection<T>(this IEnumerable<T> items)
        //    {
        //        return new AsyncObservableCollection<T>(items);
        //    }
        //}

        //public static class UriExtensions   //http://stackoverflow.com/questions/7493144/how-to-write-a-wrapper-around-an-asynchronous-method
        //{
        //    public static void DownloadString(this Uri uri, Action<string> action)
        //    {
        //        if (uri == null)
        //        {
        //            throw new ArgumentNullException("uri");
        //        }

        //        if (action == null)
        //        {
        //            throw new ArgumentNullException("action");
        //        }

        //        var webclient = new WebClient();

        //        DownloadStringCompletedEventHandler handler = null;

        //        handler = (s, e) =>
        //        {
        //            var result = e.Result;
        //            webclient.DownloadStringCompleted -= handler;
        //            webclient.Dispose();
        //            action(result);
        //        };

        //        webclient.DownloadStringCompleted += handler;
        //        webclient.DownloadStringAsync(uri);
        //    }

        //    public static void DownloadString(this Uri uri, Action<string> action, Action<Exception> exception)
        //    {
        //        if (uri == null)
        //        {
        //            throw new ArgumentNullException("uri");
        //        }

        //        if (action == null)
        //        {
        //            throw new ArgumentNullException("action");
        //        }

        //        var webclient = (WebClient)null;

        //        Action<Action> catcher = body =>
        //        {
        //            try
        //            {
        //                body();
        //            }
        //            catch (Exception ex)
        //            {
        //                ex.Data["uri"] = uri;
        //                if (exception != null)
        //                {
        //                    exception(ex);
        //                }
        //            }
        //            finally
        //            {
        //                if (webclient != null)
        //                {
        //                    webclient.Dispose();
        //                }
        //            }
        //        };

        //        var handler = (DownloadStringCompletedEventHandler)null;
        //        handler = (s, e) =>
        //        {
        //            var result = (string)null;
        //            catcher(() =>
        //            {
        //                result = e.Result;
        //                webclient.DownloadStringCompleted -= handler;
        //            });
        //            action(result);
        //        };

        //        catcher(() =>
        //        {
        //            webclient = new WebClient();
        //            webclient.DownloadStringCompleted += handler;
        //            webclient.DownloadStringAsync(uri);
        //        });
        //    }
        //}

        //public static class IenumerableExtensions
        //{
        //    //http://blogs.thesitedoctor.co.uk/tim/Trackback.aspx?guid=6a9ca083-94b9-4ba3-b7e6-d29948179db9
        //    /// <summary>
        //    /// Simple method to chunk a source IEnumerable into smaller (more manageable) lists
        //    /// </summary>
        //    /// <param name="source">The large IEnumerable to split</param>
        //    /// <param name="chunkSize">The maximum number of items each subset should contain</param>
        //    /// <returns>An IEnumerable of the original source IEnumerable in bite size chunks</returns>
        //    public static IEnumerable<IEnumerable<TSource>> ChunkData<TSource>(this IEnumerable<TSource> source, int chunkSize)
        //    {
        //        for (int i = 0; i < source.Count(); i += chunkSize)
        //        {
        //            yield return source.Skip(i).Take(chunkSize);
        //        }
        //    }
        //}

        //http://refactorthis.wordpress.com/2010/11/24/mvvm-tips-how-to-show-a-message-box-or-dialog-in-mvvm-silverlight/
        public interface IMsgBoxService
        {
            void ShowNotification(string message);
            void ExtendedNotification(string message, string caption, MessageBoxButton button, MessageBoxImage icon);
            bool AskForConfirmation(string message);
        }

        public class MsgBoxService : IMsgBoxService
        {
            public void ShowNotification(string message)    //simple messageBox
            {
                MessageBox.Show(message, "Notification", MessageBoxButton.OK);
            }

            public void ExtendedNotification(string message, string caption, MessageBoxButton button, MessageBoxImage icon)     //extended messageBox
            {
                MessageBox.Show(message, caption, button, icon);
            }

            public bool AskForConfirmation(string message)      //confirming messageBox
            {
                MessageBoxResult result = MessageBox.Show(message, "Are you sure?", MessageBoxButton.OKCancel);     //returns true if OK is clicked

                return result.HasFlag(MessageBoxResult.OK);
            }
        }

        //http://www.thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
        public class SettingBindingExtension : Binding
        {
            public SettingBindingExtension()
            {
                Initialize();
            }

            public SettingBindingExtension(string path) : base(path)
            {
                Initialize();
            }

            private void Initialize()
            {
                this.Source = ReadFit.Properties.Settings.Default;
                this.Mode = BindingMode.TwoWay;
            }
        }

        //
        //http://stackoverflow.com/questions/5039778/public-event-eventhandler-someevent-delegate
        //
        //public static class eventExtensions
        //{
        //    public static void Raise<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
        //    {
        //        if (handler != null)
        //        {
        //            handler(sender, args);
        //        }
        //    }

        //    public static void Raise(this EventHandler handler, object sender, EventArgs args)
        //    {
        //        if (handler != null)
        //        {
        //            handler(sender, args);
        //        }
        //    }

        //    //extension method to raise event i.e. myEvent.Raise(this, args);
        //}

        //public delegate void dataAccessEventHandler(object sender, dataAccessEventArgs e);

        public class dataAccessClient
        {
            private IDataAccessLayer datalayer { get; set; }    //interface to read the database

            public dataAccessClient()
            {
                datalayer = GetLayer.giveMeADataLayer();    //get access to the interface
            }

            //public event dataAccessEventHandler getDataComplete;    //event handler

            public void getDataNow(int index)   //method to load the observable collections
            {
                dataAccessEventArgs args = new dataAccessEventArgs();

                try
                {
                    args.Result = datalayer.loadObservableCollection(index);     //load the data
                }
                catch (Exception ex)
                {
                    args.Error = ex;
                    args.Result = false;
                }

                args.Source = DataService.Instance.trackData as object;
                MessageBus.Instance.Publish<dataAccessEventArgs>(args);     //publish a message that the dataload method is complete

                //source = DataService.Instance.trackData as object;

                //raiseTheEvent(source, args);    //on completion raise the event
            }

            //protected virtual void raiseTheEvent(object source, dataAccessEventArgs e)
            //{
            //    if (getDataComplete != null)
            //    {
            //        getDataComplete(source, e);  //raise event
            //    }
            //}
        }

        public class dataAccessEventArgs : EventArgs
        {
            public dataAccessEventArgs()
            {
            }

            private bool _Cancelled;
            public bool Cancelled
            {
                get { return _Cancelled; }
                set { _Cancelled = value; }
            }

            private Exception _Error;
            public Exception Error
            {
                get { return _Error; }
                set { _Error = value; }
            }

            private object _Result;
            public object Result
            {
                get { return _Result; }
                set { _Result = value; }
            }

            private object _Source;
            public object Source
            {
                get { return _Source; }
                set { _Source = value; }
            }
        }

        public class getDataEventArgs : EventArgs
        {
            private string szMessage;

            public getDataEventArgs(string textMessage)
            {
                szMessage = textMessage;
            }

            public string Message
            {
                get { return szMessage; }
                set { szMessage = value; }
            }

            private string _Result;
            public string Result
            {
                get { return _Result; }
                set
                {
                    if (value != _Result)
                    {
                        _Result = value;
                    }
                }
            }
        }

        public class ReadXmlFile
        {
            public bool readXml(string filename)
            {
                XElement doc = XElement.Load(filename);

                XNamespace ns1 = "http://www.peaksware.com/PWX/1/0";

                var activity = (from q in doc.Descendants(ns1 + "workout")
                                select new
                                {
                                    fingerprint = q.Element(ns1 + "fingerprint").Value,
                                    sporttype = q.Element(ns1 + "sportType").Value,
                                    comment = q.Element(ns1 + "cmt").Value,
                                    id = q.Element(ns1 + "device").Attribute("id").Value,
                                    make = q.Element(ns1 + "device").Element(ns1 + "make").Value,
                                    model = q.Element(ns1 + "device").Element(ns1 + "model").Value,
                                    time = q.Element(ns1 + "time").Value

                                }).ToList();   //activity

                string savetime = activity[0].time;

                var summary = (from q in doc.Descendants(ns1 + "summarydata")
                               select new
                               {
                                   beg = q.Element(ns1 + "beginning").Value,
                                   dur = q.Element(ns1 + "duration").Value,
                                   durstopped = q.Element(ns1 + "durationstopped").Value,
                                   dist = q.Element(ns1 + "dist").Value

                               }).ToList();

                var summary1 = summary.FirstOrDefault();    //summary of all laps

                var segment = (from q in doc.Descendants(ns1 + "segment")
                               let lxname = q.Element(ns1 + "summarydata")
                               let lyname = lxname.Element(ns1 + "hr")
                               select new
                               {
                                   name = q.Element(ns1 + "name") != null ? q.Element(ns1 + "name").Value : "n/a",

                                   beg = (lxname != null) ? (lxname.Element(ns1 + "beginning") != null ? lxname.Element(ns1 + "beginning").Value : "n/a") : "---",

                                   dur = (lxname != null) ? (lxname.Element(ns1 + "duration") != null ? lxname.Element(ns1 + "duration").Value : "n/a") : "---",

                                   durs = (lxname != null) ? (lxname.Element(ns1 + "durationstopped") != null ?
                                        lxname.Element(ns1 + "durationstopped").Value : "n/a") : "---",

                                   maxhr = (lyname != null) ? (lyname.Attribute("max") != null ? lyname.Attribute("max").Value : "n/a") : "---",

                                   minhr = (lyname != null) ? (lyname.Attribute("min") != null ? lyname.Attribute("min").Value : "n/a") : "---",

                                   avghr = (lyname != null) ? (lyname.Attribute("avg") != null ? lyname.Attribute("avg").Value : "n/a") : "---",

                                   dist = (lxname != null) ? (lxname.Element(ns1 + "dist") != null ? lxname.Element(ns1 + "dist").Value : "n/a") : "---"

                               }).ToList();     //lap data

                var myevent = (from q in doc.Descendants(ns1 + "event")
                               select new
                               {
                                   offset = q.Element(ns1 + "timeoffset").Value,
                                   mytype = q.Element(ns1 + "type").Value

                               }).ToList();    //

                var mytrackpoint = (from q1 in doc.Descendants(ns1 + "sample")
                                    let lxname = q1.Element(ns1 + "extension")
                                    select new
                                    {
                                        offset = q1.Element(ns1 + "timeoffset") != null ? q1.Element(ns1 + "timeoffset").Value : "n/a",
                                        heartrate = q1.Element(ns1 + "hr") != null ? q1.Element(ns1 + "hr").Value : "0",
                                        speed = q1.Element(ns1 + "spd") != null ? q1.Element(ns1 + "spd").Value : "0.0",
                                        cadence = q1.Element(ns1 + "cad") != null ? q1.Element(ns1 + "cad").Value : "0",
                                        distance = q1.Element(ns1 + "dist").Value != null ? q1.Element(ns1 + "dist").Value : "0.0",
                                        lat = q1.Element(ns1 + "lat") != null ? q1.Element(ns1 + "lat").Value : "0.0",
                                        lon = q1.Element(ns1 + "lon") != null ? q1.Element(ns1 + "lon").Value : "0.0",
                                        alt = q1.Element(ns1 + "alt") != null ? q1.Element(ns1 + "alt").Value : "0.0",
                                        sensor = lxname.Element(ns1 + "sensor") != null ? lxname.Element(ns1 + "sensor").Value : "n/a"

                                    }).ToList();  //trackpoint

                int seq = 0;
                int t5 = 0;
                double t6 = 0.0;
                DateTime t1 = DateTime.ParseExact(savetime, "s", System.Globalization.CultureInfo.InvariantCulture);

                ObservableCollection<myFitRecord> test9 = new ObservableCollection<myFitRecord>();

                foreach (var q in mytrackpoint)
                {
                    t6 = Convert.ToDouble(q.offset);
                    t5 = Convert.ToInt32(t6);

                    myFitRecord mfr = new myFitRecord
                    {
                        timeStamp = (t1.AddSeconds(t5)).ToString(),
                        heartRate = Convert.ToInt32(q.heartrate),
                        speed = Convert.ToDouble(q.speed),
                        cadence = Convert.ToInt32(q.cadence),
                        distance = Convert.ToDouble(q.distance),
                        latitude = Convert.ToDouble(q.lat),
                        longitude = Convert.ToDouble(q.lon),
                        altitude = Convert.ToDouble(q.alt),
                        //temperature = Int32.MinValue,
                        sequence = seq++
                    };

                    test9.Add(mfr);
                }

                //int yn = mysamp.Where(x => x.sensor == "YES").Count();

                if (mytrackpoint.Count() == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public class StopTimes
        {
            public static List<myStopTime> getStopTimes()
            {
                System.DateTime hms;
                TimeSpan myTs1 = new TimeSpan(0, 0, 0);
                TimeSpan myTs2 = new TimeSpan(0, 0, 0);
                TimeSpan myDiff;
                TimeSpan myTotal = new TimeSpan(0, 0, 0);
                TimeSpan myNoon = new TimeSpan(12, 0, 0);

                int seq = 0;        //index of the track records
                int trkdtacnt;
                double saveLat = 0.0;
                double saveLong = 0.0;
                string saveStartTime;
                string saveEndTime;
                double saveDistance;

                int pointer;        //index of the query

                trkdtacnt = DataService.Instance.trackData.Count();

                List<myStopTime> stp = new List<myStopTime>();

                var tst1 = (from x in DataService.Instance.trackData
                            where (x.speed == 0.0)
                            //select new { x.timeStamp, x.latitude, x.longitude, x.sequence, x.distance }).ToList();
                            select x).ToList();

                if (!tst1.IsNullOrEmpty())
                {
                    pointer = 0;    //pointer for the tst1 list

                    if (DataService.Instance.trackData[0].sequence != 0)    //for laps by date
                    {
                        for (int i = 0; i < tst1.Count; i++)
                        {
                            var searchSeq = DataService.Instance.trackData.Where(x => x.sequence == tst1[i].sequence).FirstOrDefault();     //find the record
                            int repseq = DataService.Instance.trackData.IndexOf(searchSeq);     //find the index of the record
                            if (repseq != -1)
                            {
                                //seq = repseq;
                                tst1[i].sequence = repseq;  //adjust the sequence, this will be the number of records in the previous lap
                            }
                            else
                            {
                                MessageBox.Show("error in finding sequence");
                            }
                        }
                    }

                    do
                    {
                        seq = tst1[pointer].sequence;
                        saveLat = tst1[pointer].latitude;
                        saveLong = tst1[pointer].longitude;
                        saveStartTime = tst1[pointer].timeStamp;
                        saveDistance = tst1[pointer].distance;
                        
                        if (System.DateTime.TryParse(saveStartTime, out hms))
                        {
                            TimeSpan.TryParse(hms.ToLongTimeString().TrimEnd(' ', 'A', 'P', 'M'), out myTs1);   //timespan of the start
                        }

                        do
                        {
                            pointer++;
                            seq++;

                        } while (seq < trkdtacnt && DataService.Instance.trackData[seq].distance == saveDistance);
  
                        saveEndTime = DataService.Instance.trackData[seq - 1].timeStamp;

                        if (System.DateTime.TryParse(saveEndTime, out hms))
                        {
                            TimeSpan.TryParse(hms.ToLongTimeString().TrimEnd(' ', 'A', 'P', 'M'), out myTs2);   //timespan of the end
                        }

                        myDiff = myTs2 - myTs1;

                        if (myDiff < TimeSpan.Zero)
                        {
                            MessageBox.Show("Difference less than zero: " + myDiff.ToString());
                        }

                        if (myDiff != TimeSpan.Zero)    //add up times that are greater than zero
                        {
                            myTotal += myDiff;

                            stp.Add(new myStopTime
                            {
                                Start = saveStartTime,
                                End = saveEndTime,
                                LatitudeDegrees = saveLat,
                                LongitudeDegrees = saveLong,
                                Duration = myDiff.ToString()
                            });
                        }

                    } while (pointer < tst1.Count());
                }

                stp.Add(new myStopTime
                {
                    End = "Total Stopped Time:",
                    Duration = myTotal.ToString(),
                    LatitudeDegrees = null,
                    LongitudeDegrees = null
                });

                return stp;
            }
        }

        [Serializable]
        public class CustomException : Exception
        {
            public CustomException()
                : base() { }

            public CustomException(string message)
                : base(message) { }

            public CustomException(string format, params object[] args)
                : base(string.Format(format, args)) { }

            public CustomException(string message, Exception innerException)
                : base(message, innerException) { }

            public CustomException(string format, Exception innerException, params object[] args)
                : base(string.Format(format, args), innerException) { }

            protected CustomException(SerializationInfo info, StreamingContext context)
                : base(info, context) { }

            //http://www.codeproject.com/Tips/90646/Custom-exceptions-in-C-NET
        }

        public enum Formatter { Binary, Xml, Json }

        public class Serialization      //generic serialize/deserialize - optional encryption
        {
            public static void Serialize<T>(T obj, string path, Formatter formatter)
            {
                try
                {
                    switch (formatter)
                    {
                        case (Formatter.Binary):

                            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                            {
                                (new BinaryFormatter()).Serialize(fs, obj);
                            }

                            break;

                        case (Formatter.Xml):

                            var serializer = new XmlSerializer(typeof(T));

                            using (TextWriter textWriter = new StreamWriter(path))
                            {
                                serializer.Serialize(textWriter, obj);
                            }

                            break;

                        case (Formatter.Json):

                            DataContractJsonSerializer jserial = new DataContractJsonSerializer((typeof(T)));

                            using (FileStream fs = new FileStream(path, FileMode.Create))
                            {
                                using (var jsonw = JsonReaderWriterFactory.CreateJsonWriter(fs, Encoding.GetEncoding("utf-8")))
                                {
                                    jserial.WriteObject(jsonw, obj);
                                    jsonw.Flush();
                                }
                            }

                            break;

                        default:

                            throw new CustomException("Invalid Formatter option");
                    }
                }
                catch (SerializationException sX)
                {
                    var errMsg = String.Format("Unable to serialize {0} into file {1}", obj, path);

                    throw new CustomException(errMsg, sX);
                }
            }

            public static T DeSerialize<T>(string path, Formatter formatter) where T : class
            {
                if (!File.Exists(path))
                {
                    throw new CustomException("Invalid data file path");
                }

                try
                {
                    switch (formatter)
                    {
                        case (Formatter.Binary):

                            using (var strm = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                var fmt = new BinaryFormatter();

                                var obj = fmt.Deserialize(strm);

                                if (!(obj is T))
                                {
                                    throw new CustomException("Bad Data File");
                                }

                                return obj as T;
                            }

                        case (Formatter.Xml):

                            var serializer = new XmlSerializer(typeof(T));

                            using (TextReader rdr = new StreamReader(path))
                            {
                                return (T)serializer.Deserialize(rdr);
                            }

                        default:

                            throw new CustomException("Invalid Formatter option");
                    }
                }
                catch (SerializationException sX)
                {
                    var errMsg = String.Format("Unable to deserialize {0} from file {1}", typeof(T), path);

                    throw new CustomException(errMsg, sX);
                }
            }
        }

        public class TestThis
        {
            public IDataAccessLayer datalayer { get; set; }

            public ObservableCollection<r> FitFileSearch(string pattern, string value)
            {
                bool boolValue = Convert.ToBoolean(value);

                string processedFileKey;

                datalayer = GetLayer.giveMeADataLayer();

                var result = new ObservableCollection<r>();

                DriveInfo[] listdrives = DriveInfo.GetDrives();

                foreach (var dn in listdrives)
                {
                    if (dn.DriveType == DriveType.Removable)
                    {
                        var files = FindAccessableFiles(dn.Name, pattern, true);

                        var filessort = files.OrderBy(s => s);

                        foreach (var q in files)
                        {
                            processedFileKey = datalayer.generateKey(q);

                            if (!datalayer.getUserData<bool,string>(processedFileKey, "FileNameExist"))
                            {
                                result.Add(new r { mySelectFlag = boolValue, myFileName = q });
                            }
                        }
                    }
                }

                return result;
            }

            public List<string> SearchFiles(string pattern)
            {
                var result = new List<string>();

                DriveInfo[] listdrives = DriveInfo.GetDrives();

                foreach (var dn in listdrives)
                {
                    if (dn.DriveType == DriveType.Removable)
                    {
                        var files = FindAccessableFiles(dn.Name, pattern, true);

                        result.AddRange(files);
                    }
                }

                return result;
            }

            public List<string> SearchXmlFiles(string path, string pattern)
            {
                var result = new List<string>();

                var files = FindAccessableFiles(path, pattern, true);

                result.AddRange(files);

                return result;
            }

            private static IEnumerable<String> FindAccessableFiles(string path, string file_pattern, bool recurse)
            {
                //Console.WriteLine(path);
                var list = new List<string>();
                var required_extension = "fit";

                if (File.Exists(path))
                {
                    yield return path;
                    yield break;
                }

                if (!Directory.Exists(path))
                {
                    yield break;
                }

                if (null == file_pattern)
                {
                    file_pattern = "*." + required_extension;
                }

                var top_directory = new DirectoryInfo(path);

                // Enumerate the files just in the top directory.
                IEnumerator<FileInfo> files;

                try
                {
                    files = top_directory.EnumerateFiles(file_pattern).GetEnumerator();
                }
                catch (Exception ex)
                {
                    string t1 = ex.Message;
                    files = null;
                }

                while (true)
                {
                    FileInfo file = null;

                    try
                    {
                        if (files != null && files.MoveNext())
                        {
                            file = files.Current;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        continue;
                    }

                    yield return file.FullName;
                }

                if (!recurse)
                {
                    yield break;
                }

                IEnumerator<DirectoryInfo> dirs;

                try
                {
                    dirs = top_directory.EnumerateDirectories("*").GetEnumerator();
                }
                catch (Exception ex)
                {
                    string t1 = ex.Message;
                    dirs = null;
                }

                while (true)
                {
                    DirectoryInfo dir = null;

                    try
                    {
                        if (dirs != null && dirs.MoveNext())
                        {
                            dir = dirs.Current;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (PathTooLongException)
                    {
                        continue;
                    }

                    foreach (var subpath in FindAccessableFiles(dir.FullName, file_pattern, recurse))
                    {
                        yield return subpath;
                    }
                }
            }
        }

        //[DataContract]
        //public class Address
        //{
        //    [DataMember(Name = "addressLine", EmitDefaultValue = false)]
        //    public string AddressLine { get; set; }

        //    [DataMember(Name = "adminDistrict", EmitDefaultValue = false)]
        //    public string AdminDistrict { get; set; }

        //    [DataMember(Name = "adminDistrict2", EmitDefaultValue = false)]
        //    public string AdminDistrict2 { get; set; }

        //    [DataMember(Name = "countryRegion", EmitDefaultValue = false)]
        //    public string CountryRegion { get; set; }

        //    [DataMember(Name = "formattedAddress", EmitDefaultValue = false)]
        //    public string FormattedAddress { get; set; }

        //    [DataMember(Name = "locality", EmitDefaultValue = false)]
        //    public string Locality { get; set; }

        //    [DataMember(Name = "postalCode", EmitDefaultValue = false)]
        //    public string PostalCode { get; set; }

        //    [DataMember(Name = "neighborhood", EmitDefaultValue = false)]
        //    public string Neighborhood { get; set; }

        //    [DataMember(Name = "landmark", EmitDefaultValue = false)]
        //    public string Landmark { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class BirdseyeMetadata : ImageryMetadata
        //{
        //    [DataMember(Name = "orientation", EmitDefaultValue = false)]
        //    public double Orientation { get; set; }

        //    [DataMember(Name = "tilesX", EmitDefaultValue = false)]
        //    public int TilesX { get; set; }

        //    [DataMember(Name = "tilesY", EmitDefaultValue = false)]
        //    public int TilesY { get; set; }
        //}

        //[DataContract]
        //public class BoundingBox
        //{
        //    [DataMember(Name = "southLatitude", EmitDefaultValue = false)]
        //    public double SouthLatitude { get; set; }

        //    [DataMember(Name = "westLongitude", EmitDefaultValue = false)]
        //    public double WestLongitude { get; set; }

        //    [DataMember(Name = "northLatitude", EmitDefaultValue = false)]
        //    public double NorthLatitude { get; set; }

        //    [DataMember(Name = "eastLongitude", EmitDefaultValue = false)]
        //    public double EastLongitude { get; set; }
        //}

        //[DataContract]
        //public class Detail
        //{
        //    [DataMember(Name = "compassDegrees", EmitDefaultValue = false)]
        //    public int CompassDegrees { get; set; }

        //    [DataMember(Name = "maneuverType", EmitDefaultValue = false)]
        //    public string ManeuverType { get; set; }

        //    [DataMember(Name = "startPathIndices", EmitDefaultValue = false)]
        //    public int[] StartPathIndices { get; set; }

        //    [DataMember(Name = "endPathIndices", EmitDefaultValue = false)]
        //    public int[] EndPathIndices { get; set; }

        //    [DataMember(Name = "roadType", EmitDefaultValue = false)]
        //    public string RoadType { get; set; }

        //    [DataMember(Name = "locationCodes", EmitDefaultValue = false)]
        //    public string[] LocationCodes { get; set; }

        //    [DataMember(Name = "names", EmitDefaultValue = false)]
        //    public string[] Names { get; set; }

        //    [DataMember(Name = "mode", EmitDefaultValue = false)]
        //    public string Mode { get; set; }

        //    [DataMember(Name = "roadShieldRequestParameters", EmitDefaultValue = false)]
        //    public RoadShield roadShieldRequestParameters { get; set; }
        //}

        //[DataContract]
        //public class Generalization
        //{
        //    [DataMember(Name = "pathIndices", EmitDefaultValue = false)]
        //    public int[] PathIndices { get; set; }

        //    [DataMember(Name = "latLongTolerance", EmitDefaultValue = false)]
        //    public double LatLongTolerance { get; set; }
        //}

        //[DataContract]
        //public class Hint
        //{
        //    [DataMember(Name = "hintType", EmitDefaultValue = false)]
        //    public string HintType { get; set; }

        //    [DataMember(Name = "text", EmitDefaultValue = false)]
        //    public string Text { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //[KnownType(typeof(StaticMapMetadata))]
        //[KnownType(typeof(BirdseyeMetadata))]
        //public class ImageryMetadata : Resource
        //{
        //    [DataMember(Name = "imageHeight", EmitDefaultValue = false)]
        //    public string ImageHeight { get; set; }

        //    [DataMember(Name = "imageWidth", EmitDefaultValue = false)]
        //    public string ImageWidth { get; set; }

        //    [DataMember(Name = "imageUrl", EmitDefaultValue = false)]
        //    public string ImageUrl { get; set; }

        //    [DataMember(Name = "imageUrlSubdomains", EmitDefaultValue = false)]
        //    public string[] ImageUrlSubdomains { get; set; }

        //    [DataMember(Name = "vintageEnd", EmitDefaultValue = false)]
        //    public string VintageEnd { get; set; }

        //    [DataMember(Name = "vintageStart", EmitDefaultValue = false)]
        //    public string VintageStart { get; set; }

        //    [DataMember(Name = "zoomMax", EmitDefaultValue = false)]
        //    public int ZoomMax { get; set; }

        //    [DataMember(Name = "zoomMin", EmitDefaultValue = false)]
        //    public int ZoomMin { get; set; }
        //}

        //[DataContract]
        //public class Instruction
        //{
        //    [DataMember(Name = "maneuverType", EmitDefaultValue = false)]
        //    public string ManeuverType { get; set; }

        //    [DataMember(Name = "text", EmitDefaultValue = false)]
        //    public string Text { get; set; }
        //}

        //[DataContract]
        //public class ItineraryItem
        //{
        //    [DataMember(Name = "childItineraryItems", EmitDefaultValue = false)]
        //    public ItineraryItem ChildItineraryItems { get; set; }

        //    [DataMember(Name = "compassDirection", EmitDefaultValue = false)]
        //    public string CompassDirection { get; set; }

        //    [DataMember(Name = "details", EmitDefaultValue = false)]
        //    public Detail[] Details { get; set; }

        //    [DataMember(Name = "exit", EmitDefaultValue = false)]
        //    public string Exit { get; set; }

        //    [DataMember(Name = "hints", EmitDefaultValue = false)]
        //    public Hint[] Hints { get; set; }

        //    [DataMember(Name = "iconType", EmitDefaultValue = false)]
        //    public string IconType { get; set; }

        //    [DataMember(Name = "instruction", EmitDefaultValue = false)]
        //    public Instruction Instruction { get; set; }

        //    [DataMember(Name = "maneuverPoint", EmitDefaultValue = false)]
        //    public Point ManeuverPoint { get; set; }

        //    [DataMember(Name = "sideOfStreet", EmitDefaultValue = false)]
        //    public string SideOfStreet { get; set; }

        //    [DataMember(Name = "signs", EmitDefaultValue = false)]
        //    public string[] Signs { get; set; }

        //    [DataMember(Name = "time", EmitDefaultValue = false)]
        //    public string Time { get; set; }

        //    [DataMember(Name = "tollZone", EmitDefaultValue = false)]
        //    public string TollZone { get; set; }

        //    [DataMember(Name = "towardsRoadName", EmitDefaultValue = false)]
        //    public string TowardsRoadName { get; set; }

        //    [DataMember(Name = "transitLine", EmitDefaultValue = false)]
        //    public TransitLine TransitLine { get; set; }

        //    [DataMember(Name = "transitStopId", EmitDefaultValue = false)]
        //    public int TransitStopId { get; set; }

        //    [DataMember(Name = "transitTerminus", EmitDefaultValue = false)]
        //    public string TransitTerminus { get; set; }

        //    [DataMember(Name = "travelDistance", EmitDefaultValue = false)]
        //    public double TravelDistance { get; set; }

        //    [DataMember(Name = "travelDuration", EmitDefaultValue = false)]
        //    public double TravelDuration { get; set; }

        //    [DataMember(Name = "travelMode", EmitDefaultValue = false)]
        //    public string TravelMode { get; set; }

        //    [DataMember(Name = "warning", EmitDefaultValue = false)]
        //    public Warning[] Warning { get; set; }
        //}

        //[DataContract]
        //public class Line
        //{
        //    [DataMember(Name = "type", EmitDefaultValue = false)]
        //    public string Type { get; set; }

        //    [DataMember(Name = "coordinates", EmitDefaultValue = false)]
        //    public double[][] Coordinates { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class Location : Resource
        //{
        //    [DataMember(Name = "name", EmitDefaultValue = false)]
        //    public string Name { get; set; }

        //    [DataMember(Name = "point", EmitDefaultValue = false)]
        //    public Point Point { get; set; }

        //    [DataMember(Name = "entityType", EmitDefaultValue = false)]
        //    public string EntityType { get; set; }

        //    [DataMember(Name = "address", EmitDefaultValue = false)]
        //    public Address Address { get; set; }

        //    [DataMember(Name = "confidence", EmitDefaultValue = false)]
        //    public string Confidence { get; set; }

        //    [DataMember(Name = "matchCodes", EmitDefaultValue = false)]
        //    public string[] MatchCodes { get; set; }

        //    [DataMember(Name = "geocodePoints", EmitDefaultValue = false)]
        //    public Point[] GeocodePoints { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class PinInfo
        //{
        //    [DataMember(Name = "anchor", EmitDefaultValue = false)]
        //    public Pixel Anchor { get; set; }

        //    [DataMember(Name = "bottomRightOffset", EmitDefaultValue = false)]
        //    public Pixel BottomRightOffset { get; set; }

        //    [DataMember(Name = "topLeftOffset", EmitDefaultValue = false)]
        //    public Pixel TopLeftOffset { get; set; }

        //    [DataMember(Name = "point", EmitDefaultValue = false)]
        //    public Point Point { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class Pixel
        //{
        //    [DataMember(Name = "x", EmitDefaultValue = false)]
        //    public string X { get; set; }

        //    [DataMember(Name = "y", EmitDefaultValue = false)]
        //    public string Y { get; set; }
        //}

        //[DataContract]
        //public class Point : Shape
        //{
        //    [DataMember(Name = "type", EmitDefaultValue = false)]
        //    public string Type { get; set; }

        //    /// <summary>
        //    /// Latitude,Longitude
        //    /// </summary>
        //    [DataMember(Name = "coordinates", EmitDefaultValue = false)]
        //    public double[] Coordinates { get; set; }

        //    [DataMember(Name = "calculationMethod", EmitDefaultValue = false)]
        //    public string CalculationMethod { get; set; }

        //    [DataMember(Name = "usageTypes", EmitDefaultValue = false)]
        //    public string[] UsageTypes { get; set; }
        //}

        //[DataContract]
        //[KnownType(typeof(Location))]
        //[KnownType(typeof(Route))]
        //[KnownType(typeof(TrafficIncident))]
        //[KnownType(typeof(ImageryMetadata))]
        //[KnownType(typeof(ElevationData))]
        //[KnownType(typeof(SeaLevelData))]
        //[KnownType(typeof(CompressedPointList))]
        //public class Resource
        //{
        //    [DataMember(Name = "bbox", EmitDefaultValue = false)]
        //    public double[] BoundingBox { get; set; }

        //    [DataMember(Name = "__type", EmitDefaultValue = false)]
        //    public string Type { get; set; }
        //}

        //[DataContract]
        //public class ResourceSet
        //{
        //    [DataMember(Name = "estimatedTotal", EmitDefaultValue = false)]
        //    public long EstimatedTotal { get; set; }

        //    [DataMember(Name = "resources", EmitDefaultValue = false)]
        //    public Resource[] Resources { get; set; }
        //}

        //[DataContract]
        //public class Response
        //{
        //    [DataMember(Name = "copyright", EmitDefaultValue = false)]
        //    public string Copyright { get; set; }

        //    [DataMember(Name = "brandLogoUri", EmitDefaultValue = false)]
        //    public string BrandLogoUri { get; set; }

        //    [DataMember(Name = "statusCode", EmitDefaultValue = false)]
        //    public int StatusCode { get; set; }

        //    [DataMember(Name = "statusDescription", EmitDefaultValue = false)]
        //    public string StatusDescription { get; set; }

        //    [DataMember(Name = "authenticationResultCode", EmitDefaultValue = false)]
        //    public string AuthenticationResultCode { get; set; }

        //    [DataMember(Name = "errorDetails", EmitDefaultValue = false)]
        //    public string[] errorDetails { get; set; }

        //    [DataMember(Name = "traceId", EmitDefaultValue = false)]
        //    public string TraceId { get; set; }

        //    [DataMember(Name = "resourceSets", EmitDefaultValue = false)]
        //    public ResourceSet[] ResourceSets { get; set; }
        //}

        //[DataContract]
        //public class RoadShield
        //{
        //    [DataMember(Name = "bucket", EmitDefaultValue = false)]
        //    public int Bucket { get; set; }

        //    [DataMember(Name = "shields", EmitDefaultValue = false)]
        //    public Shield[] Shields { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class Route : Resource
        //{
        //    [DataMember(Name = "id", EmitDefaultValue = false)]
        //    public string Id { get; set; }

        //    [DataMember(Name = "distanceUnit", EmitDefaultValue = false)]
        //    public string DistanceUnit { get; set; }

        //    [DataMember(Name = "durationUnit", EmitDefaultValue = false)]
        //    public string DurationUnit { get; set; }

        //    [DataMember(Name = "travelDistance", EmitDefaultValue = false)]
        //    public double TravelDistance { get; set; }

        //    [DataMember(Name = "travelDuration", EmitDefaultValue = false)]
        //    public double TravelDuration { get; set; }

        //    [DataMember(Name = "routeLegs", EmitDefaultValue = false)]
        //    public RouteLeg[] RouteLegs { get; set; }

        //    [DataMember(Name = "routePath", EmitDefaultValue = false)]
        //    public RoutePath RoutePath { get; set; }
        //}

        //[DataContract]
        //public class RouteLeg
        //{
        //    [DataMember(Name = "travelDistance", EmitDefaultValue = false)]
        //    public double TravelDistance { get; set; }

        //    [DataMember(Name = "travelDuration", EmitDefaultValue = false)]
        //    public double TravelDuration { get; set; }

        //    [DataMember(Name = "actualStart", EmitDefaultValue = false)]
        //    public Point ActualStart { get; set; }

        //    [DataMember(Name = "actualEnd", EmitDefaultValue = false)]
        //    public Point ActualEnd { get; set; }

        //    [DataMember(Name = "startLocation", EmitDefaultValue = false)]
        //    public Location StartLocation { get; set; }

        //    [DataMember(Name = "endLocation", EmitDefaultValue = false)]
        //    public Location EndLocation { get; set; }

        //    [DataMember(Name = "itineraryItems", EmitDefaultValue = false)]
        //    public ItineraryItem[] ItineraryItems { get; set; }
        //}

        //[DataContract]
        //public class RoutePath
        //{
        //    [DataMember(Name = "line", EmitDefaultValue = false)]
        //    public Line Line { get; set; }

        //    [DataMember(Name = "generalizations", EmitDefaultValue = false)]
        //    public Generalization[] Generalizations { get; set; }
        //}

        //[DataContract]
        //[KnownType(typeof(Point))]
        //public class Shape
        //{
        //    [DataMember(Name = "boundingBox", EmitDefaultValue = false)]
        //    public double[] BoundingBox { get; set; }
        //}

        //[DataContract]
        //public class Shield
        //{
        //    [DataMember(Name = "labels", EmitDefaultValue = false)]
        //    public string[] Labels { get; set; }

        //    [DataMember(Name = "roadShieldType", EmitDefaultValue = false)]
        //    public int RoadShieldType { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class StaticMapMetadata : ImageryMetadata
        //{
        //    [DataMember(Name = "mapCenter", EmitDefaultValue = false)]
        //    public Point MapCenter { get; set; }

        //    [DataMember(Name = "pushpins", EmitDefaultValue = false)]
        //    public PinInfo[] Pushpins { get; set; }

        //    [DataMember(Name = "zoom", EmitDefaultValue = false)]
        //    public string Zoom { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class TrafficIncident : Resource
        //{
        //    [DataMember(Name = "point", EmitDefaultValue = false)]
        //    public Point Point { get; set; }

        //    [DataMember(Name = "congestion", EmitDefaultValue = false)]
        //    public string Congestion { get; set; }

        //    [DataMember(Name = "description", EmitDefaultValue = false)]
        //    public string Description { get; set; }

        //    [DataMember(Name = "detour", EmitDefaultValue = false)]
        //    public string Detour { get; set; }

        //    [DataMember(Name = "start", EmitDefaultValue = false)]
        //    public string Start { get; set; }

        //    [DataMember(Name = "end", EmitDefaultValue = false)]
        //    public string End { get; set; }

        //    [DataMember(Name = "incidentId", EmitDefaultValue = false)]
        //    public long IncidentId { get; set; }

        //    [DataMember(Name = "lane", EmitDefaultValue = false)]
        //    public string Lane { get; set; }

        //    [DataMember(Name = "lastModified", EmitDefaultValue = false)]
        //    public string LastModified { get; set; }

        //    [DataMember(Name = "roadClosed", EmitDefaultValue = false)]
        //    public bool RoadClosed { get; set; }

        //    [DataMember(Name = "severity", EmitDefaultValue = false)]
        //    public int Severity { get; set; }

        //    [DataMember(Name = "toPoint", EmitDefaultValue = false)]
        //    public Point ToPoint { get; set; }

        //    [DataMember(Name = "locationCodes", EmitDefaultValue = false)]
        //    public string[] LocationCodes { get; set; }

        //    //[DataMember(Name = "type", EmitDefaultValue = false)]
        //    //public int Type { get; set; }

        //    [DataMember(Name = "verified", EmitDefaultValue = false)]
        //    public bool Verified { get; set; }
        //}

        //[DataContract]
        //public class TransitLine
        //{
        //    [DataMember(Name = "verboseName", EmitDefaultValue = false)]
        //    public string verboseName { get; set; }

        //    [DataMember(Name = "abbreviatedName", EmitDefaultValue = false)]
        //    public string abbreviatedName { get; set; }

        //    [DataMember(Name = "agencyId", EmitDefaultValue = false)]
        //    public long AgencyId { get; set; }

        //    [DataMember(Name = "agencyName", EmitDefaultValue = false)]
        //    public string agencyName { get; set; }

        //    [DataMember(Name = "lineColor", EmitDefaultValue = false)]
        //    public long lineColor { get; set; }

        //    [DataMember(Name = "lineTextColor", EmitDefaultValue = false)]
        //    public long lineTextColor { get; set; }

        //    [DataMember(Name = "uri", EmitDefaultValue = false)]
        //    public string uri { get; set; }

        //    [DataMember(Name = "phoneNumber", EmitDefaultValue = false)]
        //    public string phoneNumber { get; set; }

        //    [DataMember(Name = "providerInfo", EmitDefaultValue = false)]
        //    public string providerInfo { get; set; }
        //}

        //[DataContract]
        //public class Warning
        //{
        //    [DataMember(Name = "warningType", EmitDefaultValue = false)]
        //    public string WarningType { get; set; }

        //    [DataMember(Name = "severity", EmitDefaultValue = false)]
        //    public string Severity { get; set; }

        //    [DataMember(Name = "text", EmitDefaultValue = false)]
        //    public string Text { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class CompressedPointList : Resource
        //{
        //    [DataMember(Name = "value", EmitDefaultValue = false)]
        //    public string Value { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class ElevationData : Resource
        //{
        //    [DataMember(Name = "elevations", EmitDefaultValue = false)]
        //    public int[] Elevations { get; set; }

        //    [DataMember(Name = "zoomLevel", EmitDefaultValue = false)]
        //    public int ZoomLevel { get; set; }
        //}

        //[DataContract(Namespace = "http://schemas.microsoft.com/search/local/ws/rest/v1")]
        //public class SeaLevelData : Resource
        //{
        //    [DataMember(Name = "offsets", EmitDefaultValue = false)]
        //    public int[] Offsets { get; set; }

        //    [DataMember(Name = "zoomLevel", EmitDefaultValue = false)]
        //    public int ZoomLevel { get; set; }
        //}

        //------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by a tool.
        //     Runtime Version:4.0.30319.18051
        //
        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------

        // Type created for JSON at <<root>>
        [System.Runtime.Serialization.DataContractAttribute()]
        public partial class GoogleResult
        {
            [System.Runtime.Serialization.DataMemberAttribute()]
            public Results[] results;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string status;
        }

        // Type created for JSON at <<root>> --> results
        [System.Runtime.Serialization.DataContractAttribute(Name = "results")]
        public partial class Results
        {
            [System.Runtime.Serialization.DataMemberAttribute()]
            public double elevation;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Location location;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double resolution;
        }

        // Type created for JSON at <<root>> --> location
        [System.Runtime.Serialization.DataContractAttribute(Name = "location")]
        public partial class Location
        {
            [System.Runtime.Serialization.DataMemberAttribute()]
            public double lat;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double lng;
        }
    }
}
