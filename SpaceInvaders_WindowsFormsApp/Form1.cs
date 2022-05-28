using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace SpaceInvaders_WindowsFormsApp
{
    public partial class Form1 : Form
    {
        // Variable game parameters
        private int playerLives = 3;
        private int playerFireCooldown = 1; // seconds
        private int playerForceUpAmount = 50; // pixels
        private int playerShift = 5; // pixels per timer1 interval
        private int laserShift = 10; // pixels per timer1 interval
        private int laserWidth = 4;
        private int laserHeight = 25; // pixels
        private int objectSize = 50; // pixels
        private int spacing = 50; // pixels

        // Rewarding
        private int killReward = 50;
        private int jumpReward = 1;
        
        // Code variables
        private int score = 0;
        private int wave = 0;
        private int playerForceUp = 0;
        private string playerDirection = "right";
        int calculatedSpaces = 0;
        int[] spacesInForm = null;
        private bool laserCountdown = false;
        private int laserCountdownMillis = 0;
        
        public Form1()
        {
            InitializeComponent();

            // Calculates how many objects can fit side by side in the application
            calculatedSpaces = getAppWidth() / objectSize;
            //and then creates an array with coordinates
            spacesInForm = new int[calculatedSpaces];
            for (int i = 0; i < calculatedSpaces; i++)
            {
                spacesInForm[i] = i * objectSize;
            }
            
            // Init game
            spawnPlayer();
            spawnInvaders(20, 5);
        }
        
        public Control getPlayer()
        {
            try
            {
                return Controls.Find("playerPictureBox", true)[0];
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public int getAppWidth()
        {
            return Width;
        }
        public Int32 getUnixTimestamp()
        {
            return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public int getInvadersLeft()
        {
            int invadersCounter = 0;
            foreach (Control con in Controls)
            {
                if (con.Tag == "invader")
                {
                    invadersCounter++;
                }
            }

            return invadersCounter;
        }

        public void subtractPlayerLife()
        {
            playerLives--;
            updatePlayerLife();
            if (playerLives <= 0)
            {
                getPlayer().Visible = false;
                gameOver(false);
            }
        }

        public void gameOver(bool win)
        {
            timer1.Stop();
            timer2.Stop();
            var gameOverLabel = new Label
            {
                Name = "gameOverLabel",
                Text = "Game Over!",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 20),
                ForeColor = Color.Red,
                Size = new Size(Width, objectSize),
                Location = new Point(0, ((Height / 2) - objectSize)),
                BackColor = Color.Transparent,
            };
            if (win)
            {
                gameOverLabel.Text = "Congratulations, you win.";
                gameOverLabel.ForeColor = Color.White;
            }
            this.Controls.Add(gameOverLabel);
        }

        public void updatePlayerLife()
        {
            foreach (Control con in Controls)
            {
                if (con.Tag == "heart")
                {
                    Form1.ActiveForm.Controls.Remove(con);
                }
            }
            foreach (Control con in Controls)
            {
                if (con.Tag == "heart")
                {
                    Form1.ActiveForm.Controls.Remove(con);
                }
            }
            for (int i = 0; i < playerLives; i++)
            {
                var heartPictureBox = new PictureBox
                {
                    Name = "heartPictureBox" + i,
                    Tag = "heart",
                    Size = new Size(objectSize, objectSize),
                    Location = new Point(i * objectSize, (Height - 100)),
                    Image = Image.FromFile(Application.StartupPath + "\\Media\\Images\\heart.png"),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent,
                };
                this.Controls.Add(heartPictureBox);
            }
        }
        public void addScore(string method)
        {
            if (method == "kill")
            {
                score += killReward;
            }

            if (method == "jump")
            {
                score += jumpReward;
            }

            scoreLabel.Text = score.ToString();
        }
        
        public void spawnPlayer()
        {
            var playerPictureBox = new PictureBox
            {
                Name = "playerPictureBox",
                Tag = "player",
                Size = new Size(objectSize / 2, objectSize / 2),
                Location = new Point(0, (Height / 2)),
                Image = Image.FromFile(Application.StartupPath + "\\Media\\Images\\player.png"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
            };
            this.Controls.Add(playerPictureBox);
            updatePlayerLife();
        }
        public void spawnInvaders(int amount, int rows)
        {
            int formationRow = 1;
            int invadersRowAmount = 0;
            for (int i = 0; i < amount; i++)
            {
                if (calculatedSpaces <= invadersRowAmount)
                {
                    formationRow++;
                    invadersRowAmount = 0;
                }

                var invaderPictureBox = new PictureBox
                {
                    Name = "invader" + i + "PictureBox" + wave,
                    Tag = "invader",
                    Size = new Size(objectSize, objectSize),
                    Location = new Point(spacesInForm[invadersRowAmount], (formationRow * spacing)),
                    Image = Image.FromFile(Application.StartupPath + "\\Media\\Images\\invader.png"),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent,
                };
                this.Controls.Add(invaderPictureBox);
                invadersRowAmount++;
            }
        }

        public void spawnLaser(string side, int posX, int posY)
        {
            posX = posX + objectSize / 2;
            var laserPanel = new Panel();
            if (side == "invader")
            {
                laserPanel = new Panel
                {
                    Name = "laser" + posX + "Panel",
                    Tag = "laser-invader",
                    Size = new Size(laserWidth, laserHeight),
                    Location = new Point(posX, posY),
                    BackColor = Color.Red,
                };
            }
            else
            {
                laserPanel = new Panel
                {
                    Name = "laser" + posX + "Panel",
                    Tag = "laser-player",
                    Size = new Size(laserWidth, laserHeight),
                    Location = new Point(posX, posY),
                    BackColor = Color.GreenYellow,
                };
            }
            this.Controls.Add(laserPanel);

        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Space)
            {
                playerForceUp = playerForceUpAmount;
                addScore("jump");
            }
            // Fire laser
            if (e.KeyData == Keys.W)
            {
                if (!laserCountdown)
                {
                    laserCountdown = true;
                    laserCountdownMillis = getUnixTimestamp();
                    spawnLaser("player", getPlayer().Location.X, getPlayer().Location.Y);
                }
            }
        }

        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Detects a collision with a second spacecraft
            foreach (Control c in Controls)
            {
                if (c.Tag == "invader")
                {
                    if (c.Bounds.IntersectsWith(getPlayer().Bounds))
                    {
                        addScore("kill");
                        Form1.ActiveForm.Controls.Remove(c);
                        subtractPlayerLife();
                    }
                }
            }
            // Laser shooting movement
            foreach (Control c in Controls)
            {
                if (c.Tag == "laser-player")
                {
                    if (c.Location.Y < 0 || c.Location.Y > Height)
                    {
                        Form1.ActiveForm.Controls.Remove(c);
                    }
                    else
                    {
                        c.Location = new Point(c.Location.X, c.Location.Y - laserShift);
                        foreach (Control con in Controls)
                        {
                            if (c.Bounds.IntersectsWith(con.Bounds))
                            {
                                if (con.Tag == "invader")
                                {
                                    addScore("kill");
                                    Form1.ActiveForm.Controls.Remove(con);
                                    Form1.ActiveForm.Controls.Remove(c);
                                }
                            }   
                        }   
                    }
                }
                if (c.Tag == "laser-invader")
                {
                    if (c.Location.Y < 0 || c.Location.Y > Height)
                    {
                        Form1.ActiveForm.Controls.Remove(c);
                    }
                    else
                    {
                        c.Location = new Point(c.Location.X, c.Location.Y + laserShift);
                        foreach (Control con in Controls)
                        {
                            if (c.Bounds.IntersectsWith(con.Bounds))
                            {
                                if (con.Tag == "player")
                                {
                                    Form1.ActiveForm.Controls.Remove(c);
                                    subtractPlayerLife();
                                }
                            }   
                        }
                    }
                }
            }
            
            // Laser countdown
            if (laserCountdown)
            {
                if (getUnixTimestamp() - laserCountdownMillis >= playerFireCooldown)
                {
                    laserCountdown = false;
                }
            }

            // Player jumping / falling
            if (playerForceUp > 0)
            {
                int substractedForce = playerForceUp / 2;
                playerForceUp = substractedForce;
                getPlayer().Location = new Point(getPlayer().Location.X, getPlayer().Location.Y - substractedForce);
            }
            else
            {
                getPlayer().Location = new Point(getPlayer().Location.X, getPlayer().Location.Y + playerShift);
                if (getPlayer().Location.Y >= Height)
                {
                    subtractPlayerLife();
                }
            }

            // Scrolling player vertically
            if (getPlayer().Location.X <= 0)
            {
                playerDirection = "right";
            }

            if (getPlayer().Location.X + objectSize + playerShift >= getAppWidth())
            {
                playerDirection = "left";
            }

            if (playerDirection == "right")
            {
                getPlayer().Location = new Point(getPlayer().Location.X + playerShift, getPlayer().Location.Y);
            }
            if (playerDirection == "left")
            {
                getPlayer().Location = new Point(getPlayer().Location.X - playerShift, getPlayer().Location.Y);
            }
        }

        private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Enemy fire
            if (getInvadersLeft() <= 0)
            {
                gameOver(true);
            }
            else
            {
                Control[] invaders = new Control[getInvadersLeft()];
                int invadersCounter = 0;
                foreach (Control con in Controls)
                {
                    if (con.Tag == "invader")
                    {
                        invaders[invadersCounter] = con;
                        invadersCounter++;
                    }
                }

                Random rnd = new Random();
                Control selectedInvader = invaders[rnd.Next(0, invaders.Length)];
                spawnLaser("invader", selectedInvader.Location.X, selectedInvader.Location.Y);
            }

        }
    }
}