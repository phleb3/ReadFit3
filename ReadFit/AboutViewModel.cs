using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ReadFit.FileModel;
using System.Deployment.Application;
using System.Reflection;

namespace ReadFit
{

    class AboutViewModel : ObservableObject, IPageViewModel
    {
        public string Name
        {
            get { return "About"; }
        }

        string saveMsg { get; set; }

        MsgBoxService msgBoxobj;

        public AboutViewModel()
        {
            msgBoxobj = new MsgBoxService();

            saveMsg = "Max Heart Rate has been saved." + Environment.NewLine + "Enter 0 to clear this field";

            if (Properties.Settings.Default.MaximumHeartRate == 0)
            {
                MaxHrtRate = null;
            }
            else
            {
                MaxHrtRate = Properties.Settings.Default.MaximumHeartRate;
            }

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                VersionInfo = "Version: " + ad.CurrentVersion.ToString();
            }
            else
            {
                Assembly assem = typeof(AboutViewModel).Assembly;
                AssemblyName assemName = assem.GetName();
                Version ver = assemName.Version;
                VersionInfo = "Version " + ver.ToString();
            }
        }

        private string _VersionInfo;
        public string VersionInfo
        {
            get { return _VersionInfo; }
            set
            {
                if (value != _VersionInfo)
                {
                    _VersionInfo = value;
                    OnPropertyChanged("VersionInfo");
                }
            }
        }
      
        private ICommand changeUnits;
        public ICommand ChangeUnits
        {
            get
            {
                if (changeUnits == null)
                {
                    changeUnits = new RelayCommand(p => sendUnitMsg());
                }

                return changeUnits;
            }
        }

        private void sendUnitMsg()
        {
            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Correction", FlagState = DataService.Instance.DistanceTimeFlag });
            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Chart", FlagState = DataService.Instance.DistanceTimeFlag });
        }

        private ICommand changeGraph;
        public ICommand ChangeGraph
        {
            get
            {
                if (changeGraph == null)
                {
                    changeGraph = new RelayCommand(p => sendChangeGraph());
                }

                return changeGraph;
            }
        }

        private void sendChangeGraph()
        {
            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Chart", FlagState = DataService.Instance.DistanceTimeFlag });      //redraw chart
        }

        private ICommand saveHeartRate;
        public ICommand SaveHeartRate
        {
            get
            {
                if (saveHeartRate == null)
                {
                    saveHeartRate = new RelayCommand(p => saveTheHeartRate());
                }

                return saveHeartRate;
            }
        }

        private void saveTheHeartRate()
        {
            Properties.Settings.Default.MaximumHeartRate = MaxHrtRate.Value;

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "Chart", FlagState = DataService.Instance.DistanceTimeFlag });

            msgBoxobj.ShowNotification(saveMsg);
        }

        private int? _MaxHrtRate;
        public int? MaxHrtRate
        {
            get { return _MaxHrtRate; }
            set
            {
                if (value != _MaxHrtRate)
                {
                    _MaxHrtRate = value;
                    OnPropertyChanged("MaxHrtRate");
                }
            }
        }
    }
}
