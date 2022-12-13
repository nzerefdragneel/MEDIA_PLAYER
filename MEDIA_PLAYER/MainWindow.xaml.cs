using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Collections.ObjectModel;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using System.Linq;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Numerics;

namespace MEDIA_PLAYER
{
    public partial class MainWindow : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private string _currentPlaying = string.Empty;
        private int _currentPlayingIndex = 0;
        Playlist _playList = new Playlist();
        ObservableCollection<Media> _mediaList = new ObservableCollection<Media>();
        Media _add=new Media();
        MediaPlayer _Slider = new MediaPlayer();
        double nowPosion = 0;
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

            

            
           
            //----------------< mediaplayer_OpenMedia() >----------------
        }
        private void add_Audio_image(string path)
        {
            _add = new Media();
            var bitmap = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
            _add.imageReview = bitmap;
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(path));
            while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
            var time = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            _add.Duration_length = time;
            _add.NowDurationLength = 0;
            _add.File_Path = path;
          
            _mediaList.Add(_add);

        }
        private void add_Video_Image(string sFullname_Path_of_Video)
        {
            _add = new Media();
            //----------------< add_Video_Image() >----------------
            //*create mediaplayer in memory and jump to position
            Debug.WriteLine(sFullname_Path_of_Video);
            MediaPlayer mediaPlayer = new MediaPlayer();
           // mediaPlayer.MediaOpened += new EventHandler(mediaplayer_OpenMedia);
            mediaPlayer.MediaFailed += (o, args) =>
            {
                MessageBox.Show("Media Failed!!");
            };
            mediaPlayer.ScrubbingEnabled = true;
            mediaPlayer.Open(new Uri(sFullname_Path_of_Video));
            mediaPlayer.ScrubbingEnabled = true;

            mediaPlayer.Play();
            mediaPlayer.Pause();
           
            _add.File_Path=sFullname_Path_of_Video;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawVideo(mediaPlayer, new Rect(0, 0, 160, 100));
            drawingContext.Close();
            double dpiX = 1 / 200;
            double dpiY = 1 / 200;
            
            RenderTargetBitmap bmp = new RenderTargetBitmap(160, 100, dpiX, dpiY, PixelFormats.Pbgra32);
            
            bmp.Render(drawingVisual);
            _add.imageReview = bmp;
            while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var time = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                _add.Duration_length = time;
                Debug.WriteLine("string", _add.DurationString);
                _add.NowDurationLength = 0.0;
            }
           
            _mediaList.Add(_add);
            

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
            CanvasSeeking.Visibility = Visibility.Visible;
            nowPosion = sliProgress.Value;
            textSeeking.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            _Slider = new MediaPlayer();
            //----------------< add_Video_Image() >----------------
            //*create mediaplayer in memory and jump to position
            if (_currentPlaying != null)
            {
                _Slider.Open(new Uri(_currentPlaying));
                _Slider.ScrubbingEnabled = true;
                _Slider.Play();
                _Slider.Stop();
                _Slider.Position = TimeSpan.FromSeconds(sliProgress.Value);

                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawVideo(_Slider, new Rect(0, 0, 120, 90));
                drawingContext.Close();
                double dpiX = 1 / 200;
                double dpiY = 1 / 200;
                RenderTargetBitmap bmp = new RenderTargetBitmap(120, 90, dpiX, dpiY, PixelFormats.Pbgra32);
                bmp.Render(drawingVisual);
                imageSeeking.Source = bmp;
            }
        }
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
            _mediaList[_currentPlayingIndex].NowDurationLength = sliProgress.Value;
            CanvasSeeking.Visibility = Visibility.Collapsed;
            lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");

        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (userIsDraggingSlider)
            {
                lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
                textSeeking.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
                _Slider.Position = TimeSpan.FromSeconds(sliProgress.Value);
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext drawingContext = drawingVisual.RenderOpen();
                drawingContext.DrawVideo(_Slider, new Rect(0, 0, 120, 90));
                drawingContext.Close();
                double dpiX = 1 / 200;
                double dpiY = 1 / 200;
                RenderTargetBitmap bmp = new RenderTargetBitmap(120, 90, dpiX, dpiY, PixelFormats.Pbgra32);

                bmp.Render(drawingVisual);
                imageSeeking.Source = bmp;
                var posision = Mouse.GetPosition(Application.Current.MainWindow);
                Canvas.SetLeft(imageSeeking, posision.X - 50);
                Canvas.SetLeft(textSeeking, posision.X - 30);
            }
            else
            {
                double value = sliProgress.Value;
                TimeSpan newPosition = TimeSpan.FromSeconds(value);
                lblProgressStatus.Text=newPosition.ToString(@"hh\:mm\:ss");
                _mediaList[_currentPlayingIndex].NowDurationLength = value;
                mePlayer.Position = newPosition;
            }

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
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {

        }
        //FIXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
        private void ChooseToPlay(object sender, MouseButtonEventArgs e)
        {
            var x = PlayListView.SelectedIndex;
            if (x != -1)
            {
                Debug.WriteLine("current play " + _currentPlayingIndex.ToString());
                _mediaList[_currentPlayingIndex].NowDurationLength = sliProgress.Value;

                _currentPlaying = _mediaList[x].File_Path;
                _currentPlayingIndex = x;
                mePlayer.Source = new Uri(_currentPlaying);
                mePlayer.Play();
                mePlayer.Pause();
                mePlayer.Position = TimeSpan.FromSeconds(_mediaList[x].NowDurationLength);


                sliProgress.Value = _mediaList[x].NowDurationLength;

                lblProgressStatus.Text = _mediaList[x].NowDuration;

                Debug.WriteLine("current play " + x.ToString());
                Debug.WriteLine("current play " + _mediaList[_currentPlayingIndex].NowDurationLength.ToString());
                mediaPlayerIsPlaying = false;
               
                //change sliderbar
            }
        }
        public void ReadAllFileInFolder(string path)
        {
            // 
            var files = Directory.GetFiles(path);
           
            foreach (string d in files)
            {
                progressbarLoadmedia.Value += 5;
                Debug.WriteLine(d);
                if (IsVideoFile(d))
                {
                    add_Video_Image(d);
                    if (mePlayer.Source == null)
                    {
                        _currentPlaying = d;
                        mePlayer.Source = new Uri(d);
                        mePlayer.Play();
                        mePlayer.Stop();
                        mePlayer.MediaFailed += (o, args) =>
                        {
                            MessageBox.Show("Media Failed!!");
                        };
                    }
                }
                else if (IsAudioFile(d))
                {
                    add_Audio_image(d);
                    if (mePlayer.Source == null)
                    {
                        _currentPlaying = d;
                        Debug.WriteLine("path", d);
                        mePlayer.Source = new Uri(d);
                        mePlayer.Play();
                        mePlayer.Stop();
                        Debug.WriteLine("ok");
                        mePlayer.MediaFailed += (o, args) =>
                        {
                            MessageBox.Show("Media Failed!!");
                        };
                    }
                }
            }
        }

        private void AddFiles(object sender, RoutedEventArgs e)
        {
            progressbarLoadmedia.Visibility= Visibility.Visible;
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (System.Windows.Forms.DialogResult.OK == result)
            {

                string path = dialog.SelectedPath + "\\";

                ReadAllFileInFolder(path);
            }
            progressbarLoadmedia.Visibility= Visibility.Collapsed;
        }

        private void PlayListView_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }

        private void DeleteThis(object sender, RoutedEventArgs e)
        {

        }

        private void HiddenBtn(object sender, RoutedEventArgs e)
        {
            if (PlayListStackPannel.Visibility == Visibility.Visible)
            {
                PlayListStackPannel.Visibility = Visibility.Collapsed;
                HiddenButton.Content = "Show";    
            }
            else
            {
                PlayListStackPannel.Visibility = Visibility.Visible;
                HiddenButton.Content = "Hide";
            }
        }

        private void hoverEvent(object sender, MouseEventArgs e)
        {
            var send = sender as StackPanel; if (send != null) {
                Debug.WriteLine(send.GetType());
            send.Visibility = Visibility.Visible;
            }
        }

        private void DeleteMovie(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = PlayListView.Items.IndexOf(item);
           
            if (index != -1)
            {
                _mediaList.RemoveAt(index);
            }
        }
    }
}

