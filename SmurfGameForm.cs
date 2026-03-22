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

        private int score = 0;
        private int sante = 100;

        private List<PictureBox> ennemis = new List<PictureBox>();
        private Random random = new Random();

        private Image imgSpider;
        private Image imgBzzFly;
        private Image imgInsecte;

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

            // Charger images ennemis
            string dossier = Application.StartupPath + "\\Image\\";
            imgSpider = Image.FromFile(dossier + "spider.png");
            imgBzzFly = Image.FromFile(dossier + "fly.png");
            imgInsecte = Image.FromFile(dossier + "insect.png");

            // Créer le SmurfSprite
            smurf = new SmurfSprite();
            smurf.Size = new Size(80, 80);
            smurf.Location = new Point(400, 300);
            this.Controls.Add(smurf);
            smurf.BringToFront();

            // Timer du jeu
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 30;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // Timer ennemis
            ennemiTimer = new System.Windows.Forms.Timer();
            ennemiTimer.Interval = 2000;
            ennemiTimer.Tick += EnnemiTimer_Tick;
            ennemiTimer.Start();

            // Base de données
            dbContext = new SmurfGameDbContext();
        }

        private void EnnemiTimer_Tick(object sender, EventArgs e)
        {
            // Choisir un ennemi aléatoire
            int type = random.Next(3);
            Image img = type == 0 ? imgSpider : type == 1 ? imgBzzFly : imgInsecte;

            // Position aléatoire sur les bords
            int x = random.Next(0, this.ClientSize.Width - 50);
            int y = random.Next(0, this.ClientSize.Height - 50);

            PictureBox ennemi = new PictureBox();
            ennemi.Size = new Size(50, 50);
            ennemi.Location = new Point(x, y);
            ennemi.Image = img;
            ennemi.SizeMode = PictureBoxSizeMode.StretchImage;
            ennemi.BackColor = Color.Transparent;

            this.Controls.Add(ennemi);
            ennemi.BringToFront();
            smurf.BringToFront();
            ennemis.Add(ennemi);
            string typeNom = type == 0 ? "Spider" : type == 1 ? "BzzFly" : "Insecte";
            SauvegarderEnnemi(typeNom);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            score++;
            this.Text = $"Smurf Game - Score: {score} | Santé: {sante}";

            // Vérifier collisions
            foreach (PictureBox ennemi in ennemis.ToArray())
            {
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
                        MessageBox.Show($"Game Over! Score final: {score}", "Smurf Game");
                        Application.Exit();
                    }
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
                    if (smurf.Bottom < this.ClientSize.Height - limite) smurf.DeplacerBas();
                    return true;
                case Keys.Left:
                    if (smurf.Left > limite) smurf.DeplacerGauche();
                    return true;
                case Keys.Right:
                    if (smurf.Right < this.ClientSize.Width - limite) smurf.DeplacerDroite();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void SauvegarderSchtroumpf()
        {
            try
            {
                var schtroumpfBD = new Schtroumpf
                {
                    Nom = "Schtroumpf",
                    Sante = sante,
                    PositionX = smurf.Left,
                    PositionY = smurf.Top
                };
                dbContext.Schtroumpfs.Add(schtroumpfBD);
                dbContext.SaveChanges();
            }
            catch { }
        }

        private void SauvegarderEnnemi(string type)
        {
            try
            {
                if (type == "Spider")
                {
                    var spider = new Spider { Nom = "Spider", Sante = 50, PositionX = 0, PositionY = 0 };
                    dbContext.Spiders.Add(spider);
                }
                else if (type == "BzzFly")
                {
                    var fly = new BzzFly { Nom = "BzzFly", Sante = 30, PositionX = 0, PositionY = 0 };
                    dbContext.BzzFlies.Add(fly);
                }
                else if (type == "Insecte")
                {
                    var insecte = new BzzFly { Nom = "Insecte", Sante = 20, PositionX = 0, PositionY = 0 };
                    dbContext.BzzFlies.Add(insecte);
                   
                }
                dbContext.SaveChanges();
            }
            catch { }
        }
    }
}