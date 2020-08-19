using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SyncAudio
{
    public partial class MainWindow : Window
    {
        // Songs Folder Path's.
        string path = ("file://" + System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Songs/").Replace('\\', '/');
        string localPath = "Songs/";

        // Variables
        int index = 0;
        int hours, minutes, seconds;
        bool keyCooldown;
        double volumeSave = 0.3;

        bool songPlaying = false;
        bool songPaused = false;
        bool trackMode = false;

        string[] duration;

        List<string> songList = new List<string>();
        MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            FindSongs();

            // Set path for tracks, show error if 'Songs' folder doesn't exists.
            try {
                player.Open(new Uri(path + songList[index]));
            } catch (Exception) {
                MessageBox.Show("Can't find 'Songs' Folder.");
            }

            // Set UI values to elements.
            slider.Value = 1;
            player.Volume = slider.Value;
            volumeName.Content = CalculateVolume().ToString() + "%";
            Textbox1.Text += "**************************************\n";

            ChangeTrackMode();
        }

        // Methods
        public void FindSongs()
        { // Search for Songs if 'Songs' folder exists and put the titles in 'songList'.

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
        { // Start or stop player.

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
        { // Show runtime of track and status.

            try {
                while (songPlaying == true) {
                    switch (songPlaying) {
                        case true:
                            Dispatcher.Invoke(() => {
                                duration = player.Position.ToString().Split('.');
                                switch (songPaused) {
                                    case true:
                                        Textbox1.Text = "Playing: " + songList[index] + "  |  " + duration[0] + "  |  " + "Paused";
                                        break;
                                    case false:
                                        Textbox1.Text = "Playing: " + songList[index] + "  |  " + duration[0];
                                        break;
                                }
                            });
                            break;
                    }
                    Thread.Sleep(200);
                }
            } catch (Exception e) {
                Dispatcher.Invoke(() => {
                    Textbox1.Text = e.ToString();
                    player.Stop();
                });
            }
        }

        public void CalculateTime()
        { // Set starttime for the next track

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
        { // Change track mode

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
        { // Go to next track.

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
        { // Go to previous track.

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
        { // Calculate and Convert volume of slider and return percentage value.

            return Convert.ToInt32(slider.Value / 1 * 100);
        }

        public void PauseSong()
        { // Pause or play player.

            if (songPaused == false) {
                songPaused = true;
                Pause.Content = "Play";
                player.Pause();

            } else {
                songPaused = false;
                Pause.Content = "Pause";
                player.Play();
            }
        }

        public void Cooldown()
        { // Cooldown time for shortcuts.
            Task.Run(() => {
                keyCooldown = true;
                for (int i = 0; i < 2; i++) {
                    Thread.Sleep(1000);
                }
                keyCooldown = false;
            });
        }


        // UI Elements
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

            // Skip track cooldown
            Task.Run(() => {
                Dispatcher.Invoke(() => {
                    Prev.IsEnabled = false;
                });
                for (int i = 2; i > 0; i--) {
                    Dispatcher.Invoke(() => {
                        Prev.Content = i.ToString();
                    });
                    Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() => {
                    Prev.Content = "Next Track";
                    Prev.IsEnabled = true;
                });
            });
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        { // Show Shortcuts.

            MessageBox.Show("Shortcuts:\n\nEnter = Play/Stop Player\nSpace = Pause Track\n\nP = next Track\nO = Previous Track\n\nArrow Up = Volume Up\nArrow Down = Volume Down\n\nM = Change Track Mode\nOff = New Track begins from beginning\nOn = New Track begins where the Previous ended");
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextSong();

            // Skip track cooldown.
            Task.Run(() => {
                Dispatcher.Invoke(() => {
                    Next.IsEnabled = false;
                });
                for (int i = 2; i > 0; i--) {
                    Dispatcher.Invoke(() => {
                        Next.Content = i.ToString();
                    });
                    Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() => {
                    Next.Content = "Next Track";
                    Next.IsEnabled = true;
                });
            });

        }

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        { // Shortcuts when window is focused.

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
            if (keyCooldown == false) {
                if (e.Key == Key.P) {
                    NextSong();
                    Cooldown();
                } else if (e.Key == Key.O) {
                    PrevSong();
                    Cooldown();
                }
            }

            if (e.Key == Key.M) {
                ChangeTrackMode();
            }
        }
    }
}
