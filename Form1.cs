using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.InteropServices;

namespace lab7
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();
        }

        // Opens the file explorer to select a file
        private void Explorer_Button_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();
            textBox1.Text = open.FileName;
            SaveFileDialog save = new SaveFileDialog();
        }

        private void Encrypt_Button_Click(object sender, EventArgs e)
        {
            //Checks to see if the user entered a key, if not, nothing happens
            if(textBox2.Text.Length == 0)
            {
                MessageBox.Show("Please enter a key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Checks to see if the file we want to encrypt exists, if not, shows error message and nothing happens
            if (!File.Exists(textBox1.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Could not open soruce or destination file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FileStream Input = null;
            FileStream Output = null;

            // We try and open the input and output files, and catch exceptions
            try
            {
                string file = textBox1.Text + ".des";
                // Checks to see if the encrypted file exists and if it should be overwritten
                if (File.Exists(file))
                {
                    DialogResult dialogResult = MessageBox.Show("Output file exists. Overwrite?", "File Exists", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {

                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        Output.Close();
                        Input.Close();
                        return;
                    }
                }
                Input = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read);
                Output = new FileStream(textBox1.Text + ".des", FileMode.OpenOrCreate, FileAccess.Write); 
                Output.SetLength(0);

                byte[] bin = new byte[8];
                long readlen = 0;
                long totlen = Input.Length;
                int len;

                DES des = new DESCryptoServiceProvider();
                byte[] desKey = new byte[8];
                byte[] desIV;
                int location = 0;

                // Converts the characters to bytes. If longer than 8 characters, cycles through and adds subsequent characters to existing values
                for (int i = 0; i < textBox2.Text.Length; i++)
                {
                    if (i <= 8)
                    {
                        location = i % 8;
                        desKey[location] += (byte)textBox2.Text[i];
                    }
                    else
                    {
                        desKey[i] = (byte)textBox2.Text[i];
                    }
                }

                desIV = desKey;
                CryptoStream encStream = new CryptoStream(Output, des.CreateEncryptor(desKey, desIV), CryptoStreamMode.Write);

                // Encrypts and writes to the file
                while (readlen < totlen)
                {
                    len = Input.Read(bin, 0, 8);
                    encStream.Write(bin, 0, 8);
                    readlen = readlen + len;
                }

                // Closes the file and encoding
                encStream.Close();
                Output.Close();
                Input.Close();
            }
            catch
            {
                // We close both files if we catch an exception
                Output.Close();
                Input.Close();
            }
            
        }

        




        private void Decrypt_Button_Click(object sender, EventArgs e)
        {
            //Checks to see if the user entered a key, if not, nothing happens
            if (textBox2.Text.Length == 0)
            {
                MessageBox.Show("Please enter a key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FileStream Input = null;
            FileStream Output = null;
            string file;
            string filename = textBox1.Text;
            int index = filename.LastIndexOf('.');
            
            // Get the original filename without the .des extension
            if (index == -1)
            {
                file = filename;
            }
            else
            {
                file = filename.Substring(0, index);
            }

            // Check to see if the file has a .des extension. If not we show an error message
            bool extension = Path.GetExtension(filename).Equals(".des", StringComparison.InvariantCulture);
            if(!extension)
            {
                MessageBox.Show("Not a .des file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Checks to see if our input file exists
            if (!File.Exists(textBox1.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Could not open soruce or destination file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // We try opening the file and decrypting
            try
            {
                Input = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read);
                // We ask to overwrite if it exists
                if (File.Exists(file))
                {
                    DialogResult dialogResult = MessageBox.Show("Output file exists. Overwrite?", "File Exists", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {

                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        Output.Close();
                        Input.Close();
                        return;
                    }

                }
                
                Output = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write);
                Output.SetLength(0);

                byte[] bin = new byte[8];
                long readlen = 0;
                long totlen = Input.Length;
                int len;
                // Same method for encryption used for decryption
                DES des = new DESCryptoServiceProvider();
                byte[] desKey = new byte[8];
                byte[] desIV;
                int location = 0;
                for (int i = 0; i < textBox2.Text.Length; i++)
                {
                    if (i <= 8)
                    {
                        location = i % 8;
                        desKey[location] += (byte)textBox2.Text[i];
                    }
                    else
                    {
                        desKey[i] = (byte)textBox2.Text[i];
                    }
                }
                desIV = desKey;
                // We check to see if the key matches the file, if not we catch the exception and return an error message
                try
                {
                    CryptoStream encStream = new CryptoStream(Output, des.CreateDecryptor(desKey, desIV), CryptoStreamMode.Write);
                    while (readlen < totlen)
                    {
                        len = Input.Read(bin, 0, 8);
                        encStream.Write(bin, 0, 8);
                        readlen = readlen + len;
                    }

                    encStream.Close();
                    Output.Close();
                    Input.Close();
                }
                catch
                {
                    MessageBox.Show("Bad key or file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Output.Close();
                    Input.Close();
                    File.Delete(file);
                    return;
                }      
            }
            catch
            {
                Output.Close();
                Input.Close();
                File.Delete(file);             
            }
        }
    }
}
