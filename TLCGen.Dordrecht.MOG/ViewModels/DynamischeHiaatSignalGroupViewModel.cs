using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TLCGen.Dordrecht.DynamischeHiaat.Models;
using TLCGen.Extensions;
using TLCGen.Helpers;
using RelayCommand = GalaSoft.MvvmLight.CommandWpf.RelayCommand;

namespace TLCGen.Dordrecht.DynamischeHiaat.ViewModels
{
    internal class DynamischeHiaatSignalGroupViewModel : ViewModelBase, IViewModelWithItem, IComparable
    {
        #region Fields

        private ObservableCollectionAroundList<DynamischeHiaatDetectorViewModel, DynamischeHiaatDetectorModel> _DynamischeHiaatDetectoren;

        #endregion // Fields

        #region Properties

        [Browsable(false)]
        public DynamischeHiaatSignalGroupModel SignalGroup { get; }

        [Browsable(false)]
        public DynamischeHiaatDefaultModel SelectedDefault
        {
            get => _selectedDefault;
            set
            {
                _selectedDefault = value;
                RaisePropertyChanged();
                if(value != null)
                {
                    var v = Snelheid;
                    SelectedDefaultSnelheden = value.Snelheden;
                    RaisePropertyChanged(nameof(SelectedDefaultSnelheden));
                    Snelheid = v;
                }
            }
        }

        public List<DynamischeHiaatSpeedDefaultModel> SelectedDefaultSnelheden { get; private set; }

        [Browsable(false)]
        public ObservableCollectionAroundList<DynamischeHiaatDetectorViewModel, DynamischeHiaatDetectorModel> DynamischeHiaatDetectoren => _DynamischeHiaatDetectoren ?? (_DynamischeHiaatDetectoren = new ObservableCollectionAroundList<DynamischeHiaatDetectorViewModel, DynamischeHiaatDetectorModel>(SignalGroup.DynamischeHiaatDetectoren));

        private AddRemoveItemsManager<DynamischeHiaatDetectorViewModel, DynamischeHiaatDetectorModel, string> _DynamischeHiaatDetectorenManager;
        private DynamischeHiaatDefaultModel _selectedDefault;

        [Browsable(false)]
        public AddRemoveItemsManager<DynamischeHiaatDetectorViewModel, DynamischeHiaatDetectorModel, string> DynamischeHiaatDetectorenManager =>
            _DynamischeHiaatDetectorenManager ?? (_DynamischeHiaatDetectorenManager = new AddRemoveItemsManager<DynamischeHiaatDetectorViewModel, DynamischeHiaatDetectorModel, string>(
            DynamischeHiaatDetectoren,
            x => new DynamischeHiaatDetectorViewModel(new DynamischeHiaatDetectorModel { DetectorName = x, SignalGroupName = SignalGroup.SignalGroupName }),
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
        public bool HasDynamischeHiaat
        {
            get => SignalGroup.HasDynamischeHiaat;
            set
            {
                SignalGroup.HasDynamischeHiaat = value;
                if (value)
                {
                    var fc = DataAccess.TLCGenControllerDataProvider.Default.Controller.Fasen.FirstOrDefault(x => x.Naam == SignalGroupName);
                    if (fc != null)
                    {
                        foreach (var d in fc.Detectoren)
                        {
                            if (!DynamischeHiaatDetectoren.Any(x => x.DetectorName == d.Naam) &&
                                (d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Kop ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Lang ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Verweg))
                            {
                                DynamischeHiaatDetectoren.Add(new DynamischeHiaatDetectorViewModel(new DynamischeHiaatDetectorModel
                                {
                                    DetectorName = d.Naam,
                                    SignalGroupName = SignalGroupName
                                }));
                            }
                        }
                        DynamischeHiaatDetectoren.BubbleSort();
                        ApplySnelheidsDefaultsToDetectoren(Snelheid);
                    }
                }
                else
                {
                    DynamischeHiaatDetectoren.RemoveAll();
                }
                RaisePropertyChanged();
            }
        }

        public string Snelheid
        {
            get => SignalGroup.Snelheid;
            set
            {
                SignalGroup.Snelheid = value;
                ApplySnelheidsDefaultsToDetectoren(value);
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
            return SignalGroup.SignalGroupName.CompareTo(((DynamischeHiaatSignalGroupViewModel)obj).SignalGroup.SignalGroupName);
        }

        #endregion // IComparable

        #region Public Methods

        public void UpdateSelectableDetectoren(IEnumerable<string> detectoren)
        {
            DynamischeHiaatDetectorenManager.UpdateSelectables(detectoren);
        }

        #endregion // Public Methods

        #region Private Methods

        private void ApplySnelheidsDefaultsToDetectoren(string snelheid)
        {
            if (snelheid != null)
            {
                var dr = new int[10];
                var sd = SelectedDefault.Snelheden.FirstOrDefault(x => x.Name == snelheid);
                if (sd != null)
                {
                    for (int d = 0; d < DynamischeHiaatDetectoren.Count; d++)
                    {
                        var od = DataAccess.TLCGenControllerDataProvider.Default.Controller.Fasen.SelectMany(x => x.Detectoren).FirstOrDefault(x => x.Naam == DynamischeHiaatDetectoren[d].DetectorName);
                        if(od != null && od.Rijstrook.HasValue && od.Rijstrook > 0 && od.Rijstrook <= 10)
                        {
                            dr[od.Rijstrook.Value - 1]++;
                        }
                        else
                        {
                            continue;
                        }
                        if (dr[od.Rijstrook.Value - 1] > 0 && (dr[od.Rijstrook.Value - 1] - 1) < sd.Detectoren.Count)
                        {
                            DynamischeHiaatDetectoren[d].Moment1 = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].Moment1;
                            DynamischeHiaatDetectoren[d].Moment2 = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].Moment2;
                            DynamischeHiaatDetectoren[d].TDH1 = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].TDH1;
                            DynamischeHiaatDetectoren[d].TDH2 = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].TDH2;
                            DynamischeHiaatDetectoren[d].Maxtijd = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].Maxtijd;
                            DynamischeHiaatDetectoren[d].Spring = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].Spring;
                            DynamischeHiaatDetectoren[d].VerlengNiet = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].VerlengNiet;
                            DynamischeHiaatDetectoren[d].VerlengWel = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].VerlengWel;
                            DynamischeHiaatDetectoren[d].Vag4Mvt1 = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].Vag4Mvt1;
                            DynamischeHiaatDetectoren[d].Vag4Mvt2 = sd.Detectoren[dr[od.Rijstrook.Value - 1] - 1].Vag4Mvt2;
                        }
                    }
                }
            }
        }

        #endregion

        #region Constructor

        public DynamischeHiaatSignalGroupViewModel(DynamischeHiaatSignalGroupModel signalGroup)
        {
            SignalGroup = signalGroup;
        }

        #endregion // Constructor
    }
}
