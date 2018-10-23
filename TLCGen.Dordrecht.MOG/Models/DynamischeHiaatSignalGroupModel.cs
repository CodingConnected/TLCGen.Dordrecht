using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TLCGen.Models;

namespace TLCGen.Dordrecht.DynamischeHiaat.Models
{
    [Serializable]
    public class DynamischeHiaatSignalGroupModel
    {
        [RefersTo]
        public string SignalGroupName { get; set; }
        public bool HasDynamischeHiaat { get; set; }

        public string Snelheid { get; set; }
        public bool KijkenNaarKoplus { get; set; }

        [XmlArray(ElementName = "DynamischeHiaatDetector")]
        public List<DynamischeHiaatDetectorModel> DynamischeHiaatDetectoren { get; set; }

        public DynamischeHiaatSignalGroupModel()
        {
            DynamischeHiaatDetectoren = new List<DynamischeHiaatDetectorModel>();
        }
    }
}
