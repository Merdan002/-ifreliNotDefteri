using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using Microsoft.VisualBasic;


namespace sifrrelinotlar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Form genel görünümü
            this.Text = "🔐 Secure NotePad";
            this.BackColor = Color.FromArgb(245, 245, 245); // açık gri
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // TextBox ayarları
            txtNote.Multiline = true;
            txtNote.Font = new Font("Consolas", 12, FontStyle.Regular);
            txtNote.BackColor = Color.White;
            txtNote.ForeColor = Color.Black;
            txtNote.BorderStyle = BorderStyle.FixedSingle;
            txtNote.ScrollBars = ScrollBars.Vertical;

            // Buton tasarımları
            StyleButton(btnSave, Color.MediumSeaGreen, "💾 Kaydet ve Şifrele");
            StyleButton(btnOpen, Color.SteelBlue, "🔓 Aç ve Çöz");

            // Hover efektleri
            btnSave.MouseEnter += (s, e) => { btnSave.BackColor = Color.SeaGreen; };
            btnSave.MouseLeave += (s, e) => { btnSave.BackColor = Color.MediumSeaGreen; };
            btnOpen.MouseEnter += (s, e) => { btnOpen.BackColor = Color.DodgerBlue; };
            btnOpen.MouseLeave += (s, e) => { btnOpen.BackColor = Color.SteelBlue; };
        }
        private void StyleButton(Button btn, Color color, string text)
        {
            btn.Text = text;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Height = 40;
            btn.Width = 180;
            btn.Cursor = Cursors.Hand;
        }
        private string Encrypt(string plainText, string password)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

            return Convert.ToBase64String(bytesEncrypted);
        }

        private string Decrypt(string encryptedText, string password)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytes);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }

        private byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        private byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }
            return decryptedBytes;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Secure Note File|*.snf";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string password = Microsoft.VisualBasic.Interaction.InputBox("Lütfen şifrenizi girin:", "Şifre Gerekli");
                string encrypted = Encrypt(txtNote.Text, password);
                File.WriteAllText(sfd.FileName, encrypted);
                MessageBox.Show("Not başarıyla şifrelendi ve kaydedildi!");
            }
        }

        

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Secure Note File|*.snf";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string password = Microsoft.VisualBasic.Interaction.InputBox("Lütfen dosya şifresini girin:", "Şifre Gerekli");
                string encrypted = File.ReadAllText(ofd.FileName);
                try
                {
                    txtNote.Text = Decrypt(encrypted, password);
                    MessageBox.Show("Dosya başarıyla açıldı!");
                }
                catch
                {
                    MessageBox.Show("Şifre hatalı veya dosya bozuk!");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
