﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TextReplace.Messages;

namespace TextReplace.MVVM.ViewModel
{
    partial class SideBarViewModel : ObservableRecipient
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(HomeViewCommand))]
        [NotifyCanExecuteChangedFor(nameof(ReplaceViewCommand))]
        private object _selectedView;

        public HomeViewModel HomeVm = new HomeViewModel();
        public ReplaceViewModel ReplaceVm = new ReplaceViewModel();

        public SideBarViewModel()
        {
            _selectedView = HomeVm;
        }

        [RelayCommand]
        private void HomeView()
        {
            SelectedView = HomeVm;
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(SelectedView));
        }

        [RelayCommand]
        private void ReplaceView()
        {
            SelectedView = ReplaceVm;
            WeakReferenceMessenger.Default.Send(new ActiveContentViewMsg(SelectedView));
        }
    }
}
