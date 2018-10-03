using System;
using TLCGen.Models;

namespace TLCGen.Dordrecht.MOG.Models
{
    [Serializable]
    public class MOGDetectorModel
    {
        [RefersTo]
        public string SignalGroupName { get; set; }
        [RefersTo]
        public string DetectorName { get; set; }

        public int Instelling1 { get; set; }
    }
}
