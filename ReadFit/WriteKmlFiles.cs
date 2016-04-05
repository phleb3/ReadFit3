using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Windows;
using System.Windows.Media;
using ReadFit.FileModel;

namespace ReadFit
{
    class WriteKmlFiles
    {
        private static MsgBoxService msgBoxObj;
        private static IDataAccessLayer datalayer { get; set; }

        static WriteKmlFiles()
        {
            msgBoxObj = new MsgBoxService();

            datalayer = GetLayer.giveMeADataLayer();
        }

        public bool writeFile(MyPassedData mpd)
        {
            double East = 0.0;
            double West = 0.0;
            double South = 0.0;
            double North = 0.0;
            double CenterLat = 0.0;
            double CenterLon = 0.0;
            double NSdist = 0.0;
            double EWdist = 0.0;
            double Range = 0.0;

            string myDescription = mpd.mySport + " " + mpd.myUserId.Substring(0, 10);

            int rowid = datalayer.getUserData<int, string>(mpd.myUserId, "GetRowId");

            int productId = datalayer.getUserData<int, int>(rowid, "ProductId");

            if (productId != 0)
            {
                switch (productId)
                {
                    case 484:
                        myDescription += " Garmin Forerunner 305 data file";
                        break;

                    case 1169:
                        myDescription += " Garmin Edge 800 data file";
                        break;

                    default:
                        myDescription += " Garmin product";
                        break;
                }
            }

            string myKmlColor;

            Color value = mpd.Mycolordict[Properties.Settings.Default.KmlColor];

            myKmlColor = ((Properties.Settings.Default.KmlOpacity * 255) / 100).ToString("X2") +    //opacity
                         value.B.ToString("X2") + value.G.ToString("X2") + value.R.ToString("X2");  //BGR instead of RGB

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(null, "ogckml22.xsd");
            schemas.Add(null, "atom-author-link.xsd");      //added the files to the project

            try
            {
                schemas.Compile();
            }
            catch (XmlSchemaException ex)
            {
                msgBoxObj.ShowNotification("Error in schema compile: " + ex.Message);
            }

            XNamespace ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
            XNamespace ns1 = XNamespace.Get("http://www.w3.org/2005/Atom");
            XNamespace ns2 = XNamespace.Get("http://www.google.com/kml/ext/2.2");

            XDocument kmldoc =
                new XDocument(
                new XDeclaration("1.0", "utf-8", ""),
                new XElement(ns + "kml",
                    new XAttribute(XNamespace.Xmlns + "gx", "http://www.google.com/kml/ext/2.2"),
                    new XAttribute(XNamespace.Xmlns + "atom", "http://www.w3.org/2005/Atom"),
                new XElement(ns + "Document",
                    new XElement(ns + "name", mpd.myKmlFile)
                    )));

            //if (Properties.Settings.Default.KmlFileType != "Time Slider")   //only the time slider needs to write the lookat fields
            //{
            //    kmldoc.Descendants(ns + "name").LastOrDefault().AddAfterSelf(
            //        new XElement(ns + "open", 1),
            //        new XElement(ns1 + "author",
            //            new XElement(ns1 + "name", Properties.Settings.Default.KmlUserName)),
            //            new XElement(ns1 + "link", new XAttribute("rel", "related"), new XAttribute("href", "http://www.garmin.com")),
            //            new XElement(ns + "description", myDescription),
            //            new XElement(ns + "Style", new XAttribute("id", "myStyleLine"),
            //                new XElement(ns + "LineStyle",
            //                    new XElement(ns + "color", myKmlColor),
            //                    new XElement(ns + "colorMode", "normal"),
            //                    new XElement(ns + "width", mpd.myLineWidth)))
            //                    );
            //}

            if (DataService.Instance.trackData != null)
            {
                var posdata = from p in DataService.Instance.trackData
                              select new { p.longitude, p.latitude };

                East = posdata.Select(x => x.longitude).Max();

                West = posdata.Select(x => x.longitude).Min();

                South = posdata.Select(x => x.latitude).Min();

                North = posdata.Select(x => x.latitude).Max();

                CenterLat = (North + South) / 2.0;
                CenterLon = (East + West) / 2.0;

                NSdist = HaversineDistance(new LatLng(North, CenterLon), new LatLng(South, CenterLon), DistanceUnit.Miles);
                EWdist = HaversineDistance(new LatLng(East, CenterLat), new LatLng(West, CenterLat), DistanceUnit.Miles);

                if (NSdist > EWdist)
                {
                    Range = NSdist * 1.15 * 1609.344;
                }
                else
                {
                    Range = EWdist * 1.0 * 1609.344;
                }

                kmldoc.Descendants(ns + "name").LastOrDefault().AddAfterSelf(
                    new XElement(ns1 + "author",
                        new XElement(ns1 + "name", Properties.Settings.Default.KmlUserName)),
                    new XElement(ns1 + "link", new XAttribute("rel", "related"), new XAttribute("href", "http://www.garmin.com")),
                    new XElement(ns + "description", myDescription),
                    new XElement(ns + "LookAt",
                        new XElement(ns + "longitude", CenterLon),
                        new XElement(ns + "latitude", CenterLat),
                        new XElement(ns + "altitude", 0),
                        new XElement(ns + "heading", 0),
                        new XElement(ns + "tilt", 0),
                        new XElement(ns + "range", Range),
                        new XElement(ns + "altitudeMode", KMLAltitudeMode.relativeToGround)),
                        new XElement(ns + "Style", new XAttribute("id", "myStyleLine"),
                            new XElement(ns + "LineStyle",
                                new XElement(ns + "color", myKmlColor),
                                new XElement(ns + "colorMode", "normal"),
                                new XElement(ns + "width", mpd.myLineWidth)))

                );
            }
            else
            {
                msgBoxObj.ShowNotification("Error in getting track data");
                throw new ArgumentNullException();
            }

            //add the start and end point style elements

            if (Properties.Settings.Default.KmlWriteStart)
            {
                kmldoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Style", new XAttribute("id", "myStyleStartPoint"),
                            new XElement(ns + "IconStyle",
                            new XElement(ns + "scale", 1.1),
                            new XElement(ns + "Icon",
                                new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/paddle/grn-circle.png"))
                                )));
            }

            if (Properties.Settings.Default.KmlWriteEnd)
            {
                kmldoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Style", new XAttribute("id", "myStyleEndPoint"),
                            new XElement(ns + "IconStyle",
                            new XElement(ns + "scale", 1.1),
                            new XElement(ns + "Icon",
                                new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/paddle/E.png"))
                                )));
            }

            kmldoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Style", new XAttribute("id", "myStylePolygon"),
                            new XElement(ns + "IconStyle",
                            new XElement(ns + "color", "ff00ff00"),
                            new XElement(ns + "colorMode", "normal"),
                            new XElement(ns + "scale", 1.1),
                            new XElement(ns + "Icon",
                                new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/shapes/polygon.png"))
                                )));

            //end of the common kml write

            XDocument kmldoc1;

            switch (Properties.Settings.Default.KmlFileType)
            {
                case "Simple Path":
                    //write kml using linq to xml c# - kml version 2.2 - Simple path
                    kmldoc1 = writeSimple(kmldoc, mpd.myKmlFile, productId, mpd.myUserId, mpd.mySport);
                    break;

                case "Time Slider":
                    //garmin training center version updated to kml version 2.2 - Time slider
                    kmldoc1 = writeSlider(kmldoc, mpd.myKmlFile, productId, mpd.myUserId, mpd.mySport);
                    break;

                case "Splits":
                    //sporttracks version updated to kml version 2.2 - Splits
                    kmldoc1 = writeSplits(kmldoc, mpd.myKmlFile, productId, mpd.Mysplitdistance, mpd.myUserId, mpd.mySport);
                    break;

                default:
                    msgBoxObj.ExtendedNotification("KmlFileType error", "Write Kml File", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return false;   //should not get here
            }

            //validate the xml string
            try
            {
                bool errors = false;

                kmldoc1.Validate(schemas, (o, eKml) =>
                {
                    string messageBoxText = "Validation Error Message : " + eKml.Message + ", " + eKml.Severity;
                    string caption = "Kml Validation";

                    //NotifyData data = new NotifyData();
                    //data.notificationName = "KMLValidation";
                    //data.ID = "020";
                    //data.Title = caption;
                    //data.Text = messageBoxText;

                    //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                    //{
                        msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}

                    errors = true;
                });

                if (errors)
                {
                    string messageBoxText = mpd.myKmlFile + " did not validate";
                    string caption = "Kml Validation";

                    //NotifyData data = new NotifyData();
                    //data.notificationName = "KMLValidation";
                    //data.ID = "030";
                    //data.Title = caption;
                    //data.Text = messageBoxText;

                    //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                    //{
                        msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}

                    return false;
                }
                else
                {
                    string messageBoxText = "Success! The KML file : " + mpd.myKmlFile + "," + Environment.NewLine + "is valid";
                    string caption = "Kml Validation";

                    //NotifyData data = new NotifyData();
                    //data.notificationName = "EndWrite";
                    //data.ID = "040";
                    //data.Title = caption;
                    //data.Text = messageBoxText;

                    //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                    //{
                        msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                    //}
                }
            }
            catch (XmlSchemaException ey)
            {
                string messageBoxText = "Exception -> " + ey.Message + Environment.NewLine +
                                        "Inner exception -> " + ey.InnerException + Environment.NewLine +
                                        "Line : " + ey.LineNumber + Environment.NewLine +
                                        "Position : " + ey.LinePosition + Environment.NewLine +
                                        "Schema -> " + ey.SourceSchemaObject;

                string caption = "Kml Validation";

                //NotifyData data = new NotifyData();
                //data.notificationName = "KMLError";
                //data.ID = "050";
                //data.Title = caption;
                //data.Text = messageBoxText;

                //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                //{
                    msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                //}

                return false;
            }

            //save the xml string
            try
            {
                kmldoc1.Save(mpd.myFQKmlFile);
            }
            catch (Exception ex)
            {
                string messageBoxText = "Error in file save" + Environment.NewLine + ex.Message;
                string caption = "KML Write";

                //NotifyData data = new NotifyData();
                //data.notificationName = "KMLError";
                //data.ID = "050";
                //data.Title = caption;
                //data.Text = messageBoxText;

                //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                //{
                    msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                //}

                return false;
            }

            if (Properties.Settings.Default.KmlWriteSp) //if writing a smart phone file
            {
                kmldoc1.Descendants(ns + "Document")
                    .Where(n => n.Element(ns + "name").Value == mpd.myKmlFile)   //modify the file name in the KML file
                    .Single()
                    .SetElementValue(ns + "name", mpd.mySPKmlFile);

                kmldoc1.Descendants(ns + "LineStyle")
                    .Where(n => n.Element(ns + "width").Value == mpd.myLineWidth.ToString())   //modify the KML line width
                    .Single()
                    .SetElementValue(ns + "width", "11");

                //validate the xml string
                try
                {
                    bool errors = false;

                    kmldoc1.Validate(schemas, (o, eKml) =>
                    {
                        string messageBoxText = "Validation Error Message : " + eKml.Message;
                        string caption = "Kml Validation";

                        //NotifyData data = new NotifyData();
                        //data.notificationName = "KMLValidation";
                        //data.ID = "060";
                        //data.Title = caption;
                        //data.Text = messageBoxText;

                        //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                        //{
                            msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        //}

                        errors = true;
                    });

                    if (errors)
                    {
                        string messageBoxText = mpd.mySPKmlFile + " did not validate";
                        string caption = "Kml Validation";

                        //NotifyData data = new NotifyData();
                        //data.notificationName = "KMLValidation";
                        //data.ID = "070";
                        //data.Title = caption;
                        //data.Text = messageBoxText;

                        //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                        //{
                            msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        //}

                        return false;
                    }
                    else
                    {
                        string messageBoxText = "Success! The KML file : " + mpd.mySPKmlFile + "," + Environment.NewLine + "is valid";
                        string caption = "Kml Validation";

                        //NotifyData data = new NotifyData();
                        //data.notificationName = "EndWrite";
                        //data.ID = "080";
                        //data.Title = caption;
                        //data.Text = messageBoxText;

                        //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                        //{
                            msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Information);
                        //}
                    }
                }
                catch (XmlSchemaException ey)
                {
                    string messageBoxText = "Exception -> " + ey.Message + Environment.NewLine +
                                            "Inner exception -> " + ey.InnerException + Environment.NewLine +
                                            "Line : " + ey.LineNumber + Environment.NewLine +
                                            "Position : " + ey.LinePosition + Environment.NewLine +
                                            "Schema -> " + ey.SourceSchemaObject;

                    string caption = "Kml SP Validation";

                    //NotifyData data = new NotifyData();
                    //data.notificationName = "KMLValidation";
                    //data.ID = "090";
                    //data.Title = caption;
                    //data.Text = messageBoxText;

                    //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                    //{
                        msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    //}

                    return false;
                }

                try
                {
                    kmldoc1.Save(mpd.myFQSPKmlFile);
                }
                catch (Exception ex)
                {
                    string messageBoxText = "Error in Smart Phone file save" + Environment.NewLine + ex.Message;
                    string caption = "KML Write";

                    //NotifyData data = new NotifyData();
                    //data.notificationName = "KMLValidation";
                    //data.ID = "100";
                    //data.Title = caption;
                    //data.Text = messageBoxText;

                    //if (!NotifyService.Instance.sendGrowlNotifications("Standard", data))
                    //{
                        msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Stop);
                    //}

                    return false;
                }

            }   //end of smart phone specific processing

            return true;
        }

        private XDocument writeSimple(XDocument xdoc, string myXmlFile, int ProductId, string myuserId, string mysport)
        {
            bool isFirstRecord = true;

            DateTime stStart;
            DateTime stEnd;

            string myCdata;
            string mycoord1 = string.Empty;
            string mycoord2 = string.Empty;
            string myCoords;
            //string myLapId;

            int myStpnbr = 0;

            //TimeSpan myDiff;
            int myrowid = datalayer.getUserData<int, string>(myuserId, "GetRowId");
            Dictionary<int, IEnumerable<myFitRecord>> dict = datalayer.GetTracksByLap(myrowid);

            ObservableCollection<myLapRecord> lapRecs = datalayer.getQuery<myLapRecord>(myrowid, "GetLapDataByKey");

            XNamespace ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
            XNamespace ns1 = XNamespace.Get("http://www.w3.org/2005/Atom");
            XNamespace ns2 = XNamespace.Get("http://www.google.com/kml/ext/2.2");

            foreach (var pair in dict)
            {
                if (isFirstRecord)
                {
                    xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Placemark",
                    new XElement(ns + "styleUrl", "#myStyleLine"),
                    new XElement(ns + "LineString",
                    new XElement(ns + "extrude", 0),
                    new XElement(ns + "tessellate", 1),
                    new XElement(ns + "altitudeMode", KMLAltitudeMode.clampToGround),
                        new XElement(ns + "coordinates", GetCoordinateString(pair.Value as List<myFitRecord>))   //returns a string of all the gps coordinates
                        )));     //first lap - write all the coordinates out for the kml path linestring

                    isFirstRecord = false;
                }
                else
                {
                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Placemark",
                    new XElement(ns + "styleUrl", "#myStyleLine"),
                    new XElement(ns + "LineString",
                    new XElement(ns + "extrude", 0),
                    new XElement(ns + "tessellate", 1),
                    new XElement(ns + "altitudeMode", KMLAltitudeMode.clampToGround),
                        new XElement(ns + "coordinates", GetCoordinateString(pair.Value as List<myFitRecord>))   //returns a string of all the gps coordinates
                        )));     //all other laps - write all the coordinates out for the kml path linestring
                }
            }

            foreach (var pair in dict)
            {
                var lap = pair.Value as List<myFitRecord>;

                //var lap1 = pair.Value.AsAsyncObservableCollection();

                if (ProductId == 1169)     //edge 800
                {
                    myCdata = "<b> Heart Rate: </b>" + lap[0].heartRate.ToString() + " bpm<br/>" +
                                "<b> Cadence: </b>" + lap[0].cadence.ToString() + " rpm";

                    mycoord1 = lap[0].longitude.ToString() + "," + lap[0].latitude.ToString() + ",0";
                    mycoord2 = lap[pair.Value.Count() - 1].longitude.ToString() + "," + lap[pair.Value.Count() - 1].latitude.ToString() + ",0";
                }
                else
                {
                    myCdata = "<b> Heart Rate: </b>" + lap[0].heartRate.ToString() + " bpm<br/>" +
                                "<b> Cadence: </b>" + lap[0].cadence.ToString() + " rpm";

                    mycoord1 = lap[0].longitude.ToString() + "," + lap[0].latitude.ToString() + ",0";
                    mycoord2 = lap[dict.Values.Count() - 1].longitude.ToString() + "," + lap[dict.Values.Count() - 1].latitude.ToString() + ",0";
                }

                //Add the start and stop placemarks for each lap

                if (Properties.Settings.Default.KmlWriteStart)
                {
                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddBeforeSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", "Start Lap " + (pair.Key).ToString() + " - " + lap[0].timeStamp),
                            //new XElement(ns + "name", "Start Lap " + (pair.Key).ToString() + " - " + myElapsedLapStart.ToLocalTime()),
                            new XElement(ns + "visibility", 1),
                            new XElement(ns + "description", new XCData(myCdata)),
                            new XElement(ns + "styleUrl", "#myStyleStartPoint"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", mycoord1))));  //latitude and longitude of the start position
                }

                if (Properties.Settings.Default.KmlWriteEnd)
                {
                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddBeforeSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", "End Lap " + (pair.Key).ToString() + " - " + lap[lap.Count() - 1].timeStamp),
                            //new XElement(ns + "name", "End Lap " + (pair.Key).ToString() + " - " + myElapsedEnd.ToLocalTime()),
                            new XElement(ns + "visibility", 1),
                        //new XElement(ns + "description", new XCData(myLastPlacemark(myLapQuery))),
                            new XElement(ns + "description", new XCData(myLastPlacemark(lapRecs))),
                            new XElement(ns + "styleUrl", "#myStyleEndPoint"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", mycoord2))));  //latitude and longitude of the end position
                }

                if (pair.Key > 1)
                {
                    mycoord2 = lap[lap.Count() - 1].longitude.ToString() + "," + lap[lap.Count() - 1].latitude.ToString() + ",0";

                    xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Style", new XAttribute("id", "myStyleTotal"),
                            new XElement(ns + "IconStyle",
                            new XElement(ns + "scale", 1.1),
                            new XElement(ns + "Icon",
                                new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/shapes/info.png"))
                                )));

                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", "Totals"),
                            new XElement(ns + "visibility", 1),
                        //new XElement(ns + "description", new XCData(myTotalsPlacemark(myLapQuery))),
                            new XElement(ns + "description", new XCData(myTotalsPlacemark(lapRecs))),
                            new XElement(ns + "styleUrl", "#myStyleTotal"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", mycoord2))     //latitude and longitude of the end position
                                ));
                }

            }

            if (Properties.Settings.Default.MapStopTime)
            {
                foreach (var q in DataService.Instance.STPTM)    //for each stop record write some KML
                {
                    //msgBoxObj.ShowNotification("list    start = " + q.Start + Environment.NewLine +
                    //                    "list      end = " + q.End + Environment.NewLine +
                    //                    "List duration = " + q.Duration);

                    if (q.Start == null)
                    {
                        continue;
                    }

                    DateTime.TryParse(q.Start, out stStart);
                    DateTime.TryParse(q.End, out stEnd);
                    //DateTime.TryParse(q.Duration, out stDur);

                    myCdata = "<p><font color=red><b>Stop Time</b><br/><br/>" +
                              "<b>Start: " + "</b>" + stStart.ToString("T") + "<br/><br/>" +
                              "<b>End: " + "</b>" + stEnd.ToString("T") + "<br/><br/>" +
                              "<b>Duration: " + "</b>" + q.Duration + "</br><br/>" +
                              "<b>Latitude: " + "</b>" + q.LatitudeDegrees + "</br><br/>" +
                              "<b>Longitude: " + "</b>" + q.LongitudeDegrees + "</br></font></p>";

                    //DateTime.TryParse(q.Start, out hms);      //convert time string to datetime
                    //myUtc = hms.ToUniversalTime();                  //convert datetime to UTC
                    //myafterstop = myUtc.ToString("s") + "Z";        //build the search string

                    //endindex = mytrackq.FindIndex(s => s.Time == myafterstop);   //search for the time in the list

                    //if (endindex >= 0)
                    //{
                    //    myCoords = mytrackq[endindex].LongitudeDegrees.ToString() + "," + mytrackq[endindex].LatitudeDegrees.ToString() + ",0";

                    myCoords = q.LongitudeDegrees.ToString() + "," + q.LatitudeDegrees.ToString() + ",0";
                        myStpnbr++;

                        xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Placemark",
                                new XElement(ns + "name", "Stopped #" + myStpnbr.ToString()),
                                new XElement(ns + "visibility", 1),
                                new XElement(ns + "open", 1),
                                new XElement(ns + "description", new XCData(myCdata)),
                                new XElement(ns + "styleUrl", "#myStylePolygon"),
                                new XElement(ns + "Point",
                                    new XElement(ns + "coordinates", myCoords)
                                    )));
                    //}
                    //else
                    //{
                    //    //msgBoxObj.ShowNotification("can't find -> " + q.Start);
                    //}

                }   //end of foreach
            }

            return xdoc;    //return the xml string
        
        }   //end of writeSimple()

        private XDocument writeSplits(XDocument xdoc, string myXmlFile, int ProductId, double mySplitDist, string myuserId, string mysport)
        {
            bool myFirstSplit = true;
            int myLapCount;
            int lapLoop;
            int lapRecCnt;
            int mySearchIndex;
            int myIndex;
            int mySaveIndex = 0;
            string myKmlFileName;
            string mySplitLap;
            string myLapId;
            string myCdata;
            string myCoords;
            string mySpvsAvg;
            string myUnits;
            string myLapDescription;
            DateTime myElapsedStart;
            DateTime myElapsedLapStart;
            DateTime myElapsedCurrent;
            DateTime myElapsedLast;
            DateTime myTempCurrent;
            DateTime myElapsedEnd;
            TimeSpan hmsElapsed;
            TimeSpan hmsDiff;
            TimeSpan hmsTemp;
            TimeSpan tsPace;
            double myUnitsMult;
            double maxDistance;
            double mySplit;
            double mySplitIncrement;
            double myTempDistance;
            double myCurrentSpeed;
            double myDistFromLast;
            double mySpeed;
            double mySaveSpeed = 0.0;
            double mySplitDiv;

            myUnitsMult = Properties.Settings.Default.IsMetric ? 1.0 : 3.28084;     //metric or not

            myKmlFileName = myuserId.Substring(0, 10) + mysport + ".kml";           //file name to save under

            DateTime.TryParse(myuserId, out myElapsedStart);                        //get the start time

            int myrowid = datalayer.getUserData<int, string>(myuserId, "GetRowId");

            Dictionary<int, IEnumerable<myFitRecord>> dict = datalayer.GetTracksByLap(myrowid);    //get all track records by lap

            ObservableCollection<myLapRecord> lapRecs = datalayer.getQuery<myLapRecord>(myrowid, "GetLapDataByKey");   //get lap records - all of them

            ObservableCollection<myFitRecord> myTrackData = datalayer.getQuery<myFitRecord>(myrowid, "GetTrackData");  //get track records - all of them

            XNamespace ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
            XNamespace ns1 = XNamespace.Get("http://www.w3.org/2005/Atom");
            XNamespace ns2 = XNamespace.Get("http://www.google.com/kml/ext/2.2");

            xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Style", new XAttribute("id", "myStyleSplit"),
                    new XElement(ns + "IconStyle",
                    new XElement(ns + "scale", 1.1),
                    new XElement(ns + "Icon",
                        new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/pal4/icon56.png")
                        ))));

            myLapCount = lapRecs.Count();    //how many laps in this ride

            mySplitLap = (myLapCount > 1) ? " Lap 1" : string.Empty;

            xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                new XElement(ns + "Folder",
                        new XElement(ns + "name", mysport),
                        new XElement(ns + "open", 1),
                    new XElement(ns + "Folder",
                        new XElement(ns + "name", myElapsedStart.ToString()),
                        new XElement(ns + "open", 1),
                    new XElement(ns + "Folder",
                        new XElement(ns + "name", "Splits" + mySplitLap)
                        ))));

            double[] myDistanceArray = myTrackData.Select(i => i.distance * myUnitsMult).ToArray();     //get all distance records from trackdata - into array

            mySplit = mySplitDist;
            mySplitIncrement = mySplit;

            foreach (var pair in dict)
            {
                List<myFitRecord> trackquery = pair.Value as List<myFitRecord>;     //get all track records for this lap

                lapRecCnt = trackquery.Count();                                     //the count of all the track records for this lap

                maxDistance = trackquery[lapRecCnt - 1].distance * myUnitsMult;     //get the last distance record

                lapLoop = pair.Key - 1;

                if (string.Compare(myuserId, lapRecs[0].lpkey, true) != 0)
                {
                    msgBoxObj.ShowNotification("difference -> " + myuserId + " : " + lapRecs[0].startTime);
                }

                myLapId = lapRecs[lapLoop].startTime;
                DateTime.TryParse(lapRecs[lapLoop].startTime, out myElapsedLapStart);  //get the start time of the current lap

                myCdata = "<br/><b> Heart Rate: </b>" + trackquery[lapLoop].heartRate.ToString() + " bpm" +
                          "<br/><b> Cadence: </b>" + trackquery[lapLoop].cadence.ToString() + " rpm";

                myCoords = trackquery[lapLoop].longitude.ToString() + "," + trackquery[lapLoop].latitude.ToString() + ",0";

                if (myLapCount == 1)
                {
                    //myLapDescription = "Start Lap " + myElapsedLapStart.ToLocalTime();
                    myLapDescription = "Start Lap " + myElapsedLapStart.ToString();
                }
                else
                {
                    //myLapDescription = "Start Lap " + (lapLoop + 1).ToString() + " - " + myElapsedLapStart.ToLocalTime();   //multiple laps get a lap number
                    myLapDescription = "Start Lap " + (lapLoop + 1).ToString() + " - " + myElapsedLapStart.ToString();   //multiple laps get a lap number
                }

                if (Properties.Settings.Default.KmlWriteStart)
                {
                    if (lapLoop == 0)   //first placemark - start
                    {
                        xdoc.Descendants(ns + "name").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Placemark",
                                new XElement(ns + "name", myLapDescription),
                                new XElement(ns + "visibility", 1),
                                new XElement(ns + "open", 1),
                                new XElement(ns + "description", new XCData(myCdata)),
                                    new XElement(ns + "styleUrl", "#myStyleStartPoint"),
                                    new XElement(ns + "Point",
                                        new XElement(ns + "coordinates", myCoords)
                                        )));
                    }
                    else
                    {
                        xdoc.Descendants(ns + "Folder").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Folder",
                                new XElement(ns + "name", "Splits" + " Lap " + (lapLoop + 1).ToString()),
                            new XElement(ns + "Placemark",
                                new XElement(ns + "name", myLapDescription),
                                new XElement(ns + "visibility", 1),
                                new XElement(ns + "open", 1),
                                new XElement(ns + "description", new XCData(myCdata)),
                                    new XElement(ns + "styleUrl", "#myStyleStartPoint"),
                                    new XElement(ns + "Point",
                                        new XElement(ns + "coordinates", myCoords)
                                        ))));
                    }
                }

                //mySplit = mySplitDist;
                //mySplitIncrement = mySplit;

                for (; mySplit <= maxDistance; mySplit += mySplitIncrement)       //write the rest of the placemarks, mySplit is defined out of the for loops
                {
                    mySearchIndex = Array.BinarySearch(myDistanceArray, mySplit);       //binary search for the split distance (meters or feet)

                    if (mySearchIndex > 0)
                    {
                        //msgBoxObj.ShowNotification("Found. index = " + mySearchIndex.ToString());  //exact match
                        myIndex = mySearchIndex;
                    }
                    else if (~mySearchIndex == myDistanceArray.Length)
                    {
                        myIndex = mySearchIndex;
                        string messageBoxText = "Not found, no array object has a greater value " + myIndex.ToString();
                        string caption = "Kml Write";
                        msgBoxObj.ExtendedNotification(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        myIndex = ~mySearchIndex;   //point to the record just greater than the split distance
                    }

                    DateTime.TryParse(myTrackData[myIndex].timeStamp, out myElapsedCurrent);        //time of current record
                    hmsElapsed = myElapsedCurrent - myElapsedStart;                             //total elapsed time

                    DateTime.TryParse(myTrackData[mySaveIndex].timeStamp, out myElapsedLast);       //time of last record
                    hmsDiff = myElapsedCurrent - myElapsedLast;                                 //time from last

                    double myDist = myTrackData[myIndex].distance * (Properties.Settings.Default.IsMetric ? 1.0 : 3.2808399);   //distance so far
                    double myAvgSpeed = (myDist / hmsElapsed.TotalSeconds) * (Properties.Settings.Default.IsMetric ? 1.0 : 0.68181818);

                    DateTime.TryParse(myTrackData[myIndex - 1].timeStamp, out myTempCurrent);       //for current speed
                    hmsTemp = myElapsedCurrent - myTempCurrent;
                    myTempDistance = (myTrackData[myIndex].distance - myTrackData[myIndex - 1].distance);
                    myTempDistance *= (Properties.Settings.Default.IsMetric ? 1.0 : 3.2808399);
                    myCurrentSpeed = (myTempDistance / hmsTemp.TotalSeconds) * (Properties.Settings.Default.IsMetric ? 1.0 : 0.68181818);   //units per second

                    if (myFirstSplit)
                    {
                        myDistFromLast = myTrackData[myIndex].distance;      //distance from last
                        myDistFromLast *= (Properties.Settings.Default.IsMetric ? 1.0 : 3.2808399);
                        mySpeed = (myDistFromLast / hmsElapsed.TotalSeconds) * (Properties.Settings.Default.IsMetric ? 1.0 : 0.68181818);   //units per second
                        //mySpvsAvg = string.Empty;
                        mySpvsAvg = mySpeed.ToString("+0.0;-0.0;0.0");
                        myFirstSplit = false;
                    }
                    else
                    {
                        myDistFromLast = (myTrackData[myIndex].distance - myTrackData[mySaveIndex].distance); //distance from last
                        myDistFromLast *= (Properties.Settings.Default.IsMetric ? 1.0 : 3.2808399);
                        mySpeed = (myDistFromLast / hmsDiff.TotalSeconds) * (Properties.Settings.Default.IsMetric ? 1.0 : 0.68181818);      //units per second
                        mySpvsAvg = (mySpeed - mySaveSpeed).ToString("+0.0;-0.0;0.0");
                    }

                    //j = 60.0 / mySpeed;                 //pace
                    //tsPace = TimeSpan.FromMinutes(j);

                    tsPace = TimeSpan.FromMinutes((60.0 / mySpeed));    //pace

                    myCdata = string.Empty;     //i probably don't have to to this

                    myCdata = "<b>" + myElapsedCurrent.ToLongTimeString() + "</b><br/>" +
                              "<b> Time Elapsed: </b>" + hmsElapsed.ToString() + "<br/" +
                              "<b> Average Speed: </b>" + myAvgSpeed.ToString("#.0") + (Properties.Settings.Default.IsMetric ? " Km/h<br/>" : " MPH<br/>") +
                              "<b> Current Speed: </b>" + myCurrentSpeed.ToString("#.0") + (Properties.Settings.Default.IsMetric ? " Km/h<br/>" : " MPH<br/>") +
                              "<b> Split Speed: </b>" + mySpeed.ToString("#.0") + (Properties.Settings.Default.IsMetric ? " Km/h<br/>" : " MPH<br/>") +
                              "<b> Split Speed vs Avg: </b>" + (mySpeed - myAvgSpeed).ToString("+0.0;-0.0;0.0") + (Properties.Settings.Default.IsMetric ? " Km/h<br/>" : " MPH<br/>") +
                              "<b> Speed Change vs Last: </b>" + mySpvsAvg + "<br/>" +
                              "<b> Split Pace: </b>" + tsPace.ToString(@"mm\:ss\.f") + (Properties.Settings.Default.IsMetric ? " per Km<br/>" : " per Mile<br/>") +
                              "<b> Heart Rate: </b>" + myTrackData[myIndex].heartRate.ToString() + " bpm<br/>" +
                              "<b> Cadence: </b>" + myTrackData[myIndex].cadence.ToString() + " rpm<br/>" +
                              "<b> Latitude: </b>" + myTrackData[myIndex].latitude.ToString() + "<br/>" +
                              "<b> Longitude: </b>" + myTrackData[myIndex].longitude.ToString() + "<br/>";

                    myCoords = myTrackData[myIndex].longitude.ToString() + "," +
                               myTrackData[myIndex].latitude.ToString() + "," +
                              (myTrackData[myIndex].altitude * (Properties.Settings.Default.IsMetric ? 1.0 : 0.68181818)).ToString();

                    mySplitDiv = Properties.Settings.Default.IsMetric ? 1000.0 : 5280.0;
                    myUnits = Properties.Settings.Default.IsMetric ? " Km" : " Miles";

                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", (mySplit / mySplitDiv).ToString("0.00") + myUnits),   //fix this for no start placemark
                            new XElement(ns + "visibility", 1),
                            new XElement(ns + "open", 1),
                            new XElement(ns + "description", new XCData(myCdata)),
                            new XElement(ns + "styleUrl", "#myStyleSplit"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));

                    mySaveIndex = myIndex;  //save for the next loop
                    mySaveSpeed = mySpeed;  //save for the next loop

                }   //end of for loop - write splits - loop variable -> mySplits

                if (Properties.Settings.Default.KmlWriteEnd)
                {
                    //DateTime.TryParse(trackquery[lapLoop].timeStamp, out myElapsedEnd);
                    DateTime.TryParse(trackquery[lapRecCnt - 1].timeStamp, out myElapsedEnd);

                    myCoords = lapRecs[lapLoop].endPosLong.ToString() + "," + lapRecs[lapLoop].endPosLat.ToString() + ",0";

                    if (myLapCount == 1)    //write end placemark
                    {
                        //myLapDescription = "Stop Lap " + myElapsedEnd.ToLocalTime();
                        myLapDescription = "Stop Lap " + myElapsedEnd.ToString();
                    }
                    else
                    {
                        //myLapDescription = "Stop Lap " + (lapLoop + 1).ToString() + " - " + myElapsedEnd.ToLocalTime();
                        myLapDescription = "Stop Lap " + (lapLoop + 1).ToString() + " - " + myElapsedEnd.ToString();
                    }

                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", myLapDescription),
                            new XElement(ns + "visibility", 1),
                            new XElement(ns + "open", 1),
                            new XElement(ns + "description", new XCData(myLastPlacemark(lapRecs))),
                                new XElement(ns + "styleUrl", "#myStyleEndPoint"),
                                new XElement(ns + "Point",
                                    new XElement(ns + "coordinates", myCoords)
                                    )));
                }

            }   //end of foreach - Lap Records

            if (Properties.Settings.Default.MapStopTime)
            {
                int myStpnbr = 0;
                DateTime stStart;
                DateTime stEnd;

                foreach (var q in DataService.Instance.STPTM)    //for each stop record write some KML
                {
                    if (q.Start == null)
                    {
                        continue;
                    }

                    DateTime.TryParse(q.Start, out stStart);
                    DateTime.TryParse(q.End, out stEnd);

                    myCdata = "<p><font color=red><b>Stop Time</b><br/><br/>" +
                              "<b>Start: " + "</b>" + stStart.ToString("T") + "<br/><br/>" +
                              "<b>End: " + "</b>" + stEnd.ToString("T") + "<br/><br/>" +
                              "<b>Duration: " + "</b>" + q.Duration + "</br><br/>" +
                              "<b>Latitude: " + "</b>" + q.LatitudeDegrees + "</br><br/>" +
                              "<b>Longitude: " + "</b>" + q.LongitudeDegrees + "</br></font></p>";

                    myCoords = q.LongitudeDegrees.ToString() + "," + q.LatitudeDegrees.ToString() + ",0";

                    myStpnbr++;

                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", "Stopped #" + myStpnbr.ToString()),
                            new XElement(ns + "visibility", 1),
                            new XElement(ns + "open", 1),
                            new XElement(ns + "description", new XCData(myCdata)),
                            new XElement(ns + "styleUrl", "#myStylePolygon"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));
                }
            }

            if (myLapCount > 1) //totals for multiple lap events
            {
                myCoords = lapRecs[lapRecs.Count() - 1].endPosLong.ToString() + "," + lapRecs[lapRecs.Count() - 1].endPosLat.ToString() + ",0";

                xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Style", new XAttribute("id", "myStyleTotal"),
                        new XElement(ns + "IconStyle",
                        new XElement(ns + "scale", 1.1),
                        new XElement(ns + "Icon",
                            new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/shapes/info.png"))
                            )));

                xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Placemark",
                        new XElement(ns + "name", "Totals"),
                        new XElement(ns + "visibility", 1),
                        new XElement(ns + "open", 1),
                        new XElement(ns + "description", new XCData(myTotalsPlacemark(lapRecs))),
                            new XElement(ns + "styleUrl", "#myStyleTotal"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));

            }   //end of lap count > 1

            for (lapLoop = 0; lapLoop < myLapCount; lapLoop++)  //multigeometry
            {
                var item = dict.ElementAt(lapLoop);
                var itemKey = item.Key;
                var itemValue = item.Value;

                List<myFitRecord> mypos = itemValue as List<myFitRecord>;

                if (lapLoop == 0)
                {
                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        //new XElement(ns + "Folder",
                        //    new XElement(ns + "name", "Route"),
                        //    new XElement(ns + "visibility", 1),
                        //    new XElement(ns + "open", 1),
                        new XElement(ns + "Placemark",
                        new XElement(ns + "name", myKmlFileName),
                        new XElement(ns + "visibility", 1),
                        new XElement(ns + "open", 1),
                            new XElement(ns + "styleUrl", "#myStyleLine"),
                            new XElement(ns + "MultiGeometry",
                            new XElement(ns + "LineString",
                                new XElement(ns + "coordinates", GetCoordinateString(mypos))
                                ))));
                }
                else
                {
                    xdoc.Descendants(ns + "LineString").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "LineString",
                            new XElement(ns + "coordinates", GetCoordinateString(mypos))
                            ));
                }

            }   //end of laploop for -> multigeometry


            return xdoc;

        }   //end of writeSplits()

        private XDocument writeSlider(XDocument xdoc, string myXmlFile, int ProductId, string myuserId, string mysport)
        {
            bool isFirstPlacemark = true;

            DateTime myElapsedStart;
            DateTime myElapsedCurrent;
            DateTime myElapsedLast;
            DateTime myElapsedEnd;
            DateTime myElapsedLapStart;
            //DateTime hms1;
            DateTime stStart;
            DateTime stEnd;

            TimeSpan hmsElapsed;
            TimeSpan hmsDiff;
            TimeSpan tsPace;
            //TimeSpan myDiff;

            int loop;
            int lapIndex;
            int myStpnbr = 0;
            int myTq;
            int lapCount = 0;

            double dist;
            double distfromlast;
            double speed;
            double j = 0.0;
            double myUnitsMult;

            string myCdata;
            string myLapDescription;
            string myCoords;
            string myLapId = myuserId;
            string myUnits;
            string myAltitude;

            myUnitsMult = Properties.Settings.Default.IsMetric ? 1.0 : 3.28084;

            string myKmlFileName = myuserId.Substring(0, 10) + mysport + ".kml";

            int myrowid = datalayer.getUserData<int, string>(myuserId, "GetRowId");

            Dictionary<int, IEnumerable<myFitRecord>> dict = datalayer.GetTracksByLap(myrowid);

            ObservableCollection<myLapRecord> lapRecs = datalayer.getQuery<myLapRecord>(myrowid, "GetLapDataByKey");

            ObservableCollection<myFitRecord> myTrackData = datalayer.getQuery<myFitRecord>(myrowid, "GetTrackData");

            DateTime.TryParse(myuserId, out myElapsedStart);  //get the start time

            XNamespace ns = XNamespace.Get("http://www.opengis.net/kml/2.2");
            XNamespace ns1 = XNamespace.Get("http://www.w3.org/2005/Atom");
            XNamespace ns2 = XNamespace.Get("http://www.google.com/kml/ext/2.2");

            xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                 new XElement(ns + "Style", new XAttribute("id", "myStylePoint"),
                    new XElement(ns + "IconStyle",
                    new XElement(ns + "scale", 0.9),
                    new XElement(ns + "Icon",
                        new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/shapes/cycling.png"))
                        )));

            //http://maps.google.com/mapfiles/kml/shapes/cycling.png
            //http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png

            xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                new XElement(ns + "Folder",
                    new XElement(ns + "name", "History"),
                    new XElement(ns + "open", 0),
                    new XElement(ns + "Folder",
                        new XElement(ns + "name", mysport),
                        new XElement(ns + "open", 0),
                        new XElement(ns + "Folder",
                            new XElement(ns + "name", myElapsedStart.ToString()),
                            new XElement(ns + "open", 0),
                            new XElement(ns + "Folder",
                                new XElement(ns + "name", "Lap 1"))
                                ))));

            foreach (var pair in dict)
            {
                lapIndex = pair.Key - 1;

                lapCount = lapRecs.Count();

                myLapId = lapRecs[lapIndex].startTime;
                DateTime.TryParse(lapRecs[lapIndex].startTime, out myElapsedLapStart);  //get the start time of the current lap

                List<myFitRecord> trackquery = pair.Value as List<myFitRecord>;

                myTq = trackquery.Count();

                myCdata = "<br/><b> Heart Rate: </b>" + trackquery[0].heartRate.ToString() + " bpm" +
                          "<br/><b> Cadence: </b>" + trackquery[0].cadence.ToString() + " rpm";

                myAltitude = convToStr(trackquery[0].altitude, 3.280399);

                myCoords = trackquery[0].longitude.ToString() + "," + trackquery[0].latitude.ToString() + "," + myAltitude;

                if (lapCount == 1)
                {
                    myLapDescription = "Start Lap " + myElapsedLapStart.ToString();
                }
                else
                {
                    myLapDescription = "Start Lap " + (pair.Key).ToString() + " - " + myElapsedLapStart.ToLocalTime();   //multiple laps get a lap number
                }

                if (Properties.Settings.Default.KmlWriteStart)
                {
                    if (lapIndex == 0)   //first placemark - start
                    {
                        string strTs = convertDateTime(trackquery[0].timeStamp);

                        xdoc.Descendants(ns + "name").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Placemark",
                            new XElement(ns + "name", myLapDescription),
                                new XElement(ns + "visibility", 1),
                                new XElement(ns + "description", new XCData(myCdata)),
                                //new XElement(ns + "TimeSpan",
                                    //new XElement(ns + "begin", strTs),
                                    //new XElement(ns + "end", strTs)),
                                    new XElement(ns + "styleUrl", "#myStyleStartPoint"),
                                    new XElement(ns + "Point",
                                        new XElement(ns + "coordinates", myCoords)
                                        )));
                    }
                    else
                    {
                        xdoc.Descendants(ns + "Folder").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Folder",
                                new XElement(ns + "name", "Lap " + (pair.Key).ToString()),
                            new XElement(ns + "Placemark",
                                new XElement(ns + "name", myLapDescription),
                                new XElement(ns + "visibility", 1),
                                new XElement(ns + "description", new XCData(myCdata)),
                                    new XElement(ns + "styleUrl", "#myStyleStartPoint"),
                                    new XElement(ns + "Point",
                                        new XElement(ns + "coordinates", myCoords))
                                        )));
                    }
                }

                for (loop = 1; loop < myTq; loop++)
                {
                    DateTime.TryParse(trackquery[loop].timeStamp, out myElapsedCurrent);        //time of current record
                    hmsElapsed = myElapsedCurrent - myElapsedStart;                                 //total elapsed time

                    DateTime.TryParse(trackquery[loop - 1].timeStamp, out myElapsedLast);           //time of last record
                    hmsDiff = myElapsedCurrent - myElapsedLast;                                     //time from last

                    dist = (trackquery[loop].distance - trackquery[0].distance);                    //distance so far
                    distfromlast = (trackquery[loop].distance - trackquery[loop - 1].distance);     //distance from last

                    if (distfromlast == 0.0)
                    {
                        continue;
                    }

                    if (!Properties.Settings.Default.IsMetric)
                    {
                        dist *= 3.2808399;
                        distfromlast *= 3.2808399;      //convert meters to feet
                    }

                    speed = (distfromlast / hmsDiff.TotalSeconds);      //mph

                    if (!Properties.Settings.Default.IsMetric)
                    {
                        speed *= 0.68181818;
                        myUnits = " ft<br/>";
                    }
                    else
                    {
                        myUnits = " km<br/>";
                    }

                    j = 60.0 / speed;
                    tsPace = TimeSpan.FromMinutes(j);

                    myAltitude = convToStr(trackquery[loop].altitude, 3.2808399);

                    myCdata = string.Empty;

                    myCdata = "<b>" + myElapsedCurrent.ToLongTimeString() + "</b><br/>" +
                            "<b> Time Elapsed: </b>" + hmsElapsed.ToString() + "<br/" +
                            "<b> Distance from last: </b>" + distfromlast.ToString("#.00") + myUnits +
                            "<b> Distance Elapsed: </b>" + dist.ToString("#.00") + myUnits +
                            "<b> Speed: </b>" + speed.ToString("#.0") + " mph<br/>" +
                            "<b> Pace: </b>" + tsPace.ToString(@"mm\:ss\.f") + "/mi<br/>" +
                            "<b> Heart Rate: </b>" + trackquery[loop].heartRate.ToString() + " bpm<br/>" +
                            "<b> Cadence: </b>" + trackquery[loop].cadence.ToString() + " rpm";

                    myCoords = trackquery[loop].longitude.ToString() +
                            "," + trackquery[loop].latitude.ToString() +
                            "," + myAltitude;

                    string tmStamp1 = convertDateTime(trackquery[loop - 1].timeStamp);
                    string tmStamp2 = convertDateTime(trackquery[loop].timeStamp);

                    if (isFirstPlacemark && !Properties.Settings.Default.KmlWriteStart)
                    {
                        xdoc.Descendants(ns + "name").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Placemark",
                                new XElement(ns + "description", new XCData(myCdata)),
                            new XElement(ns + "TimeSpan",
                                new XElement(ns + "begin", tmStamp1),
                                new XElement(ns + "end", tmStamp2)),
                            new XElement(ns + "styleUrl", "#myStylePoint"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));

                        isFirstPlacemark = false;
                    }
                    else
                    {
                        xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                            new XElement(ns + "Placemark",
                                new XElement(ns + "description", new XCData(myCdata)),
                            new XElement(ns + "TimeSpan",
                                new XElement(ns + "begin", tmStamp1),
                                new XElement(ns + "end", tmStamp2)),
                            new XElement(ns + "styleUrl", "#myStylePoint"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));
                    }

                }   //end of for loop - write timespans

                DateTime.TryParse(trackquery[myTq - 1].timeStamp, out myElapsedEnd);

                myAltitude = convToStr(trackquery[myTq - 1].altitude, 3.2808399);

                myCoords = trackquery[myTq - 1].longitude.ToString() +
                    "," + trackquery[myTq - 1].latitude.ToString() +
                    "," + myAltitude;

                if (Properties.Settings.Default.KmlWriteEnd)
                {
                    if (lapIndex == 1)    //write end placemark
                    {
                        myLapDescription = "Stop Lap " + myElapsedEnd.ToString();
                    }
                    else
                    {
                        myLapDescription = "Stop Lap " + (lapIndex + 1).ToString() + " - " + myElapsedEnd.ToString();
                    }

                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", myLapDescription),
                            new XElement(ns + "visibility", 1),
                            new XElement(ns + "description", new XCData(myLastPlacemark(lapRecs))),
                                new XElement(ns + "styleUrl", "#myStyleEndPoint"),
                                new XElement(ns + "Point",
                                    new XElement(ns + "coordinates", myCoords)
                                    )));
                }

            }   //end of foreach - laps

            if (lapCount > 1)
            {
                myCoords = lapRecs[lapCount - 1].endPosLong.ToString() + lapRecs[lapCount - 1].endPosLat.ToString() + ",0";

                xdoc.Descendants(ns + "Style").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Style", new XAttribute("id", "myStyleTotal"),
                        new XElement(ns + "IconStyle",
                        new XElement(ns + "scale", 1.1),
                        new XElement(ns + "Icon",
                            new XElement(ns + "href", "http://maps.google.com/mapfiles/kml/shapes/info.png"))
                            )));

                xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                    new XElement(ns + "Placemark",
                        new XElement(ns + "name", "Totals"),
                        new XElement(ns + "visibility", 1),
                        new XElement(ns + "description", new XCData(myTotalsPlacemark(lapRecs))),
                            new XElement(ns + "styleUrl", "#myStyleTotal"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));
            }

            if (Properties.Settings.Default.MapStopTime)
            {
                foreach (var q in DataService.Instance.STPTM)    //for each stop record write some KML
                {
                    if (q.Start == null)
                    {
                        continue;       //if there are no stop records
                    }

                    DateTime.TryParse(q.Start, out stStart);
                    DateTime.TryParse(q.End, out stEnd);

                    myCdata = "<p><font color=red><b>Stop Time</b><br/><br/>" +
                              "<b>Start: " + "</b>" + stStart.ToString("T") + "<br/><br/>" +
                              "<b>End: " + "</b>" + stEnd.ToString("T") + "<br/><br/>" +
                              "<b>Duration: " + "</b>" + q.Duration + "</br><br/>" +
                              "<b>Latitude: " + "</b>" + q.LatitudeDegrees + "</br><br/>" +
                              "<b>Longitude: " + "</b>" + q.LongitudeDegrees + "</br></font></p>";

                    myCoords = q.LongitudeDegrees.ToString() + "," + q.LatitudeDegrees.ToString() + ",0";

                    myStpnbr++;

                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                            new XElement(ns + "name", "Stopped #" + myStpnbr.ToString()),
                            new XElement(ns + "visibility", 1),
                            new XElement(ns + "open", 1),
                            new XElement(ns + "description", new XCData(myCdata)),
                            new XElement(ns + "styleUrl", "#myStylePolygon"),
                            new XElement(ns + "Point",
                                new XElement(ns + "coordinates", myCoords)
                                )));
                }
            }

            for (int lapLoop = 0; lapLoop < lapCount; lapLoop++)  //multigeometry - shows the path
            {
                var item = dict.ElementAt(lapLoop);
                var itemKey = item.Key;
                var itemValue = item.Value;

                List<myFitRecord> mypos = itemValue as List<myFitRecord>;
                
                if (lapLoop == 0)
                {
                    xdoc.Descendants(ns + "Placemark").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "Placemark",
                        new XElement(ns + "name", myKmlFileName),
                        new XElement(ns + "visibility", 1),
                        new XElement(ns + "open", 1),
                            new XElement(ns + "styleUrl", "#myStyleLine"),
                            new XElement(ns + "MultiGeometry",
                            new XElement(ns + "LineString",
                                new XElement(ns + "coordinates", GetCoordinateString(mypos)))
                                )));
                }
                else
                {
                    xdoc.Descendants(ns + "LineString").LastOrDefault().AddAfterSelf(
                        new XElement(ns + "LineString",
                            new XElement(ns + "coordinates", GetCoordinateString(mypos)))
                            );
                }

            }   //end of laploop for -> multigeometry

            return xdoc;

        }   //end of writeSlider()

        private string convertDateTime(string timestamp)
        {
            DateTime tstamp;

            DateTime.TryParse(timestamp, out tstamp);

            //return tstamp.ToString("s") + "Z";

            return TimeZoneInfo.ConvertTimeToUtc(tstamp).ToString("s") + "Z";
        }

        private string GetCoordinateString(List<myFitRecord> myPosition)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(myPosition[0].longitude.ToString() +
                "," + myPosition[0].latitude.ToString() +
                "," + convToStr(myPosition[0].altitude, 3.2808399));

            for (int x = 1; x < myPosition.Count; x++)
            {
                sb.AppendLine("\t\t\t" + myPosition[x].longitude.ToString() +
                    "," + myPosition[x].latitude.ToString() +
                    "," + convToStr(myPosition[x].altitude, 3.2808399));
            }

            return sb.ToString();
        }

        private string convToStr(double first, double second)
        {
            if (!Properties.Settings.Default.IsMetric)
            {
                return (first * second).ToString("#.0");
            }
            else
            {
                return first.ToString("#.0");
            }
        }

        private string myLastPlacemark(ObservableCollection<myLapRecord> myLapQ)
        {
            double mtt = 0.0;
            double myTotalDist = 0.0;
            double myMaxSpeed = 0.0;
            double myAvgSpeed = 0.0;
            double j;

            int myCalories = 0;
            int myAvgHr = 0;
            int myMaxHr = 0;
            int myAvgCadence = 0;

            TimeSpan tsPace = new TimeSpan();

            string myTotalTime = string.Empty;
            string myCdata;

            foreach (var q in myLapQ)   //to do: add up each lap
            {
                mtt = TimeSpan.Parse(q.totalElaspedTime).TotalSeconds;
                //mtt = q.TotalTimeSeconds;
                TimeSpan interval = TimeSpan.FromSeconds(mtt);
                myTotalTime = interval.ToString(@"hh\:mm\:ss\.ff");
                myTotalDist = q.totalDistance * (Properties.Settings.Default.IsMetric ? 1.0 : 0.000621371192);  //meters to miles
                //myTotalDist = q.DistanceMeters * (Properties.Settings.Default.IsMetric ? 1.0 : 0.000621371192);
                myMaxSpeed = q.maxSpeed * (Properties.Settings.Default.IsMetric ? 1.0 : 2.2369362920544);      //meters per second to miles per hour
                //myMaxSpeed = q.MaximumSpeed * (Properties.Settings.Default.IsMetric ? 1.0 : 2.2369362920544);      //meters per second to miles per hour
                myCalories = q.totalCalories;
                //myCalories = q.Calories;
                myAvgHr = q.avgHeartRate;
                //myAvgHr = q.AverageHeartRateBpm;
                myMaxHr = q.maxHeartRate;
                //myMaxHr = q.MaximumHeartRateBpm;
                myAvgCadence = q.avgCadence;
                //myAvgCadence = q.Cadence;
                myAvgSpeed = q.avgSpeed * (Properties.Settings.Default.IsMetric ? 1.0 : 2.2369362920544);
                //myAvgSpeed = (q.DistanceMeters / q.TotalTimeSeconds) * (Properties.Settings.Default.IsMetric ? 1.0 : 2.2369362920544);     // m/s to mph

                j = 60.0 / myAvgSpeed;             //pace
                tsPace = TimeSpan.FromMinutes(j);
            }

            myCdata = "<b> Total Distance: </b>" + myTotalDist.ToString("0.0") + (Properties.Settings.Default.IsMetric ? " Kilometers<br/>" : " Miles<br/>") +
                      "<b> Total Time: </b>" + myTotalTime + "<br/>" +
                      "<b> Average Speed: </b>" + myAvgSpeed.ToString("0.0") + (Properties.Settings.Default.IsMetric ? " Km/hr<br/>" : " MPH<br/>") +
                      "<b> Average Pace: </b>" + tsPace.ToString(@"mm\:ss\.f") + (Properties.Settings.Default.IsMetric ? " per Km<br/>" : " per Mile<br/>") +
                      "<b> Maximum Speed: </b>" + myMaxSpeed.ToString("0.0") + (Properties.Settings.Default.IsMetric ? " Km/hr<br/>" : " MPH<br/>") +
                      "<b> Calories: </b>" + myCalories.ToString() + "<br/>" +
                      "<b> Average Heart Rate: </b>" + myAvgHr.ToString() + " bpm<br/>" +
                      "<b> Maximum Heart Rate: </b>" + myMaxHr.ToString() + " bpm<br/>" +
                      "<b> Average Cadence: </b>" + myAvgCadence.ToString() + " rpm";

            return myCdata;

        }   //end of myLastPlacemark()

        public static string myTotalsPlacemark(IEnumerable<myLapRecord> myLapQ)   //totals for multiple lap events
        {
            double mtt = 0.0;
            double myTotalDist = 0.0;
            double myMaxSpeed = 0.0;
            double myAvgSpeed = 0.0;
            double j;

            int myCalories = 0;
            int myAvgHr = 0;
            int myMaxHr = 0;
            int myAvgCadence = 0;

            TimeSpan tsPace = new TimeSpan();

            string myTotalTime = string.Empty;
            string myCdata;

            //string test = myLapQ.First().lpkey;

            foreach (var q in myLapQ)   //add up each lap
            {
                mtt += TimeSpan.Parse(q.totalElaspedTime).TotalSeconds;
                //mtt += q.TotalTimeSeconds;
                myTotalDist += q.totalDistance;
                //myTotalDist += q.DistanceMeters;
                myCalories += q.totalCalories;
                //myCalories += q.Calories;

                if (q.maxSpeed > myMaxSpeed)
                {
                    myMaxSpeed = q.maxSpeed;
                }
                //if (q.MaximumSpeed > myMaxSpeed)
                //{
                //    myMaxSpeed = q.MaximumSpeed;
                //}

                if (q.maxHeartRate > myMaxHr)
                {
                    myMaxHr = q.maxHeartRate;
                }
                //if (q.MaximumHeartRateBpm > myMaxHr)
                //{
                //    myMaxHr = q.MaximumHeartRateBpm;
                //}

                myAvgHr += q.avgHeartRate;
                //myAvgHr += q.AverageHeartRateBpm;

                myAvgCadence += q.avgCadence;
                //myAvgCadence += q.Cadence;
            }

            myAvgSpeed = (myTotalDist / mtt) * 2.2369362920544;
            
            j = 60.0 / myAvgSpeed;             //pace
            tsPace = TimeSpan.FromMinutes(j);

            TimeSpan interval = TimeSpan.FromSeconds(mtt);
            myTotalTime = interval.ToString(@"hh\:mm\:ss\.ff");

            myTotalDist *= (Properties.Settings.Default.IsMetric ? 1.0 : 0.000621371192);
            myMaxSpeed *= (Properties.Settings.Default.IsMetric ? 1.0 : 2.2369362920544);
            myAvgHr /= myLapQ.Count();
            myAvgCadence /= myLapQ.Count();

            myCdata = "<b> Total Distance: </b>" + myTotalDist.ToString("0.0") + (Properties.Settings.Default.IsMetric ? " Km<br/>" : " Miles<br/>") +
                      "<b> Total Time: </b>" + myTotalTime + "<br/>" +
                      "<b> Average Speed: </b>" + myAvgSpeed.ToString("0.0") + (Properties.Settings.Default.IsMetric ? " Km/hr<br/>" : " MPH<br/>") +
                      "<b> Average Pace: </b>" + tsPace.ToString(@"mm\:ss\.f") + (Properties.Settings.Default.IsMetric ? " per Km<br/>" : " per Mile<br/>") +
                      "<b> Maximum Speed: </b>" + myMaxSpeed.ToString("0.0") + (Properties.Settings.Default.IsMetric ? " Km/hr<br/>" : " MPH<br/>") +
                      "<b> Calories: </b>" + myCalories.ToString() + "<br/>" +
                      "<b> Average Heart Rate: </b>" + myAvgHr.ToString() + " bpm<br/>" +
                      "<b> Maximum Heart Rate: </b>" + myMaxHr.ToString() + " bpm<br/>" +
                      "<b> Average Cadence: </b>" + myAvgCadence.ToString() + " rpm";

            return myCdata;

        }   //end of myTotalsPlacemark()

        private double HaversineDistance(LatLng pos1, LatLng pos2, DistanceUnit unit)
        {
            double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;

            var lat = (pos2.Latitude - pos1.Latitude).ToRadians();

            var lng = (pos2.Longitude - pos1.Longitude).ToRadians();

            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                          Math.Cos(pos1.Latitude.ToRadians()) * Math.Cos(pos2.Latitude.ToRadians()) *
                          Math.Sin(lng / 2) * Math.Sin(lng / 2);

            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));

            return R * h2;
        }
    }
}
