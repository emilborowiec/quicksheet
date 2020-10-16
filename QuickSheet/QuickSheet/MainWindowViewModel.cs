﻿#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using QuickSheet.Annotations;
using QuickSheet.CheatSheetPanel;
using QuickSheet.Model;
using QuickSheet.Notifications;
using QuickSheet.Services;

#endregion

namespace QuickSheet
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            _cheatSheetViewModel = new CheatSheetViewModel();
            SwitchSheetCommand = new DelegateCommand<string>(SwitchSheet);
            AdjustFontSizeCommand = new DelegateCommand<string>(AdjustFontSize);
            ExitCommand = new DelegateCommand(Exit);
            ReloadSheetsCommand = new DelegateCommand(ReloadCheatSheets);
            ToggleDarkModeCommand = new DelegateCommand(ToggleDarkMode);
            ReloadCheatSheets();
        }

        private List<CheatSheet> _cheatSheets;
        private int _currentIndex;
        private CheatSheetViewModel _cheatSheetViewModel;
        private List<Result<CheatSheet>> _errors;


        public DelegateCommand<string> SwitchSheetCommand { get; }
        public DelegateCommand ExitCommand { get; } 
        public DelegateCommand ReloadSheetsCommand { get; }
        public DelegateCommand<string> AdjustFontSizeCommand { get; }
        public DelegateCommand ToggleDarkModeCommand { get; }

        public CheatSheet CurrentCheatSheet => CurrentIndex == -1 ? null : _cheatSheets[CurrentIndex];

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value == _currentIndex) return;
                _currentIndex = value;
                _cheatSheetViewModel.CheatSheet = CurrentCheatSheet;
            }
        }

        public CheatSheetViewModel CheatSheetViewModel
        {
            get => _cheatSheetViewModel;
            set
            {
                _cheatSheetViewModel = value;
                OnPropertyChanged(nameof(CheatSheetViewModel));
            }
        }

        public List<Result<CheatSheet>> Errors
        {
            get => _errors;
            set
            {
                _errors = value;
                OnPropertyChanged(nameof(Errors));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ToggleDarkMode()
        {
            CheatSheetViewModel.DarkMode = !CheatSheetViewModel.DarkMode;
        }

        private void ReloadCheatSheets()
        {
            _cheatSheets = new List<CheatSheet>();
            var results = CheatSheetLoader.LoadCheatSheets();
            var cheatSheets = results.Where(r => r.IsSuccess).Select(r => r.Data);
            foreach (var sheet in cheatSheets)
            {
                _cheatSheets.Add(sheet);
            }

            CurrentIndex = _cheatSheets.Count > 0 ? 0 : -1;
            CheatSheetViewModel.CheatSheet = CurrentCheatSheet;

            Errors = results.Where(r => r.IsSuccess != true).ToList();
            if (Errors.Count > 0)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke((Action)(async () =>
                {
                    await Task.Delay(1000);
                    OpenDialog("Some Quick Sheets failed to load", string.Join('\n', Errors.Select(e => e.Source + " - " + e.Message)));
                }));
            }
        }

        private void SwitchSheet(string directionString)
        {
            if (CurrentIndex == -1) return;
            if ("left".Equals(directionString))
            {
                if (CurrentIndex > 0)
                {
                    CurrentIndex -= 1;
                }
                else
                {
                    CurrentIndex = _cheatSheets.Count - 1;
                }
            }
            else
            {
                if (CurrentIndex < _cheatSheets.Count - 1)
                {
                    CurrentIndex += 1;
                }
                else
                {
                    CurrentIndex = 0;
                }
            }

            CheatSheetViewModel.CheatSheet = CurrentCheatSheet;
        }

        private void AdjustFontSize(string param)
        {
            var amount = "up".Equals(param) ? 2 : -2;
            CheatSheetViewModel.BaseFontSize += amount;
        }

        private void OpenDialog(string title, string message)
        {
            DialogService.OpenDialog(new DialogViewModel {Title = title, Message = message});
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}