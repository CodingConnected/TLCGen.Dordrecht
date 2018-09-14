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
            MessengerInstance.Register(this, new Action<NameChangedMessage>(OnNameChanged));
            MessengerInstance.Register(this, new Action<FasenSortedMessage>(OnFasenSorted));
        }

        #endregion

        #region TLCGen Events

        private void OnDetectorenChanged(DetectorenChangedMessage message)
        {
            if (message.AddedDetectoren?.Count > 0)
            {
                if (message.AddedDetectoren != null)
                {
                    //foreach (var fc in message.AddedDetectoren.Where(x => x.Type == TLCGen.Models.Enumerations.FaseTypeEnum.Auto))
                    //{
                    //    //SelectableFasen.Add(fc.Naam);
                    //}
                    ////SelectableFasen.BubbleSort();
                }
            }
            if (message.RemovedDetectoren?.Count > 0)
            {
                var rems = new List<MOGSignalGroupViewModel>();
                //foreach (var fc in message.RemovedDetectoren)
                //{
                //    var r = MOGSignalGroups.FirstOrDefault(x => x.SignalGroupName == fc.Naam);
                //    if (r != null) rems.Add(r);
                //    //SelectableFasen.Remove(fc.Naam);
                //}
                //foreach (var r in rems)
                //{
                //    MOGSignalGroups.Remove(r);
                //}
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
            RaisePropertyChanged("");
        }

        private void OnFasenSorted(FasenSortedMessage message)
        {
            MOGSignalGroups.BubbleSort();
        }

        #endregion // TLCGen Events

    }
}
