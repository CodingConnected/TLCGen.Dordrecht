using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TLCGen.Models;

namespace TLCGen.Dordrecht.MOG.Models
{
    [Serializable]
    public class MOGDetectorModel
    {
        [ModelName(TLCGen.Models.Enumerations.TLCGenObjectTypeEnum.Detector)]
        public string DetectorName { get; set; }

        public int Instelling1 { get; set; }
    }

    [Serializable]
    public class MOGSignalGroupModel
    {
        [ModelName(TLCGen.Models.Enumerations.TLCGenObjectTypeEnum.Fase)]
        public string SignalGroupName { get; set; }
        public bool HasMOG { get; set; }

        public int Instelling1 { get; set; }

        [XmlArray(ElementName = "MOGDetector")]
        public List<MOGDetectorModel> MOGDetectoren { get; set; }

        public MOGSignalGroupModel()
        {
            MOGDetectoren = new List<MOGDetectorModel>();
        }
    }

    [Serializable]
    [XmlRoot(ElementName = "MOG")]
    public class MOGModel
    {
        [XmlArray(ElementName = "SignaalGroepMetMOG")]
        public List<MOGSignalGroupModel> SignaalGroepenMetMOG { get; set; }

        public MOGModel()
        {
            SignaalGroepenMetMOG = new List<MOGSignalGroupModel>();
        }
    }
}
