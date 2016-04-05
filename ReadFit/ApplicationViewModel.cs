using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ReadFit.FileModel;

namespace ReadFit
{
    class ApplicationViewModel : ObservableObject
    {
        private ICommand _ChangePageCommand;

        private IPageViewModel _CurrentPageViewModel;
        private List<IPageViewModel> _PageViewModels;

        public MsgBoxService msgBoxObj;
        public bool myFlagUId = false;

        public ApplicationViewModel()
        {
            // Add available pages

            PageViewModels.Add(new AboutViewModel());   //add all view models here
            PageViewModels.Add(new ListDataVeiwModel());
            PageViewModels.Add(new ReadFileViewModel());
            PageViewModels.Add(new WriteKmlViewModel());

            // Set starting page

            HomeVm = new HomeViewModel();   //the navigation panel

            CurrentPageViewModel = PageViewModels[0];   //what ever viewmodel you want to start with

            msgBoxObj = new MsgBoxService();

            MessageBus.Instance.Subscribe<MyViewName>(handleViewChange);    //listen for view change messages

            bool testFlag = false;

            if (DataService.Instance.activityData != null)
            {
                if (DataService.Instance.activityData.Count() > 0)
                {
                    testFlag = true;

                    MessageBus.Instance.Publish<ID>(new ID 
                    {
                        UserId = DataService.Instance.activityData[0].timeStamp,
                        Sport = DataService.Instance.activityData[0].sport
                    });

                    //UserId = DataService.Instance.activityData[0].timeStamp;
                    //UserSport = DataService.Instance.activityData[0].sport;
                }
            }

            MessageBus.Instance.Publish<MyFlag>(new MyFlag { FlagName = "IsReady", FlagState = testFlag });
        }

        public void handleViewChange(MyViewName msg)
        {
            if (PageViewModels.Exists(x => x.Name == msg.Name))
            {
                CurrentPageViewModel = PageViewModels.FirstOrDefault(vmn => vmn.Name == msg.Name);  //change the view
            }
            else
            {
                msgBoxObj.ShowNotification(msg.Name + " error in changeview");
            }
        }

        public ICommand ChangePageCommand
        {
            get
            {
                if (_ChangePageCommand == null)
                {
                    _ChangePageCommand = new RelayCommand(
                        p => ChangeViewModel((IPageViewModel)p),
                        p => p is IPageViewModel);
                }

                return _ChangePageCommand;
            }
        }

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_PageViewModels == null)
                    _PageViewModels = new List<IPageViewModel>();

                return _PageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get { return _CurrentPageViewModel; }
            set
            {
                if (_CurrentPageViewModel != value)
                {
                    _CurrentPageViewModel = value;
                    OnPropertyChanged("CurrentPageViewModel");
                }
            }
        }

        private IPageViewModel _HomeVm;
        public IPageViewModel HomeVm
        {
            get { return _HomeVm; }
            set
            {
                if (_HomeVm != value)
                {
                    _HomeVm = value;
                    OnPropertyChanged("HomeVm");
                }
            }
        }

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (CurrentPageViewModel != null)
            {
                if (CurrentPageViewModel.Name == viewModel.Name)   //don't switch if you are already there
                {
                    return;
                }
            }

            if (!PageViewModels.Contains(viewModel))
            {
                PageViewModels.Add(viewModel);
            }

            CurrentPageViewModel = PageViewModels.FirstOrDefault(vml => vml == viewModel);
        }
    }
}
