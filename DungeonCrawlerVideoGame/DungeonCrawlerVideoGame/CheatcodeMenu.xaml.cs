using System;
using System.Collections.Generic;
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
    public partial class CheatcodeMenu : Window
    {
        private Button[] _CheatsButton;
        private int _selectedButtonIndex3 = 0;

        Boolean onsterfelijkheidStatus = false;
        Boolean tijdStatus = false;
        Boolean vijandenStatus = false;
        Boolean wapenStatus = false;

        MediaPlayer mediaPlayer = new MediaPlayer();

        public CheatcodeMenu()
        {
            InitializeComponent();

            //this.Loaded += (s, e) => this.Focus(); // Geef focus aan het venster bij laden
            FocusManager.SetFocusedElement(this, CheatMenuScreen);
            KeyDown += CheatMenuScreen_KeyDown;
            _CheatsButton = new Button[] { onsterfelijkheid, vijanden, BackToSetting, tijd, wapen };

            FocusButton(_CheatsButton[_selectedButtonIndex3]);
            this.Closed += CheatMenu_Closed;
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

        private void CheatMenu_Closed(object sender, EventArgs e)
        {

            mediaPlayer.Stop();
            mediaPlayer.Close();

        }


        private void CheatMenuScreen_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.W:
                    NavigateButtons(-1);
                    break;
                case Key.S:
                    NavigateButtons(1);
                    break;
                case Key.F:
                    HandleEnter();
                    break;

            }
        }

        public void NavigateButtons(int direction)
        {
            ApplyHoverEffect(_CheatsButton[_selectedButtonIndex3], true);


            _selectedButtonIndex3 += direction;
            if (_selectedButtonIndex3 < 0)
            {
                _selectedButtonIndex3 = _CheatsButton.Length - 1;
            }
            else if (_selectedButtonIndex3 >= _CheatsButton.Length)
            {
                _selectedButtonIndex3 = 0;
            }

            FocusButton(_CheatsButton[_selectedButtonIndex3]);
        }


        public void ApplyHoverEffect(Button button, bool grow)
        {
            double newWidth = grow ? 150 : 170;
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
            if (_CheatsButton[_selectedButtonIndex3] == onsterfelijkheid)
            {
                onsterfelijkheidStatus = !onsterfelijkheidStatus;
                if (onsterfelijkheidStatus)
                {
                    onsterfelijkheid.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/SettingsBackground.png")));
                }
                else
                {
                    onsterfelijkheid.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/UI/Button_UI2Toggle.jpeg")));
                }
            }

            if (_CheatsButton[_selectedButtonIndex3] == tijd)
            {
                tijdStatus = !tijdStatus;
                if (tijdStatus)
                {
                    tijd.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/SettingsBackground.png")));
                }
                else
                {
                    tijd.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/UI/Button_UI2Toggle.jpeg")));
                }
            }

            if (_CheatsButton[_selectedButtonIndex3] == vijanden)
            {
                vijandenStatus = !vijandenStatus;
                if (vijandenStatus)
                {
                    vijanden.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/SettingsBackground.png")));
                }
                else
                {
                    vijanden.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/UI/Button_UI2Toggle.jpeg")));
                }
            }

            if (_CheatsButton[_selectedButtonIndex3] == wapen)
            {
                wapenStatus = !wapenStatus;
                Console.WriteLine("wapenStatus: " + wapenStatus);

                if (!wapenStatus) // If currently false
                {
                    wapenStatus = true; // Activate the button
                    wapen.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Menu_UI_Map/Button_UI2.jpeg")));
                    Console.WriteLine("Loaded Button");
                }
                else // If currently true
                {
                    wapenStatus = false; // Deactivate the button
                    wapen.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/UI/Button_UI2Toggle.jpeg")));
                    Console.WriteLine("Loaded ButtonUI2Toggle");
                }

                //else
                //{
                //    wapen.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Menu_UI_Map/Button_UI2.jpeg")));
                //    Console.WriteLine("Loaded Button");
                //}
            }

            if (_CheatsButton[_selectedButtonIndex3] == BackToSetting)
            {
                SettingsScreen setting = new SettingsScreen();
                setting.Show();
                this.Close();
            }
        }
    }
}