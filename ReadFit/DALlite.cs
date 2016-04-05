using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ReadFit.FileModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace ReadFit
{
    class DALlite : IDataAccessLayer
    {
        private static MsgBoxService msgBoxobj;

        static DALlite()
        {
            msgBoxobj = new MsgBoxService();
        }

        private string getConnectionString()
        {
            //return @"Data Source=FitDataBase.db;Version=3;foreign keys=true";
            return @"Data Source=" + Properties.Settings.Default.DataBasePath + ";Version=3;foreign keys=true";
        }

        //private string getBackupString()
        //{
        //    return @"Data Source=BackupDataBase.db;Version=3;foreign keys=true";
        //}

        private string dbpath(string path)
        {
            return @"Data Source=" + path + ";Version=3;foreign keys=true";
        }

        //public ObservableCollection<T> getQuery<T>(string input, string whatData)

        public AsyncObservableCollection<T> getQuery<T>(int? input, string whatData)
        {
            //AsyncObservableCollection<T> mycoll = new AsyncObservableCollection<T>();

            List<T> myCollList = new List<T>();

            T value = default(T);

            string QueryString = null;

            switch (whatData)
            {
                case "GetTableNames":

                    QueryString = @"SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";

                    break;

                case "GetActivityData":

                    QueryString = "select * from Activity order by ActPkey desc";       //to get dates in descending order

                    //QueryString = @"SELECT Sport, Id, Name, UnitId, ProductId, ActPKey FROM Activity ORDER BY ActPKey DESC";

                    break;

                case "GetLapData":

                    QueryString = "select * from Lap order by LapPkey desc";    //to get dates in descending order

                    //QueryString = @"SELECT StartTime, StartPosLat, StartPosLong, EndPosLat, EndPosLong, TotalElapsedTime, TotalTimerTime, TotalDistance, " +
                    //    @"TotalCycles, TotalCalories, TotalFatCalories, AvgSpeed, MaxSpeed, TotalAscent, TotalDescent, AvgHeartRate, MaxHeartRate, " +
                    //    @"AvgCadence, MaxCadence, Intensity, Trigger, Sport, LapPKey FROM Lap ORDER BY LapPKey DESC";

                    break;

                case "GetLapDataByKey":

                    //QueryString = "select * from Lap where LapPKey=@Key";
                    QueryString = "select * from Lap where LapFK=@Key";

                    //QueryString = @"SELECT StartTime, StartPosLat, StartPosLong, EndPosLat, EndPosLong, TotalElapsedTime, TotalTimerTime, TotalDistance, " +
                    //    @"TotalCycles, TotalCalories, TotalFatCalories, AvgSpeed, MaxSpeed, TotalAscent, TotalDescent, AvgHeartRate, MaxHeartRate, " +
                    //    @"AvgCadence, MaxCadence, Intensity, Trigger, Sport, LapPKey FROM Lap WHERE LapPKey=@Key";

                    break;

                case "GetTrackData":

                    //QueryString = "select * from TrackPoint where TrackPKey=@Key";
                    QueryString = "select * from TrackPoint where TrackFK=@Key";

                    //QueryString = @"SELECT Timestamp, Latitude, Longitude, Distance, Altitude, Speed, HeartRate, Cadence, Temperature, Sequence FROM " +
                    //    @"TrackPoint WHERE TrackPKey=@Key";
                    
                    break;

                case "GetSessionData":

                    QueryString = "select * from Session order by SessionPkey desc";  //get all session records

                    //QueryString = @"SELECT Timestamp, StartTime, StartPosLat, StartPosLong, TotalElapsedTime, TotalTimerTime, TotalDistance, TotalCycles, " +
                    //    @"NecLat, NecLong, SwcLat, SwcLong, MessageIndex, TotalCalories, TotalFatCalories, AvgSpeed, MaxSpeed, AvgPower, MaxPower, " +
                    //    @"TotalAscent, TotalDescent, FirstLapIndex, NumberLaps, MyEvent, MyEventType, Sport, SubSport, AvgHeartRate, MaxHeartRate, " +
                    //    @"AvgCadence, MaxCadence, MyEventGroup, Trigger, SessionPKey FROM Session Where SessionPKey=@Key";

                    break;

                case "GetSessionDataByKey":

                    QueryString = "select * from Session where SessionFK=@Key";

                    break;

                case "GetElevationData":

                    QueryString = "select * from ElevationData where ElevationFK=@Key";   //get Elevation data by key

                    break;

                default:

                    throw new System.ArgumentException("Default in GetQuery Switch: " + whatData);
            }

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                {
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = QueryString;

                        if (input.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@Key", input);
                        }

                        conn.Open();

                        try
                        {
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    switch (whatData)
                                    {
                                        case "GetActivityData":
                                            myActivityRecord mar = new myActivityRecord()
                                            {
                                                sport = dr.GetString(0),
                                                id = dr.GetString(1),
                                                name = dr.GetString(2),
                                                unitId = dr.GetString(3),
                                                productId = dr.GetString(4),
                                                HasGpsData = (dr.GetInt32(5) == 1 ? true : false),
                                                timeStamp = dr.GetString(6),
                                                rowID = dr.GetInt32(7)
                                            };

                                            value = (T)Convert.ChangeType(mar, typeof(T));

                                            break;

                                        case "GetElevationData":
                                            ElevationUpdate elv = new ElevationUpdate()
                                            {
                                                latitude = dr.GetDouble(0),
                                                longitude = dr.GetDouble(1),
                                                altitude = dr.GetDouble(2),
                                                resolution = dr.GetDouble(3)
                                            };

                                            value = (T)Convert.ChangeType(elv, typeof(T));

                                            break;

                                        case "GetLapDataByKey":
                                        case "GetLapData":
                                            myLapRecord mlr = new myLapRecord()
                                            {
                                                primarKey = dr.GetInt32(0),
                                                startTime = dr.GetString(1),
                                                startPosLat = dr.GetDouble(2),
                                                startPosLong = dr.GetDouble(3),
                                                endPosLat = dr.GetDouble(4),
                                                endPosLong = dr.GetDouble(5),
                                                totalElaspedTime = dr.GetString(6),
                                                totalTimerTime = dr.GetString(7),
                                                totalDistance = dr.GetDouble(8),
                                                totalCycles = dr.GetInt32(9),
                                                totalCalories = dr.GetInt32(10),
                                                totalFatCalories = dr.GetInt32(11),
                                                avgSpeed = dr.GetDouble(12),
                                                maxSpeed = dr.GetDouble(13),
                                                avgPower = dr.GetDouble(14),
                                                maxPower = dr.GetDouble(15),
                                                totalAscent = dr.GetDouble(16),
                                                totalDesecent = dr.GetDouble(17),
                                                avgHeartRate = dr.GetInt32(18),
                                                maxHeartRate = dr.GetInt32(19),
                                                avgCadence = dr.GetInt32(20),
                                                maxCadence = dr.GetInt32(21),
                                                intensity = dr.GetInt32(22),
                                                lapTrigger = dr.GetString(23),
                                                sport = dr.GetString(24),
                                                lpkey = dr.GetString(25)
                                            };

                                            value = (T)Convert.ChangeType(mlr, typeof(T));

                                            break;

                                        case "GetTrackData":
                                            myFitRecord mfr = new myFitRecord()
                                            {
                                                timeStamp = dr.GetString(0),
                                                latitude = dr.GetDouble(1),
                                                longitude = dr.GetDouble(2),
                                                distance = dr.GetDouble(3),
                                                altitude = dr.GetDouble(4),
                                                speed = dr.GetDouble(5),
                                                heartRate = dr.GetInt32(6),
                                                cadence = dr.GetInt32(7),
                                                temperature = dr.GetInt32(8),
                                                sequence = dr.GetInt32(9)
                                            };

                                            value = (T)Convert.ChangeType(mfr, typeof(T));

                                            break;

                                        case "GetSessionDataByKey":
                                        case "GetSessionData":
                                            mySessionRecord msr = new mySessionRecord()
                                            {
                                                timeStamp = dr.GetString(0),
                                                startTime = dr.GetString(1),
                                                startPositionLat = dr.GetDouble(2),
                                                startPositionLong = dr.GetDouble(3),
                                                totalElapsedTime = dr.GetString(4),
                                                totalTimerTime = dr.GetString(5),
                                                totalDistance = dr.GetDouble(6),
                                                totalCycles = dr.GetInt32(7),
                                                necLat = dr.GetDouble(8),
                                                necLong = dr.GetDouble(9),
                                                swcLat = dr.GetDouble(10),
                                                swcLong = dr.GetDouble(11),
                                                messageIndex = dr.GetInt32(12),
                                                totalCalories = dr.GetInt32(13),
                                                totalFatCalories = dr.GetInt32(14),
                                                avgSpeed = dr.GetDouble(15),
                                                maxSpeed = dr.GetDouble(16),
                                                avgPower = dr.GetDouble(17),
                                                maxPower = dr.GetDouble(18),
                                                totalAscent = dr.GetDouble(19),
                                                totalDescent = dr.GetDouble(20),
                                                firstLapIndex = dr.GetInt32(21),
                                                numLaps = dr.GetInt32(22),
                                                myEvent = dr.GetString(23),
                                                myEventType = dr.GetString(24),
                                                sport = dr.GetString(25),
                                                subSport = dr.GetString(26),
                                                avgHeartRate = dr.GetInt32(27),
                                                maxHeartRate = dr.GetInt32(28),
                                                avgCadence = dr.GetInt32(29),
                                                maxCadence = dr.GetInt32(30),
                                                myEventGroup = dr.GetString(31),
                                                trigger = dr.GetString(32),
                                                myskey = dr.GetString(33),
                                                rowId = dr.GetInt32(34)
                                            };

                                            value = (T)Convert.ChangeType(msr, typeof(T));

                                            break;

                                        default:
                                            value = (T)Convert.ChangeType(dr.GetString(0), typeof(T));

                                            break;
                                    }

                                    //mycoll.Add(value);
                                    myCollList.Add(value);

                                }   //while
                            }   //reader
                        }
                        catch (SQLiteException ex)
                        {
                            msgBoxobj.ShowNotification("Error in db read -> " + ex.Message);
                            return null;
                        }
                    }   //cmd
                }   //conn
            }
            catch (Exception ex)
            {
                msgBoxobj.ShowNotification("Error -> " + ex.Message);
                return null;
            }

            //return mycoll;
            return myCollList.AsAsyncObservableCollection();
        }

        public T getUserData<T, TInput>(TInput input, string whatData)
        {
            string QueryString = null;

            string inputStr = string.Empty;
            int? inputInt = 0;

            if (input is string)
            {
                inputStr = input as string;
            }
            else if (input is int)
            {
                inputInt = input as int?;
            }
            
            T query = default(T);

            switch (whatData)
            {
                case "FileNameExist":

                    QueryString = @"SELECT COUNT(*) FROM ProcessedFiles WHERE FileName=@FileName";

                    query = (T)Convert.ChangeType(((getSqlData<int, string>(QueryString, "@FileName", inputStr) > 0) ? true : false), typeof(T));

                    //query = (T)Convert.ChangeType(((Convert.ToInt32(getSqlData(QueryString, "@FileName", input, true)) > 0) ? true : false), typeof(T));
                    break;

                case "ElevationDataExist":

                    QueryString = @"SELECT COUNT(*) FROM ElevationData WHERE ElevationFK=@Key";

                    query = (T)Convert.ChangeType(((getSqlData<int, int?>(QueryString, "@Key", inputInt) > 0) ? true : false), typeof(T));
                    break;

                case "ProductId":

                    QueryString = @"SELECT ProductId FROM Activity WHERE ActivityFK=@Key";     //use rowid

                    query = (T)Convert.ChangeType(getSqlData<int, int?>(QueryString, "@Key", inputInt), typeof(T));

                    break;

                case "NumberLaps":

                    QueryString = @"SELECT NumberLaps FROM Session Where SessionFK=@Key";     //use rowid

                    query = (T)Convert.ChangeType(getSqlData<int, int?>(QueryString, "@Key", inputInt), typeof(T));

                    break;

                case "ProcessedFileCount":

                    QueryString = @"SELECT COUNT(*) FROM ProcessedFiles";

                    query = (T)Convert.ChangeType(getSqlData<int, string>(QueryString, null, null), typeof(T));

                    break;

                case "GetRowId":

                    QueryString = @"SELECT Pkey FROM ProcessedFiles WHERE FileName=@Key";

                    query = (T)Convert.ChangeType(getSqlData<int, string>(QueryString, "@Key", inputStr), typeof(T));

                    break;

                case "ActivityCount":

                    QueryString = @"SELECT COUNT(*) FROM Activity";

                    query = (T)Convert.ChangeType(getSqlData<int, string>(QueryString, null, null), typeof(T));

                    break;

                case "LapCount":

                    QueryString = @"SELECT COUNT(*) FROM Lap";

                    query = (T)Convert.ChangeType(getSqlData<int, string>(QueryString, null, null), typeof(T));

                    break;

                case "SessionCount":

                    QueryString = @"SELECT COUNT(*) FROM Session";

                    query = (T)Convert.ChangeType(getSqlData<int, string>(QueryString, null, null), typeof(T));

                    break;

                case "TrackPointCount":

                    QueryString = @"SELECT COUNT(*) FROM TrackPoint";

                    query = (T)Convert.ChangeType(getSqlData<int, string>(QueryString, null, null), typeof(T));

                    break;

                case "TrackCountByKey":

                    QueryString = @"SELECT COUNT(*) FROM TrackPoint WHERE TrackFk=@Key";      //use rowid

                    query = (T)Convert.ChangeType(getSqlData<int, int?>(QueryString, "@Key", inputInt), typeof(T));

                    break;

                default:

                    throw new System.ArgumentException("Error in GetUserData Switch");
            }

            return query;
        }

        private T getSqlData<T, TInput>(string QueryString, string p1, TInput p2)
        {
            T returnValue = default(T);

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = QueryString;

                        if (p2 is string)
                        {
                            if (p2 != null)
                            {
                                cmd.Parameters.Add(new SQLiteParameter(p1) { Value = p2 });
                            }
                        }
                        else if (p2 is int?)
                        {
                            if (p2 != null)
                            {
                                cmd.Parameters.Add(new SQLiteParameter(p1) { Value = p2 });
                            }
                        }
                        else
                        {
                            msgBoxobj.ShowNotification("Error in parameter 2: " + p2.ToString());
                        }

                        cmd.CommandType = System.Data.CommandType.Text;

                        try
                        {
                            returnValue = (T)Convert.ChangeType(cmd.ExecuteScalar(), typeof(T));
                        }
                        catch (SQLiteException ex)
                        {
                            msgBoxobj.ShowNotification("Error in db read single -> " + ex.Message);
                            return returnValue;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                msgBoxobj.ShowNotification("Error in db read single -> " + ex.Message);
                return returnValue;
            }

            return returnValue;
        }

        /// <summary>
        /// Read the SQLite database
        /// </summary>
        /// <param name="QueryString"></param>
        /// <param name="parm1">Command Parameter</param>
        /// <param name="parm2">Command Parameter</param>
        /// <param name="single">Returns single value if true</param>
        /// <returns></returns>
        //private object getSqlData(string QueryString, string parm1, string parm2, bool single)
        //{
        //    object retvalue = null;

        //    using (var conn = new SQLiteConnection(getConnectionString()))
        //    {
        //        conn.Open();

        //        using (var cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = QueryString;

        //            if (parm1 != null)
        //            {
        //                cmd.Parameters.Add(new SQLiteParameter(parm1) { Value = parm2 });
        //            }

        //            cmd.CommandType = System.Data.CommandType.Text;

        //            if (single)
        //            {
        //                retvalue = cmd.ExecuteScalar();

        //                return retvalue;
        //            }
        //            else
        //            {
        //                using (var dr = cmd.ExecuteReader())
        //                {
        //                    while (dr.Read())
        //                    {
        //                        retvalue = new mySessionRecord()
        //                        {
        //                            timeStamp = dr.GetString(0),
        //                            startTime = dr.GetString(1),
        //                            startPositionLat = dr.GetDouble(2),
        //                            startPositionLong = dr.GetDouble(3),
        //                            totalElapsedTime = dr.GetString(4),
        //                            totalTimerTime = dr.GetString(5),
        //                            totalDistance = dr.GetDouble(6),
        //                            totalCycles = dr.GetInt32(7),
        //                            necLat = dr.GetDouble(8),
        //                            necLong = dr.GetDouble(9),
        //                            swcLat = dr.GetDouble(10),
        //                            swcLong = dr.GetDouble(11),
        //                            messageIndex = dr.GetInt32(12),
        //                            totalCalories = dr.GetInt32(13),
        //                            totalFatCalories = dr.GetInt32(14),
        //                            avgSpeed = dr.GetDouble(15),
        //                            maxSpeed = dr.GetDouble(16),
        //                            avgPower = dr.GetDouble(17),
        //                            maxPower = dr.GetDouble(18),
        //                            totalAscent = dr.GetDouble(19),
        //                            totalDescent = dr.GetDouble(20),
        //                            firstLapIndex = dr.GetInt32(21),
        //                            numLaps = dr.GetInt32(22),
        //                            myEvent = dr.GetString(23),
        //                            myEventType = dr.GetString(24),
        //                            sport = dr.GetString(25),
        //                            subSport = dr.GetString(26),
        //                            avgHeartRate = dr.GetInt32(27),
        //                            maxHeartRate = dr.GetInt32(28),
        //                            avgCadence = dr.GetInt32(29),
        //                            maxCadence = dr.GetInt32(30),
        //                            myEventGroup = dr.GetString(31),
        //                            trigger = dr.GetString(32),
        //                            myskey = dr.GetString(33)
        //                        };
        //                    }
        //                }

        //                return retvalue;
        //            }
        //        }
        //    }
        //}

        public T deleteRecords<T>(IEnumerable<int> recordsToDelete)
        {
            T returnValue = default(T);

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                var recId = new SQLiteParameter("@id");

                                cmd.CommandText = "DELETE FROM ProcessedFiles WHERE Pkey=@id";

                                cmd.Parameters.Add(recId);

                                foreach (var q in recordsToDelete)
                                {
                                    recId.Value = q;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            returnValue = (T)Convert.ChangeType(true, typeof(T));
                        }
                        catch (SQLiteException ey)
                        {
                            msgBoxobj.ShowNotification("Error in delete of record - " + ey.Message);
                            transaction.Rollback();
                            return (T)Convert.ChangeType(false, typeof(T));
                        }
                        catch (Exception ex)
                        {
                            msgBoxobj.ShowNotification("Error in delete of record - " + ex.Message);
                            transaction.Rollback();
                            return (T)Convert.ChangeType(false, typeof(T));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msgBoxobj.ShowNotification("Error in delete of record - " + ex.Message);
            }

            return returnValue;
        }

        //public T deleteRecords<T>(string Id, string tableName)
        //{
        //    int records = 0;
        //    //bool myFlag = false;
        //    T returnValue = default(T);

        //    if (Id == null)
        //    {
        //        if (DataService.Instance.DbTableNames.Contains(tableName))
        //        {
        //            string QueryString = "DELETE FROM " + tableName;    //delete all records from tablename

        //            using (var conn = new SQLiteConnection(getConnectionString()))
        //            {
        //                conn.Open();

        //                using (var transaction = conn.BeginTransaction())
        //                {
        //                    using (var cmd = new SQLiteCommand(QueryString, conn))
        //                    {
        //                        records = cmd.ExecuteNonQuery();
        //                    }

        //                    transaction.Commit();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            throw new System.ArgumentException("Invalid tablename in deleteAllRecords: " + tableName);
        //        }

        //        return returnValue = (T)Convert.ChangeType(records, typeof(T));
        //    }
        //    else
        //    {
        //        try
        //        {
        //            using (var conn = new SQLiteConnection(getConnectionString()))
        //            {
        //                conn.Open();

        //                using (var cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = @"DELETE FROM ProcessedFiles WHERE FileName=@Id";     //Delete FileName
        //                    cmd.Parameters.Add(new SQLiteParameter("@Id") { Value = Id });

        //                    cmd.ExecuteNonQuery();

        //                    cmd.CommandText = @"DELETE FROM Activity WHERE ActPkey= @Id";           //Delete all records with that Key - using the forign key
        //                    cmd.Parameters.Add(new SQLiteParameter("@Id") { Value = Id });

        //                    cmd.ExecuteNonQuery();
        //                }
        //            }

        //            return (T)Convert.ChangeType(true, typeof(T));
        //        }
        //        catch (SQLiteException ex)
        //        {
        //            msgBoxobj.ShowNotification("Error in delete - " + ex.Message);

        //            return (T)Convert.ChangeType(false, typeof(T));
        //        }
        //    }
        //}

        public Dictionary<int, IEnumerable<myFitRecord>> GetTracksByLap(int PrimaryKey)
        {
            Dictionary<int, IEnumerable<myFitRecord>> dict = new Dictionary<int, IEnumerable<myFitRecord>>();

            AsyncObservableCollection<myFitRecord> testTrack = getQuery<myFitRecord>(PrimaryKey, "GetTrackData");

            AsyncObservableCollection<myLapRecord> lapRecs = getQuery<myLapRecord>(PrimaryKey, "GetLapDataByKey");

            List<string> lpStTm = (from x in lapRecs
                                   select x.startTime).ToList();

            List<int> lstSeqs = new List<int>();

            foreach (var q in lpStTm)
            {
                var rt = (from x in testTrack
                          where x.timeStamp == q
                          select x.sequence);

                lstSeqs.AddRange(rt);
            }

            List<myFitRecord> temp;

            int cnt = lapRecs.Count();

            for (int p = 0; p < cnt; p++)
            {
                if (p + 1 < cnt)
                {
                    temp = (testTrack.Skip(lstSeqs[p]).Take(lstSeqs[p + 1] - lstSeqs[p])).ToList<myFitRecord>();
                }
                else
                {
                    if (lstSeqs.Count() < 1)
                    {
                        temp = (testTrack.Take(testTrack.Count())).ToList<myFitRecord>();
                    }
                    else
                    {
                        temp = (testTrack.Skip(lstSeqs[p]).Take(testTrack.Count())).ToList<myFitRecord>();
                    }
                }

                dict.Add(p + 1, temp);
            }

            return dict;
        }

        public bool loadObservableCollection(int index)
        {
            DataService.Instance.activityData = getQuery<myActivityRecord>(null, "GetActivityData");

            if (DataService.Instance.activityData.IsNullOrEmpty())
            {
                //msgBoxobj.ShowNotification("error in fetching activity data");
                return false;
            }

            if (index > DataService.Instance.activityData.Count - 1 || index < 0)
            {
                msgBoxobj.ShowNotification("Index out of range");
                return false;
            }

            DataService.Instance.lapData = getQuery<myLapRecord>(null, "GetLapData");

            if (DataService.Instance.lapData.IsNullOrEmpty())
            {
                //msgBoxobj.ShowNotification("error in fetching lap data");
                return false;
            }

            DataService.Instance.sessionData = getQuery<mySessionRecord>(null, "GetSessionData");

            if (DataService.Instance.sessionData.IsNullOrEmpty())
            {
                //msgBoxobj.ShowNotification("error in fetching session data");
                return false;
            }

            //if (!DataService.Instance.activityData.IsNullOrEmpty())
            //{
                int rowid = DataService.Instance.activityData[index].rowID;
                DataService.Instance.trackData = getQuery<myFitRecord>(rowid, "GetTrackData");
                DataService.Instance.STPTM = StopTimes.getStopTimes();

                if (DataService.Instance.STPTM.Count > 1)
                {
                    DataService.Instance.TabHeaderStp = string.Format("Stop Time ({0})", (DataService.Instance.STPTM.Count - 1).ToString());
                }

                return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        public string generateKey(string filen)
        {
            string gn = Path.GetFileNameWithoutExtension(filen);

            int[] test1 = gn.Split('-').Select(n => Convert.ToInt32(n)).ToArray();

            System.DateTime dt = new System.DateTime(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5]);

            return dt.ToString("s");
        }

        //public bool loadObservableCollection(int key)
        //{
        //    //var keyValue = key as myActivityRecord;

        //    DataService.Instance.activityData = getQuery<myActivityRecord>(null, "GetActivityData");
        //    DataService.Instance.lapData = getQuery<myLapRecord>(null, "GetLapData");

        //    if (DataService.Instance.activityData.Count > 0)
        //    {
        //        //string initialKey = keyValue.timeStamp;
        //        string initialKey = DataService.Instance.activityData[key].timeStamp;
        //        DataService.Instance.trackData = getQuery<myFitRecord>(initialKey, "GetTrackData");
        //        DataService.Instance.STPTM = StopTimes.getStopTimes();

        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public bool BackupDatabase(string filename)
        {
            bool myReturn = false;

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                using (var back = new SQLiteConnection(dbpath(filename)))
                {
                    conn.Open();
                    back.Open();

                    conn.BackupDatabase(back, "main", "main", -1, null, -1);

                    myReturn = true;
                }
            }
            catch (SQLiteException ex)
            {
                msgBoxobj.ShowNotification("Error in DataBase Backup: " + ex.Message + " " + ex.InnerException);
                myReturn = false;
            }

            return myReturn;
        }

        public bool compactDatabase()
        {
            bool myReturn = false;

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))     //Insert User
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "VACUUM";
                        cmd.ExecuteNonQuery();

                        myReturn = true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                msgBoxobj.ShowNotification("Error in compact: " + ex.Message);
                myReturn = false;
            }

            return myReturn;
        }

        public int insertTest(string what)
        {
            int rowid = 0;

            using (var conn = new SQLiteConnection(getConnectionString()))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        var idfield = new SQLiteParameter("@id");

                        cmd.CommandText = "insert into test (idfield) values (@id)";
                        cmd.Parameters.Add(idfield);
                        idfield.Value = what;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "select last_insert_rowid()";     //get the last row id

                        var t1 = (long)cmd.ExecuteScalar();

                        rowid = (int)t1;
                    }

                    transaction.Commit();
                }
            }

            return rowid;
        }

        public bool insertFitRecords(DecodeRecords fitValues)
        {
            int rowid = 0;

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            //processed file name records
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = @"insert into ProcessedFiles (FileName) values (@FileName)";
                                cmd.Parameters.Add(new SQLiteParameter("@FileName") { Value = fitValues.myFitKey });

                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "select last_insert_rowid()";     //get the last row id

                                var t1 = (long)cmd.ExecuteScalar();

                                rowid = (int)t1;
                            }

                            //activity records
                            using (var cmd = conn.CreateCommand())
                            {
                                var sprt = new SQLiteParameter("@sport");
                                var idn = new SQLiteParameter("@id");
                                var namen = new SQLiteParameter("@name");
                                var unitidn = new SQLiteParameter("@unitid");
                                var prodid = new SQLiteParameter("@productid");
                                var gdata = new SQLiteParameter("@gpsdata");
                                var pkey = new SQLiteParameter("@key");
                                var fkey = new SQLiteParameter("@fkey");

                                cmd.CommandText = @"INSERT INTO Activity " +
                                        @"(Sport, Id, Name, UnitId, ProductId, GPSData, ActPKey, ActivityFK) VALUES " +
                                        @"(@sport, @id, @name, @unitid, @productid, @gpsdata, @key, @fkey)";

                                cmd.Parameters.Add(sprt);
                                cmd.Parameters.Add(idn);
                                cmd.Parameters.Add(namen);
                                cmd.Parameters.Add(unitidn);
                                cmd.Parameters.Add(prodid);
                                cmd.Parameters.Add(gdata);
                                cmd.Parameters.Add(pkey);
                                cmd.Parameters.Add(fkey);

                                foreach (var q in fitValues.myFitAct)
                                {
                                    sprt.Value = fitValues.myFitSport;
                                    idn.Value = q.serialNumber;

                                    switch (q.product)
                                    {
                                        case 484:
                                            namen.Value = "Garmin 305";
                                            break;
                                        case 1169:
                                            namen.Value = "Edge 800";
                                            break;
                                        default:
                                            namen.Value = q.serialNumber;
                                            break;
                                    }

                                    unitidn.Value = q.serialNumber;
                                    prodid.Value = q.product;
                                    gdata.Value = (q.HasGpsData ? 1 : 0);   //Sqlite stores bool as integer. True = 1, False = 0
                                    pkey.Value = fitValues.myFitKey;
                                    fkey.Value = rowid;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            //lap records
                            using (var cmd = conn.CreateCommand())
                            {
                                var l1starttime = new SQLiteParameter("@starttm");
                                var l1startposlat = new SQLiteParameter("@startposlat");
                                var l1startposlon = new SQLiteParameter("@startposlon");
                                var l1endposlat = new SQLiteParameter("@endposlat");
                                var l1endposlon = new SQLiteParameter("@endposlon");
                                var l1totalelapsed = new SQLiteParameter("@totalelapsed");
                                var l1totaltimer = new SQLiteParameter("@totaltimer");
                                var l1totaldist = new SQLiteParameter("@totaldist");
                                var l1totalcyc = new SQLiteParameter("@totalcyc");
                                var l1totalcal = new SQLiteParameter("@totalcal");
                                var l1totalfatcal = new SQLiteParameter("@totalfatcal");
                                var l1avgspeed = new SQLiteParameter("@avgspd");
                                var l1maxspeed = new SQLiteParameter("@maxspd");
                                var l1avgpower = new SQLiteParameter("@avgpower");
                                var l1maxpower = new SQLiteParameter("@maxpower");
                                var l1totalasc = new SQLiteParameter("@totalascent");
                                var l1totaldsc = new SQLiteParameter("@totaldescent");
                                var l1avghr = new SQLiteParameter("@avghr");
                                var l1maxhr = new SQLiteParameter("@maxhr");
                                var l1avgcad = new SQLiteParameter("@avgcad");
                                var l1maxcad = new SQLiteParameter("@maxcad");
                                var l1intensity = new SQLiteParameter("@intensity");
                                var l1trigger = new SQLiteParameter("@trigger");
                                var l1sport = new SQLiteParameter("@sport");
                                var l1pkey = new SQLiteParameter("@key");
                                var l1fkey = new SQLiteParameter("@fkey");

                                cmd.CommandText = "INSERT INTO Lap " +
                                        "(StartTime, StartPosLat, StartPosLong, EndPosLat, EndPosLong, TotalElapsedTime, TotalTimerTime, TotalDistance, TotalCycles," +
                                        "TotalCalories, TotalFatCalories, AvgSpeed, MaxSpeed, AvgPower, MaxPower, TotalAscent, TotalDescent, " +
                                        "AvgHeartRate, MaxHeartRate, AvgCadence, " +
                                        "MaxCadence, Intensity, Trigger, Sport, LapPKey, LapFK) VALUES " +
                                        "(@starttm, @startposlat, @startposlon, @endposlat, @endposlon, @totalelapsed, @totaltimer, @totaldist, @totalcyc, " +
                                        "@totalcal, @totalfatcal, @avgspd, @maxspd, @avgpower, @maxpower, @totalascent, @totaldescent, " +
                                        "@avghr, @maxhr, @avgcad, @maxcad, @intensity, " +
                                        "@trigger, @sport, @key, @fkey)";

                                cmd.Parameters.Add(l1starttime);
                                cmd.Parameters.Add(l1startposlat);
                                cmd.Parameters.Add(l1startposlon);
                                cmd.Parameters.Add(l1endposlat);
                                cmd.Parameters.Add(l1endposlon);
                                cmd.Parameters.Add(l1totalelapsed);
                                cmd.Parameters.Add(l1totaltimer);
                                cmd.Parameters.Add(l1totaldist);
                                cmd.Parameters.Add(l1totalcyc);
                                cmd.Parameters.Add(l1totalcal);
                                cmd.Parameters.Add(l1totalfatcal);
                                cmd.Parameters.Add(l1avgspeed);
                                cmd.Parameters.Add(l1maxspeed);
                                cmd.Parameters.Add(l1avgpower);
                                cmd.Parameters.Add(l1maxpower);
                                cmd.Parameters.Add(l1totalasc);
                                cmd.Parameters.Add(l1totaldsc);
                                cmd.Parameters.Add(l1avghr);
                                cmd.Parameters.Add(l1maxhr);
                                cmd.Parameters.Add(l1avgcad);
                                cmd.Parameters.Add(l1maxcad);
                                cmd.Parameters.Add(l1intensity);
                                cmd.Parameters.Add(l1trigger);
                                cmd.Parameters.Add(l1sport);
                                cmd.Parameters.Add(l1pkey);
                                cmd.Parameters.Add(l1fkey);

                                foreach (var q in fitValues.myFitLap)
                                {
                                    l1starttime.Value = q.startTime;
                                    l1startposlat.Value = q.startPosLat;
                                    l1startposlon.Value = q.startPosLong;
                                    l1endposlat.Value = q.endPosLat;
                                    l1endposlon.Value = q.endPosLong;
                                    l1totalelapsed.Value = q.totalElaspedTime;
                                    l1totaltimer.Value = q.totalTimerTime;
                                    l1totaldist.Value = q.totalDistance;
                                    l1totalcyc.Value = q.totalCycles;
                                    l1totalcal.Value = q.totalCalories;
                                    l1totalfatcal.Value = q.totalFatCalories;
                                    l1avgspeed.Value = q.avgSpeed;
                                    l1maxspeed.Value = q.maxSpeed;
                                    l1avgpower.Value = q.avgPower;
                                    l1maxpower.Value = q.maxPower;
                                    l1totalasc.Value = q.totalAscent;
                                    l1totaldsc.Value = q.totalDesecent;
                                    l1avghr.Value = q.avgHeartRate;
                                    l1maxhr.Value = q.maxHeartRate;
                                    l1avgcad.Value = q.avgCadence;
                                    l1maxcad.Value = q.maxCadence;
                                    l1intensity.Value = q.intensity;
                                    l1trigger.Value = q.lapTrigger;
                                    l1sport.Value = q.sport;
                                    l1pkey.Value = fitValues.myFitKey;
                                    l1fkey.Value = rowid;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            //trackpoint records
                            using (var cmd = conn.CreateCommand())
                            {
                                var tstamp = new SQLiteParameter("@timestamp");
                                var lat = new SQLiteParameter("@latitude");
                                var lon = new SQLiteParameter("@longitude");
                                var dist = new SQLiteParameter("@distance");
                                var altitude = new SQLiteParameter("@altitude");
                                var speed = new SQLiteParameter("@speed");
                                var hrate = new SQLiteParameter("@heartrate");
                                var cadence = new SQLiteParameter("@cadence");
                                var temperature = new SQLiteParameter("@temperature");
                                var seq = new SQLiteParameter("@sequence");
                                //var pkey = new SQLiteParameter("@key");
                                var fkey = new SQLiteParameter("@fkey");

                                cmd.CommandText = @"INSERT INTO TrackPoint " +
                                    @"(Timestamp, Latitude, Longitude, Distance, Altitude, Speed, HeartRate, Cadence, Temperature, Sequence, TrackFK) VALUES " +
                                    @"(@timestamp, @latitude, @longitude, @distance, @altitude, @speed, @heartrate, @cadence, @temperature, @sequence, @fkey)";

                                cmd.Parameters.Add(tstamp);
                                cmd.Parameters.Add(lat);
                                cmd.Parameters.Add(lon);
                                cmd.Parameters.Add(dist);
                                cmd.Parameters.Add(altitude);
                                cmd.Parameters.Add(speed);
                                cmd.Parameters.Add(hrate);
                                cmd.Parameters.Add(cadence);
                                cmd.Parameters.Add(temperature);
                                cmd.Parameters.Add(seq);
                                //cmd.Parameters.Add(pkey);
                                cmd.Parameters.Add(fkey);

                                foreach (var q in fitValues.myFitTrack)
                                {
                                    tstamp.Value = q.timeStamp;
                                    lat.Value = q.latitude;
                                    lon.Value = q.longitude;
                                    dist.Value = q.distance;
                                    altitude.Value = q.altitude;
                                    speed.Value = q.speed;
                                    hrate.Value = q.heartRate;
                                    cadence.Value = q.cadence;
                                    temperature.Value = q.temperature;
                                    seq.Value = q.sequence;
                                    //pkey.Value = fitValues.myFitKey;
                                    fkey.Value = rowid;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            //session records
                            using (var cmd = conn.CreateCommand())
                            {
                                var timestamp = new SQLiteParameter("@timestamp");
                                var starttm = new SQLiteParameter("@starttime");
                                var startposlat = new SQLiteParameter("@startposlat");
                                var startposlon = new SQLiteParameter("@startposlong");
                                var totalelaptm = new SQLiteParameter("@totalelapsedtime");
                                var totaltimertm = new SQLiteParameter("@totaltimertime");
                                var totaldist = new SQLiteParameter("@totaldistance");
                                var totalcycles = new SQLiteParameter("@totalcycles");
                                var neclat = new SQLiteParameter("@neclat");
                                var neclon = new SQLiteParameter("@neclong");
                                var swclat = new SQLiteParameter("@swclat");
                                var swclon = new SQLiteParameter("@swclong");
                                var msgindx = new SQLiteParameter("@messageindex");
                                var totcal = new SQLiteParameter("@totalcalories");
                                var totfatcal = new SQLiteParameter("@totalfatcalories");
                                var avgspd = new SQLiteParameter("@avgspeed");
                                var maxspd = new SQLiteParameter("@maxspeed");
                                var avgpow = new SQLiteParameter("@avgpower");
                                var maxpow = new SQLiteParameter("@maxpower");
                                var totalascent = new SQLiteParameter("@totalascent");
                                var totaldescent = new SQLiteParameter("@totaldescent");
                                var firstlapidx = new SQLiteParameter("@firstlapindex");
                                var nbrlaps = new SQLiteParameter("@numberlaps");
                                var myevent = new SQLiteParameter("@myevent");
                                var myeventtype = new SQLiteParameter("@myeventtype");
                                var sport = new SQLiteParameter("@sport");
                                var subsport = new SQLiteParameter("@subsport");
                                var avghr = new SQLiteParameter("@avgheartrate");
                                var maxhr = new SQLiteParameter("@maxheartrate");
                                var avgcad = new SQLiteParameter("@avgcadence");
                                var maxcad = new SQLiteParameter("@maxcadence");
                                var myeventgroup = new SQLiteParameter("@myeventgroup");
                                var trigger = new SQLiteParameter("@trigger");
                                var pkey = new SQLiteParameter("@key");
                                var fkey = new SQLiteParameter("@fkey");

                                cmd.CommandText = "INSERT INTO Session " + " VALUES " +
                                    "(@timestamp, @starttime, @startposlat, @startposlong, @totalelapsedtime, @totaltimertime, @totaldistance, @totalcycles, " +
                                    "@neclat, @neclong, @swclat, @swclong, @messageindex, @totalcalories, @totalfatcalories, @avgspeed, @maxspeed, @avgpower, " +
                                    "@maxpower, @totalascent, @totaldescent, @firstlapindex, @numberlaps, @myevent, @myeventtype, @sport, @subsport, " +
                                    "@avgheartrate, @maxheartrate, @avgcadence, @maxcadence, @myeventgroup, @trigger, @key, @fkey)";

                                cmd.Parameters.Add(timestamp);
                                cmd.Parameters.Add(starttm);
                                cmd.Parameters.Add(startposlat);
                                cmd.Parameters.Add(startposlon);
                                cmd.Parameters.Add(totalelaptm);
                                cmd.Parameters.Add(totaltimertm);
                                cmd.Parameters.Add(totaldist);
                                cmd.Parameters.Add(totalcycles);
                                cmd.Parameters.Add(neclat);
                                cmd.Parameters.Add(neclon);
                                cmd.Parameters.Add(swclat);
                                cmd.Parameters.Add(swclon);
                                cmd.Parameters.Add(msgindx);
                                cmd.Parameters.Add(totcal);
                                cmd.Parameters.Add(totfatcal);
                                cmd.Parameters.Add(avgspd);
                                cmd.Parameters.Add(maxspd);
                                cmd.Parameters.Add(avgpow);
                                cmd.Parameters.Add(maxpow);
                                cmd.Parameters.Add(totalascent);
                                cmd.Parameters.Add(totaldescent);
                                cmd.Parameters.Add(firstlapidx);
                                cmd.Parameters.Add(nbrlaps);
                                cmd.Parameters.Add(myevent);
                                cmd.Parameters.Add(myeventtype);
                                cmd.Parameters.Add(sport);
                                cmd.Parameters.Add(subsport);
                                cmd.Parameters.Add(avghr);
                                cmd.Parameters.Add(maxhr);
                                cmd.Parameters.Add(avgcad);
                                cmd.Parameters.Add(maxcad);
                                cmd.Parameters.Add(myeventgroup);
                                cmd.Parameters.Add(trigger);
                                cmd.Parameters.Add(pkey);
                                cmd.Parameters.Add(fkey);

                                foreach (var q in fitValues.myFitSession)
                                {
                                    timestamp.Value = q.timeStamp;
                                    starttm.Value = q.startTime;
                                    startposlat.Value = q.startPositionLat;
                                    startposlon.Value = q.startPositionLong;
                                    totalelaptm.Value = q.totalElapsedTime;
                                    totaltimertm.Value = q.totalTimerTime;
                                    totaldist.Value = q.totalDistance;
                                    totalcycles.Value = q.totalCycles;
                                    neclat.Value = q.necLat;
                                    neclon.Value = q.necLong;
                                    swclat.Value = q.swcLat;
                                    swclon.Value = q.swcLong;
                                    msgindx.Value = q.messageIndex;
                                    totcal.Value = q.totalCalories;
                                    totfatcal.Value = q.totalFatCalories;
                                    avgspd.Value = q.avgSpeed;
                                    maxspd.Value = q.maxSpeed;
                                    avgpow.Value = q.avgPower;
                                    maxpow.Value = q.maxPower;
                                    totalascent.Value = q.totalAscent;
                                    totaldescent.Value = q.totalDescent;
                                    firstlapidx.Value = q.firstLapIndex;
                                    nbrlaps.Value = q.numLaps;
                                    myevent.Value = q.myEvent;
                                    myeventtype.Value = q.myEventType;
                                    sport.Value = q.sport;
                                    subsport.Value = q.subSport;
                                    avghr.Value = q.avgHeartRate;
                                    maxhr.Value = q.maxHeartRate;
                                    avgcad.Value = q.avgCadence;
                                    maxcad.Value = q.maxCadence;
                                    myeventgroup.Value = q.myEventGroup;
                                    trigger.Value = q.trigger;
                                    pkey.Value = fitValues.myFitKey;
                                    fkey.Value = rowid;
                                }

                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();   //successful
                        }
                        catch (SQLiteException ex)
                        {
                            msgBoxobj.ShowNotification("Error in insert -> " + fitValues.myFitKey + " -> " + ex.Message + Environment.NewLine + ex.Data + Environment.NewLine + "Roll Back");
                            transaction.Rollback();
                            return false;
                        }
                        catch (Exception ey)
                        {
                            msgBoxobj.ShowNotification("Error in insert -> " + fitValues.myFitKey + " -> " + ey.Message + Environment.NewLine + "Roll Back");
                            transaction.Rollback();
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                msgBoxobj.ShowNotification("Error in insert -> " + fitValues.myFitKey + " -> " + ex.Message);
                return false;
            }
        }

        //public bool insertProcessedFileName(string fileName)
        //{
        //    bool myReturn = false;

        //    if (!getUserData<bool, string>(fileName, "FileNameExist"))
        //    {
        //        using (var conn = new SQLiteConnection(getConnectionString()))     //Insert User
        //        {
        //            conn.Open();

        //            using (var transaction = conn.BeginTransaction())
        //            {
        //                using (var cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = @"insert into ProcessedFiles (FileName) values (@FileName)";
        //                    cmd.Parameters.Add(new SQLiteParameter("@FileName") { Value = fileName });

        //                    myReturn = (Convert.ToInt32(cmd.ExecuteNonQuery()) > 0) ? true : false;
        //                }

        //                transaction.Commit();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        msgBoxobj.ShowNotification("File already processed");
        //        myReturn = false;
        //    }

        //    return myReturn;
        //}

        public bool insertElevationRecords(List<ElevationUpdate> elevations, string timeStamp, int rowid)
        {
            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                var lat = new SQLiteParameter("@latitude");
                                var lon = new SQLiteParameter("@longitude");
                                var elv = new SQLiteParameter("@elevation");
                                var res = new SQLiteParameter("@resolution");
                                var pkey = new SQLiteParameter("@key");
                                var fkey = new SQLiteParameter("@rowid");

                                cmd.CommandText = @"INSERT INTO ElevationData " +
                                    @"(Latitude, Longitude, Elevation, Resolution, ElevationPKey, ElevationFK) VALUES " +
                                    @"(@latitude, @longitude, @elevation, @resolution, @key, @rowid)";

                                cmd.Parameters.Add(lat);
                                cmd.Parameters.Add(lon);
                                cmd.Parameters.Add(elv);
                                cmd.Parameters.Add(res);
                                cmd.Parameters.Add(pkey);
                                cmd.Parameters.Add(fkey);

                                foreach (var q in elevations)
                                {
                                    lat.Value = q.latitude;
                                    lon.Value = q.longitude;
                                    elv.Value = q.altitude;
                                    res.Value = q.resolution;
                                    pkey.Value = timeStamp;
                                    fkey.Value = rowid;

                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                        catch (SQLiteException ex)
                        {
                            msgBoxobj.ShowNotification("Error in elevation insert - Rollback => " + ex.Message);
                            transaction.Rollback();
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                msgBoxobj.ShowNotification("Error in Elevation Insert - " + ex.Message + " key = " + timeStamp);

                return false;
            }
        }

        //public bool insertActivityRecords(List<FileId> fileId, string timeStamp, string whatSport)
        //{
        //    try
        //    {
        //        using (var conn = new SQLiteConnection(getConnectionString()))
        //        {
        //            conn.Open();

        //            using (var transaction = conn.BeginTransaction())
        //            {
        //                using (var cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = @"INSERT INTO Activity " +
        //                        @"(Sport, Id, Name, UnitId, ProductId, GPSData, ActPKey) VALUES " +
        //                        @"(@sport, @id, @name, @unitid, @productid, @gpsdata, @key)";

        //                    var sprt = cmd.CreateParameter();
        //                    sprt.ParameterName = "@sport";
        //                    cmd.Parameters.Add(sprt);

        //                    var idn = cmd.CreateParameter();
        //                    idn.ParameterName = "@id";
        //                    cmd.Parameters.Add(idn);

        //                    var namen = cmd.CreateParameter();
        //                    namen.ParameterName = "@name";
        //                    cmd.Parameters.Add(namen);

        //                    var unitidn = cmd.CreateParameter();
        //                    unitidn.ParameterName = "@unitid";
        //                    cmd.Parameters.Add(unitidn);

        //                    var prodid = cmd.CreateParameter();
        //                    prodid.ParameterName = "@productid";
        //                    cmd.Parameters.Add(prodid);

        //                    var gdata = cmd.CreateParameter();
        //                    gdata.ParameterName = "@gpsdata";
        //                    cmd.Parameters.Add(gdata);

        //                    var pkey = cmd.CreateParameter();
        //                    pkey.ParameterName = "@key";
        //                    cmd.Parameters.Add(pkey);

        //                    foreach (var q in fileId)
        //                    {
        //                        sprt.Value = whatSport;
        //                        idn.Value = q.serialNumber;
        //                        switch (q.product)
        //                        {
        //                            case 484:
        //                                namen.Value = "Garmin 305";
        //                                break;
        //                            case 1169:
        //                                namen.Value = "Edge 800";
        //                                break;
        //                            default:
        //                                namen.Value = q.serialNumber;
        //                                break;
        //                        }
        //                        unitidn.Value = q.serialNumber;
        //                        prodid.Value = q.product;
        //                        gdata.Value = (q.HasGpsData ? 1 : 0);   //Sqlite stores bool as integer. True = 1, False = 0
        //                        pkey.Value = timeStamp;

        //                        cmd.ExecuteNonQuery();
        //                    }
        //                }

        //                transaction.Commit();
        //            }
        //        }

        //        return true;
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        msgBoxobj.ShowNotification("Error in Activity Insert - " + ex.Message + " key = " + timeStamp);

        //        return false;
        //    }
        //}

        //public bool insertLapRecords(List<myLapRecord> laps, string timeStamp)
        //{
        //    try
        //    {
        //        using (var conn = new SQLiteConnection(getConnectionString()))
        //        {
        //            conn.Open();

        //            using (var transaction = conn.BeginTransaction())
        //            {
        //                using (var cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = "INSERT INTO Lap " +
        //                        "(StartTime, StartPosLat, StartPosLong, EndPosLat, EndPosLong, TotalElapsedTime, TotalTimerTime, TotalDistance, TotalCycles," +
        //                        "TotalCalories, TotalFatCalories, AvgSpeed, MaxSpeed, AvgPower, MaxPower, TotalAscent, TotalDescent, " +
        //                        "AvgHeartRate, MaxHeartRate, AvgCadence, " +
        //                        "MaxCadence, Intensity, Trigger, Sport, LapPKey) VALUES " +
        //                        "(@starttm, @startposlat, @startposlon, @endposlat, @endposlon, @totalelapsed, @totaltimer, @totaldist, @totalcyc, " +
        //                        "@totalcal, @totalfatcal, @avgspd, @maxspd, @avgpower, @maxpower, @totalascent, @totaldescent, " +
        //                        "@avghr, @maxhr, @avgcad, @maxcad, @intensity, " +
        //                        "@trigger, @sport, @key)";

        //                    var l1starttime = cmd.CreateParameter();
        //                    l1starttime.ParameterName = "@starttm";
        //                    cmd.Parameters.Add(l1starttime);

        //                    var l1startposlat = cmd.CreateParameter();
        //                    l1startposlat.ParameterName = "@startposlat";
        //                    cmd.Parameters.Add(l1startposlat);

        //                    var l1startposlon = cmd.CreateParameter();
        //                    l1startposlon.ParameterName = "@startposlon";
        //                    cmd.Parameters.Add(l1startposlon);

        //                    var l1endposlat = cmd.CreateParameter();
        //                    l1endposlat.ParameterName = "@endposlat";
        //                    cmd.Parameters.Add(l1endposlat);

        //                    var l1endposlon = cmd.CreateParameter();
        //                    l1endposlon.ParameterName = "@endposlon";
        //                    cmd.Parameters.Add(l1endposlon);

        //                    var l1totalelapsed = cmd.CreateParameter();
        //                    l1totalelapsed.ParameterName = "@totalelapsed";
        //                    cmd.Parameters.Add(l1totalelapsed);

        //                    var l1totaltimer = cmd.CreateParameter();
        //                    l1totaltimer.ParameterName = "@totaltimer";
        //                    cmd.Parameters.Add(l1totaltimer);

        //                    var l1totaldist = cmd.CreateParameter();
        //                    l1totaldist.ParameterName = "@totaldist";
        //                    cmd.Parameters.Add(l1totaldist);

        //                    var l1totalcyc = cmd.CreateParameter();
        //                    l1totalcyc.ParameterName = "@totalcyc";
        //                    cmd.Parameters.Add(l1totalcyc);

        //                    var l1totalcal = cmd.CreateParameter();
        //                    l1totalcal.ParameterName = "@totalcal";
        //                    cmd.Parameters.Add(l1totalcal);

        //                    var l1totalfatcal = cmd.CreateParameter();
        //                    l1totalfatcal.ParameterName = "@totalfatcal";
        //                    cmd.Parameters.Add(l1totalfatcal);

        //                    var l1avgspeed = cmd.CreateParameter();
        //                    l1avgspeed.ParameterName = "@avgspd";
        //                    cmd.Parameters.Add(l1avgspeed);

        //                    var l1maxspeed = cmd.CreateParameter();
        //                    l1maxspeed.ParameterName = "@maxspd";
        //                    cmd.Parameters.Add(l1maxspeed);

        //                    var l1avgpower = cmd.CreateParameter();
        //                    l1avgpower.ParameterName = "@avgpower";
        //                    cmd.Parameters.Add(l1avgpower);

        //                    var l1maxpower = cmd.CreateParameter();
        //                    l1maxpower.ParameterName = "@maxpower";
        //                    cmd.Parameters.Add(l1maxpower);

        //                    var l1totalasc = cmd.CreateParameter();
        //                    l1totalasc.ParameterName = "@totalascent";
        //                    cmd.Parameters.Add(l1totalasc);

        //                    var l1totaldsc = cmd.CreateParameter();
        //                    l1totaldsc.ParameterName = "@totaldescent";
        //                    cmd.Parameters.Add(l1totaldsc);

        //                    var l1avghr = cmd.CreateParameter();
        //                    l1avghr.ParameterName = "@avghr";
        //                    cmd.Parameters.Add(l1avghr);

        //                    var l1maxhr = cmd.CreateParameter();
        //                    l1maxhr.ParameterName = "@maxhr";
        //                    cmd.Parameters.Add(l1maxhr);

        //                    var l1avgcad = cmd.CreateParameter();
        //                    l1avgcad.ParameterName = "@avgcad";
        //                    cmd.Parameters.Add(l1avgcad);

        //                    var l1maxcad = cmd.CreateParameter();
        //                    l1maxcad.ParameterName = "@maxcad";
        //                    cmd.Parameters.Add(l1maxcad);

        //                    var l1intensity = cmd.CreateParameter();
        //                    l1intensity.ParameterName = "@intensity";
        //                    cmd.Parameters.Add(l1intensity);

        //                    var l1trigger = cmd.CreateParameter();
        //                    l1trigger.ParameterName = "@trigger";
        //                    cmd.Parameters.Add(l1trigger);

        //                    var l1sport = cmd.CreateParameter();
        //                    l1sport.ParameterName = "@sport";
        //                    cmd.Parameters.Add(l1sport);

        //                    var l1pkey = cmd.CreateParameter();
        //                    l1pkey.ParameterName = "@key";
        //                    cmd.Parameters.Add(l1pkey);

        //                    foreach (var q in laps)
        //                    {
        //                        l1starttime.Value = q.startTime;
        //                        l1startposlat.Value = q.startPosLat;
        //                        l1startposlon.Value = q.startPosLong;
        //                        l1endposlat.Value = q.endPosLat;
        //                        l1endposlon.Value = q.endPosLong;
        //                        l1totalelapsed.Value = q.totalElaspedTime;
        //                        l1totaltimer.Value = q.totalTimerTime;
        //                        l1totaldist.Value = q.totalDistance;
        //                        l1totalcyc.Value = q.totalCycles;
        //                        l1totalcal.Value = q.totalCalories;
        //                        l1totalfatcal.Value = q.totalFatCalories;
        //                        l1avgspeed.Value = q.avgSpeed;
        //                        l1maxspeed.Value = q.maxSpeed;
        //                        l1avgpower.Value = q.avgPower;
        //                        l1maxpower.Value = q.maxPower;
        //                        l1totalasc.Value = q.totalAscent;
        //                        l1totaldsc.Value = q.totalDesecent;
        //                        l1avghr.Value = q.avgHeartRate;
        //                        l1maxhr.Value = q.maxHeartRate;
        //                        l1avgcad.Value = q.avgCadence;
        //                        l1maxcad.Value = q.maxCadence;
        //                        l1intensity.Value = q.intensity;
        //                        l1trigger.Value = q.lapTrigger;
        //                        l1sport.Value = q.sport;
        //                        l1pkey.Value = timeStamp;

        //                        cmd.ExecuteNonQuery();
        //                    }
        //                }

        //                transaction.Commit();
        //            }
        //        }

        //        return true;
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        msgBoxobj.ShowNotification("Error in Lap insert - " + ex.Message);

        //        return false;
        //    }
        //}

        //public bool insertTrackRecords(List<myFitRecord> trackpoints, string timeStamp)
        //{
        //    try
        //    {
        //        using (var conn = new SQLiteConnection(getConnectionString()))
        //        {
        //            conn.Open();

        //            using (var transaction = conn.BeginTransaction())
        //            {
        //                using (var cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = @"INSERT INTO TrackPoint " +
        //                        @"(Timestamp, Latitude, Longitude, Distance, Altitude, Speed, HeartRate, Cadence, Temperature, Sequence, TrackPkey) VALUES " +
        //                        @"(@timestamp, @latitude, @longitude, @distance, @altitude, @speed, @heartrate, @cadence, @temperature, @sequence, @key)";

        //                    var tstamp = cmd.CreateParameter();
        //                    tstamp.ParameterName = "@timeStamp";
        //                    cmd.Parameters.Add(tstamp);

        //                    var lat = cmd.CreateParameter();
        //                    lat.ParameterName = "@latitude";
        //                    cmd.Parameters.Add(lat);

        //                    var lon = cmd.CreateParameter();
        //                    lon.ParameterName = "@longitude";
        //                    cmd.Parameters.Add(lon);

        //                    var dist = cmd.CreateParameter();
        //                    dist.ParameterName = "@distance";
        //                    cmd.Parameters.Add(dist);

        //                    var altitude = cmd.CreateParameter();
        //                    altitude.ParameterName = "@altitude";
        //                    cmd.Parameters.Add(altitude);

        //                    var speed = cmd.CreateParameter();
        //                    speed.ParameterName = "@speed";
        //                    cmd.Parameters.Add(speed);

        //                    var hrate = cmd.CreateParameter();
        //                    hrate.ParameterName = "@heartrate";
        //                    cmd.Parameters.Add(hrate);

        //                    var cadence = cmd.CreateParameter();
        //                    cadence.ParameterName = "@cadence";
        //                    cmd.Parameters.Add(cadence);

        //                    var temperature = cmd.CreateParameter();
        //                    temperature.ParameterName = "@temperature";
        //                    cmd.Parameters.Add(temperature);

        //                    var seq = cmd.CreateParameter();
        //                    seq.ParameterName = "@sequence";
        //                    cmd.Parameters.Add(seq);

        //                    var pkey = cmd.CreateParameter();
        //                    pkey.ParameterName = "@key";
        //                    cmd.Parameters.Add(pkey);

        //                    foreach (var q in trackpoints)
        //                    {
        //                        tstamp.Value = q.timeStamp;
        //                        lat.Value = q.latitude;
        //                        lon.Value = q.longitude;
        //                        dist.Value = q.distance;
        //                        altitude.Value = q.altitude;
        //                        speed.Value = q.speed;
        //                        hrate.Value = q.heartRate;
        //                        cadence.Value = q.cadence;
        //                        temperature.Value = q.temperature;
        //                        seq.Value = q.sequence;
        //                        pkey.Value = timeStamp;

        //                        cmd.ExecuteNonQuery();
        //                    }
        //                }

        //                transaction.Commit();
        //            }
        //        }

        //        return true;
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        msgBoxobj.ShowNotification("Error in TrackPoint Insert - " + ex.Message);

        //        return false;
        //    }
        //}

        //public bool insertSessionRecords(List<mySessionRecord> mys, string timeStamp)
        //{
        //    try
        //    {
        //        using (var conn = new SQLiteConnection(getConnectionString()))
        //        {
        //            conn.Open();

        //            using (var transaction = conn.BeginTransaction())
        //            {
        //                using (var cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = "INSERT INTO Session " +
        //                        //"(Timestamp, StartTime, StartPosLat, StartPosLong, TotalElapsedTime, TotalTimerTime, " +
        //                        //"TotalDistance, TotalCycles, NecLat, NecLong, SwcLat, SwcLong, MessageIndex, TotalCalories, TotalFatCalories, AvgSpeed, " +
        //                        //"MaxSpeed, AvgPower, MaxPower, TotalAscent, TotalDescent, FirstLapIndex, NumberLaps, MyEvent, MyEventType, Sport, " +
        //                        //"SubSport, AvgHeartRate, MaxHeartRate, AvgCadence, MaxCadence, MyEventGroup, Trigger, SessionPKey)" +
        //                        " VALUES " +
        //                        "(@timestamp, @starttime, @startposlat, @startposlong, @totalelapsedtime, @totaltimertime, @totaldistance, @totalcycles, " +
        //                        "@neclat, @neclong, @swclat, @swclong, @messageindex, @totalcalories, @totalfatcalories, @avgspeed, @maxspeed, @avgpower, " +
        //                        "@maxpower, @totalascent, @totaldescent, @firstlapindex, @numberlaps, @myevent, @myeventtype, @sport, @subsport, " +
        //                        "@avgheartrate, @maxheartrate, @avgcadence, @maxcadence, @myeventgroup, @trigger, @key)";

        //                    cmd.Parameters.AddWithValue("@timestamp", mys[0].timeStamp);
        //                    cmd.Parameters.AddWithValue("@starttime", mys[0].startTime);
        //                    cmd.Parameters.AddWithValue("@startposlat", mys[0].startPositionLat);
        //                    cmd.Parameters.AddWithValue("@startposlong", mys[0].startPositionLong);
        //                    cmd.Parameters.AddWithValue("@totalelapsedtime", mys[0].totalElapsedTime);
        //                    cmd.Parameters.AddWithValue("@totaltimertime", mys[0].totalTimerTime);
        //                    cmd.Parameters.AddWithValue("@totaldistance", mys[0].totalDistance);
        //                    cmd.Parameters.AddWithValue("@totalcycles", mys[0].totalCycles);
        //                    cmd.Parameters.AddWithValue("@neclat", mys[0].necLat);
        //                    cmd.Parameters.AddWithValue("@neclong", mys[0].necLong);
        //                    cmd.Parameters.AddWithValue("@swclat", mys[0].swcLat);
        //                    cmd.Parameters.AddWithValue("@swclong", mys[0].swcLong);
        //                    cmd.Parameters.AddWithValue("@messageindex", mys[0].messageIndex);
        //                    cmd.Parameters.AddWithValue("@totalcalories", mys[0].totalCalories);
        //                    cmd.Parameters.AddWithValue("@totalfatcalories", mys[0].totalFatCalories);
        //                    cmd.Parameters.AddWithValue("@avgspeed", mys[0].avgSpeed);
        //                    cmd.Parameters.AddWithValue("@maxspeed", mys[0].maxSpeed);
        //                    cmd.Parameters.AddWithValue("@avgpower", mys[0].avgPower);
        //                    cmd.Parameters.AddWithValue("@maxpower", mys[0].maxPower);
        //                    cmd.Parameters.AddWithValue("@totalascent", mys[0].totalAscent);
        //                    cmd.Parameters.AddWithValue("@totaldescent", mys[0].totalDescent);
        //                    cmd.Parameters.AddWithValue("@firstlapindex", mys[0].firstLapIndex);
        //                    cmd.Parameters.AddWithValue("@numberlaps", mys[0].numLaps);
        //                    cmd.Parameters.AddWithValue("@myevent", mys[0].myEvent);
        //                    cmd.Parameters.AddWithValue("@myeventtype", mys[0].myEventType);
        //                    cmd.Parameters.AddWithValue("@sport", mys[0].sport);
        //                    cmd.Parameters.AddWithValue("@subsport", mys[0].subSport);
        //                    cmd.Parameters.AddWithValue("@avgheartrate", mys[0].avgHeartRate);
        //                    cmd.Parameters.AddWithValue("@maxheartrate", mys[0].maxHeartRate);
        //                    cmd.Parameters.AddWithValue("@avgcadence", mys[0].avgCadence);
        //                    cmd.Parameters.AddWithValue("@maxcadence", mys[0].maxCadence);
        //                    cmd.Parameters.AddWithValue("@myeventgroup", mys[0].myEventGroup);
        //                    cmd.Parameters.AddWithValue("@trigger", mys[0].trigger);
        //                    cmd.Parameters.AddWithValue("@key", timeStamp);

        //                    cmd.ExecuteNonQuery();
        //                }

        //                transaction.Commit();
        //            }
        //        }

        //        return true;
        //    }
        //    catch (SQLiteException ex)
        //    {
        //        msgBoxobj.ShowNotification("Error in Session Insert - " + ex.Message);

        //        return false;
        //    }
        //}

        public bool tableMaintenence(string source)
        {
            bool isRestore = false;

            if (string.IsNullOrWhiteSpace(source))
            {
                isRestore = false;
            }
            else
            {
                isRestore = true;
            }

            string sqlDropProcessedFiles = @"DROP TABLE IF EXISTS ProcessedFiles;";

            string sqlDropActivity = @"DROP TABLE IF EXISTS Activity;";

            string sqlDropLap = @"DROP TABLE IF EXISTS Lap;";

            string sqlDropTrack = @"DROP TABLE IF EXISTS TrackPoint;";

            string sqlDropSession = @"DROP TABLE IF EXISTS Session;";

            string sqlDropElevation = @"DROP TABLE IF EXISTS ElevationData;";

            //FileName - To check if this file has been processed - FileName is the TimeStamp
            string sqlCreateProcessedFiles = @"CREATE TABLE IF NOT EXISTS [ProcessedFiles] " + 
                @"([FileName] VARCHAR(25) NOT NULL, " +
                @"[Pkey] INTEGER NOT NULL PRIMARY KEY);";

            //ActPKey - Primary Key - string TimeStamp
            string sqlCreateActivity = @"CREATE TABLE IF NOT EXISTS [Activity] " +
                @"([Sport] VARCHAR(25) NOT NULL, " +
                @"[Id] VARCHAR(25) NOT NULL, " +
                @"[Name] VARCHAR(25) NOT NULL, " +
                @"[UnitId] VARCHAR(25) NOT NULL, " +
                @"[ProductId] VARCHAR(25) NOT NULL, " +
                @"[GPSData] INTEGER NOT NULL, " +
                @"[ActPKey] VARCHAR(25) NOT NULL, " +
                @"[ActivityFK] INTEGER, FOREIGN KEY(ActivityFK) REFERENCES ProcessedFiles(Pkey) ON DELETE CASCADE)";
            //@"[ActPKey] VARCHAR(25) NOT NULL PRIMARY KEY)" +

            //LapPKey - Foreign Key - string TimeStamp 
            string sqlCreateLap = @"CREATE TABLE IF NOT EXISTS [Lap] " +
                "([LapKey] INTEGER PRIMARY KEY, " +
                "[StartTime] VARCHAR(25) NOT NULL, " +
                "[StartPosLat] FLOAT NOT NULL, " +
                "[StartPosLong] FLOAT NOT NULL, " +
                "[EndPosLat] FLOAT NOT NULL, " +
                "[EndPosLong] FLOAT NOT NULL, " +
                "[TotalElapsedTime] VARCHAR(25) NOT NULL, " +
                "[TotalTimerTime] VARCHAR(25) NOT NULL, " +
                "[TotalDistance] FLOAT NOT NULL, " +
                "[TotalCycles] INTEGER NOT NULL, " +
                "[TotalCalories] INTEGER NOT NULL, " +
                "[TotalFatCalories] INTEGER NOT NULL, " +
                "[AvgSpeed] FLOAT NOT NULL, " +
                "[MaxSpeed] FLOAT NOT NULL, " +
                "[AvgPower] FLOAT NOT NULL, " +
                "[MaxPower] FLOAT NOT NULL, " +
                "[TotalAscent] FLOAT NOT NULL, " +
                "[TotalDescent] FLOAT NOT NULL, " +
                "[AvgHeartRate] INTEGER NOT NULL, " +
                "[MaxHeartRate] INTEGER NOT NULL, " +
                "[AvgCadence] INTEGER NOT NULL, " +
                "[MaxCadence] INTEGER NOT NULL, " +
                "[Intensity] INTEGER NOT NULL, " +
                "[Trigger] VARCHAR(25) NOT NULL, " +
                "[Sport] VARCHAR(25) NOT NULL, " +
                "[LapPkey] VARCHAR(25) NOT NULL, " +
                "[LapFK] INTEGER, FOREIGN KEY(LapFK) REFERENCES ProcessedFiles(Pkey) ON DELETE CASCADE)";
                //"[LapPkey] VARCHAR(25), FOREIGN KEY(LapPKey) REFERENCES Activity(ActPKey) ON DELETE CASCADE)";

            //TrackPKey - Foreign Key - string TimeStamp
            string sqlCreateTrack = @"CREATE TABLE IF NOT EXISTS [TrackPoint] " +
                @"([Timestamp] VARCHAR(25) NOT NULL, " +
                @"[Latitude] FLOAT NOT NULL, " +
                @"[Longitude] FLOAT NOT NULL, " +
                @"[Distance] FLOAT NOT NULL, " +
                @"[Altitude] FLOAT NOT NULL, " +
                @"[Speed] FLOAT NOT NULL, " +
                @"[HeartRate] INTEGER NOT NULL, " +
                @"[Cadence] INTEGER NOT NULL, " +
                @"[Temperature] INTEGER NOT NULL, " +
                @"[Sequence] INTEGER NOT NULL, " +
                //@"[TrackPKey] VARCHAR(25) NOT NULL, " +
                @"[TrackFK] INTEGER, FOREIGN KEY(TrackFK) REFERENCES ProcessedFiles(Pkey) ON DELETE CASCADE)";
                //@"[TrackPKey] VARCHAR(25), FOREIGN KEY(TrackPKey) REFERENCES Activity(ActPKey) ON DELETE CASCADE)";

            string sqlCreateSession = @"CREATE TABLE IF NOT EXISTS [Session] " +
                @"([Timestamp] VARCHAR(25) NOT NULL, " +
                @"[StartTime] VARCHAR(25) NOT NULL, " +
                @"[StartPosLat] FLOAT NOT NULL, " +
                @"[StartPosLong] FLOAT NOT NULL, " +
                @"[TotalElapsedTime] VARCHAR(25) NOT NULL, " +
                @"[TotalTimerTime] VARCHAR(25) NOT NULL, " +
                @"[TotalDistance] FLOAT NOT NULL, " +
                @"[TotalCycles] INTEGER NOT NULL, " +
                @"[NecLat] FLOAT NOT NULL, " +
                @"[NecLong] FLOAT NOT NULL, " +
                @"[SwcLat] FLOAT NOT NULL, " +
                @"[SwcLong] FLOAT NOT NULL, " +
                @"[MessageIndex] INTEGER NOT NULL, " +
                @"[TotalCalories] INTEGER NOT NULL, " +
                @"[TotalFatCalories] INTEGER NOT NULL, " +
                @"[AvgSpeed] FLOAT NOT NULL, " +
                @"[MaxSpeed] FLOAT NOT NULL, " +
                @"[AvgPower] FLOAT NOT NULL, " +
                @"[MaxPower] FLOAT NOT NULL, " +
                @"[TotalAscent] FLOAT NOT NULL, " +
                @"[TotalDescent] FLOAT NOT NULL, " +
                @"[FirstLapIndex] INTEGER NOT NULL, " +
                @"[NumberLaps] INTEGER NOT NULL, " +
                @"[MyEvent] VARCHAR(25) NOT NULL, " +
                @"[MyEventType] VARCHAR(25) NOT NULL, " +
                @"[Sport] VARCHAR(25) NOT NULL, " +
                @"[SubSport] VARCHAR(25) NOT NULL, " +
                @"[AvgHeartRate] INTEGER NOT NULL, " +
                @"[MaxHeartRate] INTEGER NOT NULL, " +
                @"[AvgCadence] INTEGER NOT NULL, " +
                @"[MaxCadence] INTEGER NOT NULL, " +
                @"[MyEventGroup] VARCHAR(25) NOT NULL, " +
                @"[Trigger] VARCHAR(25) NOT NULL, " +
                @"[SessionPKey] VARCHAR(25) NOT NULL, " +
                @"[SessionFK] INTEGER, FOREIGN KEY(SessionFK) REFERENCES ProcessedFiles(Pkey) ON DELETE CASCADE)";
                //@"[SessionPKey] VARCHAR(25), FOREIGN KEY(SessionPKey) REFERENCES Activity(ActPKey) ON DELETE CASCADE)";

            string sqlCreateElevation = @"CREATE TABLE IF NOT EXISTS [ElevationData] " +
                @"([Latitude] FLOAT NOT NULL, " +
                @"[Longitude] FLOAT NOT NULL, " +
                @"[Elevation] FLOAT NOT NULL, " +
                @"[Resolution] FLOAT NOT NULL, " +
                @"[ElevationPKey] VARCHAR(25) NOT NULL, " +
                @"[ElevationFK] INTEGER, FOREIGN KEY(ElevationFK) REFERENCES ProcessedFiles(Pkey) ON DELETE CASCADE)";
                //@"[ElevationPKey] VARCHAR(25), FOREIGN KEY(ElevationPKey) REFERENCES Activity(ActPKey) ON DELETE CASCADE)";

            try
            {
                using (var conn = new SQLiteConnection(getConnectionString()))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.CommandText = sqlDropProcessedFiles;

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = sqlDropSession;

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = sqlDropTrack;

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = sqlDropLap;

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = sqlDropActivity;

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = sqlDropElevation;

                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "VACUUM";

                        cmd.ExecuteNonQuery();

                        if (!isRestore)
                        {
                            cmd.CommandText = sqlCreateProcessedFiles;

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = sqlCreateActivity;

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = sqlCreateLap;

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = sqlCreateTrack;

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = sqlCreateSession;

                            cmd.ExecuteNonQuery();

                            cmd.CommandText = sqlCreateElevation;

                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (isRestore)
                {
                    using (var conn = new SQLiteConnection(getConnectionString()))
                    using (var back = new SQLiteConnection(dbpath(source)))
                    {
                        conn.Open();
                        back.Open();

                        back.BackupDatabase(conn, "main", "main", -1, null, -1);
                    }
                }
            }
            catch (SQLiteException ex)
            {
                msgBoxobj.ShowNotification("Error in connect: " + ex.Message);

                return false;
            }

            return true;
        }
    }
}
