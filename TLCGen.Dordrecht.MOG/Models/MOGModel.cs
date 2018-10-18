using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using TLCGen.Helpers;

namespace TLCGen.Dordrecht.MOG.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum IVERSnelheidEnum
    {
        [Description("40 km/u")]
        kmh40,
        [Description("50 km/u")]
        kmh50,
        [Description("60 km/u")]
        kmh60,
        [Description("70 km/u")]
        kmh70,
        [Description("80 km/u")]
        kmh80,
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
