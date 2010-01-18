using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PSint
{
    public partial class frMain : Form
    {
        frRun frRun1;
        private bool bReadyToCloseRun = false;

        private string path = "";

        private bool bTextChanged = false;

        public frMain()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool bWorkOn = true;

            if (bTextChanged)
            {
                switch (
                    MessageBox.Show("This source file was not saved!\n Do you want to save it?", "Unsaved Changes",
                                    MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        saveAsToolStripMenuItem_Click(null, EventArgs.Empty);
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        bWorkOn = false;
                        break;
                }
            }

            if (bWorkOn) Application.Exit();
        }

        private void dialOpen_FileOk(object sender, CancelEventArgs e)
        {
            LoadFile(dialOpen.FileName);
            bTextChanged = false;
        }

        private void LoadFile(string p)
        {
            textBox1.Text = File.ReadAllText(p, Encoding.UTF8);
            path = p;
            SetCaption();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool bWorkOn = true;

            if (bTextChanged)
            {
                switch (
                    MessageBox.Show("This source file was not saved!\n Do you want to save it?", "Unsaved Changes",
                                    MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        saveAsToolStripMenuItem_Click(null, EventArgs.Empty);
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        bWorkOn = false;
                        break;
                }
            }

            if (bWorkOn)
            {
                dialOpen.ShowDialog();
                bTextChanged = false;
            }
        }

        private void dialSave_FileOk(object sender, CancelEventArgs e)
        {
            SaveFile(dialSave.FileName);
        }

        private void SaveFile(string p)
        {
            FileStream FS = new FileStream(p, FileMode.Create);

            StreamWriter SW = new StreamWriter(FS);
            SW.Write(textBox1.Text);
            SW.Close();

            FS.Close();

            path = p;

            SetCaption();
        }

        private void SetCaption()
        {

            if (path != "")
            {
                Text = "PS interpretator - [" + Path.GetFileName(path) + "]";
            }
            else
            {
                Text = "PS interpretator - [Unknown]";
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialSave.ShowDialog();
            bTextChanged = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (path != "")
            {
                SaveFile(path);
            }
            else
                dialSave.ShowDialog();
            bTextChanged = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bTextChanged = true;
        }

        private void newProjectToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            bool bWorkOn = true;

            if (bTextChanged)
            {

                switch (
                    MessageBox.Show("This source file was not saved!\n Do you want to save it?", "Unsaved Changes",
                                    MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Yes:
                        saveAsToolStripMenuItem_Click(null, EventArgs.Empty);
                        break;
                    case DialogResult.No:
                        break;
                    case DialogResult.Cancel:
                        bWorkOn = false;
                        break;
                }
            }
            if (bWorkOn)
            {
                path = "";
                bTextChanged = false;
                textBox1.Text = "";
                SetCaption();
            }

        }

        private void runToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("IT'S RUNNING!");
            ShowRun();
            Func main = new Func(textBox1.Text);
            main.Run(this);
            frRun1.textBox2.Text += "\r\nPress ANY KEY...";
            bReadyToCloseRun = true;
            //HideRun();
        }

        private void HideRun()
        {
            frRun1.Hide();
            frRun1.Close();
            bReadyToCloseRun = false;
            this.Show();
        }

        private void ShowRun()
        {
            bReadyToCloseRun = false;
            frRun1 = new frRun(this);
            frRun1.Show();
            frRun1.textBox2.Clear();
        }     

        public void Error(string p, int n)
        {
            MessageBox.Show(p + "\nLine №" + n, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private string processVars(string sParam, Func fFunc)
        {
            string sRet = "";
            string[] splitParam = sParam.Split(' ');
            foreach (string s in splitParam)
            {
                if (s.IndexOf('@') == 0)
                {
                    sRet += fFunc.getVar(s) + " ";
                }
                else sRet += s+ " ";
            }
            sRet = sRet.Trim();
            return sRet;
        }

        public string execCmd(string cmd, string param, Func fFunc)
        {
            //Here will be (switch), which will run functions for standart cmd signatures
            try
            {
                if (cmd!="=")
                {
                    param=processVars(param,fFunc);
                }

                switch (cmd.ToLower())
                {
                    case "#function":
                        if (isCmd(param))
                        {
                            string[] sCmdPar = extractCmdParam(param);
                            param = sCmdPar[0] + execCmd(sCmdPar[1], sCmdPar[2],fFunc);
                        }
                        string funcPath = param.Split(' ')[0];
                        param = param.Substring(param.IndexOf(' ') + 1);
                        if (File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + funcPath + ".ps"))
                        {
                            string fullPath = Application.ExecutablePath + "\\" + funcPath + ".ps";
                            FileStream FS = new FileStream(fullPath, FileMode.OpenOrCreate);

                            StreamReader SR = new StreamReader(FS);
                            string funcSource = SR.ReadToEnd();
                            SR.Close();
                            FS.Close();

                            Func fnAdditional = new Func(funcSource, param);
                            string sReturn = fnAdditional.Run(this);
                            return sReturn;
                        }
                        else
                            return "Error while launching function " + funcPath;
                    case "#random"://random a b
                        if (isCmd(param))
                        {
                            string[] sCmdPar = extractCmdParam(param);
                            param = sCmdPar[0] + execCmd(sCmdPar[1], sCmdPar[2], fFunc);
                        }
                        Random rnd = new Random();
                        string[] par = param.Split(' ');
                        string ans = rnd.Next(Int32.Parse(par[0]), Int32.Parse(par[1])).ToString();
                        param = param.Substring(par[0].Length + 1 + par[1].Length);
                        param.Trim();

                        return ans + param;

                    case "#out":
                        if (isCmd(param))
                        {
                            string[] sCmdPar = extractCmdParam(param);
                            param = sCmdPar[0] + execCmd(sCmdPar[1], sCmdPar[2],fFunc);
                        }
                        frRun1.textBox2.Text += param.Replace("\\n", "\r\n");
                        frRun1.textBox2.Refresh();
                        return "";

                    case "#time":

                        if (isCmd(param))
                        {
                            string[] sCmdPar = extractCmdParam(param);
                            param = sCmdPar[0] + execCmd(sCmdPar[1], sCmdPar[2], fFunc);
                        }
                        return DateTime.Now.TimeOfDay + " " + param;

                    case "#sleep":

                        System.Threading.Thread.Sleep(Int32.Parse(param));
                        return "";

                    default:

                        throw new Exception("Err");
                    //  return "Err";
                }
            }
            catch
            {
                return "!Error!";
            }
        }

        public String[] extractCmdParam(String s)
        {
            s = " " + s;
            String[] output = new string[3];
            if (s.IndexOf(' ') >= 0)
            {
                output[0] = s.Substring(1, s.IndexOf(" #"));
                s = s.Remove(0, s.IndexOf(" #") + 1);
                output[1] = s.Substring(0, s.IndexOf(' '));
                output[2] = s.Substring(s.IndexOf(' ') + 1);
            }
            else
            {
                output[0] = "";
                output[1] = s;
                output[2] = "";
            }
            return output;
        }

        private bool isCmd(string param)
        {
            param = " " + param;
            if (param.Length != 0)
            {
                if (param.IndexOf(" #") > -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.SelectAll();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Copy();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Cut();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox1.Paste();
        }


        internal void inKbrd(string s)
        {
            if (bReadyToCloseRun)
            {
                HideRun();
            }
        }
    }
}