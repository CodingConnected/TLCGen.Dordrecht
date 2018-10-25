using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using TLCGen.Dordrecht.DynamischeHiaat.Models;
using TLCGen.Dordrecht.DynamischeHiaat.ViewModels;
using TLCGen.Dordrecht.DynamischeHiaat.Views;
using TLCGen.Extensions;
using TLCGen.Generators.CCOL.CodeGeneration;
using TLCGen.Helpers;
using TLCGen.Models;
using TLCGen.Plugins;

namespace TLCGen.Dordrecht.DynamischeHiaat
{
    [TLCGenTabItem(-1, TabItemTypeEnum.DetectieTab)]
    [TLCGenPlugin(TLCGenPluginElems.PlugMessaging | TLCGenPluginElems.TabControl | TLCGenPluginElems.XMLNodeWriter | TLCGenPluginElems.HasSettings)]
    [CCOLCodePieceGenerator]
    public class DynamischeHiaatPlugin : CCOLCodePieceGeneratorBase, ITLCGenPlugMessaging, ITLCGenTabItem, ITLCGenXMLNodeWriter, ITLCGenHasSettings
    {
        #region Fields

        private ControllerModel _controller;
        private const string _myName = "Dynamische hiaat";
        private DynamischeHiaatModel _myModel;
        private DynamischeHiaatPluginTabViewModel _myTabViewModel;

        #endregion Fields

        #region Properties

        public DynamischeHiaatDefaultsModel MyDefaults { get; private set; }
        #endregion // Properties

        #region ITLCGen shared items

        public ControllerModel Controller
        {
            get => _controller;
            set
            {
                _controller = value;
                if (_controller == null)
                {
                    _myModel = new DynamischeHiaatModel();
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
                    var tab = new FrameworkElementFactory(typeof(DynamischeHiaatPluginTabView));
                    tab.SetValue(FrameworkElement.DataContextProperty, _myTabViewModel);
                    _ContentDataTemplate.VisualTree = tab;
                }
                return _ContentDataTemplate;
            }
        }

        public bool IsEnabled { get; set; }

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
            if(_myTabViewModel.SelectedDynamischeHiaatSignalGroup == null && _myTabViewModel.DynamischeHiaatSignalGroups.Any())
            {
                _myTabViewModel.SelectedDynamischeHiaatSignalGroup = _myTabViewModel.DynamischeHiaatSignalGroups[0];
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
                if (node.LocalName == "DynamischeHiaat")
                {
                    _myModel = XmlNodeConverter.ConvertNode<DynamischeHiaatModel>(node);
                    break;
                }
            }

            if (_myModel == null)
            {
                _myModel = new DynamischeHiaatModel();
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

        #region ITLCGenHasSettings

        public void LoadSettings()
        {
            MyDefaults =
                TLCGenSerialization.DeSerializeData<DynamischeHiaatDefaultsModel>(
                    ResourceReader.GetResourceTextFile("TLCGen.Dordrecht.DynamischeHiaat.Settings.DynamischeHiaatDefaults.xml", this));
        }

        public void SaveSettings()
        {
            
        }

        #endregion // ITLCGenHasSettings

        #region CCOLCodePieceGenerator

        public override void CollectCCOLElements(ControllerModel c)
        {
            _myElements = new List<CCOLElement>();

            foreach(var msg in _myModel.SignaalGroepenMetDynamischeHiaat.Where(x => x.HasDynamischeHiaat))
            {
                _myElements.Add(new CCOLElement($"geendynhiaat{msg.SignalGroupName}", CCOLElementTypeEnum.HulpElement, "Tegenhouden toepassen dynamische hiaattijden voor fase " + msg.SignalGroupName));
                foreach(var d in msg.DynamischeHiaatDetectoren)
                {
                    _myElements.Add(new CCOLElement($"TDH{_dpf}{d.DetectorName}", CCOLElementTypeEnum.GeheugenElement, $"Onthouden oorspronkelijke TDH voor detector {d.DetectorName}"));
                    _myElements.Add(new CCOLElement($"{d.DetectorName}_1", d.Moment1, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden moment 1 voor detector {d.DetectorName}"));
                    _myElements.Add(new CCOLElement($"{d.DetectorName}_2", d.Moment1, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden moment 2 voor detector {d.DetectorName}"));
                    _myElements.Add(new CCOLElement($"tdh_{d.DetectorName}_1", d.Moment1, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden TDH 1 voor detector {d.DetectorName}"));
                    _myElements.Add(new CCOLElement($"tdh_{d.DetectorName}_2", d.Moment1, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden TDH 2 voor detector {d.DetectorName}"));
                    _myElements.Add(new CCOLElement($"max_{d.DetectorName}", d.Maxtijd, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden maximale tijd 2 voor detector {d.DetectorName}"));
                    _myElements.Add(new CCOLElement($"verleng_{d.DetectorName}", CCOLElementTypeEnum.HulpElement, $"Instructie verlengen op detector {d.DetectorName} ongeacht dynamische hiaat"));
                    var schprm = 0;
                    if (d.Spring) schprm += 0x01;
                    if (d.VerlengWel) schprm += 0x02;
                    if (d.VerlengNiet) schprm += 0x04;
                    _myElements.Add(new CCOLElement($"springverleng_{d.DetectorName}", schprm, CCOLElementTimeTypeEnum.None, CCOLElementTypeEnum.Parameter, $"Dynamische hiaattijden maximale tijd 2 voor detector {d.DetectorName}"));
                    if (d.Vag4Mvt1.HasValue || d.Vag4Mvt2.HasValue)
                    {
                        var dd = c.Fasen.SelectMany(x => x.Detectoren).FirstOrDefault(x2 => x2.Naam == d.DetectorName);
                        if (dd != null && dd.Rijstrook.HasValue)
                        {
                            if (d.Vag4Mvt1.HasValue) _myElements.Add(new CCOLElement($"vag4_{d.DetectorName}_{dd.Rijstrook}_1", d.Vag4Mvt1.Value, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden veiligheidsgroen 1e timer voor detector {d.DetectorName}"));
                            if (d.Vag4Mvt2.HasValue) _myElements.Add(new CCOLElement($"vag4_{d.DetectorName}_{dd.Rijstrook}_2", d.Vag4Mvt2.Value, CCOLElementTimeTypeEnum.TE_type, CCOLElementTypeEnum.Timer, $"Dynamische hiaattijden veiligheidsgroen 2e timer voor detector {d.DetectorName}"));
                        }
                    }

                }
            }
        }

        public override bool HasCCOLElements()
        {
            return true;
        }

        public override IEnumerable<CCOLElement> GetCCOLElements(CCOLElementTypeEnum type)
        {
            return _myElements.Where(x => x.Type == type);
        }

        public override int HasCode(CCOLCodeTypeEnum type)
        {
            switch (type)
            {
                case CCOLCodeTypeEnum.RegCIncludes:
                    return 115;
                case CCOLCodeTypeEnum.RegCPostApplication:
                    return 115;
                default:
                    return 0;
            }
        }

        public override string GetCode(ControllerModel c, CCOLCodeTypeEnum type, string ts)
        {
            StringBuilder sb = new StringBuilder();

            var sgs = _myModel.SignaalGroepenMetDynamischeHiaat.Where(x => x.HasDynamischeHiaat);
            if (!sgs.Any()) return "";

            switch (type)
            {
                case CCOLCodeTypeEnum.RegCIncludes:
                        sb.AppendLine($"{ts}#include \"dynamischehiaat.c\"");
                    return sb.ToString();
                case CCOLCodeTypeEnum.RegCPostApplication:
                    foreach(var sg in sgs)
                    {
                        var ofc = c.Fasen.FirstOrDefault(x => x.Naam == sg.SignalGroupName);
                        if (ofc == null) continue;
                        sb.AppendLine($"{ts}hiaattijden_verlenging({(sg.KijkenNaarKoplus ? "TRUE" : "FALSE")}, {_fcpf}{sg.SignalGroupName}, IH[{_hpf}geendynhiaat{sg.SignalGroupName}],");
                        for (int i = 0; i < ofc.AantalRijstroken; i++)
                        {
                            foreach(var dd in sg.DynamischeHiaatDetectoren)
                            {
                                var od = ofc.Detectoren.FirstOrDefault(x => x.Naam == dd.DetectorName);
                                if (od == null || od.Rijstrook - 1 != i) continue;
                                sb.AppendLine($"{ts}{ts}{i + 1}, {_dpf}{od.Naam}, {_tpf}{dd.DetectorName}_1, {_tpf}{dd.DetectorName}_1, {_tpf}tdh_{dd.DetectorName}_1, {_tpf}tdh_{dd.DetectorName}_1, " +
                                    $"{_tpf}max_{dd.DetectorName}, PRM[{_prmpf}springverleng_{dd.DetectorName}] & BIT0, PRM[{_prmpf}springverleng_{dd.DetectorName}] & BIT1, IH[{_hpf}verleng_{dd.DetectorName}] || PRM[{_prmpf}springverleng_{dd.DetectorName}] & BIT2, {(dd.Vag4Mvt1.HasValue ? dd.Vag4Mvt1.Value.ToString() : "NG")}, {(dd.Vag4Mvt1.HasValue ? dd.Vag4Mvt1.Value.ToString() : "NG")}, {_mpf}TDH{_dpf}{dd.DetectorName}, ");
                            }
                        }
                        sb.AppendLine($"{ts}{ts}END);");
                    }
                    return sb.ToString();
                default:
                    return "";
            }
        }

        public override List<string> GetSourcesToCopy()
        {
            if (!_myModel.SignaalGroepenMetDynamischeHiaat.Any(x => x.HasDynamischeHiaat)) return null;
            return new List<string>
            {
                "dynamischehiaat.c"
            };
        }
        
        #endregion // CCOLCodePieceGenerator

        #region Private Methods

        internal void UpdateModel()
        {
            if (_controller != null && _myModel != null)
            {
                foreach (var fc in Controller.Fasen)
                {
                    if (fc.Type == TLCGen.Models.Enumerations.FaseTypeEnum.Auto &&
                        _myTabViewModel.DynamischeHiaatSignalGroups.All(x => x.SignalGroupName != fc.Naam))
                    {
                        var msg = new DynamischeHiaatSignalGroupViewModel(new DynamischeHiaatSignalGroupModel { SignalGroupName = fc.Naam });
                        msg.SelectedDefault = MyDefaults.Defaults.FirstOrDefault(x => x.Name == _myModel.TypeDynamischeHiaat);
                        if (string.IsNullOrEmpty(msg.Snelheid) || !msg.SelectedDefault.Snelheden.Any(x => x.Name == msg.Snelheid))
                        {
                            msg.Snelheid = msg.SelectedDefault.DefaultSnelheid;
                        }
                        _myTabViewModel.DynamischeHiaatSignalGroups.Add(msg);
                    }
                }
                var rems = new List<DynamischeHiaatSignalGroupViewModel>();
                foreach (var fc in _myTabViewModel.DynamischeHiaatSignalGroups)
                {
                    if (Controller.Fasen.All(x => x.Naam != fc.SignalGroupName))
                    {
                        rems.Add(fc);
                    }
                }
                foreach (var sg in rems)
                {
                    _myTabViewModel.DynamischeHiaatSignalGroups.Remove(sg);
                }
                _myTabViewModel.DynamischeHiaatSignalGroups.BubbleSort();
                _myTabViewModel.RaisePropertyChanged("");
            }
        }

        #endregion // Private Methods

        #region Constructor

        public DynamischeHiaatPlugin()
        {
            _myTabViewModel = new DynamischeHiaatPluginTabViewModel(this);
        }

        #endregion //Constructor
    }
}
