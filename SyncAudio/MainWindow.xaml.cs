using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        string path = "file://C:/Songs/";
        string localPath = "Songs/";
        int index = 0;
        bool songPlaying = false;
        string[] duration;

        List<string> songList = new List<string>();
        MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            FindSongs();
            player.Open(new Uri(path + songList[index]));

            Textbox1.Text += "**************************************\n";

            
            if (Keyboard.IsKeyDown(Key.P)) {
                NextSong();
            }
            if (Keyboard.IsKeyDown(Key.Up) && player.Volume != 1.00) {
                player.Volume += 0.05;
            } else if (Keyboard.IsKeyDown(Key.Down) && player.Volume != 0) {
                player.Volume -= 0.05;
            }
            if (Keyboard.IsKeyDown(Key.Enter)) {
                StartStopSong();
            }
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
                    player.Play();
                    songPlaying = true;
                    Textbox1.Clear();
                    Task.Run(() => Counter());
                    break;
                case true:
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
                        Textbox1.Text = "Playing: " + songList[index] + "  |  " + duration[0];
                    });
                    Thread.Sleep(1000);
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

        public void NextSong()
        {
            string[] parse = player.Position.TotalSeconds.ToString().Split(',');
            int.TryParse(parse[0], out int seconds);
            int.TryParse(player.Position.TotalMinutes.ToString(), out int minutes);
            int.TryParse(player.Position.TotalHours.ToString(), out int hours);
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
            player.Play();
            Task.Run(() => Counter());
        }
        
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartStopSong();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                StartStopSong();
            }
        }
    }
}
