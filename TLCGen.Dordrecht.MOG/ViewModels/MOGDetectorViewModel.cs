using GalaSoft.MvvmLight;
using System;
using System.ComponentModel;
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
            return Detector.DetectorName.CompareTo(((MOGDetectorViewModel)obj).Detector.DetectorName);
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
