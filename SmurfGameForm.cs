using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SmurfControl;
using SmurfLibrary.BL.Entités;
using SmurfLibrary.DAL;

namespace SmurfGame
{
    public partial class SmurfGameForm : Form
    {
        private SmurfSprite smurf;
        private SmurfGameDbContext dbContext;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Timer ennemiTimer;
        private System.Windows.Forms.Timer itemTimer;

        private int score = 0;
        private int sante = 100;

        private List<PictureBox> ennemis = new List<PictureBox>();
        private List<PictureBox> items = new List<PictureBox>();
        private Random random = new Random();

        private Image imgSpider;
        private Image imgBzzFly;
        private Image imgInsecte;
        private Image imgBluePotion;
        private Image imgRedPotion;
        private Image imgBerry;

        public SmurfGameForm()
        {
            InitializeComponent();
            InitialiserJeu();
        }

        private void InitialiserJeu()
        {
            this.ClientSize = new Size(800, 600);
            this.BackColor = Color.ForestGreen;
            this.Text = "Smurf Game - La Forêt Magique";
            this.KeyPreview = true;
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint,
                true);
            this.UpdateStyles();

            this.BackgroundImage = Image.FromFile(
                Application.StartupPath + "\\Image\\foret.jpg");
            this.BackgroundImageLayout = ImageLayout.Stretch;

            string dossier = Application.StartupPath + "\\Image\\";
            imgSpider = Image.FromFile(dossier + "spider.png");
            imgBzzFly = Image.FromFile(dossier + "fly.png");
            imgInsecte = Image.FromFile(dossier + "insect.png");
            imgBluePotion = Image.FromFile(dossier + "bluepotion.png");
            imgRedPotion = Image.FromFile(dossier + "redpotion.png");
            imgBerry = Image.FromFile(dossier + "berry.png");

            smurf = new SmurfSprite();
            smurf.Size = new Size(120, 130);
            smurf.Location = new Point(400, 300);
            this.Controls.Add(smurf);
            smurf.BringToFront();

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 30;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            ennemiTimer = new System.Windows.Forms.Timer();
            ennemiTimer.Interval = 2000;
            ennemiTimer.Tick += EnnemiTimer_Tick;
            ennemiTimer.Start();

            itemTimer = new System.Windows.Forms.Timer();
            itemTimer.Interval = 3000;
            itemTimer.Tick += ItemTimer_Tick;
            itemTimer.Start();

            dbContext = new SmurfGameDbContext();
            // Bouton scores
            Button btnScores = new Button();
            btnScores.Text = "🏆 Scores";
            btnScores.Size = new Size(100, 35);
            btnScores.Location = new Point(10, 10);
            btnScores.BackColor = Color.FromArgb(0, 100, 0);
            btnScores.ForeColor = Color.White;
            btnScores.Font = new Font("Arial", 9, FontStyle.Bold);
            btnScores.FlatStyle = FlatStyle.Flat;
            btnScores.Click += (s, e) =>
            {
                gameTimer.Stop();
                ennemiTimer.Stop();
                itemTimer.Stop();
                new ScoreForm().ShowDialog();
                gameTimer.Start();
                ennemiTimer.Start();
                itemTimer.Start();
            };
            this.Controls.Add(btnScores);
            btnScores.BringToFront();
        }

        private void ItemTimer_Tick(object sender, EventArgs e)
        {
            if (items.Count >= 3) return;

            int type = random.Next(3);
            Image img = type == 0 ? imgBluePotion :
                        type == 1 ? imgRedPotion : imgBerry;

            int x = random.Next(50, this.ClientSize.Width - 50);
            int y = random.Next(50, this.ClientSize.Height - 50);

            PictureBox item = new PictureBox();
            item.Size = new Size(40, 40);
            item.Location = new Point(x, y);
            item.Image = img;
            item.SizeMode = PictureBoxSizeMode.StretchImage;
            item.BackColor = Color.Transparent;
            item.Tag = type == 0 ? "Blue" :
                       type == 1 ? "Red" : "Berry";

            this.Controls.Add(item);
            item.BringToFront();
            smurf.BringToFront();
            items.Add(item);
        }

        private void EnnemiTimer_Tick(object sender, EventArgs e)
        {
            if (ennemis.Count >= 5) return;

            int type = random.Next(3);
            Image img = type == 0 ? imgSpider :
                        type == 1 ? imgBzzFly : imgInsecte;

            int x = random.Next(0, this.ClientSize.Width - 50);
            int y = random.Next(0, this.ClientSize.Height - 50);

            PictureBox ennemi = new PictureBox();
            ennemi.Size = new Size(40, 40);
            ennemi.Location = new Point(x, y);
            ennemi.Image = img;
            ennemi.SizeMode = PictureBoxSizeMode.StretchImage;
            ennemi.BackColor = Color.Transparent;

            this.Controls.Add(ennemi);
            ennemi.BringToFront();
            smurf.BringToFront();
            ennemis.Add(ennemi);

            string typeNom = type == 0 ? "Spider" :
                             type == 1 ? "BzzFly" : "Insecte";
            SauvegarderEnnemi(typeNom);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            score++;
            this.Text = $"Smurf Game - Score: {score} | Santé: {sante}";

            // Déplacer ennemis vers le Schtroumpf + collision
            foreach (PictureBox ennemi in ennemis.ToArray())
            {
                int dx = smurf.Left - ennemi.Left;
                int dy = smurf.Top - ennemi.Top;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance > 0)
                {
                    ennemi.Left += (int)(dx / distance * 2);
                    ennemi.Top += (int)(dy / distance * 2);
                }

                if (smurf.Bounds.IntersectsWith(ennemi.Bounds))
                {
                    sante -= 10;
                    SauvegarderSchtroumpf();
                    this.Controls.Remove(ennemi);
                    ennemis.Remove(ennemi);

                    if (sante <= 0)
                    {
                        gameTimer.Stop();
                        ennemiTimer.Stop();
                        itemTimer.Stop();
                        MessageBox.Show(
                            $"Game Over! Score final: {score}",
                            "Smurf Game");
                        Application.Exit();
                    }
                    break;
                }
            }

            // Collision avec items
            foreach (PictureBox item in items.ToArray())
            {
                if (smurf.Bounds.IntersectsWith(item.Bounds))
                {
                    string tag = item.Tag.ToString();

                    if (tag == "Blue")
                        sante = Math.Min(100, sante + 20);
                    else if (tag == "Red")
                        sante = Math.Min(100, sante + 10);
                    else if (tag == "Berry")
                        sante = Math.Min(100, sante + 5);

                    SauvegarderItem(tag);
                    SauvegarderSchtroumpf();
                    this.Controls.Remove(item);
                    items.Remove(item);
                    break;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            int limite = 10;
            switch (keyData)
            {
                case Keys.Up:
                    if (smurf.Top > limite) smurf.DeplacerHaut();
                    return true;
                case Keys.Down:
                    if (smurf.Bottom < this.ClientSize.Height - limite)
                        smurf.DeplacerBas();
                    return true;
                case Keys.Left:
                    if (smurf.Left > limite) smurf.DeplacerGauche();
                    return true;
                case Keys.Right:
                    if (smurf.Right < this.ClientSize.Width - limite)
                        smurf.DeplacerDroite();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SauvegarderSchtroumpf()
        {
            try
            {
                dbContext.Schtroumpfs.Add(new Schtroumpf
                {
                    Nom = "Schtroumpf",
                    Sante = sante,
                    PositionX = smurf.Left,
                    PositionY = smurf.Top
                });
                dbContext.SaveChanges();
            }
            catch { }
        }

        private void SauvegarderEnnemi(string type)
        {
            try
            {
                if (type == "Spider")
                    dbContext.Spiders.Add(new Spider
                    {
                        Nom = "Spider",
                        Sante = 50,
                        PositionX = 0,
                        PositionY = 0
                    });
                else if (type == "BzzFly")
                    dbContext.BzzFlies.Add(new BzzFly
                    {
                        Nom = "BzzFly",
                        Sante = 30,
                        PositionX = 0,
                        PositionY = 0
                    });
                else if (type == "Insecte")
                    dbContext.BzzFlies.Add(new BzzFly
                    {
                        Nom = "Insecte",
                        Sante = 20,
                        PositionX = 0,
                        PositionY = 0
                    });
                dbContext.SaveChanges();
            }
            catch { }
        }

        private void SauvegarderItem(string type)
        {
            try
            {
                if (type == "Blue")
                    dbContext.BluePotions.Add(new BluePotion
                    {
                        Nom = "BluePotion",
                        BonusSante = 20,
                        PositionX = smurf.Left,
                        PositionY = smurf.Top
                    });
                else if (type == "Red")
                    dbContext.RedPotions.Add(new RedPotion
                    {
                        Nom = "RedPotion",
                        BonusSante = 10,
                        PositionX = smurf.Left,
                        PositionY = smurf.Top
                    });
                else if (type == "Berry")
                    dbContext.Berries.Add(new Berry
                    {
                        Nom = "Berry",
                        BonusSante = 5,
                        PositionX = smurf.Left,
                        PositionY = smurf.Top
                    });
                dbContext.SaveChanges();
            }
            catch { }
        }

        private void SmurfGameForm_Load(object sender, EventArgs e)
        {

        }
    }
}