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
    /// <summary>
    /// Comments for what the method's do are inside the method's.
    /// </summary>
    /// 
        // Songs Folder Path's.
        string path = ("file://" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Songs/").Replace('\\', '/');
        string localPath = "Songs/";
        string[] duration;

        // Variables
        int index = 0;
        int timeSeconds;
        bool keyCooldown;
        bool isKeyPressed = false;
        double volumeSave = 0.3;

        bool songPlaying = false;
        bool songPaused = false;
        bool trackMode = false;
        bool threadIsRunning = false;
        bool isThreadRunning = true;

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
                MessageBox.Show("Can't find 'Songs' folder or songs.");
            }

            // Set UI values to elements.
            slider.Value = 1;
            player.Volume = slider.Value;
            volumeName.Content = CalculateVolume().ToString() + "%";
            Textbox1.Text += "**************************************\n";

            ChangeTrackMode();

            // Create thread for shortcuts.
            if (threadIsRunning == false) {
                threadIsRunning = true;

                Thread thread = new Thread(KeyListener);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
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
                    if (song[2] == "wav" || song[2] == "mp3" || song[2] == "m4a") {
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
        { // Set starttime for the next track.

            switch (trackMode) {
                case true:
                    string[] parse = player.Position.TotalSeconds.ToString().Split(',');
                    int.TryParse(parse[0], out seconds);
                    int.TryParse(player.Position.TotalMinutes.ToString(), out minutes);
                    int.TryParse(player.Position.TotalHours.ToString(), out hours);
                    timeSeconds = Convert.ToInt32(player.Position.TotalSeconds);
                    break;
                case false:
                    timeSeconds = 0;
                    break;
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

            player.Position = new TimeSpan(0, 0, timeSeconds);
            player.Open(new Uri(path + songList[index]));
            while (player.Position.TotalSeconds != timeSeconds) {
                player.Position = new TimeSpan(0, 0, timeSeconds);
            }

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

            player.Position = new TimeSpan(0, 0, timeSeconds);
            player.Open(new Uri(path + songList[index]));
            while (player.Position.TotalSeconds != timeSeconds) {
                player.Position = new TimeSpan(0, 0, timeSeconds);
            }

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
        { // Cooldown time for buttons and shortcuts.

            Task.Run(() => {
                keyCooldown = true;
                Dispatcher.Invoke(() => {
                    Next.IsEnabled = false;
                    Prev.IsEnabled = false;
                });
                for (int i = 2; i > 0; i--) {
                    Dispatcher.Invoke(() => {
                        Next.Content = i.ToString();
                        Prev.Content = i.ToString();
                    });
                    Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() => {
                    Next.Content = "Next Track";
                    Next.IsEnabled = true;
                    Prev.Content = "Prev. Track";
                    Prev.IsEnabled = true;
                });
                keyCooldown = false;
            });
        }

        public void KeyListener()
        {   // Shortcuts for the player.

            while (isThreadRunning) {

                Thread.Sleep(50);
                if (Keyboard.IsKeyDown(Key.M) && !isKeyPressed) {
                    Dispatcher.Invoke(ChangeTrackMode);
                    isKeyPressed = true;
                } else if (Keyboard.IsKeyDown(Key.Enter) && !isKeyPressed) {
                    Dispatcher.Invoke(StartStopSong);
                    isKeyPressed = true;
                } else if (Keyboard.IsKeyDown(Key.Up) && !isKeyPressed && Dispatcher.Invoke(() => player.Volume != 1.00)) {
                    this.Dispatcher.Invoke(() => slider.Value = slider.Value + 0.05);
                    isKeyPressed = true;
                    
                } else if (Keyboard.IsKeyDown(Key.Down) && !isKeyPressed && Dispatcher.Invoke(() => player.Volume != 0))  {
                    this.Dispatcher.Invoke(() => slider.Value = slider.Value - 0.05);
                    isKeyPressed = true;
                } else if (Keyboard.IsKeyDown(Key.O) && !isKeyPressed && !keyCooldown) {
                     Dispatcher.Invoke(PrevSong);
                    Dispatcher.Invoke(Cooldown);
                    isKeyPressed = true;
                } else if (Keyboard.IsKeyDown(Key.P) && !isKeyPressed && !keyCooldown) {
                    Dispatcher.Invoke(NextSong);
                    Dispatcher.Invoke(Cooldown);
                    isKeyPressed = true;
                } else if (Keyboard.IsKeyDown(Key.Space) && !isKeyPressed) {
                    Dispatcher.Invoke(PauseSong);
                    isKeyPressed = true;
                } else if (!Keyboard.IsKeyDown(Key.Enter) && !Keyboard.IsKeyDown(Key.M) && !Keyboard.IsKeyDown(Key.Up) && !Keyboard.IsKeyDown(Key.Down) && !Keyboard.IsKeyDown(Key.O) && !Keyboard.IsKeyDown(Key.P) && !Keyboard.IsKeyDown(Key.Space)) {
                    isKeyPressed = false;
                }
            }
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
            CalculateTime();
            PauseSong();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            PrevSong();
            Cooldown();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isThreadRunning = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        { // Show Shortcuts.

            MessageBox.Show("Shortcuts:\n\nEnter = Play/Stop Player\nSpace = Pause Track\n\nP = next Track\nO = Previous Track\n\nArrow Up = Volume Up\nArrow Down = Volume Down\n\nM = Change Track Mode\nOff = New Track begins from beginning\nOn = New Track begins where the Previous ended");
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
            Cooldown();
        }
    }
}
