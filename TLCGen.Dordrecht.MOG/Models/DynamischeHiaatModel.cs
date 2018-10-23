using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using TLCGen.Helpers;

namespace TLCGen.Dordrecht.DynamischeHiaat.Models
{
    [Serializable]
    [XmlRoot(ElementName = "DynamischeHiaat")]
    public class DynamischeHiaatModel
    {
        public string TypeDynamischeHiaat { get; set; }

        [XmlArray(ElementName = "SignaalGroepMetDynamischeHiaat")]
        public List<DynamischeHiaatSignalGroupModel> SignaalGroepenMetDynamischeHiaat { get; set; }

        public DynamischeHiaatModel()
        {
            TypeDynamischeHiaat = "IVER'18";
            SignaalGroepenMetDynamischeHiaat = new List<DynamischeHiaatSignalGroupModel>();
        }
    }
}
