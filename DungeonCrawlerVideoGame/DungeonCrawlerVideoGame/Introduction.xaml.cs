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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DungeonCrawlerVideoGame
{

    public partial class Introduction : Window
    {
        private MediaPlayer mediaPlayer_Introduction;
        private DispatcherTimer typingTimer;  // Timer for typing effect
        private List<string> messages;         // List of messages to display
        private int currentMessageIndex;       // Index of the current message
        private string currentText;            // The text to display
        private int currentIndex;              // Current index of the character being displayed

        public Introduction()
        {
            InitializeComponent();  // Ensure this is called first
            InitializeGame();       // Separate method to set up initial state

            WindowState = WindowState.Maximized;

        }
        private void InitializeGame()
        {
            messages = new List<string>
            {
                "In a world where music has vanished, a hero rises to reclaim the lost melodies.",
                "Deep within the dark dungeons, mutated plants guard the treasures of sound.",
                "Armed with skill and determination, it's time to infiltrate their lair and restore the joy of music to the people.",
                "W,A,S,D to move.                   1, 2 & 3 to attack.",
                "Choose your hero"
            };

            currentMessageIndex = 0; // Start with the first message
            SetDialogueText(messages[currentMessageIndex]);

            KeyDown += Introduction_KeyDown;

            // Initialize background music
            mediaPlayer_Introduction = new MediaPlayer(); // Ensure mediaPlayer is initialized
            BackGroundMusic();
            this.Closed += Introduction_Closed;
        }

        /*-------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void Introduction_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the F key is pressed
            if (e.Key == Key.F)
            {
                currentMessageIndex++; // Move to the next message
                if (currentMessageIndex < messages.Count)
                {
                    SetDialogueText(messages[currentMessageIndex]); // Display the next message
                }
                else
                {
                    ShowHeroSelection();
                }
            }

            // Check for hero selection keys (1, 2, 3)
            if (MainScreen.Children.Count > 0 && e.Key >= Key.D1 && e.Key <= Key.D3)
            {
                int heroIndex = (int)e.Key - (int)Key.D1; // Get the index from the key
                SelectHero(heroIndex);
            }
        }

        /*-------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void HeroButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedHero = ((Button)sender).Content.ToString();
            MessageBox.Show($"You selected: {selectedHero}");

            // Optionally, you can also select the hero based on which button was clicked.
            if (sender == ((Button)MainScreen.Children[0]).Content) // Pinky
            {
                SelectHero(0);
            }
            else if (sender == ((Button)MainScreen.Children[1]).Content) // Swamp Dude
            {
                SelectHero(1);
            }
            else if (sender == ((Button)MainScreen.Children[2]).Content) // Owl Monster
            {
                SelectHero(2);
            }
        }

        private void ShowHeroSelection()
        {
            // Clear previous UI elements if needed
            MainScreen.Children.Clear();

            // Create a StackPanel to hold the instruction text and hero buttons
            StackPanel mainPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Create a TextBlock for the instruction
            TextBlock instructionText = new TextBlock
            {
                Text = "Press Number:", // Instruction text
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 32, // Adjust font size as needed
                Foreground = Brushes.White, // Set text color to white
                Margin = new Thickness(0, 0, 0, 10) // Add some space below the text
            };

            // Create a WrapPanel for hero buttons to be arranged horizontally
            WrapPanel heroSelectionPanel = new WrapPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Create buttons for hero selection
            Button hero1Button = CreateHeroButton("1. Puffball", "Assets/Characters/Puffball/Pink_Monster_Idle1.png", Brushes.LightPink);
            Button hero2Button = CreateHeroButton("2. Bogzilla", "Assets/Characters/Bogzilla/DudeMonster_Idle1.png", Brushes.LightGreen);
            Button hero3Button = CreateHeroButton("3. Mr.Hoots", "Assets/Characters/Mr.Hoots/OwletMonster_Idle1.png", Brushes.LightSkyBlue);

            // Assign click event handlers
            hero1Button.Click += HeroButton_Click;
            hero2Button.Click += HeroButton_Click;
            hero3Button.Click += HeroButton_Click;

            // Add buttons to the hero selection panel
            heroSelectionPanel.Children.Add(hero1Button);
            heroSelectionPanel.Children.Add(hero2Button);
            heroSelectionPanel.Children.Add(hero3Button);

            // Add the instruction text and hero selection panel to the main panel
            mainPanel.Children.Add(instructionText); // Add instruction text first
            mainPanel.Children.Add(heroSelectionPanel); // Add hero buttons panel below

            // Add the main panel to the MainScreen
            MainScreen.Children.Add(mainPanel);
        }

        private Button CreateHeroButton(string content, string imagePath, Brush backgroundColor)
        {
            // Create a button
            Button button = new Button
            {
                Width = 300,  // Set a fixed width for all buttons
                Height = 200, // Set a fixed height for all buttons
                Margin = new Thickness(10),
                Background = backgroundColor // Set the background color
            };

            // Create an Image
            Image heroImage = new Image();
            try
            {
                heroImage.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)); // Changed to UriKind.Relative
                heroImage.Width = 100;  // Adjust width as needed
                heroImage.Height = 100;  // Adjust height as needed
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }

            // Create a StackPanel to hold the image and text
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center };
            stackPanel.Children.Add(heroImage);
            stackPanel.Children.Add(new TextBlock { Text = content, HorizontalAlignment = HorizontalAlignment.Center });

            button.Content = stackPanel;
            return button;
        }
        private void SelectHero(int heroIndex)
        {
            if (heroIndex == 0)
                GameData.SelectedCharacter = 1;  // Hero 1 geselecteerd
            else if (heroIndex == 1)
                GameData.SelectedCharacter = 2;  // Hero 2 geselecteerd
            else if (heroIndex == 2)
                GameData.SelectedCharacter = 3;  // Hero 3 geselecteerd

            OpenLevel1();
        }

        private void OpenLevel1()
        {
            Level1 secondWindow = new Level1();
            secondWindow.Show();
            this.Close();
        }


        /*-------------------------------------------------------------------------------------------------------------------------------------------------------------*/

        private void SetDialogueText(string message)
        {
            currentText = message; // Store the message
            currentIndex = 0;      // Reset the index for typing effect

            // Clear previous text
            DialogueText.Text = string.Empty;

            // Initialize and start the typing timer
            typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Adjust to 100 ms for more control
            };
            typingTimer.Tick += TypingTimer_Tick;
            typingTimer.Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            if (currentIndex < currentText.Length)
            {
                DialogueText.Text += currentText[currentIndex]; // Adds letter
                currentIndex++; // Goes to next letter
            }
            else
            {
                typingTimer.Stop(); // Stop the timer when finished
            }
        }

        /*-------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        private void BackGroundMusic()
        {
            string backGroundMusic = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", "Faster_8_Bit_Menu.mp3");

            Console.WriteLine($"Trying to load music from: {backGroundMusic}");

            mediaPlayer_Introduction.MediaFailed += (sender, e) =>
            {
                Console.WriteLine($"Media failed to load: {e.ErrorException?.Message}");
            };

            try
            {
                mediaPlayer_Introduction.Open(new Uri(backGroundMusic, UriKind.RelativeOrAbsolute));
                mediaPlayer_Introduction.MediaOpened += (s, e) =>
                {
                    mediaPlayer_Introduction.Play();
                    mediaPlayer_Introduction.Volume = 0.5;
                    mediaPlayer_Introduction.MediaEnded += (sender, args) =>
                    {
                        mediaPlayer_Introduction.Position = TimeSpan.Zero;
                        mediaPlayer_Introduction.Play();
                    };
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background music: {ex.Message}");
            }
        }

        private void Introduction_Closed(object sender, EventArgs e)
        {

            mediaPlayer_Introduction.Stop();
            mediaPlayer_Introduction.Close();

        }
        /*-------------------------------------------------------------------------------------------------------------------------------------------------------------*/
    }
}
