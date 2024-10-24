using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DungeonCrawlerVideoGame
{

    /// <summary>
    /// Interaction logic for SettingsScreen.xaml
    /// </summary>
    public partial class SettingsScreen : Window
    {
        private int volumeSlider = 1;
        private Button[] _SettingsButton;
        private int _selectedButtonIndex2 = 0;
        MediaPlayer mediaPlayer = new MediaPlayer();

        public SettingsScreen()
        {
            InitializeComponent();

            //this.Loaded += (s, e) => this.Focus(); // Geef focus aan het venster bij laden
            FocusManager.SetFocusedElement(this, MainScreen2);
            KeyDown += SettingsScreen_KeyDown;
            _SettingsButton = new Button[] { cheatcodes, BackToMain };
            this.Closed += SettingMenu_Closed;
            BackGroundMusic();
        }

        private void BackGroundMusic()
        {
            string backGroundMusic = @"E:\Chom-Bombs-\Chom game\Chom game\Assets\Music\Faster_8_Bit_Menu.mp3";

            try
            {
                mediaPlayer.Open(new Uri(backGroundMusic, UriKind.Absolute));  // Open the background music file
                mediaPlayer.MediaOpened += (s, e) =>
                {
                    mediaPlayer.Play();  // Play the music after the file is loaded
                    mediaPlayer.Volume = 0.5;  // Adjust volume if necessary
                    mediaPlayer.MediaEnded += (sender, args) =>
                    {
                        mediaPlayer.Position = TimeSpan.Zero;  // Loop the music
                        mediaPlayer.Play();
                    };
                };
            }
            catch { Console.WriteLine($"Error loading background music"); }
        }

        private void SettingMenu_Closed(object sender, EventArgs e)
        {

            mediaPlayer.Stop();
            mediaPlayer.Close();

        }

        private void SettingsScreen_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    NavigateButtons(-1);
                    break;
                case Key.S:
                    NavigateButtons(1);
                    break;
                case Key.A:
                    if (volumeSlider > 1)
                    {
                        volumeSlider -= 1;
                        SetVolume(volumeSlider / 5.0f); // Zet het volume van 0.0f tot 1.0f
                        volumeSwitch(volumeSlider);
                    }
                    break;
                case Key.D:
                    if (volumeSlider < 5)
                    {
                        volumeSlider += 1;
                        SetVolume(volumeSlider / 5.0f); // Zet het volume van 0.0f tot 1.0f
                        volumeSwitch(volumeSlider);
                    }
                    break;
                case Key.F:
                    HandleEnter();
                    break;
            }
        }

        public void NavigateButtons(int direction)
        {
            ApplyHoverEffect(_SettingsButton[_selectedButtonIndex2], true);


            _selectedButtonIndex2 += direction;
            if (_selectedButtonIndex2 < 0)
            {
                _selectedButtonIndex2 = _SettingsButton.Length - 1;
            }
            else if (_selectedButtonIndex2 >= _SettingsButton.Length)
            {
                _selectedButtonIndex2 = 0;
            }

            FocusButton(_SettingsButton[_selectedButtonIndex2]);
        }


        public void ApplyHoverEffect(Button button, bool grow)
        {
            double newWidth = grow ? 270 : 290;
            double newHeight = grow ? 60 : 80;

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                From = button.ActualWidth,  // Use the button's current width as the start value
                To = newWidth,              // Set the target width based on 'grow'
                Duration = TimeSpan.FromSeconds(0.1),  // Animation duration
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Create and configure the height animation
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                From = button.ActualHeight,  // Use the button's current height as the start value
                To = newHeight,              // Set the target height based on 'grow'
                Duration = TimeSpan.FromSeconds(0.1),  // Animation duration
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // Apply the width and height animations to the button
            button.BeginAnimation(WidthProperty, widthAnimation);
            button.BeginAnimation(HeightProperty, heightAnimation);
        }

        public void FocusButton(Button button)
        {
            button.Focus();
            ApplyHoverEffect(button, false);
        }

        private void HandleEnter()
        {
            if (_SettingsButton[_selectedButtonIndex2] == cheatcodes)
            {
                CheatcodeMenu menu = new CheatcodeMenu();
                menu.Show();
                this.Close();
            }

            if (_SettingsButton[_selectedButtonIndex2] == BackToMain)
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
        }

        /*-----------------------------------------------------------------------------------------------*/

        private void volumeSwitch(int indexVolume)
        {
            if (volumeSlider == 1)
            {
                string imagePath = "pack://application:,,,/Assets/Menu_UI_Map/VolumeButton/eersteplek.png";
                volumeChange.Source = new BitmapImage(new Uri(imagePath));
            }
            if (volumeSlider == 2)
            {
                string imagePath = "pack://application:,,,/Assets/Menu_UI_Map/VolumeButton/tweedeplek.png";
                volumeChange.Source = new BitmapImage(new Uri(imagePath));
            }
            if (volumeSlider == 3)
            {
                string imagePath = "pack://application:,,,/Assets/Menu_UI_Map/VolumeButton/derdeplek.png";
                volumeChange.Source = new BitmapImage(new Uri(imagePath));
            }
            if (volumeSlider == 4)
            {
                string imagePath = "pack://application:,,,/Assets/Menu_UI_Map/VolumeButton/vierdeplek.png";
                volumeChange.Source = new BitmapImage(new Uri(imagePath));
            }
            if (volumeSlider == 5)
            {
                string imagePath = "pack://application:,,,/Assets/Menu_UI_Map/VolumeButton/vijfdeplek.png";
                volumeChange.Source = new BitmapImage(new Uri(imagePath));
            }
        }

        private void SetVolume(float volume)
        {
            // Zorg ervoor dat het volume tussen 0.0 en 1.0 ligt
            volume = Math.Max(0.0f, Math.Min(1.0f, volume));

            // Verkrijg de audio eindpunt
            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            // Stel het volume in
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }

        /*-----------------------------------------------------------------------------------------------*/

    }
}
