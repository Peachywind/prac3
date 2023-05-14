using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;
using MaterialDesignThemes.Wpf;


namespace planirvshik
{
    public partial class MainWindow : Window
    {
        private string songListFilePath = "songList.json";
        private List<string> songs = new List<string>();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private Thread sliderThread;
        private bool isLooping = false;
        private bool isShuffle = false;
        int a = 1;
        int b = 1;
        int y = 1;
        public MainWindow()
        {
            InitializeComponent();
            LoadSongs();
            AllSong.ItemsSource = songs;

            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            sliderThread = new Thread(new ThreadStart(UpdateSliderPosition));
            sliderThread.IsBackground = true;
            sliderThread.Start();

        }

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            MusicSlider.Minimum = 0;
            MusicSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            MusicSlider.Value = mediaPlayer.Position.TotalSeconds;

            // отобразить длительность трека в секундах
            TimeSpan duration = mediaPlayer.NaturalDuration.TimeSpan;
            songDuration.Text = $"{duration.Minutes:00}:{duration.Seconds:00}";

        }


        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            int currentIndex = AllSong.SelectedIndex;

            if (isLooping)
            {
                mediaPlayer.Position = TimeSpan.Zero;
                mediaPlayer.Play();
            }
            else if (isShuffle)
            {
                Random random = new Random();
                int index = random.Next(songs.Count);
                AllSong.SelectedIndex = index;
                mediaPlayer.Open(new Uri(songs[index]));
                mediaPlayer.Play();
            }
            else
            {
                RightButton();
            }
        }






        private void AddSongs_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Music Files (*.mp3;*.wav;*.wma;*.flac)|*.mp3;*.wav;*.wma;*.flac|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    songs.Add(filename);
                }
                AllSong.Items.Refresh();
                SaveSongs();
            }
        }

        private void LoadSongs()
        {
            if (File.Exists(songListFilePath))
            {
                string json = File.ReadAllText(songListFilePath);
                songs = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }

        private void SaveSongs()
        {
            string json = JsonConvert.SerializeObject(songs);
            File.WriteAllText(songListFilePath, json);
        }

        private void AllSong_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedSong = (string)AllSong.SelectedItem;
            if (selectedSong != null && File.Exists(selectedSong))
            {
                mediaPlayer.Open(new Uri(selectedSong));
                mediaPlayer.Play();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AllSong.SelectedItem != null)
            {
                string selectedSong = AllSong.SelectedItem.ToString();
                songs.Remove(selectedSong);
                SaveSongs();
                LoadSongs();
                AllSong.Items.Refresh();
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedSong = (string)AllSong.SelectedItem;
            if (a % 2 == 0)
            {

                if (selectedSong != null && File.Exists(selectedSong))
                {
                    mediaPlayer.Open(new Uri(selectedSong));
                    mediaPlayer.Play();
                }
            }
            else
            {
                mediaPlayer.Stop();
            }

            a++;
        }
         private void RightButton ()
        {

            int currentIndex = AllSong.SelectedIndex;
            if (currentIndex < AllSong.Items.Count - 1)
            {
                AllSong.SelectedIndex++;
                mediaPlayer.Play();
            }
            else if (isLooping)
            {
                AllSong.SelectedIndex = 0;
                mediaPlayer.Play();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
           RightButton();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string selectedSong = (string)AllSong.SelectedItem;
            if (AllSong.SelectedIndex > 0)
            {
                AllSong.SelectedIndex--;
                mediaPlayer.Play();
            }
        }
        private void MusicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer.Source != null)
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(MusicSlider.Value);
            }
        }



        private void UpdateSliderPosition()
        {
            Thread thread = new Thread(_ =>
            {
                while (true)
                {
                        Dispatcher.Invoke(new Action(() =>
                    {
                        if (mediaPlayer.Position.Seconds <= 9)
                        {
                            mediaPlayerTimer.Text = Convert.ToString(mediaPlayer.Position.Minutes) + ":0" + Convert.ToString(mediaPlayer.Position.Seconds);
                        }
                        else
                        {
                            mediaPlayerTimer.Text = Convert.ToString(mediaPlayer.Position.Minutes) + ":" + Convert.ToString(mediaPlayer.Position.Seconds);
                        }
                        MusicSlider.Value = mediaPlayer.Position.TotalSeconds;
                    }));
                    Thread.Sleep(1000);
                }
            }); thread.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sliderThread.Abort();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }   

        private void RepeatButton_Click_3(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Source != null && AllSong.SelectedItem != null)
            {
                if (b % 2 != 0)
                {
                    isLooping = true;
                }

                if (b % 2 == 0)
                {
                    isLooping = false;
                }
                b++;
            }
        }
        private void PlayRandomSong()
        {
            if (mediaPlayer.Source != null && AllSong.SelectedItem != null)
            {
                if (y % 2 != 0)
                {
                    isShuffle = true;
                }

                if (y % 2 == 0)
                {
                    isShuffle = false;
                }
                y++;
            }
        }




        private void RandomButton_Click_3(object sender, RoutedEventArgs e)
        {
            PlayRandomSong();
        }
    }

}