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
using System.Windows.Controls;
using Application = System.Windows.Application;
using System.Collections.Generic;
using System.Text;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls.Ribbon.Primitives;
using MaterialDesignThemes.Wpf;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Text.Json;
using System.Windows.Markup;
using static System.Windows.Forms.Design.AxImporter;
using System.Xml;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Threading.Tasks;


namespace MEDIA_PLAYER
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private int mediaPlayerIsRepeat = -1;//-1:no repeat;0:repeat:1 ;1:repeat all
        private bool mediaPlayerIsShuffling=false;
        private int shuffleIndex = -1;
        private List<int> _shuffleList = new List<int>();
        private bool autoplay = true;
        //prev list
        private List<int> _prevList = new List<int>();
        private ObservableCollection<string> _prevListName = new ObservableCollection<string>() { "Remove all"};
        private List<string> _prevListFullPathName = new List<string>();
        private string saveRecentFile = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "RecentFile.json");
        private string configAutoSave = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "corgi.json");
        //play media
        private double speedup = 1;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private string _currentPlaying = string.Empty;
        private int _currentPlayingIndex = 0;
        //MediaPlayer _ShowBackground = new MediaPlayer();

        Random rnd = new Random();

        private string playlistPath = string.Empty;

        //add to playlist change this, new playlist..
        private bool playlistIsChange = false;
        ObservableCollection<Media> _mediaList = new ObservableCollection<Media>();
       
        Media _add=new Media();
        MediaPlayer _Slider = new MediaPlayer();
        double nowPosion = 0;
        bool isDark = false;
        RGB RGB =new RGB();


      
        private void SaveConfig()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"\"darkmode\":{(isDark == true ? "true" : "false")},");
            stringBuilder.AppendLine($"\"autoplay\":{(autoplay == true ? "true" : "false")},");
            stringBuilder.AppendLine($"\"volumn\":{mePlayer.Volume.ToString().Replace(',', '.')},");
            stringBuilder.AppendLine($"\"speedup\":{mePlayer.SpeedRatio.ToString().Replace(',', '.')},");
            stringBuilder.AppendLine($"\"repeat\":{mediaPlayerIsRepeat},");
            stringBuilder.AppendLine($"\"shuffle\":{(mediaPlayerIsShuffling == true ? "true" : "false")},");
            stringBuilder.AppendLine($"\"left\":{this.Left},");
            stringBuilder.AppendLine($"\"top\":{this.Top},");
            stringBuilder.AppendLine($"\"height\":{this.Height},");
            stringBuilder.AppendLine($"\"width\":{this.Width},");
            stringBuilder.AppendLine($"\"playlist\":");
          
            string json = JsonConvert.SerializeObject(_mediaList);
            
            stringBuilder.AppendLine(json);
            stringBuilder.AppendLine("}");
              Debug.WriteLine(stringBuilder.ToString());
            File.WriteAllText(configAutoSave, stringBuilder.ToString());
        }
        public class configA
        {
            public bool darkmode { get; set; }
            public bool autoplay { get; set; }
            public float volumn { get; set; }
            public float speedup { get; set; }
            public int mediaPlayerIsRepeat { get; set; }
            public double left { get; set; }
            public double top { get; set; }
            public double height { get; set; }
                public double width { get; set; }


        }

        private void LoadConfig()
        {
            if (!File.Exists(configAutoSave)) return;
            string loaded = File.ReadAllText(configAutoSave);
            
            dynamic pos = JsonObject.Parse(loaded);

        
            isDark = pos["darkmode"].GetValue<bool>();
            DarkMode.IsChecked = isDark;
            Background();

            autoplay = pos["autoplay"].GetValue<bool>();
            AutoPlay();

            mePlayer.Volume = pos["volumn"].GetValue<double>();
            ChangeUiVolume();

            mePlayer.SpeedRatio =pos["speedup"].GetValue<double>();
            SpeedUpValue.Text = $"{mePlayer.SpeedRatio}x";
           
            mediaPlayerIsRepeat =pos["repeat"].GetValue<int>();
            ChangeUIRepeat();

            mediaPlayerIsShuffling = pos["shuffle"].GetValue<bool>();
            ChangeUIShuffle();

            this.Left = pos["left"].GetValue<double>();
            this.Top =pos["top"].GetValue<double>();
            this.Height = pos["height"].GetValue<double>();
            this.Width =pos["width"].GetValue<double>();

            var list = pos["playlist"];

            foreach (var media in list)
            {
                string path = media["File_Path"].GetValue<string>();
                double isNow = media["NowDurationLength"].GetValue<double>();
                if (!File.Exists(path)) continue;
                if (IsVideoFile(path))
                {
                    add_Video_Image(path);
                    _mediaList[_mediaList.Count - 1].NowDurationLength = isNow;
                  
                }
                else
                {
                    add_Audio_image(path);
                    _mediaList[_mediaList.Count - 1].NowDurationLength = isNow;
                }
                if (mePlayer.Source == null)
                {
                    ChangeCurrentPlay(_mediaList.Count - 1);
                }
            
            }
            
        }
        private void SetPrimaryColor(Color color,Color color2)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetPrimaryColor(color);
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();       
            theme.SetSecondaryColor(color2);
            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);
        }
        private void Background()
        {

          
            if (isDark)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()

                {

                    Source = new Uri(".\\Dark.xaml", UriKind.RelativeOrAbsolute)

                }) ;

            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()

                {

                    Source = new Uri(".\\Light.xaml", UriKind.RelativeOrAbsolute)

                });
            }
           
          

        }
        private void ChangeDarkMode(object sender, RoutedEventArgs e)
        {
            isDark = !isDark;
            DarkMode.IsChecked = isDark;
            Background();
            

        }
        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            mePlayer.Volume = 50;
     
            DarkMode.IsChecked = isDark;
            Background();
            if (ControlView.IsMouseOver)
            {
               // DisplaySlider();
            }
            else
            {
               // NoneDisplaySlider();
            }

        }
        //public Color GetColorAt(Point location)
        //{
        //    using (Graphics gdest = Graphics.FromImage(screenPixel))
        //    {
        //        using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
        //        {
        //            IntPtr hSrcDC = gsrc.GetHdc();
        //            IntPtr hDC = gdest.GetHdc();
        //            int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
        //            gdest.ReleaseHdc();
        //            gsrc.ReleaseHdc();
        //        }
        //    }

        //    return screenPixel.GetPixel(0, 0);
        //}

        private int BitBlt(IntPtr hDC, int v1, int v2, int v3, int v4, IntPtr hSrcDC, int x, int y, int sourceCopy)
        {
            throw new NotImplementedException();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayListView.ItemsSource = _mediaList;
            OutlinedComboBox.ItemsSource = _prevListName;
            if (!File.Exists(saveRecentFile)) return;
            StreamReader input;
            input = new StreamReader(saveRecentFile);
            string path = "";
            while ((path = input.ReadLine()) != null)
            {
                if (path != "" && File.Exists(path))
                {
                    Debug.WriteLine("save " + path);
                    _prevListFullPathName.Add(path);
                    _prevListName.Add(Path.GetFileNameWithoutExtension(path));
                }
            }
           
          
            MidColor.DataContext = RGB;
            input.Close();

            LoadConfig();
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
        private System.Drawing.Image screenPixel;

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
        bool checkNext()
        {
            if (mediaPlayerIsShuffling == true) return true;
            if (mediaPlayerIsRepeat != -1) return false;
            return _currentPlayingIndex + 1 < _mediaList.Count ? true : false;
        }

        private int nextPlay(int current)
        {
            //shuffle
            if (mediaPlayerIsShuffling == true)
            {
                if (shuffleIndex == _shuffleList.Count-1)
                {
                    if (_mediaList.Count == 1) return 0;
                    var next = rnd.Next(0, _mediaList.Count);
                    while (next == _currentPlayingIndex) next = rnd.Next(0, _mediaList.Count);
                    return next;
                }
                return _shuffleList[shuffleIndex+1];
            }
            //repeat
            if (mediaPlayerIsRepeat != -1) return -1;
            //auto play
            return current+1<_mediaList.Count?current + 1 : -1;
        }

        bool checkPrev()
        {
            Debug.WriteLine("check prev: shuf {0}, repeat {1}", mediaPlayerIsShuffling, mediaPlayerIsRepeat);
            if (mediaPlayerIsShuffling== true)
            {
                if (shuffleIndex < 1) return false;
                return true;
            }
            if (mediaPlayerIsRepeat!=-1) return false;
            Debug.WriteLine("current play:{0}", _currentPlayingIndex);
            return _currentPlayingIndex - 1 >= 0 ? true: false;
        }
        private int prevPlay(int current)
        {
            if (mediaPlayerIsShuffling == true)
            {
                Debug.WriteLine("list {0} index {1}", _shuffleList.Count, shuffleIndex);
                if (_shuffleList.Count == 0 || shuffleIndex<1) return -1;
                return _shuffleList[shuffleIndex - 1];
            }
            if (mediaPlayerIsRepeat != -1) return -1;
            return current - 1 >= 0 ? current - 1 : -1;
        }
        private bool checkHavingFile(string path)
        {
            foreach (var media in _mediaList)
            {
                if (string.Compare(path, media.File_Path) == 0) return true;
            }
            return false;
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video |*.flv;*.avi;*.vmv;*.mp4;*.mpg;*.m4v|Audio|*.mp3;*.mpg;*.mpeg;*.wav;*.wave|All files (*.*)|*.*";
            progressbarLoadmedia.Visibility = Visibility.Visible;
           
            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;
                if (checkHavingFile(path) == true)
                {
                    MessageBox.Show("File is exist");
                }
                else
                {
                    if (IsVideoFile(path))
                    {
                        add_Video_Image(path);
                       
                    }
                    else
                    {
                        add_Audio_image(path);
                    }
                    if (mePlayer.Source == null)
                    {
                        _currentPlaying = openFileDialog.FileName;

                        Debug.WriteLine("path", _currentPlaying);
                        mePlayer.Source = new Uri(_currentPlaying);
                        //_ShowBackground.Open(new Uri(_currentPlaying));
                        mePlayer.Play();
                        mePlayer.Stop();
                        Debug.WriteLine("ok");
                        _currentPlayingIndex = 0;
                        if (IsAudioFile(path)) AudiaPlayer.Visibility = Visibility.Visible;
                        else AudiaPlayer.Visibility = Visibility.Collapsed;
                        ListViewItem rows = PlayListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                        if (rows != null) rows.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryDarkForegroundBrush"].ToString());
                        mePlayer.MediaFailed += (o, args) =>
                        {
                            MessageBox.Show("Media Failed!!");
                        };
                    }
                }
                progressbarLoadmedia.Visibility = Visibility.Collapsed;
            }

        }
        private void updatePreList()
        {
            if(_currentPlaying=="") return;
            //if (_prevListFullPathName.Count == 0) return;
            // Loi o day
            if (_prevListFullPathName.Count >0 && string.Compare(_currentPlaying, _prevListFullPathName[_prevListFullPathName.Count-1]) == 0) return;
              for (var i=0;i<_prevListFullPathName.Count;i++)
            {
                if (string.Compare(_currentPlaying, _prevListFullPathName[i]) == 0)
                {
                   // _prevList.RemoveAt(i);
                   
                   _prevListName.RemoveAt(i+1);
                   _prevListFullPathName.RemoveAt(i);
                  
                    Debug.WriteLine("==== Bao thu ====");
                    Debug.WriteLine(_prevListName);
                    Debug.WriteLine(_prevListFullPathName);
                    Debug.WriteLine("==== Bao thu ====");
                    break;
                }
            }
           // _prevList.Add(_currentPlayingIndex);
            _prevListName.Add(Path.GetFileNameWithoutExtension(_currentPlaying));
            _prevListFullPathName.Add(_currentPlaying);
        }
        private void ChangeCurrentPlay(int current)
        {
            if (mePlayer.Source != null)
            {
                for (var i = 0; i < _mediaList.Count; i++)
                {
                    ListViewItem row = PlayListView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                    if (row!=null)
                    row.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryLightBrush"].ToString());
                }
                ListViewItem rows = PlayListView.ItemContainerGenerator.ContainerFromIndex(current) as ListViewItem;
                if (rows != null) rows.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryDarkForegroundBrush"].ToString());
            }
            if (File.Exists(_mediaList[current].File_Path))
            {
                updatePreList();
               
                _currentPlaying = _mediaList[current].File_Path;
                Debug.WriteLine("player" +_currentPlaying);
              
                _currentPlaying = _mediaList[current].File_Path;
                _currentPlayingIndex = current;

                mePlayer.Source = new Uri(_mediaList[current].File_Path);
                //_ShowBackground.Open(new Uri(_currentPlaying));
                mePlayer.Position = TimeSpan.FromSeconds(_mediaList[current].NowDurationLength);
                if (mediaPlayerIsPlaying == true) mePlayer.Play();
                else mePlayer.Pause();
                sliProgress.Value = _mediaList[current].NowDurationLength;
                lblProgressStatus.Text = _mediaList[current].NowDuration;

                ChangeColorBackGround(_mediaList[current].imageReview);
                Debug.WriteLine("path_", _mediaList[current].imageReview);
                if (IsAudioFile(_mediaList[current].File_Path))
                {
                    AudiaPlayer.Visibility = Visibility.Visible;
                    Debug.WriteLine("hiện+++++++++++++++++++++++++++++++++");
                }
                else
                {
                    AudiaPlayer.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Media not found\n Remove it", "notification");
                _mediaList.RemoveAt(current);
            }

            
        }
        private void Repeat()
        {
            _mediaList[_currentPlayingIndex].NowDurationLength = 0;
            mePlayer.Position = TimeSpan.FromSeconds(0);
            if (mediaPlayerIsPlaying == true) mePlayer.Play();
            else mePlayer.Pause();
            sliProgress.Value = _mediaList[_currentPlayingIndex].NowDurationLength;
            lblProgressStatus.Text = _mediaList[_currentPlayingIndex].NowDuration;

        }
        
        private void add_Audio_image(string path)
        {
            playlistIsChange = true;
            _add = null;
            _add = new Media();
            var bitmap = new BitmapImage(new Uri("images/play.png", UriKind.Relative));
            _add.imageReview = bitmap;
            MediaPlayer mediaPlayer = new MediaPlayer();
            if (!File.Exists(path)) return;
            mediaPlayer.MediaFailed += (o, args) =>
            {
                MessageBox.Show("Media Failed!!");
            };
            mediaPlayer.Open(new Uri(path));
            while (!mediaPlayer.NaturalDuration.HasTimeSpan) ;
            var time = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            _add.Duration_length = time;
            _add.NowDurationLength = 0;
            _add.File_Path = path;
            _mediaList.Add(_add);
            mediaPlayer = null;

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
            //</ draw video_image >
          
            //< set Image >
        }
        private void ChangeColorBackGround(ImageSource bmp)
        {
            if (IsAudioFile(_currentPlaying)) return;
            CroppedBitmap cb = new CroppedBitmap(bmp as BitmapSource,
            new Int32Rect(2, 2, 1, 1));
            var pixels = new byte[4];
            try
            {
                cb.CopyPixels(pixels, 4, 0);
            }
            catch (Exception ex)
            {
                
            }
            Console.WriteLine(pixels[0] + ":" + pixels[1] +
                              ":" + pixels[2] + ":" + pixels[3]);
            //var x = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
            RGB.Top = Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            int x, y;
        
            cb = new CroppedBitmap(bmp as BitmapSource,
            new Int32Rect(60,80 , 1, 1));
            
            pixels = new byte[4];
            try
            {
                cb.CopyPixels(pixels, 4, 0);
            }
            catch (Exception ex)
            {
            }
            RGB.Bottom= Color.FromRgb(pixels[2], pixels[1], pixels[0]);
            //MidColor.DataContext = RGB;
            TopRGB.Color = RGB.Top;
            BottomRGB.Color = RGB.Bottom;
           

        }

        private void add_Video_Image(string sFullname_Path_of_Video)
        {
            playlistIsChange = true;
            _add = null;
            _add = new Media();
           
            Debug.WriteLine(sFullname_Path_of_Video);
            MediaPlayer? mediaPlayer = new MediaPlayer();
           //mediaPlayer.MediaOpened += new EventHandler(mediaplayer_OpenMedia);
            mediaPlayer.MediaFailed += (o, args) =>
            {
                MessageBox.Show("Media Failed!!");
            };
            mediaPlayer.ScrubbingEnabled = true;
            mediaPlayer.Open(new Uri(sFullname_Path_of_Video));
            mediaPlayer.Position = TimeSpan.FromSeconds(0);

            while (!mediaPlayer.NaturalDuration.HasTimeSpan);
            
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var time = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                _add.Duration_length = time;
                Debug.WriteLine("string", _add.DurationString);
                _add.NowDurationLength = 0.0;
            }

            _add.File_Path = sFullname_Path_of_Video;
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawVideo(mediaPlayer, new Rect(0, 0, 160, 100));
            drawingContext.Close();
            Thread.Sleep(200);
            double dpiX = 1 / 200;
            double dpiY = 1 / 200;
            
            RenderTargetBitmap bmp = new RenderTargetBitmap(160, 100, dpiX, dpiY, PixelFormats.Pbgra32);
            
            bmp.Render(drawingVisual);
            
            _add.imageReview = bmp;

            _mediaList.Add(_add);
            mediaPlayer = null;

        }

        private void Forward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Replay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Forward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (sliProgress.Value + 5 > _mediaList[_currentPlayingIndex].Duration_length) 
            {
                sliProgress.Value = _mediaList[_currentPlayingIndex].Duration_length;
                if (autoplay == false) { return; }
                playNextMeda();
            }
            else  
            sliProgress.Value +=5;
        }

        private void Replay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            double value = sliProgress.Value - 5;

            TimeSpan newPosition = TimeSpan.FromSeconds(value);
            if (newPosition.TotalSeconds <0) return;
            lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
            _mediaList[_currentPlayingIndex].NowDurationLength = value;
            mePlayer.Position = newPosition;
        }
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Debug.WriteLine("playyyyyy");
            Debug.WriteLine(_mediaList[_currentPlayingIndex].NowDurationLength);
            if (mediaPlayerIsPlaying)
            {
                mePlayer.Pause();
                mediaPlayerIsPlaying = false;
                PlayBtn.Visibility = Visibility.Visible;
                PauseBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                mePlayer.Play();
                mediaPlayerIsPlaying = true;
                PauseBtn.Visibility = Visibility.Visible;
                PlayBtn.Visibility = Visibility.Collapsed;
            }
            Debug.WriteLine(_mediaList[_currentPlayingIndex].NowDurationLength);
            Debug.WriteLine($"position:{mePlayer.Position}");
            mePlayer.Position =TimeSpan.FromSeconds( _mediaList[_currentPlayingIndex].NowDurationLength);
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
            
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!mediaPlayerIsPlaying) return;
            mePlayer.Pause();
            mediaPlayerIsPlaying = false;
            //PlayBtn.Visibility = Visibility.Visible;
            //PauseBtn.Visibility = Visibility.Collapsed;
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
        private void Record_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Record_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            double value = speedup;
            switch (speedup)
            {
                case 0.25: value =0.5; break;
                case 0.5: value =1; break;
                case 1: value =1.25; break;
                case 1.25: value = 1.5; break;
                case 1.5: value = 2; break;
                case 2: value = 0.25; break;
            }
            SpeedUpValue.Text = $"{value}x";
            speedup = value;
            mePlayer.SpeedRatio = value;

        }
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
            CanvasSeeking.Visibility = Visibility.Visible;
            if (IsAudioFile(_currentPlaying)) imageSeeking.Visibility = Visibility.Collapsed;
            else imageSeeking.Visibility = Visibility.Visible;
            nowPosion = sliProgress.Value;
            textSeeking.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            _Slider = null;
            _Slider = new MediaPlayer();
            //----------------< add_Video_Image() >----------------
            //*create mediaplayer in memory and jump to position
            if (_currentPlaying != null)
            {
                _Slider.Open(new Uri(_currentPlaying));
                _Slider.ScrubbingEnabled = true;
                
                _Slider.Position = TimeSpan.FromSeconds(sliProgress.Value);

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

        private RenderTargetBitmap DrawImage()
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawVideo(_Slider, new Rect(0, 0, 120, 90));
            drawingContext.Close();
            double dpiX = 1 / 200;
            double dpiY = 1 / 200;
            RenderTargetBitmap bmp = new RenderTargetBitmap(120, 90, dpiX, dpiY, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_currentPlaying == "") return;
            if (userIsDraggingSlider)
            {
                lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
                textSeeking.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
                _Slider.Position = TimeSpan.FromSeconds(sliProgress.Value);
                var posision = Mouse.GetPosition(Application.Current.MainWindow);
                if (IsVideoFile(_currentPlaying))
                {
                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    drawingContext.DrawVideo(_Slider, new Rect(0, 0, 120, 90));
                    drawingContext.Close();
                    double dpiX = 1 / 200;
                    double dpiY = 1 / 200;
                    RenderTargetBitmap bmp = new RenderTargetBitmap(120, 90, dpiX, dpiY, PixelFormats.Pbgra32);

                    bmp.Render(drawingVisual);
                    imageSeeking.Source = bmp;
                    Canvas.SetLeft(imageSeeking, posision.X - 50);
                }
               
               
                Canvas.SetLeft(textSeeking, posision.X - 30);
            }
            else
            {
                double value = sliProgress.Value;
                TimeSpan newPosition = TimeSpan.FromSeconds(value);
                lblProgressStatus.Text=newPosition.ToString(@"hh\:mm\:ss");
                _mediaList[_currentPlayingIndex].NowDurationLength = value;
                mePlayer.Position = newPosition;

                //_ShowBackground.Position=newPosition;
                //DrawingVisual drawingVisual = new DrawingVisual();
                //DrawingContext drawingContext = drawingVisual.RenderOpen();
                //drawingContext.DrawVideo(_ShowBackground, new Rect(0, 0, 120, 90));
                //drawingContext.Close();
                //double dpiX = 1 / 200;
                //double dpiY = 1 / 200;
                //RenderTargetBitmap bmp = new RenderTargetBitmap(120, 90, dpiX, dpiY, PixelFormats.Pbgra32);

                //bmp.Render(drawingVisual);
                //ImageSource img = bmp;
                //ChangeColorBackGround(img);
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
        private void addShuffle(int next)
        {
            if (shuffleIndex == _shuffleList.Count - 1)
                _shuffleList.Add(next);
            shuffleIndex += 1;
        }
        private void playNextMeda()
        {
            if (mediaPlayerIsShuffling == true)
            {
                int next = nextPlay(_currentPlayingIndex);
                addShuffle(next);
                Debug.WriteLine("is next: {0}", next);
                if (next != -1)
                {
                    if (_mediaList[next].NowDurationLength + 1 > _mediaList[next].Duration_length)
                    {
                        Debug.WriteLine("check {0}", next);
                        _currentPlayingIndex = next;
                        Repeat();
                    }
                    else
                    {
                        updatePreList();
                        ChangeCurrentPlay(next);
                    }
                }
            }
            else
            if (mediaPlayerIsRepeat == 0)
            {
                mediaPlayerIsRepeat = -1;
                RepeatIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOff;
                Repeat();
            }
            else if (mediaPlayerIsRepeat == 1)
            {
                Repeat();
            }
            else
            {
                int next = nextPlay(_currentPlayingIndex);
                if (next != -1)
                {
                    updatePreList();
                    ChangeCurrentPlay(next);
                }
                //else here: handing cacs kieeux:v

            }
        }
        private void player_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (autoplay == false) { return; }
            playNextMeda();
        }
        private void ChooseToPlay(object sender, MouseButtonEventArgs e)
        {
            var x = PlayListView.SelectedIndex;
            if (x != -1)
            {
                _shuffleList.Clear();
                shuffleIndex = -1;
                Debug.WriteLine("current play " + _currentPlayingIndex.ToString());
                _mediaList[_currentPlayingIndex].NowDurationLength = sliProgress.Value;
                updatePreList();
                ChangeCurrentPlay(x);
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
                if (checkHavingFile(d) == true)
                {
                    //handle here
                }
                else
                {
                    if (IsVideoFile(d))
                    {
                        add_Video_Image(d);
                        if (mePlayer.Source == null)
                        {
                            AudiaPlayer.Visibility = Visibility.Collapsed;
                            _currentPlaying = d;
                            _currentPlayingIndex = 0;
                            ChangeColorBackGround(_mediaList[_mediaList.Count - 1].imageReview);
                            ListViewItem rows = PlayListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                            if (rows != null) rows.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryDarkForegroundBrush"].ToString());
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
                            AudiaPlayer.Visibility = Visibility.Visible;
                            _currentPlayingIndex = 0;
                            ListViewItem rows = PlayListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                            if (rows != null) rows.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryDarkForegroundBrush"].ToString());
                            Debug.WriteLine("ok");
                            mePlayer.MediaFailed += (o, args) =>
                            {
                                MessageBox.Show("Media Failed!!");
                            };
                        }
                    }
                }
            }
        }
        private void SetPrimaryColor(Color color)
        {
           
            PaletteHelper paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
        
            paletteHelper.SetTheme(theme);
            var isDark = true; 
            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            paletteHelper.SetTheme(theme);
            
        }
        private void AddFiles(object sender, RoutedEventArgs e)
        {
            SetPrimaryColor(Colors.MediumAquamarine);
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

        private void HiddenBtn(object sender, RoutedEventArgs e)
        {
            if (PlayListStackPannel.Visibility == Visibility.Visible)
            {
                PlayListStackPannel.Visibility = Visibility.Collapsed;
                ShowHideDisplay.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronLeft;
            }
            else
            {
                PlayListStackPannel.Visibility = Visibility.Visible;
                ShowHideDisplay.Kind = MaterialDesignThemes.Wpf.PackIconKind.ChevronRight;

            }
        }

        private void hoverEvent(object sender, MouseEventArgs e)
        {
            var send = sender as StackPanel; if (send != null) {
                Debug.WriteLine(send.GetType());
            send.Visibility = Visibility.Visible;
            }
        }
        //current=index of playlist
       private void SetUp()
        {
            mediaPlayerIsPlaying = false;
            userIsDraggingSlider = false;
            PlayBtn.Visibility = Visibility.Visible;
            PauseBtn.Visibility = Visibility.Collapsed;
            _currentPlaying = string.Empty;
            _currentPlayingIndex = 0;
            mePlayer.Source = null;
            sliProgress.Value = 0;
            lblProgressStatusEnd.Text = "00:00:00";
        }
        private void DeleteMovie(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = PlayListView.Items.IndexOf(item);
            if (index != -1)
            {
                playlistIsChange= true;
                _mediaList.RemoveAt(index);

                //nếu mà xóa hết rùi thì reset
                if (_mediaList.Count == 0)
                {
                    SetUp();
                    return;
                }
                if (index == _currentPlayingIndex)
                {
                    //nếu như xóa hết thì chả còn gì, mà xóa còn thì change bằng next
                    //nếu next éo đc thì prev nếu prev
                    //éo được thì thằng đầu list
                    var next = nextPlay(_currentPlayingIndex);
                    if (next != -1)
                    {
                        ChangeCurrentPlay(next);
                    }
                    else
                    {
                        var prev = prevPlay(_currentPlayingIndex);
                        if (prev != -1) { ChangeCurrentPlay(prev); }
                        else
                        ChangeCurrentPlay(0);
                    }
                }
            }
        }
        private void ChangeUIRepeat()
        {
            if (mediaPlayerIsRepeat == -1)
            {
                RepeatIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOff;
            }
            else if (mediaPlayerIsRepeat == 0)
            {
                mediaPlayerIsShuffling = false;
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ShuffleDisabled;
                RepeatIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOnce;
            }
            else
            {
                mediaPlayerIsShuffling = false;
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ShuffleDisabled;
                RepeatIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Repeat;
            }
        }

        private void RepeatMode(object sender, RoutedEventArgs e)
        {
          mediaPlayerIsRepeat =mediaPlayerIsRepeat==1?-1:mediaPlayerIsRepeat+1;
            ChangeUIRepeat();
          
        }

        //sửa cái này:v
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //playlistIsChange
            if (_mediaList.Count != 0)
            {
                e.CanExecute = true;
            }
        }
        private bool openPathToSave()
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "Plt";
            dialog.Filter = "Playlist (*.Plt)|*.Plt";

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }
            playlistPath = dialog.FileName;
            Nameplaylist.Text = Path.GetFileNameWithoutExtension(playlistPath);
            return true;
        }
        private void SaveAsPlaylist()
        {
            StreamWriter output;
            if (playlistPath == "") return;
            output = new StreamWriter(playlistPath);
            StringBuilder writeFile = new StringBuilder();
            for (int i = 0; i < _mediaList.Count; i++)
            {
                writeFile.AppendLine(_mediaList[i].File_Path);
            }
            Debug.WriteLine(writeFile.ToString());
            output.WriteLine(writeFile.ToString());
            output.Close();
            playlistIsChange = false;
            MessageBox.Show("Complete!", "Notication");
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (playlistPath == string.Empty)
            {
                var check=openPathToSave();
                if (check == false) return;
            }
            SaveAsPlaylist();

        }

        private void Next_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = checkNext();
        }

        private void Next_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int next = nextPlay(_currentPlayingIndex);
            Debug.WriteLine("next_excuted: {0}",next);
            if (next != -1)
            {
                if (mediaPlayerIsShuffling == true)
                {
                    addShuffle(next);
                    if (_mediaList[next].DurationString == _mediaList[next].NowDuration)
                    {
                        _currentPlayingIndex = next;
                        _currentPlaying = _mediaList[next].File_Path;
                        mePlayer.Source = new Uri(_currentPlaying);
                        //_ShowBackground.Open(new Uri(_currentPlaying));
                        Repeat();
                    }
                    else
                    {

                        ChangeCurrentPlay(next);
                    }
                }
                ChangeCurrentPlay(next);
            }
        }
        private void Prev_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = checkPrev();
        }

        private void Prev_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int prev = prevPlay(_currentPlayingIndex);
            shuffleIndex -= 1;
            Debug.WriteLine("pre={0}",prev);
            ChangeCurrentPlay(prev);
        }

        private void ChangeUIShuffle()
        {
            if (mediaPlayerIsShuffling == true)
            {
                mediaPlayerIsRepeat = -1;
                RepeatIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOff;
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Shuffle;
                _shuffleList.Clear();
                _shuffleList.Add(_currentPlayingIndex);
                shuffleIndex = 0;
            }
            else
            {
                ShuffleIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ShuffleDisabled;
            }
        }
        private void clickShuffleBtn(object sender, RoutedEventArgs e)
        {
            mediaPlayerIsShuffling = !mediaPlayerIsShuffling;
            ChangeUIShuffle();
        }

        private void Open_Playlist(object sender, RoutedEventArgs e)
        {
            SaveOldPlaylist();
            var dialog = new OpenFileDialog();
            dialog.Filter = "Playlist (*.Plt)|*.Plt";
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            _mediaList.Clear();
            shuffleIndex = -1;
            _shuffleList.Clear();
            mediaPlayerIsPlaying = false;
            PlayBtn.Visibility = Visibility.Visible;
            PauseBtn.Visibility = Visibility.Collapsed;
            userIsDraggingSlider = false;
            _currentPlaying = string.Empty;
            _currentPlayingIndex = 0;
            mePlayer.Source = null;
            playlistPath = dialog.FileName;
            Nameplaylist.Text = Path.GetFileNameWithoutExtension(playlistPath);
            StreamReader input;
            input = new StreamReader(playlistPath);
            progressbarLoadmedia.Visibility = Visibility.Visible;
            string path = "";
            while ((path = input.ReadLine()) != null)
            {
                if (path != "")
                {
                    if (!File.Exists(path)) continue;
                    
                    Debug.WriteLine("current+" + path);
                    if (IsVideoFile(path))
                    {
                        add_Video_Image(path);
                      
                    }
                    else
                    {
                        add_Audio_image(path);
                    }
                    if (mePlayer.Source == null)
                    {
                        _currentPlaying = path;
                        Debug.WriteLine("path", _currentPlaying);
                        mePlayer.Source = new Uri(_currentPlaying);
                        //_ShowBackground.Open(new Uri(_currentPlaying));
                        mePlayer.Position = TimeSpan.FromSeconds(0);
                        mePlayer.Play();
                        mePlayer.Stop();
                        _currentPlayingIndex = 0;
                        ListViewItem rows = PlayListView.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;
                        if (rows != null) rows.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryDarkForegroundBrush"].ToString());
                        Debug.WriteLine("ok");
                        if (IsAudioFile(path)) AudiaPlayer.Visibility = Visibility.Visible;
                        else AudiaPlayer.Visibility = Visibility.Collapsed;
                        mePlayer.MediaFailed += (o, args) =>
                        {
                            MessageBox.Show("Media Failed!!");
                        };
                    }
                }
            }
            Debug.WriteLine(_mediaList.Count);
            input.Close();
            progressbarLoadmedia.Visibility = Visibility.Collapsed;
            playlistIsChange = false;
            MessageBox.Show("Doc file xong");
        }

        private void AutoPlay()
        {
           
            Debug.WriteLine(autoplay);
            if (autoplay == true)
            {
                AutoPlayToggleBtn.IsChecked = true;


                if (sliProgress.Value + 1 > sliProgress.Maximum)
                {
                    playNextMeda();
                }
            }
            else
            {
                AutoPlayToggleBtn.IsChecked = false;

            }
        }
        private void ChangeAutoPlay(object sender, RoutedEventArgs e)
        {
            autoplay = !autoplay;
            AutoPlay();
        }

        private void DisplaySlider(object sender, MouseEventArgs e)
        {
            if (mePlayer.Source == null) return;
        
            SliderPosition.Visibility = Visibility.Visible;
            
        }

        private void NoneDisplaySlider(object sender, MouseEventArgs e)
        {
            
                SliderPosition.Visibility = Visibility.Collapsed;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OutlinedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = OutlinedComboBox.SelectedIndex-1;
            if (index <-1) return;
            if (index == -1)
            {
                _prevList.Clear();
                _prevListName.Clear();
                _prevListName.Add("Remove all");
                _prevListFullPathName.Clear();
                return;
             }
            if (_mediaList.Count != 0)
            {
                MessageBoxResult choice = (MessageBoxResult)MessageBox.Show("Create new playlist to add this?", "Choose", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.OKCancel);
                if (choice == MessageBoxResult.OK)
                {
                    SaveOldPlaylist();
                    CreateNewPlayList();
                }
            }
            var path = _prevListFullPathName[index];
            if (checkHavingFile(path) == true)
            {
                for (var i=0;i<_mediaList.Count;i++)
                {
                    if (string.Compare(path, _mediaList[i].File_Path) == 0) {
                        ChangeCurrentPlay(i);
                        break;
                    }
                }
            }
            else
            {
                if (IsVideoFile(path))
                {
                    add_Video_Image(path);
                }
                else { add_Audio_image(path);}
                if (_mediaList.Count != 1 || mePlayer.Source==null)
                    ChangeCurrentPlay(_mediaList.Count - 1);
            }
            playlistIsChange = true;
        }

        private void CreateNewPlayList()
        {
            Nameplaylist.Text = "";
            _mediaList.Clear();
            shuffleIndex = -1;
            _shuffleList.Clear();
            mediaPlayerIsPlaying = false;
            PlayBtn.Visibility = Visibility.Visible;
            PauseBtn.Visibility = Visibility.Collapsed;
            userIsDraggingSlider = false;
            _currentPlaying = string.Empty;
            _currentPlayingIndex = 0;
            mePlayer.Source = null;
            playlistPath = string.Empty;
            playlistIsChange= false;
        }
        private void SaveOldPlaylist()
        {
            if (playlistIsChange == true && _mediaList.Count!=0)
            {
                if (playlistPath != string.Empty)
                {
                    MessageBoxResult choice1 = (MessageBoxResult)MessageBox.Show("Play list has been change, Save as?", "Choose", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.OKCancel);
                    if (choice1 == MessageBoxResult.OK)
                    {
                        SaveAsPlaylist();
                    }
                }
                else
                {
                    MessageBoxResult choice1 = (MessageBoxResult)MessageBox.Show("Do you want to save the playlist?", "Choose", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.OKCancel);
                    if (choice1 == MessageBoxResult.OK)
                    {
                        openPathToSave();
                        SaveAsPlaylist();
                    }
                }
            }
        }
        private void NewPlaylist(object sender, RoutedEventArgs e)
        {
            SaveOldPlaylist();
            CreateNewPlayList();
        }

        private void saveprevlist()
        {
            StreamWriter output;
            output = new StreamWriter(saveRecentFile);
            StringBuilder writeFile = new StringBuilder();
            foreach (var i in _prevListFullPathName)
            {
                if (!File.Exists(i)) continue;
                writeFile.AppendLine(i);
            }
            Debug.WriteLine(writeFile.ToString());
            output.WriteLine(writeFile.ToString());
            output.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //save recent files
            SaveConfig();
            updatePreList();
            saveprevlist();
            
        }

        private void ChangeUiVolume()
        {
            VolumnMute.Visibility = Visibility.Collapsed;
            VolumnNotMute.Visibility = Visibility.Collapsed;
            pbVolume.Visibility = Visibility.Visible;
            if (mePlayer.Volume == 0)
            {
                pbVolume.Visibility = Visibility.Collapsed;
                VolumnMute.Visibility = Visibility.Visible;
            }
            else
            {
                VolumnNotMute.Visibility = Visibility.Visible;
                if (mePlayer.Volume <= 0.33) { VolumnItem.Kind = PackIconKind.VolumeLow; return; }
                if (mePlayer.Volume <= 0.66) { VolumnItem.Kind = PackIconKind.VolumeMedium; return; }
                if (mePlayer.Volume > 0.66) { VolumnItem.Kind = PackIconKind.VolumeHigh; return; }
            }
        }
        private void VolumnChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var value = pbVolume.Value;
            mePlayer.Volume = value;
            ChangeUiVolume();
           
        }

        private void Mute(object sender, RoutedEventArgs e)
        {
            pbVolume.Value = 0;
           
        }

        private void UnMute(object sender, RoutedEventArgs e)
        {
            pbVolume.Value = 0.5;
           
        }

        private void PopupBox_OnOpened(object sender, RoutedEventArgs e)
        {

        }

        private void PopupBox_OnClosed(object sender, RoutedEventArgs e)
        {

        }

        private void SpeedChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double value = Math.Round( SpeedSlider.Value,2);
            speedup = value;
            SpeedUpValue.Text = $"{value}x";
            mePlayer.SpeedRatio = value;
        }

        private void CheckingMouseLeave(object sender, MouseEventArgs e)
        {
            if(userIsDraggingSlider==true)
            {
                userIsDraggingSlider = false;
                mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
                _mediaList[_currentPlayingIndex].NowDurationLength = sliProgress.Value;
                CanvasSeeking.Visibility = Visibility.Collapsed;
                lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
            }
          
        }
        private void Screen_nameChanged(string newpath,int index)
        {
           
            _mediaList[index].File_Path = newpath;
        }
        private void RenameMovie(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
           
            int index = PlayListView.Items.IndexOf(item);
            Debug.WriteLine(index);
            if (index != -1)
            {
                var name = _mediaList[index].File_Path;
                var screen = new ChangeName(name,index);
                screen.NameChangedEvent += Screen_nameChanged;
                if (screen.ShowDialog() == true)
                {
                    if (name == _mediaList[index].File_Path) return;
                    for (int i = 0; i < _prevListFullPathName.Count; i++)
                    {
                        if (_prevListFullPathName[i] == name)
                        {
                            _prevListFullPathName[i] = _mediaList[index].File_Path;
                            _prevListName[i+1] = Path.GetFileNameWithoutExtension(_mediaList[index].File_Path);
                        }
                    }
                    File.Move(name, _mediaList[index].File_Path);
                  
                }
                else
                {
                    _mediaList[index].File_Path = name;
                    Debug.WriteLine("back");
                }
            }

        }
    }
}

