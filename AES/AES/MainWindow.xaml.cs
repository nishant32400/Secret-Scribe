using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _password = "Password1234";
        public MainWindow()
        {
            InitializeComponent();
        }
       

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.DefaultExt = ".txt";

            bool? result = op.ShowDialog();
            if (result.HasValue)
            {
                FileNameText.Text = op.FileName;
                using (StreamReader sr = new StreamReader(op.FileName))
                {
                    FileText.Text = sr.ReadToEnd();
                }
            }
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] salt = GenerateSalt();
            var localLink = FileNameText.Text.Split('.')[0] + ".aes";
            FileStream fs = new FileStream(localLink, FileMode.Create);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(_password);

            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.PKCS7;
            
            var key = new Rfc2898DeriveBytes(passwordBytes,salt,50000);
            aes.Key = key.GetBytes(aes.KeySize/8); 
            aes.IV = key.GetBytes(aes.BlockSize/8);
            aes.Mode = CipherMode.CBC;
            fs.Write(salt, 0, salt.Length);

            CryptoStream cs = new CryptoStream(fs,aes.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fsIn = new FileStream(FileNameText.Text, FileMode.Open);

            byte[] buffer = new byte[108576];
            int read;
            try
            {
                while((read = fsIn.Read(buffer,0,buffer.Length))>0)
                {
                    fsIn.Write(buffer,0, read);
                }
            }
            catch(CryptographicException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                fs.Close();
                fsIn.Close();
                using (StreamReader sr = new StreamReader(localLink))
                {
                    EncryptedText.Text = sr.ReadToEnd();
                }
                if (File.Exists(FileNameText.Text))
                {
                    File.Delete(FileNameText.Text);
                    FileNameText.Text = String.Empty;
                }
            }
        }

        private byte[] GenerateSalt()
        {
            byte[] data = new byte[32]; 
            using(RNGCryptoServiceProvider rNG = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i<10; i++)
                    rNG.GetBytes(data);


                    return data;
                
            }
        }


        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.DefaultExt = ".aes";
            bool? result = op.ShowDialog();

            if (result.HasValue && result == true)
            {
                byte[] salt = new byte[32];
                using (FileStream fs = new FileStream(op.FileName, FileMode.Open))
                {
                    fs.Read(salt, 0, salt.Length);
                }

                byte[] passwordBytes = Encoding.UTF8.GetBytes(_password);

                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;

                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Mode = CipherMode.CBC;

                FileStream fsOut = new FileStream(op.FileName.Split('.')[0] + "_decrypted.txt", FileMode.Create);
                using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    using (FileStream fsIn = new FileStream(op.FileName, FileMode.Open))
                    {
                        byte[] buffer = new byte[108576];
                        int read;
                        try
                        {
                            while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, read);
                            }
                        }
                        catch (CryptographicException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            fsOut.Close();
                            fsIn.Close();
                            using (StreamReader sr = new StreamReader(op.FileName.Split('.')[0] + "_decrypted.txt"))
                            {
                                DecryptedText.Text = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }

    }
}
