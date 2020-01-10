using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using VideoLibrary;

namespace YtDownloader
{
    public partial class Main : Form
    {
        /* DROP SHADOW */
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
         );

        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);

        private bool m_aeroEnabled;
        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();

                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW;

                return cp;
            }
        }

        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0;
                DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:                        // box shadow
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 1,
                            rightWidth = 1,
                            topHeight = 1
                        };
                        DwmExtendFrameIntoClientArea(this.Handle, ref margins);

                    }
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)     // drag the form
                m.Result = (IntPtr)HTCAPTION;

        }
        /* /DROP SHADOW */
        public static Data.Client client;
        public Main()
        {
            m_aeroEnabled = false;

            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            client = new Data.Client();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Main_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog downloadFolder = new FolderBrowserDialog();
            downloadFolder.ShowNewFolderButton = true;
            DialogResult resultado = downloadFolder.ShowDialog();
            if (resultado == DialogResult.OK)
            {
                string path = Directory.GetCurrentDirectory() + @"\downloadfolder.txt";
                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(downloadFolder.SelectedPath);
                        client.hasPath = true;
                        client.path = downloadFolder.SelectedPath + @"\";
                        MessageBox.Show("Changes done succesfull", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else 
                {
                    File.WriteAllText(path, downloadFolder.SelectedPath);
                    client.hasPath = true;
                    client.path = downloadFolder.SelectedPath + @"\";
                    MessageBox.Show("Changes done succesfull", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else MessageBox.Show("You must choose a valid folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void CloseBtn_MouseEnter(object sender, EventArgs e)
        {
            CloseBtn.BackColor = Color.FromArgb(232, 70, 58);
        }

        private void CloseBtn_MouseLeave(object sender, EventArgs e)
        {
            CloseBtn.BackColor = Color.FromArgb(64, 64, 64);
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (client.hasPath)
            {
                if (textBox1.Text != "")
                {
                    if (radioButton1.Checked || radioButton2.Checked)
                    {
                        if (radioButton1.Checked)
                        {
                            button2.Text = "Downloading...";
                            try
                            {
                                var source = client.path;
                                var youtube = YouTube.Default;
                                var vid = youtube.GetVideo(textBox1.Text);
                                File.WriteAllBytes(source + vid.FullName + ".mp3", vid.GetBytes());

                                MessageBox.Show($"Video downloaded successfull\n{source + vid.FullName}.mp3", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                button2.Text = "Download";
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("The link format was not correct or the video has comercial content", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                button2.Text = "Download";
                            }
                        }

                        if (radioButton2.Checked)
                        {
                            button2.Text = "Downloading...";
                            try
                            {
                                var source = client.path;
                                var youtube = YouTube.Default;
                                var vid = youtube.GetVideo(textBox1.Text);
                                File.WriteAllBytes(source + vid.FullName, vid.GetBytes());

                                MessageBox.Show($"Video downloaded successfull\n{source + vid.FullName}", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                button2.Text = "Download";
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("The link format was not correct or the video has comercial content", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                button2.Text = "Download";
                            }
                        }
                    }
                    else MessageBox.Show("You have to choose a format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else MessageBox.Show("You have to enter a valid link", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("You have to choose a destination folder first", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                FolderBrowserDialog downloadFolder = new FolderBrowserDialog();
                downloadFolder.ShowNewFolderButton = true;
                DialogResult resultado = downloadFolder.ShowDialog();
                if (resultado == DialogResult.OK)
                {
                    string path = Directory.GetCurrentDirectory() + @"\downloadfolder.txt";
                    if (!File.Exists(path))
                    {
                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine(downloadFolder.SelectedPath);
                            client.hasPath = true;
                            client.path = downloadFolder.SelectedPath + @"\";
                            MessageBox.Show("Changes done succesfull", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        File.WriteAllText(path, downloadFolder.SelectedPath);
                        client.hasPath = true;
                        client.path = downloadFolder.SelectedPath + @"\";
                        MessageBox.Show("Changes done succesfull", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else MessageBox.Show("You have to choose a valid folder", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
