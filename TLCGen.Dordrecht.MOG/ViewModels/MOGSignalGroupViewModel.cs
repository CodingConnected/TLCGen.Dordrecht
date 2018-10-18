using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TLCGen.Dordrecht.MOG.Models;
using TLCGen.Extensions;
using TLCGen.Helpers;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;

namespace TLCGen.Dordrecht.MOG.ViewModels
{
    internal class MOGSignalGroupViewModel : ViewModelBase, IViewModelWithItem, IComparable
    {
        #region Fields

        private ObservableCollectionAroundList<MOGDetectorViewModel, MOGDetectorModel> _MOGDetectoren;
        
        #endregion // Fields

        #region Properties

        [Browsable(false)]
        public MOGSignalGroupModel SignalGroup { get; }

        [Browsable(false)]
        public ObservableCollectionAroundList<MOGDetectorViewModel, MOGDetectorModel> MOGDetectoren => _MOGDetectoren ?? (_MOGDetectoren = new ObservableCollectionAroundList<MOGDetectorViewModel, MOGDetectorModel>(SignalGroup.MOGDetectoren));

        private AddRemoveItemsManager<MOGDetectorViewModel, MOGDetectorModel, string> _MOGDetectorenManager;
        [Browsable(false)]
        public AddRemoveItemsManager<MOGDetectorViewModel, MOGDetectorModel, string> MOGDetectorenManager =>
            _MOGDetectorenManager ?? (_MOGDetectorenManager = new AddRemoveItemsManager<MOGDetectorViewModel, MOGDetectorModel, string>(
                MOGDetectoren,
                x => new MOGDetectorViewModel(new MOGDetectorModel { DetectorName = x, SignalGroupName = SignalGroup.SignalGroupName }),
                (x, y) => x.Detector.DetectorName == y
                ));

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
                if (value)
                {
                    var fc = DataAccess.TLCGenControllerDataProvider.Default.Controller.Fasen.FirstOrDefault(x => x.Naam == SignalGroupName);
                    if (fc != null)
                    {
                        foreach (var d in fc.Detectoren)
                        {
                            if (!MOGDetectoren.Any(x => x.DetectorName == d.Naam) &&
                                (d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Kop ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Lang ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Verweg))
                            {
                                MOGDetectoren.Add(new MOGDetectorViewModel(new MOGDetectorModel
                                {
                                    DetectorName = d.Naam,
                                    SignalGroupName = SignalGroupName
                                }));
                            }
                        }
                        MOGDetectoren.BubbleSort();
                    }
                }
                RaisePropertyChanged();
            }
        }

        public IVERSnelheidEnum Snelheid
        {
            get => SignalGroup.Snelheid;
            set
            {
                SignalGroup.Snelheid = value;
                RaisePropertyChanged();
            }
        }

        [Description("Hiaatmeting vanaf ED koplus")]
        public bool KijkenNaarLus
        {
            get => SignalGroup.KijkenNaarKoplus;
            set
            {
                SignalGroup.KijkenNaarKoplus = value;
                RaisePropertyChanged();
            }
        }

        #endregion // Properties

        #region Commands

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
            MOGDetectorenManager.UpdateSelectables(detectoren);
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
