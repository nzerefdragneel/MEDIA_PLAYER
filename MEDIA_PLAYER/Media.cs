using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MEDIA_PLAYER
{
    class Media: INotifyPropertyChanged
    {
        public int Order { get; set; }
        public string File_Path { get; set; }
        public int Duration_length { get; set; }
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
            get;set;
        }
        public string NowDuration { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

}
