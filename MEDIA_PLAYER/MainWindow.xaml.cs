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

namespace MEDIA_PLAYER
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private int mediaPlayerIsRepeat = -1;//-1:no repeat;0:repeat:1 ;1:repeat all
        private bool mediaPlayerIsShuffling=false;
        private int shuffleIndex = -1;
        private List<int> _shuffleList = new List<int>();
        private List<int> _prevList = new List<int>();
        private ObservableCollection<string> _prevListName = new ObservableCollection<string>();
        private List<string> _prevListFullPathName = new List<string>();
        private bool autoplay = true;

        private double speedup = 1;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private string _currentPlaying = string.Empty;
        private int _currentPlayingIndex = 0;

        Random rnd = new Random();

        private string playlistPath = string.Empty;

        //add to playlist change this, new playlist..
        private bool playlistIsChange = false;
        ObservableCollection<Media> _mediaList = new ObservableCollection<Media>();
       
        Media _add=new Media();
        MediaPlayer _Slider = new MediaPlayer();
        double nowPosion = 0;
        bool isDark = false;
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
           

     
            DarkMode.IsChecked = isDark;
            Background();


        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayListView.ItemsSource = _mediaList;
            OutlinedComboBox.ItemsSource = _prevListName;
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
                        AudiaPlayer.Visibility = Visibility.Collapsed;
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

                        Debug.WriteLine("ok");
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
            for (var i=0;i<_prevListFullPathName.Count;i++)
            {
                if (string.Compare(_currentPlaying, _prevListFullPathName[i]) == 0)
                {
                    _prevList.RemoveAt(i);
                    _prevListName.RemoveAt(i);
                    _prevListFullPathName.RemoveAt(i);
                    break;
                }
            }
            _prevList.Add(_currentPlayingIndex);
            _prevListName.Add(Path.GetFileNameWithoutExtension(_currentPlaying));
            _prevListFullPathName.Add(_currentPlaying);
        }
        private void ChangeCurrentPlay(int current)
        {
            for (var i=0;i< PlayListView.Items.Count;i++)
            {
                ListViewItem row = PlayListView.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem;
                row.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryLightBrush"].ToString());
            }

            ListViewItem rows = PlayListView.ItemContainerGenerator.ContainerFromIndex(current) as ListViewItem;
            rows.Background = (Brush)new BrushConverter().ConvertFrom(App.Current.Resources["PrimaryDarkForegroundBrush"].ToString());


            if (File.Exists(_mediaList[current].File_Path))
            {
                updatePreList();
                _currentPlaying = _mediaList[current].File_Path;
                if (IsAudioFile(_currentPlaying))
                {
                    AudiaPlayer.Visibility = Visibility.Visible;
                }
                else
                {
                    AudiaPlayer.Visibility = Visibility.Collapsed;
                }
                _currentPlayingIndex = current;
                mePlayer.Source = new Uri(_currentPlaying);
                mePlayer.Position = TimeSpan.FromSeconds(_mediaList[current].NowDurationLength);
                if (mediaPlayerIsPlaying == true) mePlayer.Play();
                else mePlayer.Pause();
                sliProgress.Value = _mediaList[current].NowDurationLength;
                lblProgressStatus.Text = _mediaList[current].NowDuration;
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
        private void add_Video_Image(string sFullname_Path_of_Video)
        {
            playlistIsChange = true;
            _add = null;
            _add = new Media();
            //----------------< add_Video_Image() >----------------
            //*create mediaplayer in memory and jump to position
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
            TempMedia.Source = mediaPlayer.Source;
            while (!mediaPlayer.NaturalDuration.HasTimeSpan);
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                var time = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                _add.Duration_length = time;
                Debug.WriteLine("string", _add.DurationString);
                _add.NowDurationLength = 0.0;
            }

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
            
            _mediaList.Add(_add);
            mediaPlayer = null;


            //----------------</ add_Video_Image() >----------------
        }

        private EventHandler mediaplayer_OpenMedia(string x)
        {
            throw new NotImplementedException();
        }
        private void Forward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Replay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Forward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            double value = sliProgress.Value+5;
          
            TimeSpan newPosition = TimeSpan.FromSeconds(value);
            if (newPosition.TotalSeconds > mePlayer.NaturalDuration.TimeSpan.TotalSeconds) return;
            lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
            _mediaList[_currentPlayingIndex].NowDurationLength = value;
            mePlayer.Position = newPosition;
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
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null)&&(mediaPlayerIsPlaying!=true);
            if (e.CanExecute || mePlayer.Source == null)
            {
                PlayBtn.Visibility = Visibility.Visible;
            }
            else
            {
                PlayBtn.Visibility = Visibility.Collapsed;

            }
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
            if (e.CanExecute)
            {
                PauseBtn.Visibility = Visibility.Visible;
            }
            else
            {
                PauseBtn.Visibility = Visibility.Collapsed;

            }
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

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
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
                            mePlayer.Source = new Uri(d);
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
                            AudiaPlayer.Visibility = Visibility.Visible;

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

        private void DeleteThis(object sender, RoutedEventArgs e)
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
       
        private void DeleteMovie(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = PlayListView.Items.IndexOf(item);
            if (index != -1)
            {
                playlistIsChange= true;
                _mediaList.RemoveAt(index);
                if (index == _currentPlayingIndex)
                {
                    var next = nextPlay(_currentPlayingIndex);
                    if (next != -1)
                    {
                        ChangeCurrentPlay(next);
                    }
                    else
                    ChangeCurrentPlay(0);
                }
            }
        }

        private void RepeatMode(object sender, RoutedEventArgs e)
        {
          mediaPlayerIsRepeat =mediaPlayerIsRepeat==1?-1:mediaPlayerIsRepeat+1;
            
          if (mediaPlayerIsRepeat == -1)
            {
                RepeatIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOff;
            }
            else if (mediaPlayerIsRepeat==0)
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

        private void clickShuffleBtn(object sender, RoutedEventArgs e)
        {
            mediaPlayerIsShuffling = !mediaPlayerIsShuffling;
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

        private void Open_Playlist(object sender, RoutedEventArgs e)
        {
            //lưu lại mấy thằng cũ ở đây:
            
            if (playlistIsChange == true)
            {
                if (playlistPath != string.Empty)
                {
                    MessageBoxResult choice = (MessageBoxResult)MessageBox.Show("Play list has been change, Save as?", "Choose", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.OKCancel);
                    if (choice == MessageBoxResult.OK)
                    {
                        SaveAsPlaylist();
                    }
                }
                else
                {
                    MessageBoxResult choice = (MessageBoxResult)MessageBox.Show("Do you want to save the playlist?", "Choose", (System.Windows.Forms.MessageBoxButtons)MessageBoxButton.OKCancel);
                    if (choice == MessageBoxResult.OK)
                    {
                        openPathToSave();
                        SaveAsPlaylist();
                    }
                }

            }

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
            userIsDraggingSlider = false;
            _currentPlaying = string.Empty;
            _currentPlayingIndex = 0;
          
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
                    Debug.WriteLine("current+" + path);
                    if (IsVideoFile(path))
                    {
                        add_Video_Image(path);
                        AudiaPlayer.Visibility = Visibility.Collapsed;
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
                        mePlayer.Position = TimeSpan.FromSeconds(0);
                        Debug.WriteLine("ok");
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
        }

        private void ChangeAutoPlay(object sender, RoutedEventArgs e)
        {
            autoplay = !autoplay;
            
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

        }
    }
}

