using System;
using System.Collections.Generic;
using System.IO;
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

        List<string> songList = new List<string>();
        MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            FindSongs();
            player.Open(new Uri(path + songList[index]));

            Textbox1.Text += "**************************************\n";

            if (Keyboard.IsKeyDown(Key.Enter)) {
                switch (songPlaying) {
                    case true:
                        player.Stop();
                        break;
                    case false:
                        player.Play();
                        break;
                }
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
                    if (song[2] == "wav") {
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
                case true:
                    player.Stop();
                    songPlaying = false;
                    FindSongs();
                    break;
                case false:
                    player.Play();
                    songPlaying = true;
                    Textbox1.Clear();
                    Textbox1.Text += "Playing: " + songList[index];
                    break;
            }
        }
        public void StartSong()
        {
            FindSongs();
            player.Stop();

            if (index < songList.Count - 1) {
                index++;
            } else if (index == songList.Count - 1) {
                index = 0;
            }
            Textbox1.Clear();
            Textbox1.Text += "Playing: " + songList[index];

            player.Open(new Uri(path + songList[index]));
            var time = new TimeSpan(0, 3, 0);
            player.Play();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            StartStopSong();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            StartSong();
        }
    }
}
