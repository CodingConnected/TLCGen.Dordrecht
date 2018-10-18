using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TLCGen.Models;

namespace TLCGen.Dordrecht.MOG.Models
{
    [Serializable]
    public class MOGSignalGroupModel
    {
        [RefersTo]
        public string SignalGroupName { get; set; }
        public bool HasMOG { get; set; }

        public IVERSnelheidEnum Snelheid { get; set; }
        public bool KijkenNaarKoplus { get; set; }

        [XmlArray(ElementName = "MOGDetector")]
        public List<MOGDetectorModel> MOGDetectoren { get; set; }

        public MOGSignalGroupModel()
        {
            MOGDetectoren = new List<MOGDetectorModel>();
        }
    }
}
