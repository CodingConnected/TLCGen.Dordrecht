using System;
using TLCGen.Models;

namespace TLCGen.Dordrecht.DynamischHiaat.Models
{
    [Serializable]
    public class DynamischHiaatDetectorModel
    {
        [RefersTo]
        public string SignalGroupName { get; set; }
        [RefersTo]
        public string DetectorName { get; set; }

        public int Moment1 { get; set; }
        public int Moment2 { get; set; }
        public int TDH1 { get; set; }
        public int TDH2 { get; set; }
        public int Maxtijd { get; set; }
        public bool Spring { get; set; }
        public bool VerlengNiet { get; set; }
        public bool VerlengWel { get; set; }
        public int? Vag4Mvt1 { get; set; }
        public int? Vag4Mvt2 { get; set; }
    }
}
