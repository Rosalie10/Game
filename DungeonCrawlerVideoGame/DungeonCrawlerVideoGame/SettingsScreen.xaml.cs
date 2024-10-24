using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using NAudio.CoreAudioApi;

namespace DungeonCrawlerVideoGame
{
    public partial class SettingsScreen : Window
    {
        private int volumeSlider = 1;
        private Button[] _settingsButtons;
        private int _selectedButtonIndex2 = 0;
        private MediaPlayer mediaPlayer = new MediaPlayer();

        public SettingsScreen()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized; // Het venster maximaliseren

            // Start met het eerste menu-item geselecteerd
            _settingsButtons = new Button[] { cheatcodes, BackToMain };

            // Voeg keydown events toe voor knoppen navigatie
            KeyDown += SettingsScreen_KeyDown;
            this.Closed += SettingMenu_Closed;

            // Zorg ervoor dat het eerste item een hover-effect heeft bij opstart
            ApplyHoverEffect(_settingsButtons[_selectedButtonIndex2], true);

            BackGroundMusic();
        }

        private void BackGroundMusic()
        {
            string backGroundMusic = @"E:\Chom-Bombs-\Chom game\Chom game\Assets\Music\Faster_8_Bit_Menu.mp3";

            try
            {
                mediaPlayer.Open(new Uri(backGroundMusic, UriKind.Absolute));
                mediaPlayer.MediaOpened += (s, e) =>
                {
                    mediaPlayer.Play();
                    mediaPlayer.Volume = 0.5;
                    mediaPlayer.MediaEnded += (sender, args) =>
                    {
                        mediaPlayer.Position = TimeSpan.Zero;
                        mediaPlayer.Play();
                    };
                };
            }
            catch
            {
                Console.WriteLine("Error loading background music");
            }
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
                case Key.Up:
                case Key.PageUp: // Pijl omhoog of PageUp om omhoog te navigeren
                    NavigateButtons(-1);
                    break;

                case Key.S:
                case Key.Down:
                case Key.PageDown: // Pijl omlaag of PageDown om omlaag te navigeren
                    NavigateButtons(1);
                    break;

                case Key.A: // Volume verlagen met A
                    if (volumeSlider > 1)
                    {
                        volumeSlider -= 1;
                        SetVolume(volumeSlider / 5.0f);
                        volumeSwitch(volumeSlider);
                    }
                    break;

                case Key.D: // Volume verhogen met D
                    if (volumeSlider < 5)
                    {
                        volumeSlider += 1;
                        SetVolume(volumeSlider / 5.0f);
                        volumeSwitch(volumeSlider);
                    }
                    break;

                case Key.Enter: // Bevestig keuze met Enter
                    HandleEnter();
                    break;
            }
        }

        public void NavigateButtons(int direction)
        {
            ApplyHoverEffect(_settingsButtons[_selectedButtonIndex2], false);

            _selectedButtonIndex2 += direction;
            if (_selectedButtonIndex2 < 0)
            {
                _selectedButtonIndex2 = _settingsButtons.Length - 1;
            }
            else if (_selectedButtonIndex2 >= _settingsButtons.Length)
            {
                _selectedButtonIndex2 = 0;
            }

            ApplyHoverEffect(_settingsButtons[_selectedButtonIndex2], true);
        }

        public void ApplyHoverEffect(Button button, bool grow)
        {
            double newWidth = grow ? 270 : 290;
            double newHeight = grow ? 60 : 80;

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                From = button.ActualWidth,
                To = newWidth,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                From = button.ActualHeight,
                To = newHeight,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            button.BeginAnimation(WidthProperty, widthAnimation);
            button.BeginAnimation(HeightProperty, heightAnimation);
        }

        private void HandleEnter()
        {
            // Controleer welke knop is geselecteerd op basis van de index
            switch (_selectedButtonIndex2)
            {
                case 1: // Als de cheatcodes-knop geselecteerd is
                    CheatcodeMenu menu = new CheatcodeMenu();
                    menu.Show();
                    this.Close();
                    break;

                case 0: // Als de BackToMain-knop geselecteerd is
                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                    break;

                default:
                    break;
            }
        }


        private void volumeSwitch(int indexVolume)
        {
            string imagePath = string.Empty;

            if (indexVolume == 1)
            {
                imagePath = "pack://application:,,,/Assets/UI/eersteplek.png";
            }
            else if (indexVolume == 2)
            {
                imagePath = "pack://application:,,,/Assets/UI/tweedeplek.png";
            }
            else if (indexVolume == 3)
            {
                imagePath = "pack://application:,,,/Assets/UI/derdeplek.png";
            }
            else if (indexVolume == 4)
            {
                imagePath = "pack://application:,,,/Assets/UI/vierdeplek.png";
            }
            else if (indexVolume == 5)
            {
                imagePath = "pack://application:,,,/Assets/UI/vijfdeplek.png";
            }

            volumeChange.Source = new BitmapImage(new Uri(imagePath));
        }


        private void SetVolume(float volume)
        {
            volume = Math.Max(0.0f, Math.Min(1.0f, volume));
            var deviceEnumerator = new MMDeviceEnumerator();
            var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
        }
    }
}
