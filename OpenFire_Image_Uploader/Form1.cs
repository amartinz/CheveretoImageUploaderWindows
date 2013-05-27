using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Net;
using System.Collections.Specialized;
using System.Xml;

namespace OpenFire_Image_Uploader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Variables

        ImageFormat imgFormat;
        string key = "YOUR_API_KEY", format = "xml";
        string url = "YOUR_URL_TO_THE_API";
        string link = "", viewer = "", delete = "";
        DragUpload du;

        #endregion



        #region Clicks

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dlg.FileName;
                    string[] filename = dlg.FileName.Split('.');
                    setExtension(filename[filename.Length - 1]);

                    button2.Enabled = true;
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Text = "Uploading...";
            button2.Enabled = false;
            string encodedImg = ImageToBase64(Image.FromFile(textBox1.Text), imgFormat);
            sendPicture(encodedImg);
            button2.Enabled = true;
            button2.Text = "Send";
        }

        #endregion

        #region convert

        private void setExtension(string ext)
        {
            ext = ext.ToLower();
            if (ext == "png")
                imgFormat = System.Drawing.Imaging.ImageFormat.Png;
            else if (ext == "bmp")
                imgFormat = System.Drawing.Imaging.ImageFormat.Bmp;
            else if (ext == "jpeg")
                imgFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            else if (ext == "jpg")
                imgFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            else if (ext == "gif")
                imgFormat = System.Drawing.Imaging.ImageFormat.Gif;
        }
        public string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        public Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
        
        #endregion

        #region sending

        private void sendPicture(string encodedImg)
        {
            using (WebClient client = new WebClient())
            {
                byte[] response = client.UploadValues(url, new NameValueCollection()
                {
                    { "key", key },{"format",format},{"upload",encodedImg}
                });
                convertXMLtoLinks(client.Encoding.GetString(response));
            }
        }
        private void convertXMLtoLinks(string s)
        {
            XmlDocument xmlDoc = new XmlDocument(); //* create an xml document object.
            xmlDoc.LoadXml(s); //* load the XML document from the specified file.

            XmlNodeList directLink = xmlDoc.GetElementsByTagName("image_url");
            XmlNodeList viewerLink = xmlDoc.GetElementsByTagName("image_viewer");
            XmlNodeList deleteLink = xmlDoc.GetElementsByTagName("image_delete_link");

            link = directLink[0].InnerText;
            viewer = viewerLink[0].InnerText;
            delete = deleteLink[0].InnerText;

            saveLinks(link, viewer, delete);
        }
        private void saveLinks(string link, string viewer, string delete)
        {
            int count = 0;
            if (!Directory.Exists(Application.StartupPath + "\\uploaded\\"))
                Directory.CreateDirectory(Application.StartupPath + "\\uploaded\\");
            while (File.Exists(Application.StartupPath + "\\uploaded\\img_" + count + ".txt"))
            {
                count++;
            }
            StreamWriter sw = File.CreateText(Application.StartupPath + "\\uploaded\\img_" + count + ".txt");
            sw.WriteLine("Direct Link: " + link);
            sw.WriteLine("Viewer Link: " + viewer);
            sw.WriteLine("Delete Link: " + delete);
            sw.Close();

            DialogResult res = MessageBox.Show(this, "Do you want to open the Links now?", "Success!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
                System.Diagnostics.Process.Start(Application.StartupPath + "\\uploaded\\img_" + count + ".txt");
        }

        #endregion

        #region FormEvents

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(5000, "Welcome!", "Welcome to the OpenFire Image Uploader!\n\nRightclick me to use me!", ToolTipIcon.Info);
            du = new DragUpload();
            du.Show();
            tHide.Start();
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            /*if(e.Button==MouseButtons.Right)
            cmNotification.Show();*/
        }

        #region ContextMenu

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            System.Environment.Exit(0);
        }
        private void quickUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }
        private void normalUploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            du.Show();
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Placeholder for further development!", "Comming soon!");
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        private void tHide_Tick(object sender, EventArgs e)
        {
            this.Hide();
            du.Hide();
            tHide.Stop();
        }

        #endregion

        

    }
}
