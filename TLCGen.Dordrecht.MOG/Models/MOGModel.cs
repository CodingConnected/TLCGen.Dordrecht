using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TLCGen.Dordrecht.MOG.Models
{
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
