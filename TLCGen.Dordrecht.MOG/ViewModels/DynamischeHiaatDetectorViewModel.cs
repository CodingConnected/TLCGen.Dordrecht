using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using TLCGen.Dordrecht.DynamischeHiaat.Models;
using TLCGen.Helpers;

namespace TLCGen.Dordrecht.DynamischeHiaat.ViewModels
{
    internal class DynamischeHiaatDetectorViewModel : ViewModelBase, IViewModelWithItem, IComparable
    {
        #region Fields
        #endregion // Fields

        #region Properties

        [Browsable(false)]
        public DynamischeHiaatDetectorModel Detector { get; }

        [Browsable(false)]
        public string DetectorName => Detector.DetectorName;

        public int Moment1
        {
            get => Detector.Moment1;
            set
            {
                if (value == 0 || value != Detector.Moment2)
                {
                    Detector.Moment1 = value;
                }
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public int Moment2
        {
            get => Detector.Moment2;
            set
            {
                if (value == 0 || value != Detector.Moment1)
                {
                    Detector.Moment2 = value;
                }
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public int TDH1
        {
            get => Detector.TDH1;
            set
            {

                Detector.TDH1 = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public int TDH2
        {
            get => Detector.TDH2;
            set
            {
                Detector.TDH2 = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public int Maxtijd
        {
            get => Detector.Maxtijd;
            set
            {
                Detector.Maxtijd = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public bool Spring
        {
            get => Detector.Spring;
            set
            {
                Detector.Spring = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public bool VerlengNiet
        {
            get => Detector.VerlengNiet;
            set
            {
                Detector.VerlengNiet = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public bool VerlengWel
        {
            get => Detector.VerlengWel;
            set
            {
                Detector.VerlengWel = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public int? Vag4Mvt1
        {
            get => Detector.Vag4Mvt1;
            set
            {
                Detector.Vag4Mvt1 = value;
                RaisePropertyChanged<object>(broadcast: true);
            }
        }

        public int? Vag4Mvt2
        {
            get => Detector.Vag4Mvt2;
            set
            {
                Detector.Vag4Mvt2 = value;
                RaisePropertyChanged<object>(broadcast: true);
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
            var d2 = Regex.Replace(((DynamischeHiaatDetectorViewModel)obj).Detector.DetectorName, $@"^{Detector.SignalGroupName}", "");
            if (d1.Length < d2.Length) d1 = d1.PadLeft(d2.Length, '0');
            if (d2.Length < d1.Length) d2 = d2.PadLeft(d1.Length, '0');
            return string.CompareOrdinal(d1, d2);
        }

        #endregion // IComparable

        #region Constructor

        public DynamischeHiaatDetectorViewModel(DynamischeHiaatDetectorModel detector)
        {
            Detector = detector;
        }

        #endregion // Constructor
    }
}
