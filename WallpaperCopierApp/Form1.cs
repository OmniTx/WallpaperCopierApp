using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;


namespace WallpaperCopierApp
{
    public partial class WallpaperCopier : Form

    {
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        public WallpaperCopier()
        {
            InitializeComponent();

            // Load configuration
            AppConfig config = LoadConfig();

            // Set initial theme state
            this.BackColor = config.IsLightTheme ? Color.White : Color.FromArgb(45, 45, 48);
            btnTheme.Text = config.IsLightTheme ? "🌙" : "🌞"; // Set initial emoji based on theme
            lblStatus.ForeColor = config.IsLightTheme ? Color.Black : Color.White; // Set initial status label color

            // Event Handlers
            this.Load -= new EventHandler(Form1_Load);
            this.Load += new EventHandler(Form1_Load);

            this.FormClosing -= new FormClosingEventHandler(Form1_FormClosing);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);

            btnBrowse.Click -= new EventHandler(BtnBrowse_Click);
            btnBrowse.Click += new EventHandler(BtnBrowse_Click);

            btnTheme.Click -= new EventHandler(BtnTheme_Click);
            btnTheme.Click += new EventHandler(BtnTheme_Click);

            btnAbout.Click -= new EventHandler(BtnAbout_Click);
            btnAbout.Click += new EventHandler(BtnAbout_Click);

            btnSave.Click -= new EventHandler(BtnSave_Click);
            btnSave.Click += new EventHandler(BtnSave_Click);

            btnExit.Click -= new EventHandler(BtnExit_Click);
            btnExit.Click += new EventHandler(BtnExit_Click);

            this.FormBorderStyle = FormBorderStyle.None; // Remove title bar

            this.MouseDown -= new MouseEventHandler(Form1_MouseDown);
            this.MouseDown += new MouseEventHandler(Form1_MouseDown);

            this.MouseMove -= new MouseEventHandler(Form1_MouseMove);
            this.MouseMove += new MouseEventHandler(Form1_MouseMove);

            this.MouseUp -= new MouseEventHandler(Form1_MouseUp);
            this.MouseUp += new MouseEventHandler(Form1_MouseUp);

            // Add mouse event handlers for all controls to enable dragging
            foreach (Control control in this.Controls)
            {
                control.MouseDown += new MouseEventHandler(Form1_MouseDown);
                control.MouseMove += new MouseEventHandler(Form1_MouseMove);
                control.MouseUp += new MouseEventHandler(Form1_MouseUp);
            }

            // Add mouse event handlers for panel1 to enable dragging
            panel1.MouseDown += new MouseEventHandler(Form1_MouseDown);
            panel1.MouseMove += new MouseEventHandler(Form1_MouseMove);
            panel1.MouseUp += new MouseEventHandler(Form1_MouseUp);

            // Add mouse event handlers for panel2 to enable dragging
            panel2.MouseDown += new MouseEventHandler(Form1_MouseDown);
            panel2.MouseMove += new MouseEventHandler(Form1_MouseMove);
            panel2.MouseUp += new MouseEventHandler(Form1_MouseUp);
        }
        private AppConfig LoadConfig()
        {
            string configPath = Path.Combine(Path.GetTempPath(), "WC-Config");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
            }
            return new AppConfig();
        }

        private void SaveConfig(AppConfig config)
        {
            string configPath = Path.Combine(Path.GetTempPath(), "WC-Config");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(configPath, json);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            txtSaveLocation.Text = Properties.Settings.Default.LastUsedDirectory;
            LoadWallpaperPreview();
            btnTheme.Text = "🌞"; // Initialize with sun emoji
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.LastUsedDirectory = txtSaveLocation.Text;
            Properties.Settings.Default.Save();

            // Save current theme state to config
            AppConfig config = new AppConfig { IsLightTheme = (this.BackColor == Color.White) };
            SaveConfig(config);
        }
        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtSaveLocation.Text = fbd.SelectedPath;
                }
            }
        }
        private void BtnTheme_Click(object sender, EventArgs e)
        {
            bool isLightTheme = this.BackColor == Color.White;
            this.BackColor = isLightTheme ? Color.FromArgb(45, 45, 48) : Color.White; // #2d2d30 is equivalent to (45, 45, 48) in RGB
            btnTheme.Text = isLightTheme ? "🌙" : "🌞"; // Switch between sun and moon emojis

            // Update status label color based on theme
            lblStatus.ForeColor = isLightTheme ? Color.White : Color.Black;
            lblStatus.Text = "Theme switched to " + (isLightTheme ? "dark" : "light") + " mode.";

            // Save theme state to config
            AppConfig config = new AppConfig { IsLightTheme = !isLightTheme };
            SaveConfig(config);
        }
        private void BtnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Wallpaper Copier\nVersion 2.0\nCreated by Imran Ahmed");
        }
        private void BtnSave_Click(object sender, EventArgs e)
        {
            string wallpaperPath = GetCurrentWallpaper();
            if (!string.IsNullOrEmpty(wallpaperPath) && File.Exists(wallpaperPath))
            {
                SaveWallpaper(wallpaperPath);
                lblStatus.Text = "Wallpaper saved successfully!";
            }
            else
            {
                lblStatus.Text = "Failed to save wallpaper. Could not find the current wallpaper.";
            }
        }
        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(diff));
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private string GetCurrentWallpaper()
        {
            string wallpaperPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft\\Windows\\Themes\\TranscodedWallpaper"
            );
            return wallpaperPath;
        }
        private void SaveWallpaper(string sourcePath)
        {
            string saveLocation = txtSaveLocation.Text;
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            string date = DateTime.Now.ToString("MMddyyyy");
            string part = "01";
            string newFileName;
            int partNumber = 1;

            do
            {
                newFileName = $"{fileName}_01{date}{part.PadLeft(2, '0')}.jpg";
                partNumber++;
                part = partNumber.ToString();
            } while (File.Exists(Path.Combine(saveLocation, newFileName)));

            string destFile = Path.Combine(saveLocation, newFileName);
            File.Copy(sourcePath, destFile);
        }
        private void LoadWallpaperPreview()
        {
            string wallpaperPath = GetCurrentWallpaper();
            if (File.Exists(wallpaperPath))
            {
                previewBox.Image = Image.FromFile(wallpaperPath);
            }
            else
            {
                MessageBox.Show("Could not find the current wallpaper.");
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}    public class AppConfig
    {
        public bool IsLightTheme { get; set; } = true;
    }
