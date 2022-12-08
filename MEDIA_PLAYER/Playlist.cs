using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace MEDIA_PLAYER
{
    class Playlist: INotifyPropertyChanged
    {
        public BindingList<Media> mediaList = new BindingList<Media>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string m_Path { get; set; }
        public string next { get; set; }
        public string Short_Name { get { return Path.GetFileNameWithoutExtension(m_Path); } }
        public Playlist()
            {
                var time = DateTime.Now.Second.ToString();
                m_Path = AppDomain.CurrentDomain.BaseDirectory + "Playlists\\" + time + ".pl";
            }

         
      
    }
}
