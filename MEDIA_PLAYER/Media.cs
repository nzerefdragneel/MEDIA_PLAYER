using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace MEDIA_PLAYER
{
    class Media: INotifyPropertyChanged
    {
        public string File_Path { get; set; }

        public double Duration_length { get; set; }
        public ImageSource imageReview { get; set; }
        public string Description
        {
            get
            {
                return Path.GetFileNameWithoutExtension(File_Path);
            }
            
        }
        public string DurationString
        {
            get => TimeSpan.FromSeconds(Duration_length).ToString(@"hh\:mm\:ss");
        }
        public double NowDurationLength { get; set; }
        public string NowDuration {
            get => TimeSpan.FromSeconds(NowDurationLength).ToString(@"hh\:mm\:ss"); 
        }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
    class RGB
    {
        public Color Top { get; set; }

        public Color Bottom { get; set; }
    }
}
