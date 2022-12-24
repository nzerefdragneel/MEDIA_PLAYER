using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;

namespace MEDIA_PLAYER
{
    /// <summary>
    /// Interaction logic for ChangeName.xaml
    /// </summary>
    /// 
    public partial class ChangeName : Window,INotifyPropertyChanged
    {
        public string NewNamePath { get; set; }
        public string NewNameEtx { get; set; }
        public string name { get; set; }
        public int index { get; set; }

        public delegate void NameValueChangeHandler(string newValue,int index);

        public event NameValueChangeHandler NameChangedEvent;
        public ChangeName(string name, int index)
        {
            InitializeComponent();
            this.DataContext = this;
            this.name = Path.GetFileNameWithoutExtension(name);
            NewNamePath = Path.GetDirectoryName(name);
            NewNameEtx = Path.GetExtension(name);
            Debug.WriteLine(NewNamePath);
            this.index = index;
            Debug.WriteLine(name);
        }

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            var text = sender as TextBox;
            if (text.Text == "") return;
            var fullpath = NewNamePath+"//" + text.Text + NewNameEtx;
            NameChangedEvent?.Invoke(fullpath,index);
        }

        private void ChangeNameOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BackButton(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
