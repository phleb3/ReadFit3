using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReadFit.FileModel;

namespace ReadFit
{
    class getHTTPRequest
    {
        public static IDataAccessLayer datalayer { get; set; }

        static MsgBoxService msgBoxobj;

        static getHTTPRequest()
        {
            datalayer = GetLayer.giveMeADataLayer();
            msgBoxobj = new MsgBoxService();
        }

        static int myIndex = 0;
        static int takeCnt = 0;

        public void testElevationRead(bool gpsFlag, int rowid)
        {

            string url1 = "https://maps.googleapis.com/maps/api/elevation/json?locations=";
            string url2 = "&sensor=true";

            string timestamp = string.Empty;

            List<Uri> myuris = new List<Uri>();
            List<LatLong> loclist = new List<LatLong>();

            if (DataService.Instance.myElevations != null)
            {
                DataService.Instance.myElevations.Clear();
            }
            else
            {
                DataService.Instance.myElevations = new AsyncObservableCollection<ElevationCorrection>();
            }

            timestamp = DataService.Instance.trackData[0].timeStamp;    //generate a string "yyyy-mm-ddThh:mm:ss"
            DateTime dt = DateTime.Parse(timestamp);
            string myTimeStamp = dt.ToString("s");

            myIndex = 0;

            if (datalayer.getUserData<bool, int>(rowid, "ElevationDataExist"))     //if found on the database, don't call Google Elevation
            {
                foreach (var q in datalayer.getQuery<ElevationUpdate>(rowid, "GetElevationData"))     //getQuery returns an observablecollection
                {
                    //if (isInBoundingBox(q.latitude, q.longitude))     //correct for the Champlain Bridge
                    //{
                    //    if (firstRec)
                    //    {
                    //        adjustElevation = DataService.Instance.trackData[myIndex].altitude - q.altitude;
                    //        firstRec = false;
                    //    }
                    //    DataService.Instance.myElevations.Add(new ElevationCorrection
                    //    {
                    //        corrected = DataService.Instance.trackData[myIndex].altitude - adjustElevation,
                    //        uncorrected = DataService.Instance.trackData[myIndex].altitude,
                    //        timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                    //        distance = DataService.Instance.trackData[myIndex++].distance
                    //    });
                    //}
                    //else
                    //{
                    DataService.Instance.myElevations.Add(new ElevationCorrection
                    {
                        corrected = q.altitude,
                        uncorrected = DataService.Instance.trackData[myIndex].altitude,
                        timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                        distance = DataService.Instance.trackData[myIndex++].distance
                    });
                    //}
                }

                //send a message to update the correction graph plot
                MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });

                MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "HideTab", FlagState = false });    
            }
            else
            {
                if (gpsFlag)
                {
                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = false });     //publish message to show circular progress bar

                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "HideTab", FlagState = false });    //publish message to hide correction tab

                    var chunk1 = DataService.Instance.trackData.ChunkData(500).ToArray();    //split file up into 500 item chuncks

                    foreach (var q in chunk1)
                    {
                        loclist = (from x in q
                                   select new LatLong
                                   {
                                       Lat = Math.Round(x.latitude, 7, MidpointRounding.AwayFromZero),
                                       Lon = Math.Round(x.longitude, 7, MidpointRounding.AwayFromZero)

                                   }).ToList();

                        myuris.Add(new Uri(string.Format("{0}{1}{2}{3}", url1, "enc:", EncodeLatLong(loclist), url2)));
                    }

                    Task<string>[] tasks = myuris.Select(DownloadStringAsync).ToArray();    //create an array of tasks, and start each one using .ToArray()

                    Task.Factory.ContinueWhenAll(tasks, a =>
                    {
                        finalProcess(tasks, myTimeStamp, rowid);    //when all tasks are completed

                    });
                }
                else
                {
                    //there was no gps data
                    DataService.Instance.myElevations.Clear();
                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });

                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "HideTab", FlagState = true });
                }
            }
        }

        private void finalProcess(Task<string>[] mytasks, string myTimeStamp, int rowid)   //parse the returned JSON strings, and build corrected elevation file
        {
            int elevationCount = 0;
            int trackdataCount = 0;
            string myErrorMsg;
            string myResult;

            bool hasError = false;

            List<ElevationUpdate> elevations = new List<ElevationUpdate>();

            foreach (var q in mytasks)
            {
                if (q.Exception != null)
                {
                    hasError = true;
                    myErrorMsg = q.Exception.InnerException.Message;
                    msgBoxobj.ExtendedNotification(myErrorMsg, "Elevation Update", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                }
                else
                {
                    myResult = q.Result;
                    elevations.AddRange(parseGoogleJason(myResult));    //these come back in the order in which they were sent
                }
            }

            if (!hasError)
            {
                //do database add here
                if (!datalayer.insertElevationRecords(elevations, myTimeStamp, rowid))
                {
                    MessageBox.Show("Database insert error");
                }
               
                elevationCount = elevations.Count();
                trackdataCount = DataService.Instance.trackData.Count();

                DataService.Instance.myElevations.Clear();

                if (elevationCount != trackdataCount)
                {
                    msgBoxobj.ShowNotification("Error! counts do not match" + elevationCount + " : " + trackdataCount);
                }
                else
                {
                    for (int i = 0; i < trackdataCount; i++)
                    {
                        DataService.Instance.myElevations.Add(new ElevationCorrection   //for the correction graph plot
                        {
                            corrected = elevations[i].altitude,
                            uncorrected = DataService.Instance.trackData[i].altitude,
                            timeStamp = DataService.Instance.trackData[i].timeStamp,
                            distance = DataService.Instance.trackData[i].distance
                        });
                    }

                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsIdle", FlagState = true });  //publish message to hide circular progress bar

                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "HideTab", FlagState = false }); //publish message to show corrections tab

                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });

                    msgBoxobj.ShowNotification("Finished Correcting Elevations");
                }
            }
        }

        //
        //http://stackoverflow.com/questions/10763152/completed-event-handler-for-task-factory-startnew-parallel-foreach
        //
        private static Task<String> DownloadStringAsync(Uri uri)
        {
            var webclient = new WebClient();

            var tcs = new TaskCompletionSource<string>();

            DownloadStringCompletedEventHandler handler = null;

            handler = (source, args) =>
            {
                using (webclient)
                {
                    // Set the task completion source based on the
                    // event.
                    if (args.Cancelled)
                    {
                        // Set cancellation.
                        tcs.SetCanceled();
                        return;
                    }

                    // Exception?
                    if (args.Error != null)
                    {
                        // Set exception.
                        tcs.SetException(args.Error);
                        return;
                    }

                    // Set result.
                    tcs.SetResult(args.Result);
                };
            };

            webclient.DownloadStringCompleted += handler;
            webclient.DownloadStringAsync(uri);

            return tcs.Task;
        }

        //public bool readGoogleElevations(out bool myGoogleFlag)
        //{
        //    string url1 = "https://maps.googleapis.com/maps/api/elevation/json?locations=";
        //    string url2 = "&sensor=true";
        //    string calledURL = string.Empty;
        //    string timestamp = string.Empty;

        //    int mySkip = 0;
        //    int myTake = 500;
        //    int trackcnt = 0;

        //    //double googleElevation = 0.0;
        //    //double googleLatatude = 0.0;
        //    //double googleLongitude = 0.0;
        //    //double googleResolution = 0.0;

        //    //HttpWebRequest grequest;
        //    //HttpWebResponse gresponse;
        //    //DataContractJsonSerializer jsonSerializer;

        //    //bool firstRec = true;
        //    //double adjustElevation = 0.0;

        //    List<ElevationUpdate> elevations = new List<ElevationUpdate>();

        //    List<LatLong> loclist = new List<LatLong>();

        //    trackcnt = DataService.Instance.trackData.Count();

        //    timestamp = DataService.Instance.trackData[0].timeStamp;    //generate a string "yyyy-mm-ddThh:mm:ss"
        //    DateTime dt = DateTime.Parse(timestamp);
        //    string myTimeStamp = dt.ToString("s");

        //    myIndex = 0;

        //    if (DataService.Instance.myElevations != null)
        //    {
        //        DataService.Instance.myElevations.Clear();
        //    }
        //    else
        //    {
        //        DataService.Instance.myElevations = new AsyncObservableCollection<ElevationCorrection>();
        //    }

        //    int myrowid = datalayer.getUserData<int, string>(myTimeStamp, "GetRowId");

        //    if (datalayer.getUserData<bool, int>(myrowid, "ElevationDataExist"))     //if found on the database, don't call Google Elevation
        //    {
        //        myGoogleFlag = false;

        //        //int myrowid = datalayer.getUserData<int>(myTimeStamp, "GetRowId");

        //        foreach (var q in datalayer.getQuery<ElevationUpdate>(myrowid, "GetElevationData"))     //getQuery returns an observablecollection
        //        {
        //            //if (isInBoundingBox(q.latitude, q.longitude))     //correct for the Champlain Bridge
        //            //{
        //            //    if (firstRec)
        //            //    {
        //            //        adjustElevation = DataService.Instance.trackData[myIndex].altitude - q.altitude;
        //            //        firstRec = false;
        //            //    }
        //            //    DataService.Instance.myElevations.Add(new ElevationCorrection
        //            //    {
        //            //        corrected = DataService.Instance.trackData[myIndex].altitude - adjustElevation,
        //            //        uncorrected = DataService.Instance.trackData[myIndex].altitude,
        //            //        timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
        //            //        distance = DataService.Instance.trackData[myIndex++].distance
        //            //    });
        //            //}
        //            //else
        //            //{
        //            DataService.Instance.myElevations.Add(new ElevationCorrection
        //            {
        //                corrected = q.altitude,
        //                uncorrected = DataService.Instance.trackData[myIndex].altitude,
        //                timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
        //                distance = DataService.Instance.trackData[myIndex++].distance
        //            });
        //            //}
        //        }

        //        //send a message to update the correction graph plot
        //        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });
        //        return true;
        //    }
        //    else
        //    {
        //        myGoogleFlag = true;

        //        while (mySkip < trackcnt)
        //        {
        //            loclist = (from x in DataService.Instance.trackData
        //                       select new LatLong
        //                       {
        //                           Lat = Math.Round(x.latitude, 7, MidpointRounding.AwayFromZero),
        //                           Lon = Math.Round(x.longitude, 7, MidpointRounding.AwayFromZero)

        //                       }).Skip(mySkip).Take(myTake).ToList();

        //            mySkip += myTake;

        //            var googleUri = new Uri(string.Format("{0}{1}{2}{3}", url1, "enc:", EncodeLatLong(loclist), url2));

        //            googleUri.DownloadString(t => 
        //                {
        //                    string t7 = t;

        //                    if (string.IsNullOrEmpty(t7))
        //                    {
        //                        msgBoxobj.ShowNotification("Empty");
        //                    }
        //                    else
        //                    {
        //                        elevations.AddRange(parseGoogleJason(t7));
        //                    }

        //                    if (elevations.Count() == trackcnt)
        //                    {
        //                        msgBoxobj.ShowNotification("Finished");

        //                        List<ElevationCorrection> myelv = new List<ElevationCorrection>();
        //                        DataService.Instance.myElevations.Clear();

        //                        foreach (var q in elevations)
        //                        {
        //                            //DataService.Instance.myElevations.Add(new ElevationCorrection   //for the correction graph plot
        //                            myelv.Add(new ElevationCorrection
        //                            {
        //                                corrected = q.altitude,
        //                                uncorrected = DataService.Instance.trackData[myIndex].altitude,
        //                                timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
        //                                distance = DataService.Instance.trackData[myIndex].distance
        //                            });

        //                            Interlocked.Increment(ref myIndex);
        //                        }

        //                        var temp1 = (from data in myelv
        //                                    orderby data.distance ascending
        //                                    select data);

        //                        foreach (var q in temp1)
        //                        {
        //                            DataService.Instance.myElevations.Add(q);
        //                        }

        //                        MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });
        //                    }

        //                });

        //            //int u = 0;

        //            //calledURL = string.Format("{0}{1}{2}{3}", url1, "enc:", EncodeLatLong(loclist), url2);

        //            //try
        //            //{
        //            //    grequest = WebRequest.Create(calledURL) as HttpWebRequest;

        //            //    grequest.Proxy = null;  //try this

        //            //    using (gresponse = grequest.GetResponse() as HttpWebResponse)
        //            //    {
        //            //        if (gresponse.StatusCode != HttpStatusCode.OK)
        //            //        {
        //            //            throw new Exception(String.Format("Server error (HTTP {0}: {1}).", gresponse.StatusCode, gresponse.StatusDescription));
        //            //        }

        //            //        jsonSerializer = new DataContractJsonSerializer(typeof(GoogleResult));

        //            //        object objResponse = jsonSerializer.ReadObject(gresponse.GetResponseStream());

        //            //        GoogleResult jsonResponse = objResponse as GoogleResult;

        //            //        if (jsonResponse.status.Equals("OK"))
        //            //        {
        //            //            foreach (var q in jsonResponse.results)
        //            //            {
        //            //                googleElevation = q.elevation;

        //            //                googleLatatude = q.location.lat;

        //            //                googleLongitude = q.location.lng;

        //            //                googleResolution = q.resolution;

        //            //                elevations.Add(new ElevationUpdate      //for the database insert
        //            //                    {
        //            //                        altitude = googleElevation,
        //            //                        latitude = googleLatatude,
        //            //                        longitude = googleLongitude,
        //            //                        resolution = googleResolution
        //            //                    });

        //            //                DataService.Instance.myElevations.Add(new ElevationCorrection   //for the correction graph plot
        //            //                    {
        //            //                        corrected = googleElevation,
        //            //                        uncorrected = DataService.Instance.trackData[myIndex].altitude,
        //            //                        timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
        //            //                        distance = DataService.Instance.trackData[myIndex++].distance
        //            //                    });

        //            //                //DataService.Instance.trackData[myIndex++].altitude = googleElevation;
        //            //            }
        //            //        }
        //            //        else
        //            //        {
        //            //            MessageBox.Show("Error in status, status = " + jsonResponse.status);
        //            //        }
        //            //    }
        //            //}
        //            //catch (Exception e)
        //            //{
        //            //    MessageBox.Show("Error in WebRequest -> " + e.Message);
        //            //    return false;
        //            //}
        //        }

        //        //if (!datalayer.insertElevationRecords(elevations, myTimeStamp))
        //        //{
        //        //    MessageBox.Show("Database insert error");
        //        //}
        //        //else
        //        //{
        //        //    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });
        //        //}
        //    }

        //    return true;
        //}

        //private void parseGoogleJason(string text, Action<List<ElevationUpdate>> action)
        private List<ElevationUpdate> parseGoogleJason(string text)
        {
            List<ElevationUpdate> returnedElevations = new List<ElevationUpdate>();

            if (!string.IsNullOrEmpty(text))
            {
                object objResponse = null;
                double googleElevation;
                double googleLatitude;
                double googleLongitude;
                double googleResolution;

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GoogleResult));

                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(text)))
                {
                    objResponse = jsonSerializer.ReadObject(stream);
                }

                GoogleResult jsonResponse = objResponse as GoogleResult;

                if (jsonResponse.status.Equals("OK"))
                {
                    foreach (var q in jsonResponse.results)
                    {
                        googleElevation = q.elevation;

                        googleLatitude = q.location.lat;

                        googleLongitude = q.location.lng;

                        googleResolution = q.resolution;

                        returnedElevations.Add(new ElevationUpdate      //for the database insert
                        {
                            altitude = googleElevation,
                            latitude = googleLatitude,
                            longitude = googleLongitude,
                            resolution = googleResolution
                        });
                    }
                }
            }

            return returnedElevations;  
        }

        public bool readGoogleJson(out bool myGoogleFlag)        //google elevation api (json)
        {
            string url1 = "https://maps.googleapis.com/maps/api/elevation/json?locations=";
            string url2 = "&sensor=true";
            string calledURL = string.Empty;
            string googleStatus = string.Empty;
            string timestamp = string.Empty;

            int mySkip = 0;
            int myTake = 500;
            int trackcnt = 0;

            double googleElevation = 0.0;
            double googleLatatude = 0.0;
            double googleLongitude = 0.0;
            double googleResolution = 0.0;

            HttpWebRequest grequest;
            //HttpWebResponse gresponse;
            DataContractJsonSerializer jsonSerializer;

            //bool firstRec = true;
            //double adjustElevation = 0.0;

            List<ElevationUpdate> elevations = new List<ElevationUpdate>();
            //AsyncObservableCollection<ElevationUpdate> elevations1 = new AsyncObservableCollection<ElevationUpdate>();

            //string myPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            List<LatLong> loclist = new List<LatLong>();

            trackcnt = DataService.Instance.trackData.Count();

            timestamp = DataService.Instance.trackData[0].timeStamp;
            DateTime dt = DateTime.Parse(timestamp);
            string myTimeStamp = dt.ToString("s");

            myIndex = 0;

            if (DataService.Instance.myElevations != null)
            {
                DataService.Instance.myElevations.Clear();
            }
            else
            {
                DataService.Instance.myElevations = new AsyncObservableCollection<ElevationCorrection>();
            }

            int myrowid = datalayer.getUserData<int, string>(myTimeStamp, "GetRowId");

            if (datalayer.getUserData<bool, int>(myrowid, "ElevationDataExist"))     //if found on the database, don't call Google Elevation
            {
                myGoogleFlag = false;

                //int myrowid = datalayer.getUserData<int, string>(myTimeStamp, "GetRowId");

                foreach (var q in datalayer.getQuery<ElevationUpdate>(myrowid, "GetElevationData"))     //getQuery returns an observablecollection
                {
                    //if (isInBoundingBox(q.latitude, q.longitude))     //correct for the Champlain Bridge
                    //{
                    //    if (firstRec)
                    //    {
                    //        adjustElevation = DataService.Instance.trackData[myIndex].altitude - q.altitude;
                    //        firstRec = false;
                    //    }
                    //    DataService.Instance.myElevations.Add(new ElevationCorrection
                    //    {
                    //        corrected = DataService.Instance.trackData[myIndex].altitude - adjustElevation,
                    //        uncorrected = DataService.Instance.trackData[myIndex].altitude,
                    //        timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                    //        distance = DataService.Instance.trackData[myIndex++].distance
                    //    });
                    //}
                    //else
                    //{
                        DataService.Instance.myElevations.Add(new ElevationCorrection
                        {
                            corrected = q.altitude,
                            uncorrected = DataService.Instance.trackData[myIndex].altitude,
                            timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                            distance = DataService.Instance.trackData[myIndex++].distance
                        });
                    //}
                }

                //send a message to update the correction graph plot
                MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });
                return true;
            }
            else
            {
                myGoogleFlag = true;

                while (mySkip < trackcnt)
                {
                    loclist = (from x in DataService.Instance.trackData
                               select new LatLong
                               {
                                   Lat = Math.Round(x.latitude, 7, MidpointRounding.AwayFromZero),
                                   Lon = Math.Round(x.longitude, 7, MidpointRounding.AwayFromZero)

                               }).Skip(mySkip).Take(myTake).ToList();

                    mySkip += myTake;

                    calledURL = url1 + "enc:";

                    calledURL += EncodeLatLong(loclist);     //convert the list of points to a encoded string

                    calledURL += url2;

                    Interlocked.Increment(ref takeCnt);     //increment thread safe counter

                    //int l1 = calledURL.Length;

                    try
                    {
                        grequest = WebRequest.Create(calledURL) as HttpWebRequest;

                        grequest.Proxy = null;  //try this

                        //async webrequest - todo: check if records comeback in the right order
                        DoWithResponse(grequest, (responsegoogle) =>
                        {
                            Interlocked.Decrement(ref takeCnt);     //decrement counter

                            if (responsegoogle.StatusCode != HttpStatusCode.OK)
                            {
                                throw new Exception(String.Format("Server error (HTTP {0}: {1}).", responsegoogle.StatusCode, responsegoogle.StatusDescription));
                            }

                            jsonSerializer = new DataContractJsonSerializer(typeof(GoogleResult));

                            object objResponse = jsonSerializer.ReadObject(responsegoogle.GetResponseStream());

                            GoogleResult jsonResponse = objResponse as GoogleResult;

                            if (jsonResponse.status.Equals("OK"))
                            {
                                foreach (var q in jsonResponse.results)
                                {
                                    googleElevation = q.elevation;

                                    googleLatatude = q.location.lat;

                                    googleLongitude = q.location.lng;

                                    googleResolution = q.resolution;

                                    elevations.Add(new ElevationUpdate      //for the database insert
                                        {
                                            altitude = googleElevation,
                                            latitude = googleLatatude,
                                            longitude = googleLongitude,
                                            resolution = googleResolution
                                        });

                                    
                                    DataService.Instance.myElevations.Add(new ElevationCorrection   //for the correction graph plot
                                        {
                                            corrected = googleElevation,
                                            uncorrected = DataService.Instance.trackData[myIndex].altitude,
                                            timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                                            distance = DataService.Instance.trackData[myIndex++].distance
                                        });
                                    
                                    //DataService.Instance.trackData[myIndex++].altitude = googleElevation;
                                }
                                //datalayer.insertElevationRecords(elevations, myrowid);      //do the database insert
                                elevations.Clear();
                            }
                            else
                            {
                                MessageBox.Show("Error in status, status = " + googleStatus);
                            }
                        });

                        //using (gresponse = grequest.GetResponse() as HttpWebResponse)
                        //{
                        //    if (gresponse.StatusCode != HttpStatusCode.OK)
                        //    {
                        //        throw new Exception(String.Format("Server error (HTTP {0}: {1}).", gresponse.StatusCode, gresponse.StatusDescription));
                        //    }

                        //    jsonSerializer = new DataContractJsonSerializer(typeof(GoogleResult));

                        //    object objResponse = jsonSerializer.ReadObject(gresponse.GetResponseStream());

                        //    GoogleResult jsonResponse = objResponse as GoogleResult;

                        //    googleStatus = jsonResponse.status;

                        //    if (googleStatus.Equals("OK"))
                        //    {
                        //        foreach (var q in jsonResponse.results)
                        //        {
                        //            googleElevation = q.elevation;

                        //            googleLatatude = q.location.lat;

                        //            googleLongitude = q.location.lng;

                        //            googleResolution = q.resolution;

                        //            elevations.Add(new ElevationUpdate      //for the database insert
                        //                {
                        //                    altitude = googleElevation,
                        //                    latitude = googleLatatude,
                        //                    longitude = googleLongitude,
                        //                    resolution = googleResolution
                        //                });

                                    
                        //            DataService.Instance.myElevations.Add(new ElevationCorrection   //for the correction graph plot
                        //                {
                        //                    corrected = googleElevation,
                        //                    uncorrected = DataService.Instance.trackData[myIndex].altitude,
                        //                    timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                        //                    distance = DataService.Instance.trackData[myIndex++].distance
                        //                });
                                    
                        //            //DataService.Instance.trackData[myIndex++].altitude = googleElevation;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        MessageBox.Show("Error in status, status = " + googleStatus);
                        //    }
                        //}
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error in WebRequest -> " + e.Message);
                        return false;
                    }
                }
                if (trackcnt == 0)
                {
                    //datalayer.insertElevationRecords(elevations, myTimeStamp);      //do the database insert
                    //send a message to update the correction graph plot
                    MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });
                    MessageBox.Show("got here");
                    return true;
                }
                else
                {
                    return false;
                }
            }

            ////send a message to update the correction graph plot
            //MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });

            //return true;
        }

        private bool isInBoundingBox(double lat, double lon)    //kludge for the Lake Champlain Bridge
        {
            //if (lat > 44.028 && lat < 44.036)
            //{
            //    if (lon > -73.426 && lon < -73.420)
            //    {
            //        return true;
            //    }
            //}

            if (lat > 44.0295 && lat < 44.0597)
            {
                if (lon > -73.4258 && lon < -73.4206)
                {
                    return true;
                }
            }
            return false;
        }

        //public void getweb()
        //{
        //    //string url = "http://gisdata.usgs.gov//XMLWebServices2/Elevation_Service.asmx/getElevation?X_Value=-73.390545&Y_Value=44.021380&Elevation_Units=METERS&Source_Layer=-1&Elevation_Only=YES HTTP/1.1";
        //    string url = "http://maps.googleapis.com/maps/api/elevation/json?locations=44.021380,-73.390545|44.021420,-73.390508|44.021450,-73.390501&sensor=true";
        //    //string url = "http://dev.virtualearth.net/REST/v1/Elevation/List?points=lat1,long1,lat2,long2,latn,longn&heights=heights&key=";
        //    //string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cumulus.txt";
        //    string fileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\googleasync.txt";

        //    //string str1 = "http://dev.virtualearth.net/REST/v1/Elevation/SeaLevel?points=";
        //    //string str1 = "http://dev.virtualearth.net/REST/v1/Elevation/List?points=";
        //    //string str2 = "&key=";
        //    //string bingKey = "Apoud-wEXM4X2DJqRG2-qVgStr0CRPzWK7qElJAlU5bmILmLzJ4QfuG8YQgVNk4A";
        //    //string bingKey = "AhxzP5nms3_p7-eh0YGX4wzlxmi4W5TVIhNiTn0lH_lU_L_0vP6i0V7NGROnGLsw";

        //    //double googleElevation;
        //    //double googleLatitude;
        //    //double googleLongitude;
        //    //double googleResolution;
        //    //string grst = string.Empty;

        //    //string url = string.Format("{0}{1},{2},{3},{4},{5},{6}{7}{8}", str1, "44.021380","-73.390545","44.021420","-73.390508","44.021450","-73.390501", str2, bingKey);

        //    //object objResponse = null;

        //    Uri googleUri = new Uri(url);   //create url

        //    googleUri.DownloadString(t => 
        //        {
        //            parseJson1(t);  //download json string async, and parse it

        //        }, ex =>
        //            {
        //                msgBoxobj.ExtendedNotification(ex.InnerException.Message, "Correct Elevations", MessageBoxButton.OK, MessageBoxImage.Error);
        //            });

        //    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    ////request.Timeout = 5000;
        //    //request.Proxy = null;

        //    //DoWithResponse(request, (response) =>
        //    //{
        //    //    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GoogleResult));

        //    //    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());

        //    //    GoogleResult jsonResponse = objResponse as GoogleResult;

        //    //    if (jsonResponse.status.Equals("OK"))
        //    //    {
        //    //        foreach (var q in jsonResponse.results)
        //    //        {
        //    //            googleElevation = q.elevation;

        //    //            googleLatitude = q.location.lat;

        //    //            googleLongitude = q.location.lng;

        //    //            googleResolution = q.resolution;

        //    //            MessageBox.Show("google returned: " + googleElevation + " meters");
        //    //        }
        //    //    }
        //    //});

        //    //try
        //    //{
        //    //    using (WebResponse response = (HttpWebResponse)request.GetResponse())
        //    //    {
        //    //        using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        //    //        {
        //    //            byte[] bytes = ReadFully(response.GetResponseStream());

        //    //            stream.Write(bytes, 0, bytes.Length);
        //    //        }
        //    //    }
        //    //}
        //    //catch (WebException ex)
        //    //{
        //    //    MessageBox.Show("Error Occured: " + ex.Message + " : " + ex.InnerException);
        //    //}
        //}

        //public static byte[] ReadFully(Stream input)
        //{
        //    byte[] buffer = new byte[16 * 1024];

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        int read;

        //        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, read);
        //        }

        //        return ms.ToArray();
        //    }
        //}

        //private void parseJson1(string text)
        //{
        //    if (text == null)
        //    {
        //        return;
        //    }

        //    object objResponse = null;
        //    double googleElevation;
        //    double googleLatitude;
        //    double googleLongitude;
        //    double googleResolution;

        //    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GoogleResult));

        //    using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(text)))
        //    {
        //        objResponse = jsonSerializer.ReadObject(stream);
        //    }

        //    GoogleResult jsonResponse = objResponse as GoogleResult;

        //    if (jsonResponse.status.Equals("OK"))
        //    {
        //        foreach (var q in jsonResponse.results)
        //        {
        //            googleElevation = q.elevation;

        //            googleLatitude = q.location.lat;

        //            googleLongitude = q.location.lng;

        //            googleResolution = q.resolution;

        //            MessageBox.Show("google returned: " + googleElevation + " meters");
        //        }
        //    }
        //}

        //public void httpPost()
        //{
        //    WebRequest request = null;
        //    byte[] byteArray;
        //    string lat;
        //    string lon;
        //    double requestElevation;
        //    double requestLatitude;
        //    double requestLongitude;
        //    string postData;
        //    XDocument xmldoc;

        //    List<ElevationUpdate> elevations = new List<ElevationUpdate>();


        //    if (DataService.Instance.myElevations != null)
        //    {
        //        DataService.Instance.myElevations.Clear();
        //    }
        //    else
        //    {
        //        DataService.Instance.myElevations = new AsyncObservableCollection<ElevationCorrection>();
        //    }

        //    //request = (HttpWebRequest)WebRequest.Create("http://gisdata.usgs.gov/XMLWebServices2/Elevation_Service.asmx/getElevation");

        //    //request.Proxy = null;

        //    //request.Method = "POST";

        //    //request.ContentType = "application/x-www-form-urlencoded";

        //    foreach (var q in DataService.Instance.trackData)
        //    {
        //        lat = Math.Round(q.latitude, 7, MidpointRounding.AwayFromZero).ToString();
        //        lon = Math.Round(q.longitude, 7, MidpointRounding.AwayFromZero).ToString();

        //        request = (HttpWebRequest)WebRequest.Create("http://gisdata.usgs.gov/XMLWebServices2/Elevation_Service.asmx/getElevation");

        //        request.Proxy = null;

        //        request.Method = "POST";

        //        request.ContentType = "application/x-www-form-urlencoded";

        //        postData = string.Format("X_Value={0}&Y_Value={1}&Elevation_Units=METERS&Source_Layer=-1&Elevation_Only= HTTP/1.1", lon, lat);

        //        byteArray = Encoding.UTF8.GetBytes(postData);

        //        request.ContentLength = byteArray.Length;

        //        using (Stream dataStream = request.GetRequestStream())
        //        {
        //            dataStream.Write(byteArray, 0, byteArray.Length);
        //        }

        //        using (WebResponse response = (HttpWebResponse)request.GetResponse())
        //        {
        //            xmldoc = XDocument.Load(response.GetResponseStream());

        //            requestElevation = xmldoc.Descendants("Elevation_Query").Select(x => (double)x.Element("Elevation")).DefaultIfEmpty(-1.0).First();

        //            requestLatitude = xmldoc.Descendants("Elevation_Query").Select(x => (double)x.Attribute("y")).FirstOrDefault();

        //            requestLongitude = xmldoc.Descendants("Elevation_Query").Select(x => (double)x.Attribute("x")).FirstOrDefault();
        //        }
        //    }

        //    MessageBox.Show("Done");
        //}

        void DoWithResponse(HttpWebRequest request, Action<HttpWebResponse> responseAction)
        {
            Action wrapperAction = () =>
            {
                request.BeginGetResponse(new AsyncCallback((iar) =>
                {
                    var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                    responseAction(response);

                }), request);
            };

            wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
            {
                var action = (Action)iar.AsyncState;
                action.EndInvoke(iar);

            }), wrapperAction);
        }

        ////public void testJson()      //bing maps elevation api
        ////{
        ////    string calledURL = string.Empty;
        ////    string elevationRequest = string.Empty;
        ////    Response elevationResponse = null;

        ////    int mySkip = 0;
        ////    int myTake = 85;
        ////    int trackcnt = 0;

        ////    bool myFirstFlag = false;

        ////    string myPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        ////    string fileName = myPath + "\\bing.txt";

        ////    List<ElevationUpdate> elist = new List<ElevationUpdate>();

        ////    myIndex = 0;

        ////    trackcnt = DataService.Instance.trackData.Count();
        ////    //trackcnt = 10;

        ////    while (mySkip < trackcnt)
        ////    {
        ////        calledURL = string.Empty;

        ////        elist = (from x in DataService.Instance.trackData
        ////                 select new ElevationUpdate
        ////                 {
        ////                     altitude = x.altitude,
        ////                     latitude = Math.Round(x.latitude, 7, MidpointRounding.AwayFromZero),
        ////                     longitude = Math.Round(x.longitude, 7, MidpointRounding.AwayFromZero),
        ////                     sequence = x.sequence

        ////                 }).Skip(mySkip).Take(myTake).ToList();

        ////        mySkip += myTake;

        ////        myFirstFlag = true;

        ////        foreach (var q in elist)
        ////        {
        ////            if (myFirstFlag)
        ////            {
        ////                calledURL += q.latitude.ToString() + "," + q.longitude.ToString();
        ////                myFirstFlag = false;
        ////            }
        ////            else
        ////            {
        ////                calledURL += "," + q.latitude.ToString() + "," + q.longitude.ToString();
        ////            }
        ////        }

        ////        //int l1 = calledURL.Length;

        ////        try
        ////        {
        ////            elevationRequest = CreateRequest(calledURL);
        ////            elevationResponse = MakeRequest(elevationRequest);

        ////            if (elevationResponse != null)
        ////            {
        ////                ProcessResponse(elevationResponse);
        ////            }
        ////            else
        ////            {
        ////                return;
        ////            }
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            MessageBox.Show("Error -> " + ex.Message);
        ////            return;
        ////        }
        ////    }

        ////    MessageBus.Instance.Publish<ObservableCollection<ElevationCorrection>>(temp99);

        ////    MessageBox.Show("Finished");
        ////    return;
        ////}

        ////private static string CreateRequest(string queryString)
        ////{
        ////    string UrlRequest = "http://dev.virtualearth.net/REST/v1/Elevation/List?points=" +
        ////                         queryString +
        ////                         //"?output=xml" +
        ////                         "&key=" + myBingMapKey;
        ////    return (UrlRequest);
        ////}

        ////private static Response MakeRequest(string requestUrl)
        ////{
        ////    try
        ////    {
        ////        HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;

        ////        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
        ////        {
        ////            if (response.StatusCode != HttpStatusCode.OK)
        ////            {
        ////                throw new Exception(String.Format("Server error (HTTP {0}: {1}).", response.StatusCode, response.StatusDescription));
        ////            }

        ////            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));

        ////            object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());

        ////            Response jsonResponse = objResponse as Response;

        ////            return jsonResponse;
        ////        }
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        MessageBox.Show("Error in WebRequest -> " + e.Message);
        ////        return null;
        ////    }
        ////}

        ////private void ProcessResponse(Response elevationResponse)
        ////{
        ////    int statusCode = elevationResponse.StatusCode;
        ////    string statusDesc = elevationResponse.StatusDescription;
        ////    double bingAltitude = 0.0;

        ////    if (statusDesc.Equals("OK"))
        ////    {
        ////        int elvnbr = elevationResponse.ResourceSets[0].Resources.Length;

        ////        for (int j = 0; j < elvnbr; j++)
        ////        {
        ////            ElevationData elevation = (ElevationData)elevationResponse.ResourceSets[0].Resources[j];

        ////            //int[] elvs = elevation.Elevations;

        ////            foreach (var q in elevation.Elevations)
        ////            {
        ////                bingAltitude = Convert.ToDouble(q);
        ////                temp99.Add(new ElevationCorrection { corrected = bingAltitude, uncorrected = DataService.Instance.trackData[myIndex].altitude });

        ////                if (bingAltitude > DataService.Instance.trackData[myIndex].altitude)
        ////                {
        ////                    DataService.Instance.trackData[myIndex++].altitude = bingAltitude;
        ////                }
        ////            }
        ////        }
        ////    }
        ////    else
        ////    {
        ////        MessageBox.Show("Status -> " + statusDesc);
        ////    }
        ////}

        ////public bool getLocation()   //google elevation api
        ////{
        ////    string url1 = "https://maps.googleapis.com/maps/api/elevation/xml?locations=";
        ////    string url2 = "&sensor=true";
        ////    string calledURL = string.Empty;
        ////    string googleResponse = string.Empty;

        ////    int mySkip = 0;
        ////    int myTake = 500;
        ////    int trackcnt = 0;

        ////    string myPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        ////    string fileName = myPath + "\\googlexmldoc.txt";

        ////    List<LatLong> loclist = new List<LatLong>();

        ////    trackcnt = DataService.Instance.trackData.Count();

        ////    myIndex = 0;

        ////    while (mySkip < trackcnt)
        ////    {
        ////        loclist = (from x in DataService.Instance.trackData
        ////                   select new LatLong
        ////                   {
        ////                       Lat = Math.Round(x.latitude, 7, MidpointRounding.AwayFromZero),
        ////                       Lon = Math.Round(x.longitude, 7, MidpointRounding.AwayFromZero)

        ////                   }).Skip(mySkip).Take(myTake).ToList();

        ////        mySkip += myTake;

        ////        calledURL = url1 + "enc:";

        ////        calledURL += EncodeLatLong(loclist);     //convert the list of points to a encoded string

        ////        calledURL += url2;

        ////        //int l1 = calledURL.Length;

        ////        XmlDocument xmlElevation = new XmlDocument();

        ////        xmlElevation.Load(calledURL);

        ////        //xmlElevation.Save(fileName);

        ////        if ((googleResponse = xmlElevation.SelectSingleNode("ElevationResponse/status").InnerText) == "OK")
        ////        {
        ////            XmlNodeList xnlist = xmlElevation.SelectNodes("ElevationResponse/result");

        ////            if (xnlist.Count != loclist.Count)
        ////            {
        ////                MessageBox.Show("Count mismatch in google response: xnlist = " + xnlist.Count.ToString() + " loclist = " + loclist.Count.ToString());
        ////                return false;
        ////            }
        ////            else
        ////            {
        ////                foreach (XmlNode node in xnlist)
        ////                {
        ////                    double lat = Convert.ToDouble(node.SelectSingleNode("location/lat").InnerText);
        ////                    double lon = Convert.ToDouble(node.SelectSingleNode("location/lng").InnerText);
        ////                    double elv = Convert.ToDouble(node["elevation"].InnerText);

        ////                    temp99.Add(new ElevationCorrection { corrected = elv, uncorrected = DataService.Instance.trackData[myIndex].altitude });

        ////                    DataService.Instance.trackData[myIndex++].altitude = elv;
                            
        ////                    //to do: find the correct record, and update it. - first the observable collection, second update database
        ////                }
        ////            }
        ////        }
        ////        else
        ////        {
        ////            xmlElevation.Save(fileName);
        ////            MessageBox.Show("Error response from Google: " + googleResponse + Environment.NewLine + "Break");
        ////            return false;
        ////        }

        ////        //System.Threading.Thread.Sleep(750);    //don't read so fast
        ////    }

        ////    MessageBus.Instance.Publish<ObservableCollection<ElevationCorrection>>(temp99);

        ////    return true;
        ////}

        public bool getElevation()
        {
            string url = string.Empty;
            string lat = string.Empty;
            string lon = string.Empty;
            string gisDataResponse = string.Empty;
            double returnedLat = 0.0;
            double returnedLon = 0.0;
            double returndeEle = 0.0;

            List<ElevationUpdate> elevations = new List<ElevationUpdate>();

            string myPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string myHost = "http://gisdata.usgs.gov//XMLWebServices2/Elevation_Service.asmx/getElevation?";
            string myLayer = "NED.CONUS_NED_13E";

            if (DataService.Instance.myElevations != null)
            {
                DataService.Instance.myElevations.Clear();
            }
            else
            {
                DataService.Instance.myElevations = new AsyncObservableCollection<ElevationCorrection>();
            }

            XmlDocument xmlElevation = new XmlDocument();

            foreach (var q in DataService.Instance.trackData)
            {
                lat = Math.Round(q.latitude, 7, MidpointRounding.AwayFromZero).ToString();
                lon = Math.Round(q.longitude, 7, MidpointRounding.AwayFromZero).ToString();

                url = string.Format("{0}X_Value={1}&Y_Value={2}&Elevation_Units=METERS&Source_Layer={3}&Elevation_Only=YES HTTP/1.1", myHost, lon, lat, myLayer);

                try
                {
                    xmlElevation.Load(url);

                    XmlNodeList t9 = xmlElevation.GetElementsByTagName("Elevation_Query");

                    foreach (XmlNode node in t9)
                    {
                        returnedLon = Convert.ToDouble(node.Attributes[0].Value);
                        returnedLat = Convert.ToDouble(node.Attributes[1].Value);
                    }

                    XmlNodeList xnlist = xmlElevation.GetElementsByTagName("Elevation");

                    foreach (XmlNode node in xnlist)
                    {
                        returndeEle = Convert.ToDouble(node.InnerText);

                        //DataService.Instance.trackData[q.sequence].altitude = returndeEle;
                    }

                    elevations.Add(new ElevationUpdate      //for the database insert
                    {
                        altitude = returndeEle,
                        latitude = returnedLat,
                        longitude = returnedLon
                        //resolution = googleResolution
                    });


                    DataService.Instance.myElevations.Add(new ElevationCorrection   //for the correction graph plot
                    {
                        corrected = returndeEle,
                        uncorrected = DataService.Instance.trackData[myIndex].altitude,
                        timeStamp = DataService.Instance.trackData[myIndex].timeStamp,
                        distance = DataService.Instance.trackData[myIndex++].distance
                    });
                    
                }
                catch (XmlException ex)
                {
                    MessageBox.Show("Error in load: " + ex.Message);
                }
            }

            MessageBox.Show("End");

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });

            return true;
        }

        /// <summary>
        /// encoded a list of latlon objects into a string
        /// </summary>
        /// <param name="points">the list of latlon objects to encode</param>
        /// <returns>the encoded string</returns>
        public static string EncodeLatLong(List<LatLong> points)
        {
            int plat = 0;
            int plng = 0;
            int len = points.Count;

            StringBuilder encoded_points = new StringBuilder();

            for (int i = 0; i < len; ++i)
            {
                //Round to 5 decimal places and drop the decimal
                int late5 = (int)(points[i].Lat * 1e5);
                int lnge5 = (int)(points[i].Lon * 1e5);

                //encode the differences between the points
                encoded_points.Append(encodeSignedNumber(late5 - plat));
                encoded_points.Append(encodeSignedNumber(lnge5 - plng));

                //store the current point
                plat = late5;
                plng = lnge5;
            }
            return encoded_points.ToString();
        }

        /// <summary>
        /// Encode a signed number in the encode format.
        /// </summary>
        /// <param name="num">the signed number</param>
        /// <returns>the encoded string</returns>
        private static string encodeSignedNumber(int num)
        {
            int sgn_num = num << 1; //shift the binary value
            if (num < 0) //if negative invert
            {
                sgn_num = ~(sgn_num);
            }
            return (encodeNumber(sgn_num));
        }

        /// <summary>
        /// Encode an unsigned number in the encode format.
        /// </summary>
        /// <param name="num">the unsigned number</param>
        /// <returns>the encoded string</returns>
        private static string encodeNumber(int num)
        {
            int minASCII = 63;
            int binaryChunkSize = 5;

            StringBuilder encodeString = new StringBuilder();

            while (num >= 0x20)
            {
                //while another chunk follows
                encodeString.Append((char)((0x20 | (num & 0x1f)) + minASCII));
                //OR value with 0x20, convert to decimal and add 63
                num >>= binaryChunkSize; //shift to next chunk
            }
            encodeString.Append((char)(num + minASCII));

            return encodeString.ToString();
        }
    }
}
