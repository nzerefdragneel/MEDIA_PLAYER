using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using ProgressBar = System.Windows.Controls.ProgressBar;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Numerics;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Security.AccessControl;

namespace MEDIA_PLAYER
{
    public partial class MainWindow : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private string _currentPlaying = string.Empty;
        Playlist _playList = new Playlist();
        ObservableCollection<Media> _mediaList = new ObservableCollection<Media>();
        Media _add=new Media();
        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayListView.ItemsSource = _mediaList;
        }
        private string _shortName
        {
            get
            {
                var info = new FileInfo(_currentPlaying);
                var name = info.Name;
                return name;
            }
        }
        static string[] videoExt =
      {
            ".FLV",".AVI",".WMV",".MP4",".MPG",".MPEG",".M4V"
        };
        static string[] audioExt =
        {
            ".MP3",".WAV",".WAVE",".WMA"
        };
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        bool IsAudioFile(string path)
        {
            return -1 != Array.IndexOf(audioExt, Path.GetExtension(path).ToUpperInvariant());
        }
        bool IsVideoFile(string path)
        {
            return -1 != Array.IndexOf(videoExt, Path.GetExtension(path).ToUpperInvariant());
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video |*.flv;*.avi;*.vmv;*.mp4;*.mpg;*.m4v|Audio|*.mp3;*.mpg;*.mpeg;*.wav;*.wave|All files (*.*)|*.*";
            progressbarLoadmedia.Visibility = Visibility.Visible;
            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;
                if (IsVideoFile(path))
                {
                    add_Video_Image(path);
                }
                else
                {
                    add_Audio_image(path);
                }
                if (mePlayer.Source==null )
                {
                    _currentPlaying = openFileDialog.FileName;
                    
                    Debug.WriteLine("path",_currentPlaying);
                    mePlayer.Source = new Uri(_currentPlaying);
                    mePlayer.Play();
                    mePlayer.Stop();
                    Debug.WriteLine("ok");
                    mePlayer.MediaFailed += (o, args) =>
                    {
                        MessageBox.Show("Media Failed!!");
                    };
                }
                progressbarLoadmedia.Visibility = Visibility.Collapsed;
            }

        }
        private void mediaplayer_OpenMedia(object sender, EventArgs e)
        {
            //----------------< mediaplayer_OpenMedia() >----------------
            //*create mediaplayer in memory and jump to position
            //< draw video_image >
            ProgressBar progressLoad = new ProgressBar();
            progressLoad.Height = 30;
            progressLoad.Width = 50;
            PlayListStackPannel.Children.Add(progressLoad);
            MediaPlayer mediaPlayer = sender as MediaPlayer;
          
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawVideo(mediaPlayer, new Rect(0, 0, 160, 100));
                drawingContext.Close();
                double dpiX = 1 / 200;
                double dpiY = 1 / 200;
                RenderTargetBitmap bmp = new RenderTargetBitmap(160, 100, dpiX, dpiY, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);
                _add.imageReview = bmp;
                /*
                MemoryStream stream = new MemoryStream();
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(stream);
                Bitmap bitmap = new Bitmap(stream);*/
                //ImageSource bit = bmp;
                //var bitmap = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
                //</ draw video_image >
                //bit = bitmap;
                ////< set Image >
                //Image newImage = new Image();
                //newImage.Source = bit;
                //newImage.Stretch = Stretch.Uniform;
                //newImage.Height = 100;
                //</ set Image >

                //< add >
                //   image.Source = bmp;

            

            
            while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var time= TimeSpan.FromSeconds(mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
                _add.DurationString = time.ToString(@"hh\:mm\:ss");
                Debug.WriteLine("string",_add.DurationString);
                _add.NowDuration = "00:00:00";
            }
            _mediaList.Add(_add);
            PlayListStackPannel.Children.Remove(progressLoad);
            _add = new Media();
            //----------------< mediaplayer_OpenMedia() >----------------
        }
        private void add_Audio_image(string path)
        {
            var bitmap = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
            _add.imageReview = bitmap;
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(path));
            while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
            var time = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            _add.DurationString = TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");
            _add.NowDuration = "00:00:00";
            _add.File_Path = path;
            _mediaList.Add(_add);
        }
        private void add_Video_Image(string sFullname_Path_of_Video)
        {
            //----------------< add_Video_Image() >----------------
            //*create mediaplayer in memory and jump to position
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaOpened += new EventHandler(mediaplayer_OpenMedia);
            mediaPlayer.MediaFailed += (o, args) =>
            {
                MessageBox.Show("Media Failed!!");
            };
            mediaPlayer.ScrubbingEnabled = true;
            mediaPlayer.Open(new Uri(sFullname_Path_of_Video));
            // mediaPlayer.Position = TimeSpan.FromSeconds(0);
            if (IsAudioFile(sFullname_Path_of_Video))
            {
                MessageBox.Show("Media Audio!!");
            }
            _add.File_Path=sFullname_Path_of_Video;     
            
            //----------------</ add_Video_Image() >----------------
        }

        private EventHandler mediaplayer_OpenMedia(string x)
        {
            throw new NotImplementedException();
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null)&&(mediaPlayerIsPlaying!=true);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
            mediaPlayerIsPlaying = false;
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;

            Debug.WriteLine(mediaPlayerIsPlaying);
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
            Debug.WriteLine(sliProgress.Value);
            Debug.WriteLine(mePlayer.Position.ToString());

        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            //while (!mePlayer.NaturalDuration.HasTimeSpan);
            if ((mePlayer.NaturalDuration.HasTimeSpan))
            {
                lblProgressStatusEnd.Text = TimeSpan.FromSeconds(mePlayer.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh\:mm\:ss");
                Debug.WriteLine(TimeSpan.FromSeconds(mePlayer.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh\:mm\:ss"));
            }
            Debug.WriteLine("open");
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {

        }

        private void ChooseToPlay(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

