using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FacialExpressionSample
{
    public class UIData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private BitmapSource _UIImage;

        public BitmapSource UIImage
        {
            get { return _UIImage; }
            set
            {
                if (_UIImage != value)
                {
                    _UIImage = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("UIImage"));
                    }
                }
            }
        }

        private ImageSource _BboxImage;

        public ImageSource BboxImage
        {
            get { return _BboxImage; }
            set
            {
                if (_BboxImage != value)
                {
                    _BboxImage = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("BboxImage"));
                    }
                }
            }
        }

        private bool _progressRing_IsActive = false;

        public bool progressRing_IsActive
        {
            get { return _progressRing_IsActive; }
            set
            {
                if (_progressRing_IsActive != value)
                {
                    _progressRing_IsActive = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("progressRing_IsActive"));
                    }
                }
            }
        }

        private string _TextMessage;

        public string TextMessage
        {
            get { return _TextMessage; }
            set
            {
                if (_TextMessage != value)
                {
                    _TextMessage = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("TextMessage"));
                    }
                }
            }
        }

        private string _FacialResult;

        public string FacialResult
        {
            get { return _FacialResult; }
            set
            {
                if (_FacialResult != value)
                {
                    _FacialResult = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FacialResult"));
                    }
                }
            }
        }

        private int _LastCount;

        public int LastCount
        {
            get { return _LastCount; }
            set
            {
                if (_LastCount != value)
                {
                    _LastCount = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("LastCount"));
                    }
                }
            }
        }
    }
}
