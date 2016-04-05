using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using ReadFit.FileModel;

namespace ReadFit
{
    class HomeViewModel : ObservableObject, IPageViewModel
    {
        public string Name
        {
            get { return "Home"; }      //set the viewmodel name
        }

        public MsgBoxService msgBoxObj;

        public HomeViewModel()
        {
            msgBoxObj = new MsgBoxService();    //messagebox abstraction - use this instead of messagebox.show

            IsIdle = true;      //hides the circular progress bar

            Application.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);   //if you want to handle all close methods

            MessageBus.Instance.Subscribe<string>(handleMessage);
            MessageBus.Instance.Subscribe<MyFlag>(handleFlagMessage);
            MessageBus.Instance.Subscribe<ExtendedMsg>(handleExtendedMessage);
        }

        private void handleMessage(string msg)
        {
            msgBoxObj.ShowNotification(msg);    //handle simple string messages
        }

        private void handleExtendedMessage(ExtendedMsg msg)
        {
            msgBoxObj.ExtendedNotification(msg.EMsg, msg.ECaption, msg.Ebutton, msg.EImage);    //handle extended messages
        }

        private void handleFlagMessage(MyFlag flg)
        {
            switch (flg.FlagName)
            {
                case "IsIdle":
                    IsIdle = flg.FlagState;     //handle a subscribed bool flag message
                    break;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();     //if you use user settings
        }

        private ICommand exitCommand;

        public ICommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                {
                    exitCommand = new RelayCommand(param => exitProgam());
                }

                return exitCommand;
            }
        }

        public void exitProgam()
        {
            Application.Current.Shutdown();
        }

        private bool _IsIdle;
        public bool IsIdle
        {
            get { return _IsIdle; }
            set
            {
                if (value != _IsIdle)
                {
                    _IsIdle = value;
                    OnPropertyChanged("IsIdle");
                }
            }
        }
    }
}
