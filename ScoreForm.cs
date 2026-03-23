using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SmurfLibrary.DAL;

namespace SmurfGame
{
    public partial class ScoreForm : Form
    {
        private SmurfGameDbContext dbContext;

        public ScoreForm()
        {
            InitializeComponent();
            InitialiserFormulaire();
            ChargerScores();
        }

        private void InitialiserFormulaire()
        {
            this.ClientSize = new Size(500, 400);
            this.Text = "🏆 Tableau des Scores";
            this.BackColor = Color.FromArgb(34, 85, 34);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Titre
            Label lblTitre = new Label();
            lblTitre.Text = "🏆 MEILLEURS SCORES";
            lblTitre.Font = new Font("Arial", 18, FontStyle.Bold);
            lblTitre.ForeColor = Color.Gold;
            lblTitre.Size = new Size(400, 40);
            lblTitre.Location = new Point(50, 20);
            lblTitre.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitre);

            // DataGridView
            DataGridView dgv = new DataGridView();
            dgv.Name = "dgvScores";
            dgv.Size = new Size(440, 250);
            dgv.Location = new Point(30, 80);
            dgv.BackgroundColor = Color.FromArgb(20, 60, 20);
            dgv.ForeColor = Color.White;
            dgv.GridColor = Color.Green;
            dgv.BorderStyle = BorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 100, 0);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gold;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(20, 60, 20);
            dgv.DefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 150, 0);
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgv);

            // Bouton Fermer
            Button btnFermer = new Button();
            btnFermer.Text = "Fermer";
            btnFermer.Size = new Size(120, 40);
            btnFermer.Location = new Point(190, 345);
            btnFermer.BackColor = Color.FromArgb(0, 100, 0);
            btnFermer.ForeColor = Color.White;
            btnFermer.Font = new Font("Arial", 10, FontStyle.Bold);
            btnFermer.FlatStyle = FlatStyle.Flat;
            btnFermer.Click += (s, e) => this.Close();
            this.Controls.Add(btnFermer);

            dbContext = new SmurfGameDbContext();
        }

        private void ChargerScores()
        {
            try
            {
                DataGridView dgv = (DataGridView)this.Controls["dgvScores"];

                // Récupérer les données du Schtroumpf depuis la BD
                var scores = dbContext.Schtroumpfs
                    .OrderByDescending(s => s.Id)
                    .Take(10)
                    .ToList();

                dgv.Columns.Clear();
                dgv.Columns.Add("Id", "# Partie");
                dgv.Columns.Add("PositionX", "Position X");
                dgv.Columns.Add("PositionY", "Position Y");
                dgv.Columns.Add("Sante", "Santé finale");

                foreach (var s in scores)
                {
                    dgv.Rows.Add(s.Id, s.PositionX, s.PositionY, s.Sante);
                }
            }
            catch { }
        }
    }
}