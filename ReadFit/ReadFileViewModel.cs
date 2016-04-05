using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.IO;
using ReadFit.FileModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dynastream.Fit;

namespace ReadFit
{
    class ReadFileViewModel : ObservableObject, IPageViewModel
    {
        public string Name
        {
            get { return "Read File"; }
        }

        MsgBoxService msgBoxobj;
        Stopwatch stopwatch { get; set; }
        List<string> recordType { get; set; }
        ObservableCollection<string> test { get; set; }
        public IFitRead readlayer { get; set; }
        public IDataAccessLayer datalayer { get; set; }

        private List<myFit> _myFitFile;
        public List<myFit> myFitFile
        {
            get { return _myFitFile; }
            set
            {
                if (value != _myFitFile)
                {
                    _myFitFile = value;
                    OnPropertyChanged("myFitFile");
                }
            }
        }

        List<myFitRecord> myRecords { get; set; }
        List<myLapRecord> myLaps { get; set; }
        List<string> myEvent { get; set; }
        List<myActRecord> myActivity { get; set; }
        List<mySessionRecord> mySession { get; set; }
        List<FileId> myFileIdRecord { get; set; }

        DecodeRecords myDecodeRecords { get; set; }

        public ReadFileViewModel()
        {
            okToImport = false;

            //readlayer = GetLayer.giveMeAReadLayer();

            datalayer = GetLayer.giveMeADataLayer();

            msgBoxobj = new MsgBoxService();

            FilterIndex = 2;

            OpenInitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            myDecodeRecords = null;

            myFitFile = new List<myFit>();
            myRecords = new List<myFitRecord>();
            myLaps = new List<myLapRecord>();
            myEvent = new List<string>();
            myActivity = new List<myActRecord>();
            mySession = new List<mySessionRecord>();
            myFileIdRecord = new List<FileId>();

            stopwatch = new Stopwatch();

            recordType = new List<string>();

            //int tc = datalayer.getUserData<int>(null, "TrackPointCount");

            //DataService.Instance.DbTableNames = datalayer.getQuery<string>(null, "GetTableNames");
            //DataService.Instance.DbTableNames = datalayer.getQuery<string>(null, "GetTableNames");

            pbMax = 100;
            pbMax1 = 100;
            ProgressValue = 0;
            ProgressValue1 = 0;
            pbVisible = false;
            pbVisible1 = false;

            IsSelected = false;

            DisplayList = new AsyncObservableCollection<r>();
            //DisplayList = new List<r>();

            //bool success = datalayer.deleteRecords<bool>(null, "ProcessedFiles");   //only for testing
            //success = datalayer.deleteRecords<bool>(null, "Activity");

            //MyRecsLocal.myFitAct = new List<FileId>();
            //MyRecsLocal.myFitAct.Add(new FileId { HasGpsData = false });

            //int ret = datalayer.insertTest("test124");

            //int myrowid = datalayer.getUserData<int>("2013-04-28T12:29:20", "GetRowId");
        }

        private bool _pbVisible;    //is the progress bar visible
        public bool pbVisible
        {
            get { return _pbVisible; }
            set
            {
                if (value != _pbVisible)
                {
                    _pbVisible = value;
                    OnPropertyChanged("pbVisible");
                }
            }
        }

        private bool _pbVisible1;
        public bool pbVisible1
        {
            get { return _pbVisible1; }
            set
            {
                if (value != _pbVisible1)
                {
                    _pbVisible1 = value;
                    OnPropertyChanged("pbVisible1");
                }
            }
        }
      

        private bool _IsSelected;   //for header checkbox
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

        private AsyncObservableCollection<r> _DisplayList;      //for the filenames to be imported
        public AsyncObservableCollection<r> DisplayList
        {
            get { return _DisplayList; }
            set
            {
                if (value != _DisplayList)
                {
                    _DisplayList = value;
                    OnPropertyChanged("DisplayList");
                }
            }
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
            if (!DisplayList.IsNullOrEmpty())
            {
                foreach (var q in DisplayList)
                {
                    q.mySelectFlag = IsSelected;
                }

                OnPropertyChanged("DisplayList");

                okToImport = IsSelected;
            }

            //okToImport = IsSelected;
        }

        private ICommand searchForData;
        public ICommand SearchForData
        {
            get
            {
                if (searchForData == null)
                {
                    searchForData = new RelayCommand(p => search((string)p));
                }

                return searchForData;
            }
        }

        private void search(string value)
        {
            bool boolValue = Convert.ToBoolean(value);

            Stopwatch stopwatch = new Stopwatch();

            TaskScheduler uiScheduler;

            if (boolValue)
            {
                uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            else
            {
                uiScheduler = TaskScheduler.Default;
            }

            Task.Factory.StartNew(() =>
                {
                    if (boolValue)
                    {
                        stopwatch.Start();
                    }

                    TestThis tstthis = new TestThis();

                    //result = tstthis.SearchFiles("2???-??-??-??-??-??.fit");

                    DisplayList = tstthis.FitFileSearch("2???-??-??-??-??-??.fit", value).AsAsyncObservableCollection();

                }).ContinueWith(t =>
                    {
                        if (boolValue)
                        {
                            stopwatch.Stop();

                            msgBoxobj.ShowNotification("Elapsed time: " + stopwatch.Elapsed);
                        }

                        if (DisplayList.Count() > 0)
                        {
                            okToImport = true;

                            IsSelected = DisplayList.All(x => x.mySelectFlag);
                        }
                        else
                        {
                            if (boolValue)
                            {
                                msgBoxobj.ShowNotification("Nothing to Import");
                            }
                        }

                    }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, uiScheduler).ContinueWith(z =>
                    {
                        msgBoxobj.ShowNotification("ERROR: " + z.Exception.Message);

                    }, TaskContinuationOptions.OnlyOnFaulted);

        }

        private bool okToImport { get; set; }

        private bool canExecute()
        {
            return okToImport;
        }

        private List<object> _myitems;
        public List<object> myitems
        {
            get { return _myitems; }
            set
            {
                if (value != _myitems)
                {
                    _myitems = value;
                    OnPropertyChanged("myitems");
                }
            }
        }

        private int _pbMax;
        public int pbMax
        {
            get { return _pbMax; }
            set
            {
                if (value != _pbMax)
                {
                    _pbMax = value;
                    OnPropertyChanged("pbMax");
                }
            }
        }

        private int _pbMax1;
        public int pbMax1
        {
            get { return _pbMax1; }
            set
            {
                if (value != _pbMax1)
                {
                    _pbMax1 = value;
                    OnPropertyChanged("pbMax1");
                }
            }
        }
      
        private ICommand testTaskCommand;
        public ICommand TestTaskCommand
        {
            get
            {
                if (testTaskCommand == null)
                {
                    testTaskCommand = new RelayCommand(p => testThisTask((int)p));
                }

                return testTaskCommand;
            }
        }

        private int _ProgressValue;
        public int ProgressValue
        {
            get { return _ProgressValue; }
            set
            {
                if (value != _ProgressValue)
                {
                    _ProgressValue = value;
                    OnPropertyChanged("ProgressValue");
                }
            }
        }

        private int _ProgressValue1;
        public int ProgressValue1
        {
            get { return _ProgressValue1; }
            set
            {
                if (value != _ProgressValue1)
                {
                    _ProgressValue1 = value;
                    OnPropertyChanged("ProgressValue1");
                }
            }
        }
      

        private static int testThisInt;
        private static int testThisInt1;

        private void testThisTask(int parm)
        {
            IsSelected = DisplayList.All(x => x.mySelectFlag == true);

            OnPropertyChanged("DisplayList");

            if (DisplayList.All(x => x.mySelectFlag == false))
            {
                okToImport = false;
            }
            else
            {
                okToImport = true;
            }
        }

        //private void testThisTask()
        //{
        //    ProgressValue = 0;
        //    CancellationTokenSource cts = new CancellationTokenSource();
        //    var cancellationToken = cts.Token;
        //    var progressReporter = new ProgressReporter();

        //    var task = Task.Factory.StartNew(() =>
        //    {
        //      for (int i = 0; i != 100; ++i)
        //      {
        //        // Check for cancellation 
        //        cancellationToken.ThrowIfCancellationRequested();
 
                
 
        //        // Report progress of the work. 
        //        progressReporter.ReportProgress(() =>
        //        {
        //          // Note: code passed to "ReportProgress" can access UI elements freely. 
        //          //this.progressBar.Value = i;
        //            //ProgressValue = i;
        //            ProgressValue = testThisInt;
        //        });
        //      }
 
        //      // After all that work, cause the error if requested.
        //      //if (causeError)
        //      //{
        //      //  throw new InvalidOperationException("Oops...");
        //      //}
 
        //      // The answer, at last! 
        //      return 42;
        //    }, cancellationToken);
 
        //    // ProgressReporter can be used to report successful completion,
        //    //  cancelation, or failure to the UI thread. 
        //        progressReporter.RegisterContinuation(task, () =>
        //        {
        //          // Update UI to reflect completion.
        //          //this.progressBar.Value = 100;
        //            ProgressValue = 100;
 
        //          // Display results.
        //          if (task.Exception != null)
        //          {
        //            msgBoxobj.ShowNotification("Background task error: " + task.Exception.ToString());
        //          }
        //          else if (task.IsCanceled)
        //          {
        //            msgBoxobj.ShowNotification("Background task cancelled");
        //          }
        //          else
        //          {
        //            msgBoxobj.ShowNotification("Background task result: " + task.Result + " testthisint = " + testThisInt);
        //          }

        //          ProgressValue = 0;
        //          // Reset UI.
        //          //this.TaskIsComplete();
        //        });
        //}

        private ICommand importDataCommand;
        public ICommand ImportDataCommand
        {
            get
            {
                if (importDataCommand == null)
                {
                    importDataCommand = new RelayCommand(p => dataImport(), p => canExecute());
                }

                return importDataCommand;
            }
        }

        private void dataImport()
        {
            var actualwork = (from x in DisplayList
                              where x.mySelectFlag == true
                              select x).ToList();

            pbVisible = true;   //read progress
            pbVisible1 = true;  //write progress

            pbMax = actualwork.Count();
            pbMax1 = pbMax;
            OnPropertyChanged("pbMax");
            OnPropertyChanged("pbMax1");

            //DecodeRecords mydcrecs;

            bool myFlag = true;
            string result = string.Empty;
            var progressReporter = new ProgressReporter();
            var progressReporter1 = new ProgressReporter();

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = false });     //publish message to show circular progress bar

            Stopwatch mystop = new Stopwatch();

            //http://stackoverflow.com/questions/12712117/how-to-post-results-of-parallel-foreach-to-a-queue-which-is-continually-read-in
            //Producer-Consumer pattern

            if (DataService.Instance.myFitQueue == null)
            {
                DataService.Instance.myFitQueue = new System.Collections.Concurrent.BlockingCollection<DecodeRecords>();
            }

            Action myfill1 = () =>
                {
                    mystop.Start();

                    ReadFitParallel rfp = new ReadFitParallel();

                    foreach (var q in actualwork)
                    {
                        if ((myDecodeRecords = rfp.processFileList(q.myFileName)) == null)
                        {
                            //Debug.WriteLine("Foreach - null - " + myDecodeRecords.myFitKey);
                            myFlag = false;
                            break;
                        }
                        else
                        {
                            //Debug.WriteLine("Foreach - add - " + myDecodeRecords.myFitKey);
                            DataService.Instance.myFitQueue.Add(myDecodeRecords);               //add to the blocking collection
                        }

                        Interlocked.Increment(ref testThisInt);     //thread safe counter increment

                        progressReporter.ReportProgress(() =>
                        {
                            ProgressValue = testThisInt;    //report the counter value back to the UI thread - see FileModel.cs for the ReportProgress class
                        });
                    }

                    DataService.Instance.myFitQueue.CompleteAdding();
                };

            //Action myfill = () =>
            //    {
            //        mystop.Start();

            //        Parallel.ForEach(actualwork, (fn1, loopstate) =>
            //        {
            //            ReadFitParallel rfp = new ReadFitParallel();        //parallel read on a background thread - NEEDS WORK

            //            try
            //            {
            //                myDecodeRecords = rfp.processFileList(fn1.myFileName);
            //            }
            //            catch (Exception ex)
            //            {
            //                msgBoxobj.ShowNotification("error in parallel - " + ex.Message);
            //            }

            //            if (myDecodeRecords == null)
            //            {
            //                Debug.WriteLine("Parllel - null - " + myDecodeRecords.myFitKey);
            //                myFlag = false;
            //                loopstate.Stop();
            //            }
            //            else
            //            {
            //                Debug.WriteLine("Parallel - add - " + myDecodeRecords.myFitKey);
            //                DataService.Instance.myFitQueue.Add(myDecodeRecords);               //add to the blocking collection
            //            }

            //            Interlocked.Increment(ref testThisInt);     //thread safe counter increment

            //            progressReporter.ReportProgress(() =>
            //            {
            //                ProgressValue = testThisInt;    //report the counter value back to the UI thread - see FileModel.cs for the ReportProgress class
            //            });
            //        });

            //        DataService.Instance.myFitQueue.CompleteAdding();
            //    };

            Action myload = () =>
                {
                    foreach (var q in DataService.Instance.myFitQueue.GetConsumingEnumerable())
                    {
                        //Debug.WriteLine("in load " + q.myFitKey);
                        
                        if (q.myFitAct.Count == 1)
                        {
                            q.myFitAct[0].HasGpsData = q.myFitTrack.Any(n => n.latitude != 0.0 && n.longitude != 0.0);
                        }
                        else
                        {
                            myFlag = false;
                            msgBoxobj.ShowNotification("Error: more than one occurance!");
                        }

                        if (!datalayer.insertFitRecords(q))     //read the blocking collection
                        {
                            myFlag = false;
                            break;
                        }

                        Interlocked.Increment(ref testThisInt1);

                        progressReporter1.ReportProgress(() =>
                            {
                                ProgressValue1 = testThisInt1;
                            });
                    }

                    mystop.Stop();

                    endOfImport(myFlag, mystop.Elapsed.ToString());
                };

            Task.Factory.StartNew(() =>
                {
                    Parallel.Invoke(myfill1, myload);
                });

        //    Task.Factory.StartNew(() =>
        //        {
        //            Stopwatch stopwatch = new Stopwatch();

        //            stopwatch.Start();

        //            Parallel.ForEach(actualwork, (fn1, loopstate) =>
        //                {
        //                    ReadFitParallel rfp = new ReadFitParallel();        //parallel read on a background thread

        //                    if (!rfp.processFileList(fn1.myFileName))
        //                    {
        //                        myFlag = false;
        //                        loopstate.Stop();
        //                    }

        //                    Interlocked.Increment(ref testThisInt);     //thread safe counter increment

        //                    progressReporter.ReportProgress(() =>
        //                    {
        //                        ProgressValue = testThisInt;    //report the counter value back to the UI thread - see FileModel.cs for the ReportProgress class
        //                    });
        //                });

        //            stopwatch.Stop();
        //            result = stopwatch.Elapsed.ToString();

        //        }).ContinueWith(t => endOfImport(myFlag, result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void endOfImport(bool flag, string value)
        {
            bool myretflg = false;

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = true });  //publish message to hide circular progress bar

            if (flag)
            {
                try
                {
                    Task.Factory.StartNew(() => myretflg = datalayer.loadObservableCollection(0)).ContinueWith(t =>
                    {
                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "LoadCollections", FlagState = true });     //publish message to refresh
                        //collectionviews
                        msgBoxobj.ShowNotification("Done - Elapsed Time = " + value);
                        okToImport = false;
                        ProgressValue = 0;
                        ProgressValue1 = 0;
                        pbVisible = false;
                        pbVisible1 = false;

                        //DataService.Instance.trackData = datalayer.getQuery<myFitRecord>(DataService.Instance.activityData.FirstOrDefault().timeStamp, "GetTrackData");

                        MessageBus.Instance.Publish<ID>(new ID
                        {
                            UserId = DataService.Instance.activityData.FirstOrDefault().timeStamp,
                            Sport = DataService.Instance.activityData.FirstOrDefault().sport
                        });

                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsReady", FlagState = true });

                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "UpdateRecordCount" });     //update new record count
                    });

                    //}, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    msgBoxobj.ShowNotification("error in end - " + ex.Message);
                }
            }
            else
            {
                msgBoxobj.ShowNotification("Import Failed");
                pbVisible = false;
                pbVisible1 = false;
            }

            //if (msgBoxobj.AskForConfirmation("Do you want to search for files again?"))
            //{
                search("false");
            //}
        }

        private ICommand openFileCommand;
        public ICommand OpenFileCommand
        {
            get
            {
                if (openFileCommand == null)
                {
                    openFileCommand = new RelayCommand(p => processFile((string)p));
                }

                return openFileCommand;
            }
        }

        private void processFile(string openFileName)
        {
            FileName = openFileName;

            string processedFileKey = generateKey(FileName);

            if (!datalayer.getUserData<bool, string>(processedFileKey, "FileNameExist"))
            {
                ReadFitParallel rfp = new ReadFitParallel();

                if ((myDecodeRecords = rfp.processFileList(openFileName)) == null)
                {
                    msgBoxobj.ShowNotification("Import Failed");
                }
                else
                {
                    bool t5 = datalayer.insertFitRecords(myDecodeRecords);

                    Task.Factory.StartNew(() => datalayer.loadObservableCollection(0)).ContinueWith(t =>
                        {
                            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "LoadCollections", FlagState = true }); //refresh collection view

                            msgBoxobj.ShowNotification("File was read successfully");

                            MessageBus.Instance.Publish<ID>(new ID
                            {
                                UserId = DataService.Instance.activityData.FirstOrDefault().timeStamp,
                                Sport = DataService.Instance.activityData.FirstOrDefault().sport
                            });

                            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsReady", FlagState = true });

                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            else
            {
                msgBoxobj.ShowNotification("File was aready read");
            }
        }

        private string generateKey(string filen)
        {
            string gn = Path.GetFileNameWithoutExtension(filen);

            int[] test1 = gn.Split('-').Select(n => Convert.ToInt32(n)).ToArray();

            System.DateTime dt = new System.DateTime(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5]);

            return dt.ToString("s");
        }

        private string _FileName;
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (value != _FileName)
                {
                    _FileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        private string _FilePath;
        public string FilePath
        {
            get { return _FilePath; }
            set
            {
                if (value != _FilePath)
                {
                    _FilePath = value;
                    OnPropertyChanged("FilePath");
                }
            }
        }

        private string fileFilters;
        public string FileFilters
        {
            get
            {
                if (fileFilters == null)
                {
                    fileFilters = "FIT files (*.fit)|*.fit";
                }

                return fileFilters;
            }
        }

        private int _FilterIndex;
        public int FilterIndex
        {
            get { return _FilterIndex; }
            set
            {
                if (value != _FilterIndex)
                {
                    _FilterIndex = value;
                    OnPropertyChanged("FilterIndex");
                }
            }
        }

        private string _OpenInitialDirectory;
        public string OpenInitialDirectory
        {
            get { return _OpenInitialDirectory; }
            set
            {
                if (value != _OpenInitialDirectory)
                {
                    _OpenInitialDirectory = value;
                    OnPropertyChanged("OpenInitialDirectory");
                }
            }
        }

        private ICommand testFitRead;
        public ICommand TestFitRead
        {
            get
            {
                if (testFitRead == null)
                {
                    testFitRead = new RelayCommand(p => demoread());
                }

                return testFitRead;
            }
        }

        private void demoread()
        {
            //msgBoxobj.ShowNotification("Got Here");

            string fileName = @"C:\Users\mark\Documents\2013-08-10-09-58-34.fit";
            string result;

            FileStream fitSource = new FileStream(fileName, FileMode.Open);

            Decode decodeDemo = new Decode();

            MesgBroadcaster mesgBroadcaster = new MesgBroadcaster();

            // Connect the Broadcaster to our event (message) source (in this case the Decoder)
            decodeDemo.MesgEvent += mesgBroadcaster.OnMesg;
            decodeDemo.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;

            // Subscribe to message events of interest by connecting to the Broadcaster
            mesgBroadcaster.MesgEvent += new MesgEventHandler(OnMesg);
            mesgBroadcaster.MesgDefinitionEvent += new MesgDefinitionEventHandler(OnMesgDefn);

            mesgBroadcaster.FileIdMesgEvent += new MesgEventHandler(OnFileIDMesg);
            mesgBroadcaster.UserProfileMesgEvent += new MesgEventHandler(OnUserProfileMesg);

            bool status = decodeDemo.IsFIT(fitSource);

            status &= decodeDemo.CheckIntegrity(fitSource);

            // Process the file
            if (status == true)
            {
                result = string.Format("Decoding...");
                myFitFile.Add(new myFit { Field = result });

                decodeDemo.Read(fitSource);

                result = string.Format("Decoded FIT file {0}", fileName);
                myFitFile.Add(new myFit { Field = result });
            }
            else
            {
                result = string.Format("Integrity Check Failed {0}", fileName);
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("Attempting to decode...");
                myFitFile.Add(new myFit { Field = result });

                decodeDemo.Read(fitSource);
            }

            fitSource.Close();

            msgBoxobj.ShowNotification("End of demoread");
        }

        // Client implements their handlers of interest and subscribes to MesgBroadcaster events
        private void OnMesgDefn(object sender, MesgDefinitionEventArgs e)
        {
            string result;

            result = string.Format("OnMesgDef: Received Defn for local message #{0}, it has {1} fields", e.mesgDef.LocalMesgNum, e.mesgDef.NumFields);
            myFitFile.Add(new myFit { Field = result });
        }

        private void OnMesg(object sender, MesgEventArgs e)
        {
            string result;

            result = string.Format("OnMesg: Received Mesg with global ID#{0}, its name is {1}", e.mesg.Num, e.mesg.Name);
            myFitFile.Add(new myFit { Field = result });

            for (byte i = 0; i < e.mesg.GetNumFields(); i++)
            {
                for (int j = 0; j < e.mesg.fields[i].values.Count; j++)
                {
                    result = string.Format("\tField{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    if (e.mesg.Num == 20)
                    {
                        if (result.Contains("Altitude"))
                        {
                            myFitFile.Add(new myFit { Field = result });
                        }
                    }
                }
            }
        }

        private void OnFileIDMesg(object sender, MesgEventArgs e)
        {
            string result;

            result = string.Format("FileIdHandler: Received {1} Mesg with global ID#{0}", e.mesg.Num, e.mesg.Name);
            myFitFile.Add(new myFit { Field = result });

            FileIdMesg myFileId = (FileIdMesg)e.mesg;
            
            try
            {
                result = string.Format("\tType: {0}", myFileId.GetType());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tManufacturer: {0}", myFileId.GetManufacturer());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tProduct: {0}", myFileId.GetProduct());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tSerialNumber {0}", myFileId.GetSerialNumber());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tNumber {0}", myFileId.GetNumber());
                myFitFile.Add(new myFit { Field = result });

                Dynastream.Fit.DateTime dtTime = new Dynastream.Fit.DateTime(myFileId.GetTimeCreated().GetTimeStamp());
                myFitFile.Add(new myFit { Field = dtTime.ToString() });

            }
            catch (FitException exception)
            {
                msgBoxobj.ShowNotification("OnFileIDMesg Error: " + exception.Message + Environment.NewLine + "Inner Exception: " + exception.InnerException);
                //Console.WriteLine("\tOnFileIDMesg Error {0}", exception.Message);
                //Console.WriteLine("\t{0}", exception.InnerException);
            }
        }
        private void OnUserProfileMesg(object sender, MesgEventArgs e)
        {
            string result;

            result = string.Format("UserProfileHandler: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name);
            myFitFile.Add(new myFit { Field = result });

            UserProfileMesg myUserProfile = (UserProfileMesg)e.mesg;
            
            try
            {
                result = string.Format("\tType {0}", myUserProfile.GetFriendlyName());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tGender {0}", myUserProfile.GetGender().ToString());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tAge {0}", myUserProfile.GetAge());
                myFitFile.Add(new myFit { Field = result });

                result = string.Format("\tWeight  {0}", myUserProfile.GetWeight());
                myFitFile.Add(new myFit { Field = result });
            }
            catch (FitException exception)
            {
                msgBoxobj.ShowNotification("OnUserProfileMesg Error: " + exception.Message + Environment.NewLine + "Inner Exception: " + exception.InnerException);
                //Console.WriteLine("\tOnUserProfileMesg Error {0}", exception.Message);
                //Console.WriteLine("\t{0}", exception.InnerException);
            }
        }
    }
}
