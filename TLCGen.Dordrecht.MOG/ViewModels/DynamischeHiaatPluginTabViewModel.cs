using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using TLCGen.Dordrecht.DynamischeHiaat.Models;
using TLCGen.Extensions;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Models;

namespace TLCGen.Dordrecht.DynamischeHiaat.ViewModels
{
    internal class DynamischeHiaatPluginTabViewModel : ViewModelBase
    {
        #region Fields

        private DynamischeHiaatModel _model;
        private DynamischeHiaatPlugin _plugin;
        private ControllerModel _controller;
        private ObservableCollectionAroundList<DynamischeHiaatSignalGroupViewModel, DynamischeHiaatSignalGroupModel> _DynamischeHiaatSignalGroups;
        private DynamischeHiaatSignalGroupViewModel _selectedDynamischeHiaatSignalGroup;

        #endregion // Fields

        #region Properties

        public string TypeDynamischeHiaat
        {
            get => Model.TypeDynamischeHiaat;
            set
            {
                Model.TypeDynamischeHiaat = value;
                RaisePropertyChanged<object>(broadcast: true);
                foreach(var msg in DynamischeHiaatSignalGroups)
                {
                    msg.SelectedDefault = _plugin.MyDefaults.Defaults.FirstOrDefault(x => x.Name == TypeDynamischeHiaat);
                    if(string.IsNullOrEmpty(msg.Snelheid) || !msg.SelectedDefault.Snelheden.Any(x => x.Name == msg.Snelheid))
                    {
                        msg.Snelheid = msg.SelectedDefault.DefaultSnelheid;
                    }
                }
            }
        }

        public DynamischeHiaatSignalGroupViewModel SelectedDynamischeHiaatSignalGroup
        {
            get => _selectedDynamischeHiaatSignalGroup;
            set
            {
                _selectedDynamischeHiaatSignalGroup = value;
                if (value != null)
                {
                    var fc = _controller.Fasen.FirstOrDefault(x => x.Naam == _selectedDynamischeHiaatSignalGroup.SignalGroupName);
                    if(fc != null)
                    {
                        _selectedDynamischeHiaatSignalGroup.UpdateSelectableDetectoren(fc.Detectoren.Select(x => x.Naam));
                    }
                }
                RaisePropertyChanged();
            }
        }

        public List<DynamischeHiaatDefaultModel> Defaults => _plugin.MyDefaults.Defaults;

        public ObservableCollectionAroundList<DynamischeHiaatSignalGroupViewModel, DynamischeHiaatSignalGroupModel> DynamischeHiaatSignalGroups
        {
            get
            {
                if(_DynamischeHiaatSignalGroups == null)
                {
                     _DynamischeHiaatSignalGroups = new ObservableCollectionAroundList<DynamischeHiaatSignalGroupViewModel, DynamischeHiaatSignalGroupModel>(_model.SignaalGroepenMetDynamischeHiaat);
                    foreach(var msg in _DynamischeHiaatSignalGroups)
                    {
                        msg.SelectedDefault = _plugin.MyDefaults.Defaults.FirstOrDefault(x => x.Name == TypeDynamischeHiaat);
                    }
                }
                return _DynamischeHiaatSignalGroups;
            }
        }

        public ControllerModel Controller { get => _controller; set => _controller = value; }

        public DynamischeHiaatModel Model
        {
            get => _model;
            set
            {
                _model = value;
                if (value != null)
                {
                    _DynamischeHiaatSignalGroups = null;
                }
                RaisePropertyChanged("");
            }
        }

        #endregion // Properties

        #region Public Methods

        public void UpdateTLCGenMessaging()
        {
            MessengerInstance.Register(this, new Action<FasenChangedMessage>(OnFasenChanged));
            MessengerInstance.Register(this, new Action<DetectorenChangedMessage>(OnDetectorenChanged));
            MessengerInstance.Register(this, new Action<FaseDetectorTypeChangedMessage>(OnFaseDetectorTypeChanged));
            MessengerInstance.Register(this, new Action<NameChangedMessage>(OnNameChanged));
            MessengerInstance.Register(this, new Action<FasenSortedMessage>(OnFasenSorted));
        }

        private void OnFaseDetectorTypeChanged(FaseDetectorTypeChangedMessage obj)
        {
            var fc = DataAccess.TLCGenControllerDataProvider.Default.Controller.Fasen.FirstOrDefault(x => x.Detectoren.Any(x2 => x2.Naam == obj.DetectorDefine));
            if (fc != null)
            {
                var d = fc.Detectoren.First(x => x.Naam == obj.DetectorDefine);
                var mfc = DynamischeHiaatSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                if (mfc != null && mfc.HasDynamischeHiaat && 
                    (d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Kop ||
                     d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Lang ||
                     d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Verweg))
                {
                    if (!mfc.DynamischeHiaatDetectoren.Any(x => x.DetectorName == d.Naam))
                    {
                        mfc.DynamischeHiaatDetectoren.Add(new DynamischeHiaatDetectorViewModel(new DynamischeHiaatDetectorModel
                        {
                            DetectorName = d.Naam,
                            SignalGroupName = fc.Naam
                        }));
                    }
                    mfc.DynamischeHiaatDetectoren.BubbleSort();
                }
                else if (mfc != null && mfc.DynamischeHiaatDetectoren.Any(x => x.DetectorName == d.Naam))
                {
                    var r = mfc.DynamischeHiaatDetectoren.First(x => x.DetectorName == d.Naam);
                    mfc.DynamischeHiaatDetectoren.Remove(r);
                    mfc.DynamischeHiaatDetectoren.BubbleSort();
                }
                if (mfc != null) mfc.DynamischeHiaatDetectorenManager.UpdateSelectables(fc.Detectoren.Select(x => x.Naam));
            }
        }

        #endregion

        #region TLCGen Events

        private void OnDetectorenChanged(DetectorenChangedMessage message)
        {
            if (message.AddedDetectoren?.Count > 0)
            {
                foreach(var d in message.AddedDetectoren)
                {
                    var fc = DataAccess.TLCGenControllerDataProvider.Default.Controller.Fasen.FirstOrDefault(x => x.Detectoren.Any(x2 => x2.Naam == d.Naam));
                    if(fc != null)
                    {
                        var mfc = DynamischeHiaatSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                        if (mfc != null && mfc.HasDynamischeHiaat)
                        {
                            if (!mfc.DynamischeHiaatDetectoren.Any(x => x.DetectorName == d.Naam) &&
                                (d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Kop ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Lang ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Verweg))
                            {
                                mfc.DynamischeHiaatDetectoren.Add(new DynamischeHiaatDetectorViewModel(new DynamischeHiaatDetectorModel
                                {
                                    DetectorName = d.Naam,
                                    SignalGroupName = fc.Naam
                                }));
                            }
                            mfc.DynamischeHiaatDetectoren.BubbleSort();
                            mfc.DynamischeHiaatDetectorenManager.UpdateSelectables(fc.Detectoren.Select(x => x.Naam));
                        }
                    }
                }
            }
            if (message.RemovedDetectoren?.Count > 0)
            {
                foreach(var mfc in DynamischeHiaatSignalGroups)
                {
                    var rem = mfc.DynamischeHiaatDetectoren.Where(x => message.RemovedDetectoren.Any(x2 => x2.Naam == x.DetectorName));
                    foreach(var r in rem)
                    {
                        mfc.DynamischeHiaatDetectoren.Remove(r);
                        mfc.DynamischeHiaatDetectorenManager.SelectableItems.Remove(r.DetectorName);
                    }
                }
            }
        }

        private void OnFasenChanged(FasenChangedMessage message)
        {
            if (message.AddedFasen?.Count > 0)
            {
                foreach (var fc in message.AddedFasen.Where(x => x.Type == TLCGen.Models.Enumerations.FaseTypeEnum.Auto))
                {
                    var sn = _plugin.MyDefaults.Defaults.FirstOrDefault(x => x.Name == TypeDynamischeHiaat);
                    var msg = new DynamischeHiaatSignalGroupViewModel(new DynamischeHiaatSignalGroupModel
                    {
                        SignalGroupName = fc.Naam,
                        Snelheid = (sn == null ? "" : sn.DefaultSnelheid)
                    });
                    msg.SelectedDefault = _plugin.MyDefaults.Defaults.FirstOrDefault(x => x.Name == TypeDynamischeHiaat);
                    DynamischeHiaatSignalGroups.Add(msg);
                }
                DynamischeHiaatSignalGroups.BubbleSort();
            }
            if (message.RemovedFasen?.Count > 0)
            {
                var rems = new List<DynamischeHiaatSignalGroupViewModel>();
                foreach (var fc in message.RemovedFasen)
                {
                    var r = DynamischeHiaatSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                    if (r != null) rems.Add(r);
                }
                foreach (var r in rems)
                {
                    DynamischeHiaatSignalGroups.Remove(r);
                }
            }
        }

        private void OnNameChanged(NameChangedMessage message)
        {
            ModelManagement.TLCGenModelManager.Default.ChangeNameOnObject(_model, message.OldName, message.NewName);
            foreach (var mfc in DynamischeHiaatSignalGroups) mfc.DynamischeHiaatDetectoren.BubbleSort();
            RaisePropertyChanged("");
        }

        private void OnFasenSorted(FasenSortedMessage message)
        {
            DynamischeHiaatSignalGroups.BubbleSort();
        }

        #endregion // TLCGen Events

        #region Constructor

        public DynamischeHiaatPluginTabViewModel(DynamischeHiaatPlugin plugin)
        {
            _plugin = plugin;
        }

        #endregion // Constructor
    }
}
