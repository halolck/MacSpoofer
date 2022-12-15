using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Security.Permissions;
using System.Management;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System;
using System.Diagnostics;
using System.Threading;

namespace MacTempSpoofer
{
    public partial class Form1 : Form
    {
        string newmac = "";
        string outpath = "";
        int index = 0;
        int delay = 0;
        bool finishcheck = false;
        bool argsflug = false;
        public Form1(string[] args)
        {
            InitializeComponent();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(
                    a => Adapter.IsValidMac(a.GetPhysicalAddress().GetAddressBytes(), true)
                ).OrderByDescending(a => a.Speed))
            {
                comboBox1.Items.Add(new Adapter(adapter));
            }
            comboBox1.SelectedIndex = 0;
            SettingLoad();
            if (args.Length != 0)
            {
                argsflug = true;
                newmac = args[0];
                outpath = args[1];
                index = int.Parse(args[2]);
                delay = int.Parse(args[3]);
                finishcheck = bool.Parse(args[4]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (argsflug)
            {
                this.Visible = false;
                this.Update();
                textBox2.Text = newmac;
                textBox3.Text = outpath;
                numericUpDown1.Value = delay;
                checkBox2.Checked = finishcheck;
                this.Visible = !finishcheck;
                this.Update();
                StartButton();
                MessageBox.Show("aaa");
            }

        }
        private void UpdateAddresses()
        {
            Adapter a = comboBox1.SelectedItem as Adapter;
            this.textBox1.Text = a.Mac.Replace("-", "").ToUpper();
           // this.textBox2.Text = a.RegistryMac;
            if (textBox2.Text.Contains('-'))
            {
                //this.textBox2.Text = a.RegistryMac.Replace("-", "").ToUpper();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAddresses();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartButton();
            

        }
        private async void StartButton()
        {
            this.Enabled = false;
            this.Update();
            SetRegistryMac(textBox2.Text);
            if (!checkBox1.Checked)
            {
                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.FileName = textBox3.Text;
                await Task.Delay((int)numericUpDown1.Value * 1000);

                Process.Start(pInfo);
                Process[] proce = Process.GetProcessesByName(textBox3.Text);
                while (proce.Length != 0)
                {
                    proce = Process.GetProcessesByName(textBox3.Text);
                    Thread.Sleep((int)numericUpDown1.Value * 1000);

                }
                Thread.Sleep((int)numericUpDown1.Value * 1000);

                SetRegistryMac("");
                ResultDialog rd = new ResultDialog("Spoof Success!");
                rd.Show();
                SettingSave();
                await Task.Delay(3000);
                if (checkBox2.Checked)
                {
                    Application.Exit();
                }
            }
            else
            {
                ResultDialog rd = new ResultDialog("Perment Spoof Success!");
                rd.Show();
                if (checkBox2.Checked && argsflug == true)
                {
                    await Task.Delay(1000);
                    Application.Exit();
                }

            }
            this.Enabled = true;
        }

        private void FirstSetRegistryMac(string mac,string name)
        {
            object obconv = name;
            Adapter a = obconv as Adapter;

            if (a.SetRegistryMac(mac))
            {
                System.Threading.Thread.Sleep(100);
                UpdateAddresses();


            }
        }

        private void SetRegistryMac(string mac)
        {
            Adapter a = comboBox1.SelectedItem as Adapter;

            if (a.SetRegistryMac(mac))
            {
                System.Threading.Thread.Sleep(100);
                UpdateAddresses();
                if (mac == "")
                {
                    ResultDialog rd = new ResultDialog("Reset Success!");
                    rd.Show();
                }
                
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            this.Update();
            textBox2.Text = "";

            SetRegistryMac("");
            this.Enabled = true;
            SettingResetSave();
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Enabled = !checkBox1.Checked;
            checkBox2.Enabled = !checkBox1.Checked;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == String.Empty || textBox3.Text == string.Empty)
            {
                MessageBox.Show("空白部分があります。");
                return;

            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ショートカットファイル (*.lnk)|*.lnk|All files (*.*)|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //作成するショートカットのパス
                string shortcutPath = System.IO.Path.Combine(sfd.FileName);
                //ショートカットのリンク先
                string targetPath = Application.ExecutablePath;

                //WshShellを作成
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                //ショートカットのパスを指定して、WshShortcutを作成
                IWshRuntimeLibrary.IWshShortcut shortcut =
                    (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                //リンク先
                shortcut.TargetPath = targetPath;
                //コマンドパラメータ 「リンク先」の後ろに付く
                shortcut.Arguments = $"{textBox2.Text} {textBox3.Text} {comboBox1.SelectedIndex} {numericUpDown1.Value} {checkBox2.Checked}";
                //作業フォルダ
                shortcut.WorkingDirectory = Application.StartupPath;
                //ショートカットキー（ホットキー）
                //実行時の大きさ 1が通常、3が最大化、7が最小化
                shortcut.WindowStyle = 1;
                //コメント
                shortcut.Description = "テストのアプリケーション";
                //アイコンのパス 自分のEXEファイルのインデックス0のアイコン
                shortcut.IconLocation = Application.ExecutablePath + ",0";

                //ショートカットを作成
                shortcut.Save();

                //後始末
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shortcut);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
                MessageBox.Show("ショートカットの作成が完了しました。\r\nショートカットを実行すると自動的にspoofingされたアプリケーションが実行されます。","ショートカットの作成");
            }
            
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "実行ファイル|*.exe";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = ofd.FileName;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {

        }
        private void SettingResetSave()
        {

            Settings appSettings = new Settings();

            appSettings.Savemac = textBox1.Text;
            appSettings.Outputpath = textBox3.Text;
            appSettings.Delay = (int)numericUpDown1.Value;
            appSettings.Index = comboBox1.SelectedIndex;
            appSettings.Finish = checkBox2.Checked;
            //保存先のファイル名
            string fileName = @"settings.config";

            //＜XMLファイルに書き込む＞
            //XmlSerializerオブジェクトを作成
            //書き込むオブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer1 =
                new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //ファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer1.Serialize(sw, appSettings);
            //閉じる
            sw.Close();

        }
        private void SettingSave()
        {

            Settings appSettings = new Settings();

            appSettings.Savemac = textBox2.Text;
            appSettings.Outputpath = textBox3.Text;
            appSettings.Delay = (int)numericUpDown1.Value;
            appSettings.Index = comboBox1.SelectedIndex;
            appSettings.Finish = checkBox2.Checked;
            //保存先のファイル名
            string fileName = @"settings.config";

            //＜XMLファイルに書き込む＞
            //XmlSerializerオブジェクトを作成
            //書き込むオブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer1 =
                new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //ファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                fileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer1.Serialize(sw, appSettings);
            //閉じる
            sw.Close();

        }

        private void SettingLoad()
        {
            try
            {
                string fileName = @"settings.config";
                Settings appSettings = new Settings();
                System.Xml.Serialization.XmlSerializer serializer2 = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                System.IO.StreamReader sr = new System.IO.StreamReader(
                    fileName, new System.Text.UTF8Encoding(false));
                appSettings =
                    (Settings)serializer2.Deserialize(sr);
                textBox2.Text = appSettings.Savemac;
                textBox3.Text = appSettings.Outputpath;
                numericUpDown1.Value = appSettings.Delay;
                comboBox1.SelectedIndex = appSettings.Index;
                checkBox2.Checked = appSettings.Finish;
                sr.Close();
            }
            catch
            {

            }
            

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();

        }
    }

    public class Adapter
    {
        public ManagementObject adapter;
        public string adaptername;
        public string customname;
        public int devnum;

        public Adapter(ManagementObject a, string aname, string cname, int n)
        {
            this.adapter = a;
            this.adaptername = aname;
            this.customname = cname;
            this.devnum = n;
        }

        public Adapter(NetworkInterface i) : this(i.Description) { }

        public Adapter(string aname)
        {
            this.adaptername = aname;

            var searcher = new ManagementObjectSearcher("select * from win32_networkadapter where Name='" + adaptername + "'");
            var found = searcher.Get();
            this.adapter = found.Cast<ManagementObject>().FirstOrDefault();

            // Extract adapter number; this should correspond to the keys under
            // HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}
            try
            {
                var match = Regex.Match(adapter.Path.RelativePath, "\\\"(\\d+)\\\"$");
                this.devnum = int.Parse(match.Groups[1].Value);
            }
            catch
            {
                return;
            }

            // Find the name the user gave to it in "Network Adapters"
            this.customname = NetworkInterface.GetAllNetworkInterfaces().Where(
                i => i.Description == adaptername
            ).Select(
                i => " (" + i.Name + ")"
            ).FirstOrDefault();
        }

        public NetworkInterface ManagedAdapter
        {
            get
            {
                return NetworkInterface.GetAllNetworkInterfaces().Where(
                    nic => nic.Description == this.adaptername
                ).FirstOrDefault();
            }
        }

        public string Mac
        {
            get
            {
                try
                {
                    return BitConverter.ToString(this.ManagedAdapter.GetPhysicalAddress().GetAddressBytes()).Replace("-", "").ToUpper();
                }
                catch { return null; }
            }
        }

        public string RegistryKey
        {
            get
            {
                return String.Format(@"SYSTEM\ControlSet001\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{0:D4}", this.devnum);
            }
        }

        public string RegistryMac
        {
            get
            {
                try
                {
                    using (RegistryKey regkey = Registry.LocalMachine.OpenSubKey(this.RegistryKey, false))
                    {
                        if (regkey.GetValue("NetworkAddress") != null)
                        {
                            return regkey.GetValue("NetworkAddress").ToString();

                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool SetRegistryMac(string value)
        {
            bool shouldReenable = false;
            try
            {
                

                using (RegistryKey regkey = Registry.LocalMachine.OpenSubKey(this.RegistryKey, true))
                {
                    if (regkey.GetValue("AdapterModel") as string != this.adaptername
                        && regkey.GetValue("DriverDesc") as string != this.adaptername)
                        throw new Exception("Adapter not found in registry");

                    string question = value.Length > 0 ?
                        "Changing MAC-adress of adapter {0} from {1} to {2}. Proceed?" :
                        "Clearing custom MAC-address of adapter {0}. Proceed?";


                    var result = (uint)adapter.InvokeMethod("Disable", null);
                    if (result != 0)
                        throw new Exception("Failed to disable network adapter.");

                    // If we're here the adapter has been disabled, so we set the flag that will re-enable it in the finally block
                    shouldReenable = true;

                    if (regkey != null)
                    {
                        if (value.Length > 0)
                            regkey.SetValue("NetworkAddress", value, RegistryValueKind.String);
                        else
                            regkey.DeleteValue("NetworkAddress");
                    }

                    return true;
                }
            }

            catch (Exception ex)
            {
                if (value == "")
                {
                    ResultDialog rd = new ResultDialog("Already Original");
                    rd.Show();
                    return false;
                }

                MessageBox.Show(ex.ToString());
                return false;
            }

            finally
            {
                if (shouldReenable)
                {
                    uint result = (uint)adapter.InvokeMethod("Enable", null);
                    if (result != 0 && value != "")
                        MessageBox.Show("Failed to re-enable network adapter.");
                }
            }
        }

        public override string ToString()
        {
            return this.adaptername + this.customname;
        }

        public static string GetNewMac()
        {
            System.Random r = new System.Random();
            ///*  Generate first byte:
            //    - Take 6 random bits
            //    - Shift them two bits to the right
            //    - Make the least significant two bits 10
            //    In hex, the resulting byte should end with 2, 6, A or E */
            //long firstbyte = (r.Next(63) << 2) + 2;

            //// Generate second byte
            //long firsttwobytes = (firstbyte << 8) + r.Next(255);

            //// Shift the first two bytes to be the first two bytes of the mac address
            //long mac = firsttwobytes << 32;

            //// Choose the remaining four bytes randomly
            //mac = mac + r.Next();

            //return mac.ToString("x12").ToUpper();

            byte[] bytes = new byte[6];
            r.NextBytes(bytes);

            // Set second bit to 1
            bytes[0] = (byte)(bytes[0] | 0x02);
            // Set first bit to 0
            bytes[0] = (byte)(bytes[0] & 0xfe);

            return MacToString(bytes);
        }

        public static bool IsValidMac(string mac, bool actual)
        {
            // 6 bytes == 12 hex characters (without dashes/dots/anything else)
            if (mac.Length != 12)
                return false;

            // Should be uppercase
            if (mac != mac.ToUpper())
                return false;

            // Should not contain anything other than hexadecimal digits
            if (!Regex.IsMatch(mac, "^[0-9A-F]*$"))
                return false;

            if (!actual) // The second character should be a 2, 6, A or E
            {
                char c = mac[1];
                char b = mac[0];
                return (c == '2' || c == '6' || c == 'A' || c == 'E' || (b == '0' && c == '0'));
            }

            return true;
        }

        public static bool IsValidMac(byte[] bytes, bool actual)
        {
            return IsValidMac(Adapter.MacToString(bytes), actual);
        }

        public static string MacToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
        }

        
    }
    public class Settings
    {
        private string _savemac;
        private int _delay;
        private string _outputpath;
        private int _index;
        private bool _finish;




        public string Savemac
        {
            get { return _savemac; }
            set { _savemac = value; }
        }

        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }
        public string Outputpath
        {
            get { return _outputpath; }
            set { _outputpath = value; }
        }
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public bool Finish
        {
            get { return _finish; }
            set { _finish = value; }
        }

        public Settings()
        {
            _savemac = "";
            _delay = 3;
            _outputpath = "";
            _index = 0;
            _finish = false;
        }



    }
}
