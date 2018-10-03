using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using TLCGen.Dordrecht.MOG.Models;
using TLCGen.Helpers;

namespace TLCGen.Dordrecht.MOG.ViewModels
{
    internal class MOGDetectorViewModel : ViewModelBase, IViewModelWithItem, IComparable
    {
        #region Fields
        #endregion // Fields

        #region Properties

        [Browsable(false)]
        public MOGDetectorModel Detector { get; }

        [Browsable(false)]
        public string DetectorName => Detector.DetectorName;

        public int Instelling1
        {
            get => Detector.Instelling1;
            set
            {
                Detector.Instelling1 = value;
                RaisePropertyChanged();
            }
        }

        #endregion // Properties

        #region IViewModelWithItem

        public object GetItem()
        {
            return Detector;
        }

        #endregion // IViewModelWithItem

        #region IComparable

        public int CompareTo(object obj)
        {
            var d1 = Regex.Replace(Detector.DetectorName, $@"^{Detector.SignalGroupName}", "");
            var d2 = Regex.Replace(((MOGDetectorViewModel)obj).Detector.DetectorName, $@"^{Detector.SignalGroupName}", "");
            if (d1.Length < d2.Length) d1 = d1.PadLeft(d2.Length, '0');
            if (d2.Length < d1.Length) d2 = d2.PadLeft(d1.Length, '0');
            return string.CompareOrdinal(d1, d2);
        }

        #endregion // IComparable

        #region Constructor

        public MOGDetectorViewModel(MOGDetectorModel detector)
        {
            Detector = detector;
        }

        #endregion // Constructor
    }
}
