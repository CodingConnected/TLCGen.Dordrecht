using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TLCGen.Dordrecht.DynamischeHiaat.Models
{
    [Serializable]
    [XmlRoot(ElementName = "DynamischeHiaatDefaults")]
    public class DynamischeHiaatDefaultsModel
    {
        [XmlArrayItem(ElementName = "DynamischeHiaatDefault")]
        public List<DynamischeHiaatDefaultModel> Defaults { get; set; }

        public DynamischeHiaatDefaultsModel()
        {
            Defaults = new List<DynamischeHiaatDefaultModel>();
        }
    }

    [Serializable]
    public class DynamischeHiaatDefaultModel
    {
        public string Name { get; set; }
        public string DefaultSnelheid { get; set; }

        [XmlArrayItem(ElementName = "Snelheid")]
        public List<DynamischeHiaatSpeedDefaultModel> Snelheden { get; set; }

        public DynamischeHiaatDefaultModel()
        {
            Snelheden = new List<DynamischeHiaatSpeedDefaultModel>();
        }
    }

    [Serializable]
    public class DynamischeHiaatSpeedDefaultModel
    {
        public string Name { get; set; }

        [XmlArrayItem(ElementName = "Detector")]
        public List<DynamischeHiaatDetectorModel> Detectoren { get; set; }

        public DynamischeHiaatSpeedDefaultModel()
        {
            Detectoren = new List<DynamischeHiaatDetectorModel>();
        }
    }
}