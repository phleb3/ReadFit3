using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Media;
using System.Reflection;
using ReadFit.FileModel;
using System.Deployment.Application;

namespace ReadFit
{
    class XmlDataAccess : IXmlFunctions
    {
        private static MsgBoxService msgBoxobj;

        static XmlDataAccess()
        {
            msgBoxobj = new MsgBoxService();
        }

        public void ReadFile(string filename)
        {
            throw new NotImplementedException();
        }

        public bool WriteFile(MyPassedData mypass)
        {
            //throw new NotImplementedException();

            WriteKmlFiles wkf = new WriteKmlFiles();
            bool flag = wkf.writeFile(mypass);

            return flag;

            //write file on background thread
        }

        public XDocument ReadInitialWkml(string myFilePath)
        {
            Dictionary<string, double> mld = new Dictionary<string, double>();
            Dictionary<string, Color> mycolordict = new Dictionary<string, Color>();

            XDocument splitdistance = null;
            string path = string.Empty;

            try
            {
                //splitdistance = XDocument.Load("utilitiesDocument.xml");  //get the splitdistance and splitincrement values

                splitdistance = XDocument.Load(myFilePath);
            }
            catch (Exception ex)
            {
                msgBoxobj.ShowNotification("Error in reading xmlfile" + "message -> " + ex.Message + "exception -> " + ex.InnerException + "stacktrace -> " + ex.StackTrace);
            }

            var query = from c in splitdistance.Descendants("Split")
                        select new
                        {
                            namestr = c.Element("Name").Value,
                            dist = c.Element("Distance").Value
                        };

            if (query.IsNullOrEmpty())
            {
                msgBoxobj.ShowNotification("no data");
            }

            foreach (var mydist in query)
            {
                mld.Add(mydist.namestr, Convert.ToDouble(mydist.dist));
            }

            //msgBoxobj.ShowNotification("dictionary count = " + mld.Count());

            ObservableCollection<string> mycolornames = new ObservableCollection<string>();

            foreach (string mc in ColorHelper.GetColorNames())  //add the color names to an observable collection and dictionary
            {
                mycolornames.Add(mc);   //observable collection

                mycolordict.Add(mc, (Color)ColorConverter.ConvertFromString(mc));   //dictionary - color string name, color hex value!!
            }

            MessageBus.Instance.Publish<ReturnInitial>(new ReturnInitial { Mld = mld, MyColorDict = mycolordict, MyColorNames = mycolornames });

            return splitdistance;
        }

        private static class ColorHelper     //returns the names of colors, uses Reflection for BindingFlags
        {
            public static IEnumerable<string> GetColorNames()
            {
                foreach (PropertyInfo p in typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    yield return p.Name;    //returns color names one at a time
                }
            }
        }
    }
}
