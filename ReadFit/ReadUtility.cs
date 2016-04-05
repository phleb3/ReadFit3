using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ReadFit.FileModel;
using Dynastream.Fit;

namespace ReadFit
{
    class ReadUtility : IFitRead
    {
        private static MsgBoxService msgBoxobj;

        static ReadUtility()
        {
            msgBoxobj = new MsgBoxService();
        }

        private string convertToLocalTime(string timeStamp)
        {
            System.DateTime localDateTime = System.DateTime.Now;
            System.DateTime ept = System.DateTime.Now;

            try
            {
                localDateTime = System.DateTime.Parse(timeStamp);
            }
            catch (System.FormatException)
            {
                msgBoxobj.ShowNotification("Invalid date format");
                return null;
            }

            if (TimeZoneInfo.Local.IsDaylightSavingTime(localDateTime))
            {
                ept = localDateTime.AddHours(-4);
            }
            else
            {
                ept = localDateTime.AddHours(-5);
            }

            return ept.ToString();
        }

        private string convertToElapsedTime(object tspan)
        {
            string ts2 = null;
            double ts = Convert.ToDouble(tspan);

            if (ts > 90000000.0)
            {
                ts2 = "0.0";   //if time > 24 hours, then error
            }
            else
            {
                string ts1 = Convert.ToString(TimeSpan.FromMilliseconds(ts));
                int t1 = ts1.IndexOf('.');

                if (t1 >= 0)
                {
                    ts2 = ts1.Substring(0, t1 + 3);
                }
                else
                {
                    ts2 = ts1 + ".00";
                }
            }

            return ts2;
        }

        public void getEventData(Dynastream.Fit.MesgEventArgs e, int i, int j, List<string> test)
        {
            Dynastream.Fit.DateTime dt = null;
            string result;

            switch (e.mesg.fields[i].GetName().ToLower())
            {
                case "timestamp":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    string tu = convertToLocalTime(dt.ToString());
                    test.Add(tu);
                    break;
                case "data":
                    result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    test.Add(result);
                    break;
                case "event":
                    result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    test.Add(result);
                    break;
                case "eventtype":
                    result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    test.Add(result);
                    break;
                case "eventgroup":
                    result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    test.Add(result);
                    break;
                default:
                    msgBoxobj.ShowNotification("Error in event switch - " + e.mesg.fields[i].GetName());
                    break;
            }
        }

        public void getFileIdData(Dynastream.Fit.MesgEventArgs e, int i, int j, FileId mfid)
        {
            Dynastream.Fit.DateTime dt = null;

            switch (e.mesg.fields[i].GetName().ToLower())
            {
                case "serialnumber":
                    mfid.serialNumber = e.mesg.fields[i].values[j].ToString();
                    break;
                case "timecreated":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    mfid.timeCreated = convertToLocalTime(dt.ToString());
                    break;
                case "manufacturer":
                    mfid.manufacturer = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "product":
                    mfid.product = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "number":
                    mfid.number = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "type":
                    mfid.myType = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                default:
                    msgBoxobj.ShowNotification("error in fileid switch - " + e.mesg.fields[i].GetName());
                    break;
            }
        }

        public void getSessionData(Dynastream.Fit.MesgEventArgs e, int i, int j, mySessionRecord msr)
        {
            Dynastream.Fit.DateTime dt = null;
            double sctodeg = (180.0 / Math.Pow(2.0, 31.0));

            switch (e.mesg.fields[i].GetName().ToLower())
            {
                case "timestamp":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    msr.timeStamp = convertToLocalTime(dt.ToString());
                    break;
                case "starttime":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    msr.startTime = convertToLocalTime(dt.ToString());
                    break;
                case "startpositionlat":
                    msr.startPositionLat = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "startpositionlong":
                    msr.startPositionLong = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "totalelapsedtime":
                    msr.totalElapsedTime = convertToElapsedTime(e.mesg.fields[i].values[j]);
                    break;
                case "totaltimertime":
                    msr.totalTimerTime = convertToElapsedTime(e.mesg.fields[i].values[j]);
                    break;
                case "totaldistance":
                    //msr.totalDistance = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0328084;
                    msr.totalDistance = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.01; //cm to meters
                    break;
                case "totalcycles":
                    msr.totalCycles = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "neclat":
                    msr.necLat = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "neclong":
                    msr.necLong = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "swclat":
                    msr.swcLat = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "swclong":
                    msr.swcLong = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "messageindex":
                    msr.messageIndex = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "totalcalories":
                    msr.totalCalories = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "totalfatcalories":
                    msr.totalFatCalories = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "avgspeed":
                    //msr.avgSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0022369;
                    msr.avgSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.001;     //mm/s to m/s
                    break;
                case "maxspeed":
                    //msr.maxSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0022369;
                    msr.maxSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.001;     //mm/s to m/s
                    break;
                case "avgpower":
                    break;
                case "maxpower":
                    break;
                case "totalascent":
                    //msr.totalAscent = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 3.28084;
                    msr.totalAscent = Convert.ToDouble(e.mesg.fields[i].values[j].ToString());              //meters
                    break;
                case "totaldescent":
                    //msr.totalDescent = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 3.28084;
                    msr.totalDescent = Convert.ToDouble(e.mesg.fields[i].values[j].ToString());             //meters
                    break;
                case "firstlapindex":
                    msr.firstLapIndex = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "numlaps":
                    msr.numLaps = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "event":
                    msr.myEvent = e.mesg.fields[i].values[j].ToString();
                    break;
                case "eventtype":
                    msr.myEventType = e.mesg.fields[i].values[j].ToString();
                    break;
                case "sport":
                    int temp = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    switch (temp)
                    {
                        case 0:
                            msr.sport = "Other";
                            break;
                        case 1:
                            msr.sport = "Running";
                            break;
                        case 2:
                            msr.sport = "Biking";
                            break;
                        default:
                            msgBoxobj.ShowNotification("Error in sport - " + temp);
                            break;
                    }
                    break;
                case "subsport":
                    msr.subSport = e.mesg.fields[i].values[j].ToString();
                    break;
                case "avgheartrate":
                    msr.avgHeartRate = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "maxheartrate":
                    msr.maxHeartRate = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "avgcadence":
                    msr.avgCadence = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "maxcadence":
                    msr.maxCadence = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "eventgroup":
                    msr.myEventGroup = e.mesg.fields[i].values[j].ToString();
                    break;
                case "trigger":
                    //msr.myTrigger = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    switch (e.mesg.fields[i].values[j].ToString())
                    {
                        case "0":
                            msr.trigger = "Manual";
                            break;
                        case "1":
                            msr.trigger = "Time";
                            break;
                        case "2":
                            msr.trigger = "Distance";
                            break;
                        case "3":
                            msr.trigger = "Position Start";
                            break;
                        case "4":
                            msr.trigger = "Position Lap";
                            break;
                        case "5":
                            msr.trigger = "Waypoint";
                            break;
                        case "6":
                            msr.trigger = "Position Marked";
                            break;
                        case "7":
                            msr.trigger = "Session End";
                            break;
                        case "8":
                            msr.trigger = "Equipment";
                            break;
                        default:
                            msr.trigger = "Invalid";
                            break;
                    }
                    break;
                case "totalwork":
                    break;
                case "normalizedpower":
                    break;
                case "trainingstressscore":
                    break;
                case "intensityfactor":
                    break;
                case "leftrightbalance":
                    break;
                case "unknown":
                    string t2 = e.mesg.fields[i].values[j].ToString();
                    break;
                default:
                    msgBoxobj.ShowNotification("Error in session switch - " + e.mesg.fields[i].GetName());
                    break;
            }
        }

        public void getActData(Dynastream.Fit.MesgEventArgs e, int i, int j, myActRecord myact)
        {
            Dynastream.Fit.DateTime dt = null;
            double sctodeg = (180.0 / Math.Pow(2.0, 31.0));

            switch (e.mesg.fields[i].GetName().ToLower())
            {
                case "timestamp":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    myact.timeStamp = convertToLocalTime(dt.ToString());
                    break;
                case "totaltimertime":
                    myact.totalTimerTime = convertToElapsedTime(e.mesg.fields[i].values[j]);
                    break;
                case "numsessions":
                    myact.numSessions = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "type":
                    myact.myType = e.mesg.fields[i].values[j].ToString();
                    break;
                case "event":
                    myact.myEvent = e.mesg.fields[i].values[j].ToString();
                    break;
                case "eventtype":
                    myact.myEventType = e.mesg.fields[i].values[j].ToString();
                    break;
                default:
                    msgBoxobj.ShowNotification("Error in Act switch - " + e.mesg.fields[i].GetName());
                    break;
            }
        }
        public void getTrackData(Dynastream.Fit.MesgEventArgs e, int i, int j, myFitRecord mfr)
        {
            double rawElevation = 0.0;

            Dynastream.Fit.DateTime dt = null;
            double sctodeg = (180.0 / Math.Pow(2.0, 31.0));

            switch (e.mesg.fields[i].GetName().ToLower())
            {
                case "timestamp":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    mfr.timeStamp = convertToLocalTime(dt.ToString());
                    break;
                case "positionlat":
                    mfr.latitude = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "positionlong":
                    mfr.longitude = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    break;
                case "distance":
                    //mfr.distance = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0328084;
                    mfr.distance = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.01;          //meters
                    break;
                case "altitude":
                    rawElevation = Convert.ToDouble(e.mesg.fields[i].values[j].ToString());
                    mfr.altitude = (rawElevation - 2500.0) * 0.2;   //meters
                    break;
                case "speed":
                    //mfr.speed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0022369;
                    mfr.speed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.001;            //mm/s to m/s
                    break;
                case "heartrate":
                    mfr.heartRate = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "cadence":
                    mfr.cadence = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "temperature":
                    mfr.temperature = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                default:
                    msgBoxobj.ShowNotification("Track error in switch: " + e.mesg.fields[i].GetName() + " was not found");
                    break;
            }
        }

        public void getLapData(Dynastream.Fit.MesgEventArgs e, int i, int j, myLapRecord mlr)
        {
            Dynastream.Fit.DateTime dt = null;
            double sctodeg = (180.0 / Math.Pow(2.0, 31.0));
            double testdeg = 0.0;

            switch (e.mesg.fields[i].GetName().ToLower())
            {
                case "timestamp":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    mlr.timeStamp = convertToLocalTime(dt.ToString());
                    break;
                case "starttime":
                    dt = new Dynastream.Fit.DateTime((uint)e.mesg.fields[i].values[j]);
                    mlr.startTime = convertToLocalTime(dt.ToString());
                    break;
                case "startpositionlat":
                    testdeg = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    mlr.startPosLat = (testdeg < 179.9999) ? testdeg : 0.0;
                    break;
                case "startpositionlong":
                    testdeg = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    mlr.startPosLong = (testdeg < 179.9999) ? testdeg : 0.0;
                    break;
                case "endpositionlat":
                    testdeg = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    mlr.endPosLat = (testdeg < 179.9999) ? testdeg : 0.0;
                    break;
                case "endpositionlong":
                    testdeg = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * sctodeg;
                    mlr.endPosLong = (testdeg < 179.9999) ? testdeg : 0.0;
                    break;
                case "totalelapsedtime":
                    mlr.totalElaspedTime = convertToElapsedTime(e.mesg.fields[i].values[j]);
                    break;
                case "totaltimertime":
                    mlr.totalTimerTime = convertToElapsedTime(e.mesg.fields[i].values[j]);
                    break;
                case "totaldistance":
                    //mlr.totalDistance = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0328084;
                    mlr.totalDistance = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.01;         //cm to meters
                    break;
                case "totalcycles":
                    mlr.totalCycles = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "totalcalories":
                    mlr.totalCalories = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "totalfatcalories":
                    mlr.totalFatCalories = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "avgspeed":
                    //mlr.avgSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0022369;
                    mlr.avgSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.001;         //mm/s to m/s
                    break;
                case "maxspeed":
                    //mlr.maxSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.0022369;
                    mlr.maxSpeed = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.001;
                    break;
                case "avgpower":
                    break;
                case "maxpower":
                    break;
                case "totalascent":
                    mlr.totalAscent = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.01;      //meters
                    break;
                case "totaldescent":
                    mlr.totalDesecent = Convert.ToDouble(e.mesg.fields[i].values[j].ToString()) * 0.01;
                    break;
                case "avgheartrate":
                    mlr.avgHeartRate = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "maxheartrate":
                    mlr.maxHeartRate = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "avgcadence":
                    mlr.avgCadence = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "maxcadence":
                    mlr.maxCadence = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "intensity":
                    mlr.intensity = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    break;
                case "laptrigger":
                    switch (e.mesg.fields[i].values[j].ToString())
                    {
                        case "0":
                            mlr.lapTrigger = "Manual";
                            break;
                        case "1":
                            mlr.lapTrigger = "Time";
                            break;
                        case "2":
                            mlr.lapTrigger = "Distance";
                            break;
                        case "3":
                            mlr.lapTrigger = "Position Start";
                            break;
                        case "4":
                            mlr.lapTrigger = "Position Lap";
                            break;
                        case "5":
                            mlr.lapTrigger = "Waypoint";
                            break;
                        case "6":
                            mlr.lapTrigger = "Position Marked";
                            break;
                        case "7":
                            mlr.lapTrigger = "Session End";
                            break;
                        case "8":
                            mlr.lapTrigger = "Equipment";
                            break;
                        default:
                            mlr.lapTrigger = "Invalid";
                            break;
                    }
                    break;
                case "sport":
                    int temp = Convert.ToInt32(e.mesg.fields[i].values[j]);
                    switch (temp)
                    {
                        case 0:
                            mlr.sport = "Other";
                            break;
                        case 1:
                            mlr.sport = "Running";
                            break;
                        case 2:
                            mlr.sport = "Biking";
                            break;
                        default:
                            msgBoxobj.ShowNotification("Error in sport - " + temp);
                            break;
                    }
                    break;
                case "totalwork":
                    break;
                case "messageindex":
                    break;
                case "normalizedpower":
                    break;
                case "leftrightbalance":
                    break;
                case "event":
                    break;
                case "eventtype":
                    break;
                case "eventgroup":
                    break;
                case "unknown":
                    //result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    //test.Add(result);
                    break;
                default:
                    msgBoxobj.ShowNotification("Error in Lap switch - " + e.mesg.fields[i].GetName());
                    //string result = string.Format("Field{0} Index{1} (\"{2}\" Field#{4}) Value: {3}", i, j, e.mesg.fields[i].GetName(), e.mesg.fields[i].values[j], e.mesg.fields[i].Num);
                    //test.Add(result);
                    //msgBoxobj.ShowNotification("default - " + result);
                    break;
            }
        }
    }
}
