using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SyncAudio
{
    public partial class MainWindow : Window
    {
        string path = ("file://" + System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Songs/").Replace('\\', '/');
        string localPath = "Songs/";
        int index = 0;
        bool songPlaying = false;
        bool songPaused = false;
        string[] duration;
        int hours, minutes, seconds;
        double volumeSave = 0.3;
        bool trackMode = false;

        List<string> songList = new List<string>();
        MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            FindSongs();
            try {
                player.Open(new Uri(path + songList[index]));

            } catch (Exception e) {

                MessageBox.Show("Can't find 'Songs' Folder.");
            }
            slider.Value = 1;
            player.Volume = slider.Value;
            volumeName.Content = CalculateVolume().ToString() + "%";
            Textbox1.Text += "**************************************\n";
            ChangeTrackMode();
        }

        public void FindSongs()
        {
            Textbox1.Clear();
            Textbox1.Text += "**************************************\n";
            Textbox1.Text += "Searching for songs..\n";
            Textbox1.Text += "**************************************\n";

            if (Directory.Exists(localPath)) {
                songList.Clear();
                string[] fileEntries = Directory.GetFiles(localPath);

                foreach (var item in fileEntries) {
                    string[] song = item.Split('/', '.');
                    if (song[2] == "wav" || song[2] == "mp3") {
                        songList.Add(song[1] + "." + song[2]);
                        Textbox1.Text += song[1] + "\n";
                    }
                }
                if (songList.Count > 0) {
                    Textbox1.Text += "**************************************\n";
                    Textbox1.Text += "\t" + songList.Count + " songs found.\n";
                } else {
                    Textbox1.Text += "No Songs found.\n";
                }
            } else {
                Textbox1.Text += "Error: Can't find 'Songs' folder.\n";
            }
        }

        public void StartStopSong()
        {
            switch (songPlaying) {
                case false:
                    Start.Content = "Stop";
                    player.Play();
                    songPlaying = true;
                    Textbox1.Clear();
                    Task.Run(() => Counter());
                    break;
                case true:
                    Start.Content = "Start";
                    player.Stop();
                    songPlaying = false;
                    FindSongs();
                    break;
            }
        }

        public void Counter()
        {
            try {
                while (songPlaying == true) {
                    Textbox1.Dispatcher.Invoke(() => {
                        duration = player.Position.ToString().Split('.');
                        if (songPaused == true) {
                            Textbox1.Text = "Playing: " + songList[index] + "  |  " + duration[0] + "  |  " + "Paused";
                        } else {
                            Textbox1.Text = "Playing: " + songList[index] + "  |  " + duration[0];
                        }
                    });
                    Thread.Sleep(200);
                }
            } catch (Exception e) {
                Textbox1.Dispatcher.Invoke(() => {
                    Textbox1.Text = e.ToString();
                });
                player.Dispatcher.Invoke(() => {
                    player.Stop();
                });
            }
        }

        public void CalculateTime()
        {
            if (trackMode == true) {
                string[] parse = player.Position.TotalSeconds.ToString().Split(',');
                int.TryParse(parse[0], out seconds);
                int.TryParse(player.Position.TotalMinutes.ToString(), out minutes);
                int.TryParse(player.Position.TotalHours.ToString(), out hours);
            } else if (trackMode == false) {
                hours = 0;
                minutes = 0;
                seconds = 0;
            }
        }

        public void ChangeTrackMode()
        {
            if (trackMode == true) {
                trackState.Content = "Off";
                trackState.Foreground = Brushes.Red;
                trackMode = false;
            } else if (trackMode == false) {
                trackState.Content = "On";
                trackState.Foreground = Brushes.Green;
                trackMode = true;
            }
        }

        public void NextSong()
        {
            CalculateTime();

            FindSongs();
            player.Stop();

            if (index < songList.Count - 1) {
                index++;
            } else if (index == songList.Count - 1) {
                index = 0;
            }
            Textbox1.Clear();
            player.Open(new Uri(path + songList[index]));
            player.Position = new TimeSpan(hours, minutes, seconds);
            songPlaying = true;
            songPaused = false;
            Start.Content = "Stop";
            player.Play();
            Task.Run(() => Counter());
        }

        public void PrevSong()
        {
            CalculateTime();

            FindSongs();
            player.Stop();

            if (index != 0) {
                index--;
            } else {
                index = songList.Count - 1;
            }
            Textbox1.Clear();
            player.Open(new Uri(path + songList[index]));
            player.Position = new TimeSpan(hours, minutes, seconds);
            songPlaying = true;
            songPaused = false;
            Start.Content = "Stop";
            player.Play();
            Task.Run(() => Counter());
        }

        public int CalculateVolume()
        {
            return Convert.ToInt32(slider.Value / 1 * 100);
        }

        public void PauseSong()
        {
            if (songPaused == false) {
                songPaused = true;
                songPlaying = true;
                Pause.Content = "Play";
                player.Pause();

            } else {
                songPaused = false;
                songPlaying = false;
                Pause.Content = "Pause";
                player.Play();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartStopSong();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            volumeName.Content = CalculateVolume().ToString() + "%";
            player.Volume = slider.Value;
        }

        private void volumeName_Click(object sender, RoutedEventArgs e)
        {
            if (slider.Value != 0) {
                volumeSave = slider.Value;
                slider.Value = 0;
                volumeName.Content = "Mute";
            } else {
                slider.Value = volumeSave;
            }
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            PauseSong();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            PrevSong();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Shortcuts:\n\nEnter = Play/Stop Player\nSpace = Pause Track\n\nArrow Right = next Track\nArrow Left = Previous Track\n\nArrow Up = Volume Up\nArrow Down = Volume Down\n\nM = Change Track Mode\nOff = New Track begins from beginning\nOn = New Track begins where the Previous ended");
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                StartStopSong();
            } else if (e.Key == Key.Space) {
                PauseSong();
            }

            if (e.Key == Key.Up && player.Volume != 1.00) {
                player.Volume += 0.05;
            } else if (e.Key == Key.Down && player.Volume != 0) {
                player.Volume -= 0.05;
            }

            if (e.Key == Key.Right) {
                NextSong();
            } else if (e.Key == Key.Left) {
                PrevSong();
            }

            if (e.Key == Key.M) {
                ChangeTrackMode();
            }
        }
    }
}
