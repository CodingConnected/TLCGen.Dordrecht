using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using TLCGen.Dordrecht.MOG.Models;
using TLCGen.Extensions;
using TLCGen.Helpers;
using TLCGen.Messaging.Messages;
using TLCGen.Models;

namespace TLCGen.Dordrecht.MOG.ViewModels
{
    internal class MOGPluginTabViewModel : ViewModelBase
    {
        #region Fields

        private MOGModel _model;
        private ControllerModel _controller;
        private ObservableCollectionAroundList<MOGSignalGroupViewModel, MOGSignalGroupModel> _MOGSignalGroups;
        private MOGSignalGroupViewModel _selectedMOGSignalGroup;

        #endregion // Fields

        #region Properties

        public MOGSignalGroupViewModel SelectedMOGSignalGroup
        {
            get => _selectedMOGSignalGroup;
            set
            {
                _selectedMOGSignalGroup = value;
                if (value != null)
                {
                    var fc = _controller.Fasen.FirstOrDefault(x => x.Naam == _selectedMOGSignalGroup.SignalGroupName);
                    if(fc != null)
                    {
                        _selectedMOGSignalGroup.UpdateSelectableDetectoren(fc.Detectoren.Select(x => x.Naam));
                    }
                }
                RaisePropertyChanged();
            }
        }

        public ObservableCollectionAroundList<MOGSignalGroupViewModel, MOGSignalGroupModel> MOGSignalGroups =>
            _MOGSignalGroups ?? (_MOGSignalGroups = new ObservableCollectionAroundList<MOGSignalGroupViewModel, MOGSignalGroupModel>(_model.SignaalGroepenMetMOG));

        public ControllerModel Controller { get => _controller; set => _controller = value; }

        public MOGModel Model
        {
            get => _model;
            set
            {
                _model = value;
                if (value != null)
                {
                    _MOGSignalGroups = null;
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
                var mfc = MOGSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                if (mfc != null && 
                    (d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Kop ||
                     d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Lang ||
                     d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Verweg))
                {
                    if (!mfc.MOGDetectoren.Any(x => x.DetectorName == d.Naam))
                    {
                        mfc.MOGDetectoren.Add(new MOGDetectorViewModel(new MOGDetectorModel
                        {
                            DetectorName = d.Naam,
                            SignalGroupName = fc.Naam
                        }));
                    }
                    mfc.MOGDetectoren.BubbleSort();
                }
                else if (mfc != null && mfc.MOGDetectoren.Any(x => x.DetectorName == d.Naam))
                {
                    var r = mfc.MOGDetectoren.First(x => x.DetectorName == d.Naam);
                    mfc.MOGDetectoren.Remove(r);
                    mfc.MOGDetectoren.BubbleSort();
                }
                if (mfc != null) mfc.MOGDetectorenManager.UpdateSelectables(fc.Detectoren.Select(x => x.Naam));
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
                        var mfc = MOGSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                        if (mfc != null)
                        {
                            if (!mfc.MOGDetectoren.Any(x => x.DetectorName == d.Naam) &&
                                (d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Kop ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Lang ||
                                 d.Type == TLCGen.Models.Enumerations.DetectorTypeEnum.Verweg))
                            {
                                mfc.MOGDetectoren.Add(new MOGDetectorViewModel(new MOGDetectorModel
                                {
                                    DetectorName = d.Naam,
                                    SignalGroupName = fc.Naam
                                }));
                            }
                            mfc.MOGDetectoren.BubbleSort();
                            mfc.MOGDetectorenManager.UpdateSelectables(fc.Detectoren.Select(x => x.Naam));
                        }
                    }
                }
            }
            if (message.RemovedDetectoren?.Count > 0)
            {
                foreach(var mfc in MOGSignalGroups)
                {
                    var rem = mfc.MOGDetectoren.Where(x => message.RemovedDetectoren.Any(x2 => x2.Naam == x.DetectorName));
                    foreach(var r in rem)
                    {
                        mfc.MOGDetectoren.Remove(r);
                        mfc.MOGDetectorenManager.SelectableItems.Remove(r.DetectorName);
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
                    MOGSignalGroups.Add(new MOGSignalGroupViewModel(new MOGSignalGroupModel { SignalGroupName = fc.Naam }));
                }
                MOGSignalGroups.BubbleSort();
            }
            if (message.RemovedFasen?.Count > 0)
            {
                var rems = new List<MOGSignalGroupViewModel>();
                foreach (var fc in message.RemovedFasen)
                {
                    var r = MOGSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                    if (r != null) rems.Add(r);
                }
                foreach (var r in rems)
                {
                    MOGSignalGroups.Remove(r);
                }
            }
        }

        private void OnNameChanged(NameChangedMessage message)
        {
            ModelManagement.TLCGenModelManager.Default.ChangeNameOnObject(_model, message.OldName, message.NewName);
            foreach (var mfc in MOGSignalGroups) mfc.MOGDetectoren.BubbleSort();
            RaisePropertyChanged("");
        }

        private void OnFasenSorted(FasenSortedMessage message)
        {
            MOGSignalGroups.BubbleSort();
        }

        #endregion // TLCGen Events

    }
}
