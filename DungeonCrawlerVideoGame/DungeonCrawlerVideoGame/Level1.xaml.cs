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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DungeonCrawlerVideoGame
{
    public partial class Level1 : Window
    {
        private double speed = 5;
        private double canvasWidth = 3500;
        private double canvasHeight = 2000;
        private double viewportWidth = 3500 / 3;
        private double viewportHeight = 2000 / 3;
        private Point startingPosition = new Point(100, 100);
        private List<Rectangle> allWalls;
        //enemy
        private Random random = new Random();
        private int limitEnemy = 10;
        private int countEnemy = 100;
        private DispatcherTimer timer;
        private DispatcherTimer chestAnimationTimer;
        private List<Level1Enemy> activeEnemies = new List<Level1Enemy>();
        private DispatcherTimer moveEnemiesTimer; // Timer voor het bewegen van vijanden
        private DispatcherTimer gameTimer; // Timer voor het bijhouden van de speeltijd
        private double gameTimeElapsed; // Tijd die is verstreken in seconden
        private double minSpawnTime = 5; // Minimale spawn tijd in seconden
        private double maxSpawnTime = 60; // Maximale spawn tijd in seconden
        private List<double> spawnIntervals = new List<double>(); // Lijst van spawn tijden
        private bool isMainCharacterDead = false;


        // Animation properties
        private List<ImageBrush> idleSprites;                       //defines idle sprite
        private List<ImageBrush> rightRunningSprites;               //defines running right sprite
        private List<ImageBrush> leftRunningSprites;                //defines running left sprite
        private List<ImageBrush> upRunningSprites;                  //defines running up sprite
        private List<ImageBrush> downRunningSprites;              //defines down sprite
        private List<ImageBrush> fightAnimation1;
        private List<ImageBrush> fightAnimation2;
        private List<ImageBrush> fightAnimation3;
        private List<ImageBrush> chestAnimation;


        private int currentSpriteIndex = 0;
        private int currentSpriteChest = 0;
        private DispatcherTimer animationTimer;
        private bool isMovingRight = false;                         //tracks character movement to the right
        private bool isMovingLeft = false;                          //tracks character movement to the left
        private bool isMovingUp = false;                            //tracks character movement to the up
        private bool isMovingDown = false;                          //tracks character movement to the right
        private Border[] weaponSlots;
        int verslagenChoms = 0;

        //Hitboxes 
        private Rect hitBoxMainCharacter = new Rect();
        private List<Rect> enemyHitBoxes = new List<Rect>();
        private int threeHearts = 3;

        private MediaPlayer mediaPlayer = new MediaPlayer();
        private string currentMusicPlaying = string.Empty;
        public int WeaponToDefeat { get; set; }


        public Level1()
        {
            InitializeComponent();
            this.KeyDown += LevelKeyDown;
            this.KeyUp += LevelKeyUp;

            WindowState = WindowState.Maximized;
            SpawnEnemy(null, EventArgs.Empty);

            // Setup sprite animation
            SetupCharacterSprites();
            StartSpriteAnimation();

            weaponSlots = new Border[] { WeaponSlot1, WeaponSlot2, WeaponSlot3, WeaponSlot4 };
            this.KeyDown += (sender, e) => Level1_KeyDownWeapon(sender, e);

            this.Closed += Level1_Closed;

            EnemyCollision();
            OpenChestAnimation();
        }

        private void Level1_Closed(object sender, EventArgs e)
        {

            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
                currentMusicPlaying = string.Empty;
            }
        }

        private void Level1_KeyDownWeapon(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                ClearHighlighted(i);
            }

            int selectedWeaponIndex = 0;

            switch (e.Key)
            {
                case Key.D1:
                    HighlightSlot(0);
                    selectedWeaponIndex = 1;
                    currentSpriteIndex = (currentSpriteIndex + 1) % fightAnimation1.Count;
                    MainCharacter.Fill = fightAnimation1[currentSpriteIndex];
                    Console.WriteLine("Weapon 1 selected");
                    break;

                case Key.D2:
                    HighlightSlot(1);
                    selectedWeaponIndex = 2;
                    currentSpriteIndex = (currentSpriteIndex + 1) % fightAnimation2.Count;
                    MainCharacter.Fill = fightAnimation2[currentSpriteIndex];
                    Console.WriteLine("Weapon 2 selected");
                    break;

                case Key.D3:
                    HighlightSlot(2);
                    selectedWeaponIndex = 3;
                    currentSpriteIndex = (currentSpriteIndex + 1) % fightAnimation3.Count;
                    MainCharacter.Fill = fightAnimation3[currentSpriteIndex];
                    Console.WriteLine("Weapon 3 selected");
                    break;
            }


            if (selectedWeaponIndex != -1)
            {
                Console.WriteLine($"Checking weapon {selectedWeaponIndex} against enemies");
                CheckWeaponAndMusic(selectedWeaponIndex);
            }
        }


        private void CheckWeaponAndMusic(int selectedWeaponIndex)
        {
            foreach (var enemy in activeEnemies.ToList())
            {
                Console.WriteLine($"Selected Weapon: {selectedWeaponIndex}, Enemy's WeaponToDefeat: {enemy.WeaponToDefeat}, IsDefeated: {enemy.IsDefeated}");

                if (enemy.WeaponToDefeat == selectedWeaponIndex && !enemy.IsDefeated)
                {
                    verslagenChoms += 1;
                    KilledEnemiesText.Text = "Chom Boms: " + Convert.ToString(verslagenChoms);
                    Console.WriteLine($"Correct weapon used. Enemy defeated!");

                    enemy.IsDefeated = true;
                    enemy.StartEnemyDeath();

                    OpenChest();
                }
                else
                {
                    Console.WriteLine("Wrong weapon or enemy already defeated.");
                }
            }
        }

        private int AssignWeaponBasedOnMusic(string music)
        {

            if (music.Contains("Backstreet Boys")) return 1;
            else if (music.Contains("Other Song")) return 2;
            else return 3;
        }





        private void HighlightSlot(int index)
        {
            if (index < 0 || index >= weaponSlots.Length) return;

            var shadowEffect = new DropShadowEffect
            {
                Color = Colors.Black,
                ShadowDepth = 5,
                Opacity = 50
            };

            weaponSlots[index].Effect = shadowEffect;
        }

        private void ClearHighlighted(int index)
        {
            if (index < 0 || index >= weaponSlots.Length) return;

            weaponSlots[index].Effect = null;
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            // Cycle through the sprites based on movement state
            if (isMovingRight)
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % rightRunningSprites.Count;
                MainCharacter.Fill = rightRunningSprites[currentSpriteIndex];

            }
            else if (isMovingLeft)
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % leftRunningSprites.Count;
                MainCharacter.Fill = leftRunningSprites[currentSpriteIndex];
            }
            else if (isMovingUp)
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % upRunningSprites.Count;
                MainCharacter.Fill = upRunningSprites[currentSpriteIndex];
            }
            else if (isMovingDown)
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % downRunningSprites.Count;
                MainCharacter.Fill = downRunningSprites[currentSpriteIndex];
            }
            else
            {
                currentSpriteIndex = (currentSpriteIndex + 1) % idleSprites.Count();
                MainCharacter.Fill = idleSprites[currentSpriteIndex];
            }
        }

        // Sets up the png's for the animation
        private void SetupCharacterSprites()
        {
            if (GameData.SelectedCharacter == 1)        //Pink monster
            {
                idleSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_Idle1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_Idle2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_Idle3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_Idle4.png")))
                };

                rightRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_01.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_02.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_03.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_04.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_05.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_06.png"))),
                };

                leftRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_RunLeft1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_RunLeft2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_RunLeft3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_RunLeft4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_RunLeft5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_RunLeft6.png")))
                };

                upRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_RunUp1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_RunUp2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_RunUp3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_RunUp4.png"))),
                };

                downRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_01.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_02.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_03.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_04.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_05.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/PinkMonster_Run_06.png"))),
                };

                fightAnimation1 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V1Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V1Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V1Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V1Attack4.png"))),
                };

                fightAnimation2 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V4Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V4Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V4Attack4.png"))),
                };

                fightAnimation3 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V3Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V3Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V3Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/PinkMonster/Pink_Monster_V3Attack4.png"))),
                };
            }

            else if (GameData.SelectedCharacter == 2)       //Dude Monster sprites
            {
                idleSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_Idle1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_Idle2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_Idle3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_Idle4.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\DudeMonster\DudeMonster_Idle1.png

                rightRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight6.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\DudeMonster\DudeMonster_RunRight1.png

                leftRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunLeft1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunLeft2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunLeft3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunLeft4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunLeft5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunLeft6.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\DudeMonster\DudeMonster_RunLeft1.png

                upRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunUp1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunUp2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunUp3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunUp4.png"))),
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\DudeMonster\DudeMonster_RunUp1.png

                downRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_RunRight6.png")))
                };


                fightAnimation1 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V1Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V1Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V1Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V1Attack4.png"))),

                };


                fightAnimation2 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V2Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V2Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V2Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V2Attack4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V2Attack5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V2Attack6.png"))),
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\DudeMonster\DudeMonster_V2Attack1.png

                fightAnimation3 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V4Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V4Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V4Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/DudeMonster/DudeMonster_V4Attack4.png"))),
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\DudeMonster\DudeMonster_V4Attack1.png
            }

            else if (GameData.SelectedCharacter == 3)       //Owlet Monster sprites
            {
                idleSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_Idle1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_Idle2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_Idle3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_Idle4.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_Idle1.png

                rightRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight6.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_RunRight1.png

                leftRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunLeft1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunLeft2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunLeft3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunLeft4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunLeft5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunLeft6.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_RunLeft1.png

                upRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunUp1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunUp2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunUp3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunUp4.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_RunUp1.png

                downRunningSprites = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_RunRight6.png")))
                };


                fightAnimation1 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V1Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V1Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V1Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V1Attack4.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_V1Attack1.png

                fightAnimation2 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V2Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V2Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V2Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V2Attack4.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V2Attack5.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V2Attack6.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_V2Attack1.png

                fightAnimation3 = new List<ImageBrush>
                {
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V3Attack1.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V3Attack2.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V3Attack3.png"))),
                new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Character/OwletMonster/OwletMonster_V3Attack4.png")))
                };
                //C:\Users\shen\OneDrive - NHL Stenden\Documents\Github\Chom-Bombs-\Chom game\Chom game\Assets\Character\OwletMonster\OwletMonster_V3Attack1.png
            }
        }

        // Starts the animation timer
        private void StartSpriteAnimation()
        {
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(100); // Change frame every 100ms (adjust for desired speed)
            animationTimer.Tick += OnAnimationTick;
            animationTimer.Start();
        }

        private void LevelKeyDown(object sender, KeyEventArgs e)
        {
            double currentLeft = Canvas.GetLeft(MainCharacter);
            double currentTop = Canvas.GetTop(MainCharacter);

            double newLeft = currentLeft;
            double newTop = currentTop;

            switch (e.Key)
            {
                case Key.W:
                    newTop = currentTop - speed;
                    isMovingUp = true; // Set to true if moving
                    break;
                case Key.S:
                    newTop = currentTop + speed;
                    isMovingDown = true;
                    break;
                case Key.A:
                    newLeft = currentLeft - speed;
                    isMovingLeft = true;
                    break;
                case Key.D:
                    newLeft = currentLeft + speed;
                    isMovingRight = true;
                    break;
                default:
                    break;
            }

            newLeft = Math.Max(0, Math.Min(canvasWidth - MainCharacter.Width, newLeft));
            newTop = Math.Max(0, Math.Min(canvasHeight - MainCharacter.Height, newTop));

            if (!IsCollidingWithWall(newLeft, newTop))
            {
                Canvas.SetLeft(MainCharacter, newLeft);
                Canvas.SetTop(MainCharacter, newTop);

            }

            UpdateCamera();
            EnemyCollision();

        }

        private void LevelKeyUp(object sender, KeyEventArgs e)
        {
            // Stop movement in specific directions based on key released
            switch (e.Key)
            {
                case Key.W:
                    isMovingUp = false;
                    break;
                case Key.S:
                    isMovingDown = false;
                    break;
                case Key.A:
                    isMovingLeft = false;
                    break;
                case Key.D:
                    isMovingRight = false;
                    break;
            }

        }

        private void UpdateGameTime(object sender, EventArgs e)
        {
            gameTimeElapsed++;
            CheckForMusicNoteCollision();

            // Start met het spawnen van vijanden na 10 seconden
            if (gameTimeElapsed >= 10 && !timer.IsEnabled)
            {
                timer.Start();
            }
        }

        public void SpawnEnemy(object sender, EventArgs e)
        {
            // Check if the main character is dead
            if (isMainCharacterDead)
            {
                return;  // Stop spawning if the player is dead
            }

            if (activeEnemies.Count >= limitEnemy)
            {
                return;
            }

            spawnIntervals.Add(random.NextDouble() * (maxSpawnTime - minSpawnTime) + minSpawnTime);

            if (gameTimeElapsed >= 10)
            {
                double spawnRange = 200;
                double minDistance = 50;

                double currentLeft = Canvas.GetLeft(MainCharacter);
                double currentTop = Canvas.GetTop(MainCharacter);

                double newEnemyLeft, newEnemyTop;
                bool validSpawn = false;
                int retryCount = 0;
                int maxRetries = 10;

                // Add all your walls here
                List<Rectangle> walls = new List<Rectangle>
        {
            Wall, Wall2, Wall3, Wall4, Wall5, Wall6, Wall7, Wall8, Wall9, Wall10,
            Wall11, Wall12, Wall13, Wall14, Wall15, Wall16, Wall17, Wall18,
            Wall19, Wall20, Wall21, Wall22, Wall23, Wall24
        };

                Console.WriteLine("Attempting to spawn enemy...");

                do
                {
                    retryCount++;
                    if (retryCount > maxRetries)
                    {
                        Console.WriteLine("Failed to find a valid spawn point after multiple attempts.");
                        return; // Exit if no valid spawn found
                    }

                    // Randomize enemy position near the main character but within the spawn range
                    newEnemyLeft = currentLeft + random.NextDouble() * spawnRange - spawnRange / 2;
                    newEnemyTop = currentTop + random.NextDouble() * spawnRange - spawnRange / 2;

                    Console.WriteLine($"Trying spawn point: Left = {newEnemyLeft}, Top = {newEnemyTop}");

                    // Fetch the actual size of the enemy image
                    double enemyWidth = 50;  // Adjust according to your enemy's width
                    double enemyHeight = 50; // Adjust according to your enemy's height

                    // Ensure that the spawn point satisfies the minimum distance requirement from the player
                    if (Math.Abs(newEnemyLeft - currentLeft) >= minDistance || Math.Abs(newEnemyTop - currentTop) >= minDistance)
                    {
                        Console.WriteLine("Satisfies minimum distance requirement.");

                        Rect enemyRect = new Rect(newEnemyLeft, newEnemyTop, enemyWidth, enemyHeight);

                        validSpawn = true; // Assume valid spawn unless a wall collision is detected

                        foreach (var wall in walls)
                        {
                            Rect wallRect = new Rect(Canvas.GetLeft(wall), Canvas.GetTop(wall), wall.Width, wall.Height);

                            // Check if the enemy's bounding box collides with any wall
                            if (enemyRect.IntersectsWith(wallRect))
                            {
                                validSpawn = false; // Collision detected, retry
                                Console.WriteLine("Spawn point collides with a wall. Retrying...");
                                break; // Exit the wall check loop and try again
                            }
                        }

                        if (validSpawn)
                        {
                            Console.WriteLine("Found valid spawn point.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Does not satisfy minimum distance requirement. Retrying...");
                    }

                } while (!validSpawn);

                if (validSpawn)
                {
                    // Now create the actual enemy after collision check passes
                    Level1Enemy enemy = new Level1Enemy(LevelCanvas,
                        "pack://application:,,,/Assets/Character/ChomBombs/ChomBomb_Blue/ChomBomb_Blue_BoomDown/chomb_animations_BlueBoomDown (1).png",
                        newEnemyLeft, newEnemyTop, spawnRange, minDistance);

                    activeEnemies.Add(enemy);
                    Console.WriteLine($"New enemy spawned with ID: {enemy.ID}, with the music: {enemy.Music}");

                    string selectedMusic = enemy.GenerateMusicEnemy();
                    if (currentMusicPlaying != selectedMusic)
                    {
                        mediaPlayer.Stop();
                        mediaPlayer.Open(new Uri(selectedMusic, UriKind.Relative));
                        mediaPlayer.Play();
                        currentMusicPlaying = selectedMusic;
                    }

                    enemy.WeaponToDefeat = AssignWeaponBasedOnMusic(selectedMusic);

                    // Restart the spawn timer
                    timer.Interval = TimeSpan.FromSeconds(10);
                    timer.Start();
                }
            }
        }






        private void MoveEnemies(object sender, EventArgs e)
        {
            double heroX = Canvas.GetLeft(MainCharacter) + (MainCharacter.Width / 2); // Center of the hero
            double heroY = Canvas.GetTop(MainCharacter) + (MainCharacter.Height / 2); // Center of the hero

            // Move each active enemy toward the hero
            foreach (var enemy in activeEnemies)
            {
                enemy.Move(heroX, heroY, allWalls);
            }
        }
        private void EnemyCollision()
        {
            double currentLeft = Canvas.GetLeft(MainCharacter);
            double currentTop = Canvas.GetTop(MainCharacter);

            Rect hitBoxMainCharacter = new Rect(currentLeft, currentTop, MainCharacter.Width, MainCharacter.Height);

            foreach (var enemy in activeEnemies.ToList())
            {
                Rect enemyHitBox = new Rect(Canvas.GetLeft(enemy.EnemyImage), Canvas.GetTop(enemy.EnemyImage), enemy.EnemyImage.Width, enemy.EnemyImage.Height);

                if (enemyHitBox.IntersectsWith(hitBoxMainCharacter) && !enemy.IsDefeated)
                {
                    if (!enemy.HasHitCharacter)
                    {
                        enemy.HasHitCharacter = true;
                        EnemyCollisionTrue(enemy);
                        LivesText.Text = "Levens: " + Convert.ToString(threeHearts);

                        if (threeHearts <= 0)
                        {
                            MainCharacterDeath();
                        }
                    }
                }
            }
        }

        private void HandleEnemyCollision(Level1Enemy enemy)
        {

            EnemyCollisionTrue(enemy);
        }


        private bool IsCollidingWithWall(double newLeft, double newTop)
        {

            Rect hitBoxMainCharacter = new Rect(newLeft, newTop, MainCharacter.Width, MainCharacter.Height);





            allWalls = new List<Rectangle>
    {
        Wall, Wall2, Wall3, Wall4, Wall5, Wall6, Wall7, Wall8, Wall9, Wall10,
        Wall11, Wall12, Wall13, Wall14, Wall15, Wall16, Wall17, Wall18,
        Wall19, Wall20, Wall21, Wall22, Wall23, Wall24
    };


            foreach (var wall in allWalls)
            {

                Rect wallRect = GetTransformedBoundingBox(wall);


                if (hitBoxMainCharacter.IntersectsWith(wallRect))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnemyCollisionTrue(Level1Enemy enemy)
        {

            threeHearts -= 1;
            Console.WriteLine("Character got hit!");

            enemy.StartEnemyDeath();
        }

        public void SubscribeToEnemyDeath(Level1Enemy enemy)
        {

            enemy.EnemyDied += OnEnemyDied;
        }

        private void OnEnemyDied(Level1Enemy enemy)
        {

            activeEnemies.Remove(enemy);

            Console.WriteLine($"Enemy with ID {enemy.ID} has died.");
        }


        private Rect GetTransformedBoundingBox(Rectangle wall)
        {

            double wallLeft = Canvas.GetLeft(wall);
            double wallTop = Canvas.GetTop(wall);
            double wallWidth = wall.Width;
            double wallHeight = wall.Height;


            Rect originalRect = new Rect(wallLeft, wallTop, wallWidth, wallHeight);


            if (wall.RenderTransform is TransformGroup transformGroup)
            {

                foreach (var transform in transformGroup.Children)
                {
                    if (transform is RotateTransform rotateTransform)
                    {

                        Point center = new Point(wallLeft + wallWidth / 2, wallTop + wallHeight / 2);
                        originalRect = RotateRect(originalRect, center, rotateTransform.Angle);
                    }
                    else if (transform is ScaleTransform scaleTransform)
                    {

                        originalRect = ScaleRect(originalRect, scaleTransform);
                    }
                    else if (transform is TranslateTransform translateTransform)
                    {
                        originalRect.Offset(translateTransform.X, translateTransform.Y);

                    }

                }
            }

            return originalRect;
        }


        private Rect RotateRect(Rect rect, Point center, double angle)
        {

            double radians = angle * (Math.PI / 180);


            Point topLeft = new Point(rect.Left, rect.Top);
            Point topRight = new Point(rect.Right, rect.Top);
            Point bottomLeft = new Point(rect.Left, rect.Bottom);
            Point bottomRight = new Point(rect.Right, rect.Bottom);


            topLeft = RotatePoint(topLeft, center, radians);
            topRight = RotatePoint(topRight, center, radians);
            bottomLeft = RotatePoint(bottomLeft, center, radians);
            bottomRight = RotatePoint(bottomRight, center, radians);


            double newLeft = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            double newTop = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            double newRight = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            double newBottom = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

            return new Rect(newLeft, newTop, newRight - newLeft, newBottom - newTop);
        }


        private Point RotatePoint(Point point, Point center, double radians)
        {
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            double dx = point.X - center.X;
            double dy = point.Y - center.Y;

            double newX = center.X + (dx * cos - dy * sin);
            double newY = center.Y + (dx * sin + dy * cos);

            return new Point(newX, newY);
        }


        private Rect ScaleRect(Rect rect, ScaleTransform scaleTransform)
        {
            double newWidth = rect.Width * scaleTransform.ScaleX;
            double newHeight = rect.Height * scaleTransform.ScaleY;


            double offsetX = (newWidth - rect.Width) / 2;
            double offsetY = (newHeight - rect.Height) / 2;

            return new Rect(rect.Left - offsetX, rect.Top - offsetY, newWidth, newHeight);
        }




        private void UpdateCamera()
        {
            double characterLeft = Canvas.GetLeft(MainCharacter);
            double characterTop = Canvas.GetTop(MainCharacter);

            double targetScrollX = characterLeft - viewportWidth / 2 + MainCharacter.Width / 2;
            double targetScrollY = characterTop - viewportHeight / 2 + MainCharacter.Height / 2;

            double clampedScrollX = Math.Max(0, Math.Min(canvasWidth - viewportWidth, targetScrollX));
            double clampedScrollY = Math.Max(0, Math.Min(canvasHeight - viewportHeight, targetScrollY));

            CameraScrollViewer.ScrollToHorizontalOffset(clampedScrollX);
            CameraScrollViewer.ScrollToVerticalOffset(clampedScrollY);
        }

        private void Level1_Loaded(object sender, RoutedEventArgs e)
        {
            // Start de timer voor het bijhouden van de speeltijd
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += UpdateGameTime;
            gameTimer.Start();

            // Start de timer voor het spawnen van extra vijanden
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += SpawnEnemy;
            timer.Start();

            // Start de timer voor het bewegen van vijanden
            moveEnemiesTimer = new DispatcherTimer();
            moveEnemiesTimer.Interval = TimeSpan.FromMilliseconds(50);
            moveEnemiesTimer.Tick += MoveEnemies;
            moveEnemiesTimer.Start();
        }

        private void MainCharacterDeath()
        {
            string deathMessage = "Je bent dood!!!!";

            mediaPlayer.Stop();
            mediaPlayer.Open(new Uri("E:\\Chom-Bombs-\\Chom game\\Chom game\\Assets\\Music\\Nostalgia_Fast_PossibleDeathMusic.mp3"));
            mediaPlayer.Play();

            timer.Stop();
            isMainCharacterDead = true;

            MessageBoxResult result = MessageBox.Show(deathMessage, "Game Over", MessageBoxButton.OK);

            if (result == MessageBoxResult.OK)
            {
                this.Close();

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

        private void CheckForMusicNoteCollision()
        {
            double currentLeft = Canvas.GetLeft(MainCharacter);
            double currentTop = Canvas.GetTop(MainCharacter);

            Rect hitBoxMainCharacter = new Rect(currentLeft, currentTop, MainCharacter.Width, MainCharacter.Height);


            List<Rectangle> musicNotes = new List<Rectangle> { MusicNote, MusicNote2, MusicNote3, MusicNote4 };

            foreach (var note in musicNotes)
            {
                Rect noteHitBox = new Rect(Canvas.GetLeft(note), Canvas.GetTop(note), note.Width, note.Height);


                if (hitBoxMainCharacter.IntersectsWith(noteHitBox))
                {

                    threeHearts++;
                    LivesText.Text = "Levens: " + Convert.ToString(threeHearts);

                    Console.WriteLine("Music note collected! Lives increased to: " + threeHearts);


                    Canvas.SetLeft(note, -100);
                    note.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void OpenChestAnimation()
        {
            chestAnimation = new List<ImageBrush>
            {
                   new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/00.png"))),
                   new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/10.png"))),
                   new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/20.png"))),
                   new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Assets/Background/30.png"))),
            };
            chestAnimationTimer = new DispatcherTimer();
            chestAnimationTimer.Interval = TimeSpan.FromMilliseconds(500); // Adjust speed here
            chestAnimationTimer.Tick += AnimateChest;
        }

        private void AnimateChest(object sender, EventArgs e)
        {
            if (currentSpriteChest < chestAnimation.Count - 1)
            {

                chest.Fill = chestAnimation[currentSpriteChest];
                currentSpriteChest++;
            }
            else
            {
                chest.Fill = chestAnimation[chestAnimation.Count - 1];
                chestAnimationTimer.Stop();
            }
        }
        private void OpenChest()
        {
            double currentLeft = Canvas.GetLeft(MainCharacter);
            double currentTop = Canvas.GetTop(MainCharacter);

            Rect hitBoxMainCharacter = new Rect(currentLeft, currentTop, MainCharacter.Width, MainCharacter.Height);

            double leftChest = Canvas.GetLeft(chest);
            double TopChest = Canvas.GetTop(chest);

            Rect hitBoxChest = new Rect(leftChest, TopChest, chest.Width, chest.Height);

            if (verslagenChoms >= 5)
            {
                Console.WriteLine("Starting chest animation");
                chestAnimationTimer.Start();

                if (hitBoxMainCharacter.IntersectsWith(hitBoxChest))
                {
                    BossFight boss = new BossFight();
                    this.Close();
                    boss.Show();

                }
            }

        }
    }

}

