using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ReadFit.FileModel;
using System.Collections.Concurrent;
using System.Threading;

namespace ReadFit
{
    public class ReadFitParallel : ObservableObject
    {
        //public class myLooseEnds
        //{
        //    public List<string> myrecordType { get; set; }
        //    public List<string> myEvent2 { get; set; }
        //}

        public ReadFitParallel()
        {
            trackSequence = 0;
        }

        static ReadFitParallel()
        {
            msgBoxobj = new MsgBoxService();
        }

        static MsgBoxService msgBoxobj;

        //static ThreadLocal<DecodeRecords> myRecsLocal = new ThreadLocal<DecodeRecords>(() => new DecodeRecords());

        //public static DecodeRecords MyRecsLocal
        //{
        //    get { return myRecsLocal.Value; }
        //}

        //ThreadLocal<int> myTrackSequence = new ThreadLocal<int>(() => 0);

        //static ThreadLocal<myLooseEnds> myRecsParallel = new ThreadLocal<myLooseEnds>(() => new myLooseEnds());

        //public static myLooseEnds MyRecsParallel
        //{
        //    get { return myRecsParallel.Value; }
        //}

        //private IDataAccessLayer datalayer { get; set; }
        private IFitRead readlayer { get; set; }

        private List<string> recordType { get; set; }
        private int trackSequence { get; set; }

        private List<myFitRecord> myRecords1 { get; set; }
        private List<myLapRecord> myLaps1 { get; set; }
        private List<string> myEvent1 { get; set; }
        private List<myActRecord> myActivity1 { get; set; }
        private List<mySessionRecord> mySession1 { get; set; }
        private List<FileId> myFileIdRecord1 { get; set; }

        public DecodeRecords processFileList(string fname)
        {
            DataService.Instance.myFitFile1 = new List<myFit>();
            myRecords1 = new List<myFitRecord>();
            myLaps1 = new List<myLapRecord>();
            myEvent1 = new List<string>();
            myActivity1 = new List<myActRecord>();
            mySession1 = new List<mySessionRecord>();
            myFileIdRecord1 = new List<FileId>();

            DecodeRecords myReturnedRecords = null;

            recordType = new List<string>();

            //datalayer = GetLayer.giveMeADataLayer();

            Dynastream.Fit.Decode decodeDemo = null;
            Dynastream.Fit.MesgBroadcaster mesgBroadcaster = null;

            //bool myReturnValue = true;

            string fn = Path.GetFileName(fname);

            string processedFileKey = generateKey(fname);

            string myext = Path.GetExtension(fname);

            if (myext != ".fit")
            {
                //myReturnValue = false;
                myReturnedRecords = null;
            }
            else //if (!datalayer.getUserData<bool>(processedFileKey, "FileNameExist"))    //don't try to read a file that has already been processed
            {
                try
                {
                    using (FileStream fitSource = new FileStream(fname, FileMode.Open))
                    {
                        decodeDemo = new Dynastream.Fit.Decode();
                        mesgBroadcaster = new Dynastream.Fit.MesgBroadcaster();

                        // Connect the Broadcaster to our event (message) source (in this case the Decoder)
                        decodeDemo.MesgEvent += mesgBroadcaster.OnMesg;
                        decodeDemo.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;

                        // Subscribe to message events of interest by connecting to the Broadcaster
                        mesgBroadcaster.MesgEvent += new Dynastream.Fit.MesgEventHandler(OnMesg);
                        mesgBroadcaster.MesgDefinitionEvent += new Dynastream.Fit.MesgDefinitionEventHandler(OnMesgDefn);

                        mesgBroadcaster.FileIdMesgEvent += new Dynastream.Fit.MesgEventHandler(OnFileIDMesg);
                        mesgBroadcaster.UserProfileMesgEvent += new Dynastream.Fit.MesgEventHandler(OnUserProfileMesg);
                        mesgBroadcaster.LapMesgEvent += new Dynastream.Fit.MesgEventHandler(onLapMesg);

                        bool status = decodeDemo.IsFIT(fitSource);
                        status &= decodeDemo.CheckIntegrity(fitSource);

                        // Process the file
                        if (status == true)
                        {
                            decodeDemo.Read(fitSource);
                        }
                        else
                        {
                            msgBoxobj.ShowNotification("Integrity Check Failed: " + fn + Environment.NewLine + "Attempting to decode...");

                            decodeDemo.Read(fitSource);
                        }

                        //if (datalayer == null)
                        //{
                        //    msgBoxobj.ShowNotification("null datalayer");
                        //}

                        //if (!writeToDb(processedFileKey, myFileIdRecord1, myLaps1, myRecords1, mySession1, myLaps1[0].sport))
                        //{
                        //    msgBoxobj.ShowNotification("Error in db write, Key = " + processedFileKey);

                        //    myReturnValue = false;
                        //}

                        //fill the blocking collection

                        myReturnedRecords = new DecodeRecords
                        {
                            myFitKey = processedFileKey,
                            myFitAct = myFileIdRecord1,
                            myFitLap = myLaps1,
                            myFitTrack = myRecords1,
                            myFitSession = mySession1,
                            myFitSport = myLaps1[0].sport
                        };
                    }
                }
                catch (Exception ex)
                {
                    msgBoxobj.ShowNotification("Error - " + ex.Message);

                    //myReturnValue = false;

                    return null;
                }
                finally
                {
                    // Disconnect the Broadcaster to our event (message) source (in this case the Decoder)
                    decodeDemo.MesgEvent -= mesgBroadcaster.OnMesg;
                    decodeDemo.MesgDefinitionEvent -= mesgBroadcaster.OnMesgDefinition;

                    // Unsubscribe to message events of interest by disconnecting to the Broadcaster
                    mesgBroadcaster.MesgEvent -= new Dynastream.Fit.MesgEventHandler(OnMesg);
                    mesgBroadcaster.MesgDefinitionEvent -= new Dynastream.Fit.MesgDefinitionEventHandler(OnMesgDefn);

                    mesgBroadcaster.FileIdMesgEvent -= new Dynastream.Fit.MesgEventHandler(OnFileIDMesg);
                    mesgBroadcaster.UserProfileMesgEvent -= new Dynastream.Fit.MesgEventHandler(OnUserProfileMesg);

                    decodeDemo = null;
                    mesgBroadcaster = null;

                    //myFileIdRecord1.Clear();
                    //myLaps1.Clear();
                    //myRecords1.Clear();
                    //mySession1.Clear();

                    trackSequence = 0;

                    processedFileKey = string.Empty;
                }
            }

            //return myReturnValue;
            return myReturnedRecords;
        }

        //private bool writeToDb(string fileKey, List<FileId> myFileId, List<myLapRecord> myLap, List<myFitRecord> myRecs, List<mySessionRecord> mySess, string mySport)
        //{
        //    bool myReturnValue;

        //    if (myFileId.Count == 1)
        //    {
        //        myFileId[0].HasGpsData = myRecs.Any(n => n.latitude != 0.0 && n.longitude != 0.0);
        //    }
        //    else
        //    {
        //        msgBoxobj.ShowNotification("Error: more than one occurance!");
        //    }

        //    if (myReturnValue = datalayer.insertActivityRecords(myFileId, fileKey, mySport))
        //    {
        //        if (myReturnValue = datalayer.insertLapRecords(myLap, fileKey))
        //        {
        //            if (myReturnValue = datalayer.insertTrackRecords(myRecs, fileKey))
        //            {
        //                if (myReturnValue = datalayer.insertSessionRecords(mySess, fileKey))
        //                {
        //                    if (!datalayer.insertProcessedFileName(fileKey))
        //                    {
        //                        myReturnValue = false;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return myReturnValue;
        //}

        private string generateKey(string filen)
        {
            string gn = Path.GetFileNameWithoutExtension(filen);

            int[] test1 = gn.Split('-').Select(n => Convert.ToInt32(n)).ToArray();

            System.DateTime dt = new System.DateTime(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5]);

            return dt.ToString("s");
        }

        private void OnMesgDefn(object sender, Dynastream.Fit.MesgDefinitionEventArgs e)
        {
            string result;

            result = string.Format("OnMesgDef: Received Defn for local message #{0}, it has {1} fields", e.mesgDef.LocalMesgNum, e.mesgDef.NumFields);
            DataService.Instance.myFitFile1.Add(new myFit { Field = result });

            //Console.WriteLine("OnMesgDef: Received Defn for local message #{0}, it has {1} fields", e.mesgDef.LocalMesgNum, e.mesgDef.NumFields);
        }

        private void OnMesg(object sender, Dynastream.Fit.MesgEventArgs e)
        {
            myFitRecord mfr = null;
            myLapRecord mlr = null;
            myActRecord myact = null;
            mySessionRecord msr = null;
            FileId mfid = null;
            string result;

            readlayer = GetLayer.giveMeAReadLayer();

            if (!recordType.Contains(e.mesg.Name))
            {
                recordType.Add(e.mesg.Name);
            }

            switch (e.mesg.Name)
            {
                case "Record":
                    mfr = new myFitRecord();
                    break;

                case "Lap":
                    mlr = new myLapRecord();
                    break;

                case "Event":
                    break;

                case "Activity":
                    myact = new myActRecord();
                    break;

                case "Session":
                    msr = new mySessionRecord();
                    break;

                case "FileId":
                    mfid = new FileId();
                    break;

                default:
                    result = string.Format("OnMesg: Received Mesg with global ID#{0}, its name is {1}", e.mesg.Num, e.mesg.Name);

                    DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                    break;
            }

            //Console.WriteLine("OnMesg: Received Mesg with global ID#{0}, its name is {1}", e.mesg.Num, e.mesg.Name);

            if (readlayer == null)
            {
                msgBoxobj.ShowNotification("null readlayer");
            }

            for (byte i = 0; i < e.mesg.GetNumFields(); i++)
            {
                for (int j = 0; j < e.mesg.fields[i].values.Count; j++)
                {
                    switch (e.mesg.Name)
                    {
                        case "Record":
                            readlayer.getTrackData(e, i, j, mfr);
                            break;

                        case "Lap":
                            readlayer.getLapData(e, i, j, mlr);
                            break;

                        case "Event":
                            readlayer.getEventData(e, i, j, myEvent1);
                            break;

                        case "Activity":
                            readlayer.getActData(e, i, j, myact);
                            break;

                        case "Session":
                            readlayer.getSessionData(e, i, j, msr);
                            break;

                        case "FileId":
                            readlayer.getFileIdData(e, i, j, mfid);
                            break;

                        default:
                            result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                            DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                            break;
                    }

                    //Console.WriteLine("\tField{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                }
            }

            if (e.mesg.Name == "Record")
            {
                mfr.sequence = trackSequence++;
                myRecords1.Add(mfr);
            }
            else if (e.mesg.Name == "Lap")
            {
                if (mlr.totalElaspedTime == "0.0")
                {
                    mlr.totalElaspedTime = mlr.totalTimerTime;
                }

                myLaps1.Add(mlr);
            }
            else if (e.mesg.Name == "Activity")
            {
                myActivity1.Add(myact);
            }
            else if (e.mesg.Name == "Session")
            {
                mySession1.Add(msr);
            }
            else if (e.mesg.Name == "FileId")
            {
                myFileIdRecord1.Add(mfid);
            }
        }

        private void OnFileIDMesg(object sender, Dynastream.Fit.MesgEventArgs e)
        {
            string result;

            result = string.Format("FileIdHandler: Received {1} Mesg with global ID#{0}", e.mesg.Num, e.mesg.Name);
            DataService.Instance.myFitFile1.Add(new myFit { Field = result });

            //Console.WriteLine("FileIdHandler: Received {1} Mesg with global ID#{0}", e.mesg.Num, e.mesg.Name);

            Dynastream.Fit.FileIdMesg myFileId = (Dynastream.Fit.FileIdMesg)e.mesg;
            try
            {
                result = string.Format("Type: {0}", myFileId.GetType());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("Manufacturer: {0}", myFileId.GetManufacturer());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("Product: {0}", myFileId.GetProduct());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("SerialNumber {0}", myFileId.GetSerialNumber());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("Number {0}", myFileId.GetNumber());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                //Console.WriteLine("\tType: {0}", myFileId.GetType());
                //Console.WriteLine("\tManufacturer: {0}", myFileId.GetManufacturer());
                //Console.WriteLine("\tProduct: {0}", myFileId.GetProduct());
                //Console.WriteLine("\tSerialNumber {0}", myFileId.GetSerialNumber());
                //Console.WriteLine("\tNumber {0}", myFileId.GetNumber());

                Dynastream.Fit.DateTime dtTime = new Dynastream.Fit.DateTime(myFileId.GetTimeCreated().GetTimeStamp());

                DataService.Instance.myFitFile1.Add(new myFit { Field = result });
            }
            catch (Dynastream.Fit.FitException exception)
            {
                msgBoxobj.ShowNotification("OnFileIDMesg Error: " + exception.Message + Environment.NewLine + exception.InnerException);

                //Console.WriteLine("\tOnFileIDMesg Error {0}", exception.Message);
                //Console.WriteLine("\t{0}", exception.InnerException);
            }
        }

        private void OnUserProfileMesg(object sender, Dynastream.Fit.MesgEventArgs e)
        {
            string result;

            result = string.Format("UserProfileHandler: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name);
            DataService.Instance.myFitFile1.Add(new myFit { Field = result });

            //Console.WriteLine("UserProfileHandler: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name);

            Dynastream.Fit.UserProfileMesg myUserProfile = (Dynastream.Fit.UserProfileMesg)e.mesg;

            try
            {
                result = string.Format("Type {0}", myUserProfile.GetFriendlyName());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("Gender {0}", myUserProfile.GetGender().ToString());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("Age {0}", myUserProfile.GetAge());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                result = string.Format("Weight  {0}", myUserProfile.GetWeight());
                DataService.Instance.myFitFile1.Add(new myFit { Field = result });

                //Console.WriteLine("\tType {0}", myUserProfile.GetFriendlyName());
                //Console.WriteLine("\tGender {0}", myUserProfile.GetGender().ToString());
                //Console.WriteLine("\tAge {0}", myUserProfile.GetAge());
                //Console.WriteLine("\tWeight  {0}", myUserProfile.GetWeight());
            }
            catch (Dynastream.Fit.FitException exception)
            {
                msgBoxobj.ShowNotification("OnUserProfileMesg Error: " + exception.Message + Environment.NewLine + exception.InnerException);

                //Console.WriteLine("\tOnUserProfileMesg Error {0}", exception.Message);
                //Console.WriteLine("\t{0}", exception.InnerException);
            }
        }

        private void onLapMesg(object sender, Dynastream.Fit.MesgEventArgs e)
        {
            string result;

            result = string.Format("UserProfileHandler: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name);

            Dynastream.Fit.LapMesg myLapMessage = (Dynastream.Fit.LapMesg)e.mesg;
        }
    }
}