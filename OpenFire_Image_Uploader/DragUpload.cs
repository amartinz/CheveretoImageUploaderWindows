using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace OpenFire_Image_Uploader
{
    public partial class DragUpload : Form
    {
        private ImageFormat imgFormat;
        string key = "YOUR_API_KEY", format = "xml";
        string url = "YOUR_URL_TO_THE_API";
        private string link = "", viewer = "", delete = "", date = "";
        private StringBuilder sbLinks;

        public DragUpload()
        {
            InitializeComponent();
        }

        private void DragUpload_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void lbDrag_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move;
        }

        private void lbDrag_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dateien = (string[])e.Data.GetData(DataFormats.FileDrop);
                for (int i = 0; i < dateien.Length; i++)
                {
                    string dat = dateien[i];
                    string[] filename = dateien[i].Split('.');
                    if (filename.Length >= 2)
                        if (checkExtension(filename[1]))
                        {
                            if (!lbDrag.Items.Contains(dateien[i]))
                                lbDrag.Items.Add(dateien[i]);
                        }
                }
            }
        }

        private void setExtension(string ext)
        {
            ext = ext.ToLower();
            if (ext == "png")
                imgFormat = ImageFormat.Png;
            else if (ext == "bmp")
                imgFormat = ImageFormat.Bmp;
            else if (ext == "jpeg")
                imgFormat = ImageFormat.Jpeg;
            else if (ext == "jpg")
                imgFormat = ImageFormat.Jpeg;
            else if (ext == "gif")
                imgFormat = ImageFormat.Gif;

            //MessageBox.Show(ext);
        }

        private bool checkExtension(string ext)
        {
            bool b = false;
            ext = ext.ToLower();
            if ((ext == "png") || (ext == "bmp") || (ext == "jpeg") || (ext == "jpg") || (ext == "gif"))
                b = true;
            return (b);

            //MessageBox.Show(ext);
        }

        private void bClear_Click(object sender, EventArgs e)
        {
            lbDrag.Items.Clear();
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            if (!(lbDrag.Items.Count >= 1))
            {
                MessageBox.Show("You need to add at least one Picture!", "Error!");
            }
            else
            {
                DateTime saveUtcNow = DateTime.UtcNow.ToLocalTime();
                date = saveUtcNow.Year.ToString() + saveUtcNow.Month.ToString() + saveUtcNow.Day.ToString() + "_" + saveUtcNow.Hour.ToString() + saveUtcNow.Minute.ToString() + saveUtcNow.Second.ToString();
                sbLinks = new StringBuilder();

                bSend.Text = "Uploading...";
                bSend.Enabled = false;

                for (int i = 0; i < lbDrag.Items.Count; i++)
                {
                    string s = lbDrag.Items[i].ToString();
                    string[] filename = s.Split('.');
                    setExtension(filename[filename.Length - 1]);
                    string encodedImg = ImageToBase64(Image.FromFile(s), imgFormat);
                    sendPicture(encodedImg);
                }

                saveToFile();

                bSend.Enabled = true;
                bSend.Text = "Send";
            }
        }

        private void saveToFile()
        {
            if (!Directory.Exists(Application.StartupPath + "\\uploaded\\"))
                Directory.CreateDirectory(Application.StartupPath + "\\uploaded\\");
            StreamWriter sw = File.CreateText(Application.StartupPath + "\\uploaded\\multi_img_" + date + ".txt");
            sw.WriteLine(sbLinks.ToString());
            sw.Close();

            DialogResult res = MessageBox.Show(this, "Do you want to open the Links now?", "Success!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
                System.Diagnostics.Process.Start(Application.StartupPath + "\\uploaded\\multi_img_" + date + ".txt");
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

        private void saveLinks(string link, string viewer, string delete)
        {
            sbLinks.Append("Direct Link: " + link + "\r\nViewer Link: " + viewer + "\r\nDelete Link: " + delete + "\r\n\r\n");
        }

        #region convert

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

        #endregion convert
    }
}