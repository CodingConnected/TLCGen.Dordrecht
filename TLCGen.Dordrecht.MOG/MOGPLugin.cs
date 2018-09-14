using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml;
using TLCGen.Dordrecht.MOG.Models;
using TLCGen.Dordrecht.MOG.ViewModels;
using TLCGen.Dordrecht.MOG.Views;
using TLCGen.Extensions;
using TLCGen.Helpers;
using TLCGen.Models;
using TLCGen.Plugins;

namespace TLCGen.Dordrecht.MOG
{
    [TLCGenTabItem(-1, TabItemTypeEnum.MainWindow)]
    [TLCGenPlugin(TLCGenPluginElems.PlugMessaging | TLCGenPluginElems.TabControl | TLCGenPluginElems.XMLNodeWriter)]
    public class AFMPlugin : ITLCGenPlugMessaging, ITLCGenTabItem, ITLCGenXMLNodeWriter
    {
        #region Fields

        private bool _isEnabled;
        private ControllerModel _controller;
        private const string _myName = "MOG";
        private MOGModel _myModel;
        private MOGPluginTabViewModel _myTabViewModel;

        #endregion Fields

        #region ITLCGen shared items

        public ControllerModel Controller
        {
            get => _controller;
            set
            {
                _controller = value;
                if (_controller == null)
                {
                    _myModel = new MOGModel();
                    _myTabViewModel.Controller = null;
                    _myTabViewModel.Model = _myModel;
                }
                if (_controller != null && _myModel != null)
                {
                    _myTabViewModel.Controller = _controller;
                }
                UpdateModel();
            }
        }

        public string GetPluginName()
        {
            return _myName;
        }

        #endregion // ITLCGen shared items

        #region ITLCGenPlugMessaging

        public void UpdateTLCGenMessaging()
        {
            _myTabViewModel.UpdateTLCGenMessaging();
        }

        #endregion // ITLCGenPlugMessaging

        #region ITLCGenTabItem

        public string DisplayName => _myName;

        public System.Windows.Media.ImageSource Icon => null;

        DataTemplate _ContentDataTemplate;
        public DataTemplate ContentDataTemplate
        {
            get
            {
                if (_ContentDataTemplate == null)
                {
                    _ContentDataTemplate = new DataTemplate();
                    var tab = new FrameworkElementFactory(typeof(MOGPluginTabView));
                    tab.SetValue(FrameworkElement.DataContextProperty, _myTabViewModel);
                    _ContentDataTemplate.VisualTree = tab;
                }
                return _ContentDataTemplate;
            }
        }

        public bool IsEnabled { get => _isEnabled; set => _isEnabled = value; }

        public bool CanBeEnabled()
        {
            return true;
        }

        public void LoadTabs()
        {
            
        }

        public void OnDeselected()
        {
            
        }

        public bool OnDeselectedPreview()
        {
            return true;
        }

        public void OnSelected()
        {
            if(_myTabViewModel.SelectedMOGSignalGroup == null && _myTabViewModel.MOGSignalGroups.Any())
            {
                _myTabViewModel.SelectedMOGSignalGroup = _myTabViewModel.MOGSignalGroups[0];
            }
        }

        public bool OnSelectedPreview()
        {
            return true;
        }

        #endregion ITLCGenTabItem

        #region ITLCGenXMLNodeWriter

        public void GetXmlFromDocument(XmlDocument document)
        {
            _myModel = null;

            foreach (XmlNode node in document.FirstChild.ChildNodes)
            {
                if (node.LocalName == "MOG")
                {
                    _myModel = XmlNodeConverter.ConvertNode<MOGModel>(node);
                    break;
                }
            }

            if (_myModel == null)
            {
                _myModel = new MOGModel();
            }
            _myTabViewModel.Model = _myModel;
            _myTabViewModel.RaisePropertyChanged("");
        }

        public void SetXmlInDocument(XmlDocument document)
        {
            XmlDocument doc = TLCGenSerialization.SerializeToXmlDocument(_myModel);
            XmlNode node = document.ImportNode(doc.DocumentElement, true);
            document.DocumentElement.AppendChild(node);
        }

        #endregion // ITLCGenXMLNodeWriter

        #region Private Methods

        internal void UpdateModel()
        {
            if (_controller != null && _myModel != null)
            {
                foreach (var fc in Controller.Fasen)
                {
                    if (_myTabViewModel.MOGSignalGroups.All(x => x.SignalGroupName != fc.Naam))
                    {
                        _myTabViewModel.MOGSignalGroups.Add(
                            new MOGSignalGroupViewModel(
                                new MOGSignalGroupModel { SignalGroupName = fc.Naam }));
                    }
                }
                var rems = new List<MOGSignalGroupViewModel>();
                foreach (var fc in _myTabViewModel.MOGSignalGroups)
                {
                    if (Controller.Fasen.All(x => x.Naam != fc.SignalGroupName))
                    {
                        rems.Add(fc);
                    }
                }
                foreach (var sg in rems)
                {
                    _myTabViewModel.MOGSignalGroups.Remove(sg);
                }
                _myTabViewModel.MOGSignalGroups.BubbleSort();
                _myTabViewModel.RaisePropertyChanged("");
            }
        }

        #endregion // Private Methods

        #region Constructor

        public AFMPlugin()
        {
            _myTabViewModel = new MOGPluginTabViewModel();
        }

        #endregion //Constructor
    }
}
