﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CP77SaveEditor.Core.Common;
using CP77SaveEditor.Core.Savegame;
using CP77SaveEditor.Core.Savegame.Variables;
using CP77SaveEditor.Models;

namespace CP77SaveEditor.ViewModels
{
    public class SavegameViewModel : INotifyPropertyChanged
    {
        private SavegameModel _selectedSavegame;

        public ObservableCollection<SavegameModel> Savegames { get; set; }

        public SavegameModel SelectedSavegame
        {
            get
            {
                return _selectedSavegame;
            }
            set
            {
                if (_selectedSavegame != value)
                {
                    _selectedSavegame = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReadSavegameProgress Progress { get; set; }

        public ICommand InitializeSavegames { get; private set; }

        public ICommand OpenSavegame { get; private set; }

        public SavegameViewModel()
        {
            InitializeSavegames = new DelegateCommand(ExecuteInitializeSavegameList);
            OpenSavegame = new DelegateCommand(ExecuteOpenSavegame);
            Savegames = new ObservableCollection<SavegameModel>();
            Progress = new ReadSavegameProgress();
            ExecuteInitializeSavegameList(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ExecuteInitializeSavegameList(object obj)
        {
            Savegames.Clear();
            string gamesavesPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "Saved Games\\CD Projekt Red\\Cyberpunk 2077");
            var filesPaths = Directory.GetDirectories(gamesavesPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in filesPaths)
            {
                var thumbnailFilePath = Path.Combine(filePath, "screenshot.png");
                var model = new SavegameModel
                {
                    Name = Path.GetFileName(filePath),
                    Path = Path.Combine(filePath, "sav.dat"),
                    ThumbnailPath = thumbnailFilePath
                };
                Savegames.Add(model);
            }
        }

        private void ExecuteOpenSavegame(object parameter)
        {
            if (parameter == null)
            {
                return;
            }

            var savegame = parameter as SavegameModel;
            if (savegame == null)
            {
                throw new ArgumentException("parameter");
            }
            
            SavegameFile.ReadAsync(savegame.Path, savegame.Name, Progress)
                .ContinueWith(t =>
                {
                    var file = t.Result;
                    var savegameDataModel = new SavegameDataModel
                    {
                        Version1 = file.TypeCode1,
                        Version2 = file.TypeCode2,
                        Version3 = file.TypeCode3,
                        VariableNames = new ObservableCollection<string>(file.VariableNames),
                        Variables = new ObservableCollection<VariableModel>(file.Variables.Select(ToVariableModel))
                    };
                    SelectedSavegame = new SavegameModel
                    {
                        Name = savegame.Name,
                        Path = savegame.Path,
                        Data = savegameDataModel
                    };
                });
        }


        private static VariableModel ToVariableModel(Variable v, int i)
        {
            var set = v as VariableSet;
            var children = set == null
                ? new ObservableCollection<VariableModel>()
                : new ObservableCollection<VariableModel>(set.Variables.Select(ToVariableModel));

            var typed = v as TypedVariable;
            var type = typed == null
                ? "None"
                : typed.Type;

            var value = typed == null || typed.Value == null
                ? ""
                : typed.Value.ToString();

            return new VariableModel
            {
                Index = i,
                Name = v == null ? "" : v.Name,
                Type = type,
                Value = value,
                DebugString = v == null ? "" : v.ToString(),
                Children = children
            };
        }
    }
}
