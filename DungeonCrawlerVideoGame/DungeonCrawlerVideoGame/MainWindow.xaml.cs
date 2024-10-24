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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DungeonCrawlerVideoGame.Properties;

namespace DungeonCrawlerVideoGame
{
    public partial class MainWindow : Window
    {
        private int _selectedButtonIndex = -1;
        private Button[] _menuButtons;
        private MediaPlayer mediaPlayer_MainWindow;

        public MainWindow()
        {
            InitializeComponent();


            _menuButtons = new Button[] { StartGame, Settings, ExitGame };



            KeyDown += MainWindow_KeyDown;
            //FocusManager.SetFocusedElement(this, MainScreen);

            WindowState = WindowState.Maximized;

            mediaPlayer_MainWindow = new MediaPlayer();
            BackGroundMusic();

            this.Closed += MainWindow_Closed;
        }

        private void BackGroundMusic()
        {
            string backGroundMusic = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", "Faster_8_Bit_Menu.mp3");

            Console.WriteLine($"Trying to load music from: {backGroundMusic}");

            mediaPlayer_MainWindow.MediaFailed += (sender, e) =>
            {
                Console.WriteLine($"Media failed to load: {e.ErrorException?.Message}");
            };

            try
            {
                mediaPlayer_MainWindow.Open(new Uri(backGroundMusic, UriKind.RelativeOrAbsolute));
                mediaPlayer_MainWindow.MediaOpened += (s, e) =>
                {
                    mediaPlayer_MainWindow.Play();
                    mediaPlayer_MainWindow.Volume = 0.5;
                    mediaPlayer_MainWindow.MediaEnded += (sender, args) =>
                    {
                        mediaPlayer_MainWindow.Position = TimeSpan.Zero;
                        mediaPlayer_MainWindow.Play();
                    };
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background music: {ex.Message}");
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {

            mediaPlayer_MainWindow.Stop();
            mediaPlayer_MainWindow.Close();

        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {

            if (_selectedButtonIndex == -1) // Check if no button is selected
            {
                _selectedButtonIndex = 0; // Set to "Start Game" when any key is pressed
                //FocusButton(_menuButtons[_selectedButtonIndex]);

            }
            else
            {
                switch (e.Key)
                {
                    case Key.W:
                        NavigateButtons(-1);
                        break;
                    case Key.S:
                        NavigateButtons(1);
                        break;
                    case Key.Enter:
                        HandleEnter();
                        break;
                        //case Key.R:
                        //    OpenBossFight();
                        //    break;

                }

            }
        }

        //private void OpenBossFight()
        //{
        //    BossFight boss = new BossFight();
        //    boss.Show(); // Open het SettingsScreen
        //    this.Close(); // Sluit het huidige venster als dat nodig is
        //}

        public void NavigateButtons(int direction)
        {
            ApplyHoverEffect(_menuButtons[_selectedButtonIndex], false);


            _selectedButtonIndex += direction;
            if (_selectedButtonIndex < 0)
            {
                _selectedButtonIndex = _menuButtons.Length - 1;
            }
            else if (_selectedButtonIndex >= _menuButtons.Length)
            {
                _selectedButtonIndex = 0;
            }

            ApplyHoverEffect(_menuButtons[_selectedButtonIndex], true);
        }


        public void ApplyHoverEffect(Button button, bool grow)
        {
            double newWidth2 = grow ? 450 : 300;
            double newHeight2 = grow ? 150 : 100;

            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = newWidth2,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };


            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                To = newHeight2,
                Duration = TimeSpan.FromSeconds(0.1),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            button.BeginAnimation(WidthProperty, widthAnimation);
            button.BeginAnimation(HeightProperty, heightAnimation);
        }

        //public void FocusButton(Button button)
        //{
        //    //button.Focus();

        //}

        private void HandleEnter()
        {
            //if (_menuButtons[_selectedButtonIndex] == StartGame)
            //{
            //    OpenIntroduction();
            //}

            //if (_menuButtons[_selectedButtonIndex] == Settings)
            //{
            //    OpenSettingsScreen();
            //}

            if (_menuButtons[_selectedButtonIndex] == ExitGame)
            {
                this.Close();
            }
        }

        //private void OpenIntroduction()
        //{
        //    Introduction Intro = new Introduction();
        //    Intro.Show(); // Open het SettingsScreen
        //    this.Close(); // Sluit het huidige venster als dat nodig is
        //}

        //private void OpenSettingsScreen()
        //{
        //    SettingsScreen thirdWindow = new SettingsScreen();
        //    thirdWindow.Show();
        //    this.Close(); // Sluit het huidige venster als dat nodig is

        //}
    }
}