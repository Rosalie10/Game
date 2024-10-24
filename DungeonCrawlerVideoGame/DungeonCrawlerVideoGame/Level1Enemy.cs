using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

namespace DungeonCrawlerVideoGame
{
    public partial class Level1Enemy : Window
    {
        public Image EnemyImage { get; private set; }
        public bool IsDying { get; set; } = false;
        public bool IsDefeated { get; set; }
        private double XPosition { get; set; } // X-coordinate of the enemy
        private double YPosition { get; set; } // Y-coordinate of the enemy
        private double speed = 8; // Speed of the enemy
        private const double AvoidanceDistance = 50; // Distance to check for walls
        private Random random = new Random();
        private DispatcherTimer deathTimer;
        private int deathFrameCount = 0;

        public bool HasHitCharacter { get; set; } = false;
        public event Action<Level1Enemy> EnemyDied;
        public int ID { get; private set; }

        public string Music { get; private set; }
        private Dictionary<int, string> EnemyMusic;

        public MediaPlayer mediaPlayer_Level1;
        public int WeaponToDefeat { get; set; }

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private Direction currentDirection = Direction.Down;

        // Sprites voor animatie
        private List<BitmapImage> enemyWalkDownSprites;
        private List<BitmapImage> enemyWalkUpSprites;
        private List<BitmapImage> enemyWalkRightSprites;
        private List<BitmapImage> enemyWalkLeftSprites;
        private List<BitmapImage> enemyDeath;
        private List<String> musicEnemy;


        private int currentSpriteIndex = 0;
        private DispatcherTimer animationTimer;

        public Level1Enemy(Canvas gameCanvas, string imagePath, double heroX, double heroY, double spawnRange, double minDistance = 400, double maxDistance = 500, double width = 100, double height = 100)
        {
            EnemyImage = new Image
            {
                Width = width,
                Height = height
            };

            // Initialize vijand sprites
            SetupEnemySprites();

            // Stel de animatietimer in
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(100); // 10 frames per seconde
            animationTimer.Tick += AnimateEnemy;
            animationTimer.Start();

            // Random spawn distance between minDistance (200p) and maxDistance (400p)
            double distance = random.NextDouble() * (maxDistance - minDistance) + minDistance;

            // Random angle in radians
            double angle = random.NextDouble() * 2 * Math.PI;

            // Calculate spawn position using polar coordinates
            XPosition = heroX + distance * Math.Cos(angle);
            YPosition = heroY + distance * Math.Sin(angle);

            AddToCanvas(gameCanvas);
            ID = GenerateNewID(); // Assign a new ID


            ListMusicEnemys();
            Music = GenerateMusicEnemy();

            mediaPlayer_Level1 = new MediaPlayer();
            mediaPlayer_Level1.Open(new Uri(Music));
            mediaPlayer_Level1.Play();

            IsDefeated = false;


        }


        private int GenerateNewID()
        {

            Random random = new Random();
            return random.Next(1, 10000);
        }

        private void SetupEnemySprites()
        {

            // Walk down sprites
            enemyWalkDownSprites = new List<BitmapImage>
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkDown/chomb_animations_Yellow_WalkDown (1).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkDown/chomb_animations_Yellow_WalkDown (2).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkDown/chomb_animations_Yellow_WalkDown (3).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkDown/chomb_animations_Yellow_WalkDown (4).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkDown/chomb_animations_Yellow_WalkDown (5).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkDown/chomb_animations_Yellow_WalkDown (6).png")),
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\ChomBombs\ChomBomb_Yellow\ChomBomb_Yellow_WalkDown\chomb_animations_Yellow_WalkDown (1).png
            };

            // Walk up sprites
            enemyWalkUpSprites = new List<BitmapImage>
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkUp/chomb_animations_Yellow_WalkUp (1).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkUp/chomb_animations_Yellow_WalkUp (2).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkUp/chomb_animations_Yellow_WalkUp (3).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkUp/chomb_animations_Yellow_WalkUp (4).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkUp/chomb_animations_Yellow_WalkUp (5).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkUp/chomb_animations_Yellow_WalkUp (6).png")),
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\ChomBombs\ChomBomb_Yellow\ChomBomb_Yellow_WalkUp\chomb_animations_Yellow_WalkUp (1).png
            };

            // Walk left sprites
            enemyWalkLeftSprites = new List<BitmapImage>
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSide/chomb_animations_Yellow_WalkSide (1).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSide/chomb_animations_Yellow_WalkSide (2).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSide/chomb_animations_Yellow_WalkSide (3).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSide/chomb_animations_Yellow_WalkSide (4).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSide/chomb_animations_Yellow_WalkSide (5).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSide/chomb_animations_Yellow_WalkSide (6).png")),
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\ChomBombs\ChomBomb_Yellow\ChomBomb_Yellow_WalkSide\chomb_animations_Yellow_WalkSide (1).png
            };

            // Walk right sprites
            enemyWalkRightSprites = new List<BitmapImage>
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSideRight/chomb_animations_Yellow_WalkSide_Right1.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSideRight/chomb_animations_Yellow_WalkSide_Right2.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSideRight/chomb_animations_Yellow_WalkSide_Right3.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSideRight/chomb_animations_Yellow_WalkSide_Right4.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSideRight/chomb_animations_Yellow_WalkSide_Right5.png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_WalkSideRight/chomb_animations_Yellow_WalkSide_Right6.png")),
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\ChomBombs\ChomBomb_Yellow\ChomBomb_Yellow_WalkSideRight\chomb_animations_Yellow_WalkSide_Right1.png
            };

            enemyDeath = new List<BitmapImage>
            {
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomSide/chomb_animations_Yellow_BoomSide (1).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomSide/chomb_animations_Yellow_BoomSide (2).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomSide/chomb_animations_Yellow_BoomSide (3).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomSide/chomb_animations_Yellow_BoomSide (4).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomSide/chomb_animations_Yellow_BoomSide (5).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomSide/chomb_animations_Yellow_BoomSide (6).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomFX/chomb_animations_Yellow_FX (1).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomFX/chomb_animations_Yellow_FX (2).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomFX/chomb_animations_Yellow_FX (3).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomFX/chomb_animations_Yellow_FX (4).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomFX/chomb_animations_Yellow_FX (5).png")),
                new BitmapImage(new Uri("pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Yellow/ChomBomb_Yellow_BoomFX/chomb_animations_Yellow_FX (6).png")),
            };
        }

        // Methode voor het animeren van de vijand
        private void AnimateEnemy(object sender, EventArgs e)
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % GetCurrentSpriteList().Count;
            EnemyImage.Source = GetCurrentSpriteList()[currentSpriteIndex]; // Verander sprite
        }

        private List<BitmapImage> GetCurrentSpriteList()
        {
            switch (currentDirection)
            {
                case Direction.Up:
                    return enemyWalkUpSprites;
                case Direction.Down:
                    return enemyWalkDownSprites;
                case Direction.Left:
                    return enemyWalkLeftSprites;
                case Direction.Right:
                    return enemyWalkRightSprites;
                default:
                    return enemyWalkDownSprites;
            }
        }

        public void StartEnemyDeath()
        {
            if (!IsDying)
            {
                IsDefeated = true;
                IsDying = true;
                deathFrameCount = 0;

                deathTimer = new DispatcherTimer();
                deathTimer.Interval = TimeSpan.FromMilliseconds(100);
                deathTimer.Tick += EnemyDeath;
                deathTimer.Start();

            }
        }

        private void EnemyDeath(object sender, EventArgs e)
        {
            if (deathFrameCount < enemyDeath.Count)
            {
                EnemyImage.Source = enemyDeath[deathFrameCount];
                deathFrameCount++;

            }
            else
            {
                // Stop the music if it's playing
                if (IsMusicPlaying())
                {
                    mediaPlayer_Level1.Stop();
                }

                // Stop the death timer and remove the enemy from the canvas
                deathTimer.Stop();
                if (EnemyImage.Parent is Canvas canvas)
                {
                    canvas.Children.Remove(EnemyImage);
                    mediaPlayer_Level1.Stop(); // Add this line to stop the music

                }

                // Notify that this specific enemy has died
                EnemyDied?.Invoke(this);  // Only this enemy is passed as an argument
            }
        }

        // Method to add the enemy to the Canvas
        public void AddToCanvas(Canvas gameCanvas)
        {
            Canvas.SetLeft(EnemyImage, XPosition);
            Canvas.SetTop(EnemyImage, YPosition);
            gameCanvas.Children.Add(EnemyImage);
            Console.WriteLine($"Enemy added at position: ({XPosition}, {YPosition})");
        }

        // Method to move the enemy toward the hero while avoiding walls
        public void Move(double heroX, double heroY, List<Rectangle> walls)
        {
            // Calculate direction toward the hero
            double directionX = heroX - (XPosition + EnemyImage.Width / 2);
            double directionY = heroY - (YPosition + EnemyImage.Height / 2);
            double distance = Math.Sqrt(directionX * directionX + directionY * directionY);

            if (distance > 0)
            {
                // Normalize direction
                directionX /= distance;
                directionY /= distance;

                // Determine current direction
                if (Math.Abs(directionX) > Math.Abs(directionY)) // Horizontal movement
                {
                    currentDirection = directionX > 0 ? Direction.Right : Direction.Left;
                }
                else // Vertical movement
                {
                    currentDirection = directionY > 0 ? Direction.Down : Direction.Up;
                }

                // Avoid walls
                if (IsCollidingWithWall(XPosition + directionX * speed, YPosition + directionY * speed, walls))
                {
                    //   Console.WriteLine("Enemy is colliding with a wall!");
                    // You could handle this by stopping or adjusting direction
                }
                else
                {
                    // Update position
                    XPosition += directionX * speed;
                    YPosition += directionY * speed;
                    Canvas.SetLeft(EnemyImage, XPosition);
                    Canvas.SetTop(EnemyImage, YPosition);
                }
            }
        }

        // Collision detection with walls
        public bool IsCollidingWithWall(double newLeft, double newTop, List<Rectangle> walls)
        {
            Rect enemyRect = new Rect(newLeft, newTop, EnemyImage.Width, EnemyImage.Height);

            foreach (var wall in walls)
            {
                Rect wallRect = GetTransformedBoundingBox(wall); // Assuming this method exists in Level1Enemy
                if (enemyRect.IntersectsWith(wallRect))
                {
                    StartEnemyDeath();
                    return true;
                }
            }

            return false; // No collision
        }

        // Assuming GetTransformedBoundingBox method exists in Level1Enemy or Level1
        private Rect GetTransformedBoundingBox(Rectangle wall)
        {
            double wallLeft = Canvas.GetLeft(wall);
            double wallTop = Canvas.GetTop(wall);
            double wallWidth = wall.Width;
            double wallHeight = wall.Height;

            return new Rect(wallLeft, wallTop, wallWidth, wallHeight);
        }

        private void ListMusicEnemys()
        {
            EnemyMusic = new Dictionary<int, string>();
            EnemyMusic.Add(1, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", "Backstreet Boys - I Want It That Way (Lyrics) [ ezmp3.cc ].mp3"));
            EnemyMusic.Add(2, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", "Justin Timberlake - Can't Stop The Feeling! [Lyrics] [ ezmp3.cc ].mp3"));
            EnemyMusic.Add(3, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", "Journey - Don't Stop Believin'(Lyrics)[ezmp3.cc].mp3"));

            //EnemyMusic.Add(1, "E:\\Chom-Bombs-\\Chom game\\Chom game\\Assets\\Music\\Backstreet Boys - I Want It That Way (Lyrics) [ ezmp3.cc ].mp3");
            //EnemyMusic.Add(2, "E:\\Chom-Bombs -\\Chom game\\Chom game\\Assets\\Music\\Justin Timberlake - Can't Stop The Feeling! [Lyrics] [ ezmp3.cc ].mp3");
            //EnemyMusic.Add(3, "E:\\Chom-Bombs -\\Chom game\\Chom game\\Assets\\Music\\Journey - Don't Stop Believin'(Lyrics)[ezmp3.cc].mp3");
            //System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Music", "Faster_8_Bit_Menu.mp3"
        }
        public string GenerateMusicEnemy()
        {

            int randomKey = random.Next(1, EnemyMusic.Count + 1);
            string selectedMusic = EnemyMusic[randomKey];


            return selectedMusic;

        }


        public bool IsMusicPlaying()
        {
            return mediaPlayer_Level1 != null && mediaPlayer_Level1.Position > TimeSpan.Zero
                   && mediaPlayer_Level1.NaturalDuration.HasTimeSpan
                   && mediaPlayer_Level1.Position < mediaPlayer_Level1.NaturalDuration.TimeSpan;
        }


    }
}

