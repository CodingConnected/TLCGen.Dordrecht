using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TLCGen.Dordrecht.MOG.Models;
using TLCGen.Helpers;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;

namespace TLCGen.Dordrecht.MOG.ViewModels
{
    internal class MOGSignalGroupViewModel : ViewModelBase, IViewModelWithItem, IComparable
    {
        #region Fields

        private List<string> _allFaseDetectoren = new List<string>();
        private ObservableCollection<string> _selectableDetectoren;
        private ObservableCollectionAroundList<MOGDetectorViewModel, MOGDetectorModel> _MOGDetectoren;
        private MOGDetectorViewModel _selectedMOGDetector;
        private string _selectedMOGDetectorToAdd;

        #endregion // Fields

        #region Properties

        [Browsable(false)]
        public MOGSignalGroupModel SignalGroup { get; }

        [Browsable(false)]
        public ObservableCollection<string> SelectableDetectoren => _selectableDetectoren ?? (_selectableDetectoren = new ObservableCollection<string>());

        [Browsable(false)]
        public MOGDetectorViewModel SelectedMOGDetector
        {
            get => _selectedMOGDetector;
            set
            {
                _selectedMOGDetector = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public string SelectedMOGDetectorToAdd
        {
            get => _selectedMOGDetectorToAdd;
            set
            {
                _selectedMOGDetectorToAdd = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public ObservableCollectionAroundList<MOGDetectorViewModel, MOGDetectorModel> MOGDetectoren => _MOGDetectoren ?? (_MOGDetectoren = new ObservableCollectionAroundList<MOGDetectorViewModel, MOGDetectorModel>(SignalGroup.MOGDetectoren));

        [Browsable(false)]
        public string SignalGroupName
        {
            get => SignalGroup.SignalGroupName;
            set
            {
                SignalGroup.SignalGroupName = value;
                RaisePropertyChanged();
            }
        }

        [Browsable(false)]
        public bool HasMOG
        {
            get => SignalGroup.HasMOG;
            set
            {
                SignalGroup.HasMOG = value;
                RaisePropertyChanged();
            }
        }

        public int Instelling1
        {
            get => SignalGroup.Instelling1;
            set
            {
                SignalGroup.Instelling1 = value;
                RaisePropertyChanged();
            }
        }

        #endregion // Properties

        #region Commands

        private RelayCommand _addMOGDetectorCommand;
        public ICommand AddMOGDetectorCommand => _addMOGDetectorCommand ?? (_addMOGDetectorCommand = new RelayCommand(AddMOGDetectorCommand_executed, AddMOGDetectorCommand_canExecute));

        private bool AddMOGDetectorCommand_canExecute()
        {
            return !string.IsNullOrWhiteSpace(SelectedMOGDetectorToAdd);
        }

        private void AddMOGDetectorCommand_executed()
        {
            var nd = new MOGDetectorViewModel(new MOGDetectorModel { DetectorName = _selectedMOGDetectorToAdd });
            MOGDetectoren.Add(nd);
            SelectedMOGDetector = nd;
            UpdateSelectableDetectoren(null);
        }

        private RelayCommand _removeMOGDetectorCommand;
        public ICommand RemoveMOGDetectorCommand => _removeMOGDetectorCommand ?? (_removeMOGDetectorCommand = new RelayCommand(RemoveMOGDetectorCommand_executed, RemoveMOGDetectorCommand_canExecute));

        private bool RemoveMOGDetectorCommand_canExecute()
        {
            return SelectedMOGDetector != null;
        }

        private void RemoveMOGDetectorCommand_executed()
        {
            var id = MOGDetectoren.IndexOf(SelectedMOGDetector);
            MOGDetectoren.Remove(SelectedMOGDetector);
            if(id < MOGDetectoren.Count)
            {
                SelectedMOGDetector = MOGDetectoren[id];
            }
            else if(MOGDetectoren.Any())
            {
                SelectedMOGDetector = MOGDetectoren.Last();
            }
            else
            {
                SelectedMOGDetector = null;
            }
            UpdateSelectableDetectoren(null);
        }
        #endregion // Commands

        #region IViewModelWithItem

        public object GetItem()
        {
            return SignalGroup;
        }

        #endregion // IViewModelWithItem

        #region IComparable

        public int CompareTo(object obj)
        {
            return SignalGroup.SignalGroupName.CompareTo(((MOGSignalGroupViewModel)obj).SignalGroup.SignalGroupName);
        }

        #endregion // IComparable

        #region Public Methods

        public void UpdateSelectableDetectoren(IEnumerable<string> detectoren)
        {
            if (detectoren != null)
            {
                _allFaseDetectoren.Clear();
                foreach(var d in detectoren)
                {
                    _allFaseDetectoren.Add(d);
                }
            }
            var sd = SelectedMOGDetectorToAdd;
            SelectableDetectoren.Clear();
            foreach (var d in _allFaseDetectoren.Where(x => MOGDetectoren.All(x2 => x2.Detector.DetectorName != x)))
            {
                SelectableDetectoren.Add(d);
            }
            if (SelectableDetectoren.Contains(sd))
            {
                SelectedMOGDetectorToAdd = sd;
            }
            else if(SelectableDetectoren.Any())
            {
                SelectedMOGDetectorToAdd = SelectableDetectoren[0];
            }
        }

        #endregion // Public Methods

        #region Constructor

        public MOGSignalGroupViewModel(MOGSignalGroupModel signalGroup)
        {
            SignalGroup = signalGroup;
        }

        #endregion // Constructor
    }
}
