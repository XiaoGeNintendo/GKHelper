using Microsoft.VisualBasic;
using Newtonsoft.Json;
using ReaLTaiizor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GKHelper
{
    public partial class form : Form
    {

        List<Lesson> lessons=new List<Lesson>();
        DateTime gk;
        bool wallpaper = false;
        IntPtr programIntPtr;
        List<Label> lessonLabels = new List<Label>();
        List<Label> lessonLabels2 = new List<Label>();
        string backgroundImagePath = null;

        Font lessonLabelFont = new System.Drawing.Font("华文中宋", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));

        Color colorDayEnd=Color.Gray, colorFinish=Color.Red, colorNext=Color.Green,colorNow=Color.Blue,colorSoon=Color.Lime;
        public DateTime AppendTime(DateTime date, string str)
        {
            string[] info = str.Split(':');
            return date.AddHours(Double.Parse(info[0])).AddMinutes(Double.Parse(info[1]));
        }

        public void ReadTimetable(string name)
        {
            lessons.Clear();
            foreach(Label i in lessonLabels)
            {
                Controls.Remove(i);
            }
            lessonLabels.Clear();

            DateTime date = DateTime.Today;
            using (StreamReader sr = new StreamReader(name))
            {
                string line;
                dutyLabel.Text = sr.ReadLine() + "\n" + sr.ReadLine();
                rubbishLabel.Text = sr.ReadLine() + "\n" + sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line == "")
                    {
                        continue;
                    }
                    Console.WriteLine(line);
                    string[] res = line.Split(' ');
                    Lesson l = new Lesson(AppendTime(date, res[0]), AppendTime(date, res[1]), res[2]);
                    Console.WriteLine("New lesson:" + l);
                    lessons.Add(l);

                    //create label 1
                    Label lbl = new Label
                    {
                        Font = lessonLabelFont,
                        Location = new System.Drawing.Point(3 + 26 * (lessons.Count - 1), 195),
                        Name = "lessonLabel_" + lessons.Count,
                        Size = new System.Drawing.Size(28, 42),
                        Text = String.Join("\n", l.subject)
                    };
                    Controls.Add(lbl);
                    lessonLabels.Add(lbl);

                    ////create label 2
                    //Label lbl2 = new Label
                    //{
                    //    Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134))),
                    //    Location = new System.Drawing.Point(3 + 55 * (lessons.Count - 1)%5, 67+ 85 * ((lessons.Count - 1) / 5)),
                    //    Name = "lessonLabel_" + lessons.Count,
                    //    Size = new System.Drawing.Size(55, 85),
                    //    Text = String.Join("\n", l.subject),
                    //    Visible = false
                    //};
                    //Controls.Add(lbl2);
                    //lessonLabels2.Add(lbl2);
                }
                Console.WriteLine(lessons.Count + " lessons read!");
            }
        }

        public form()
        {
            InitializeComponent();

            //Load Base Configuration File
            double defaultScale = 1.0;

            using (StreamReader sr = new StreamReader("Config.txt"))
            {
                gk = DateTime.Parse(sr.ReadLine());
                Console.WriteLine("Read Gaokao Time:" + gk);
                try
                {
                    string res = sr.ReadLine();
                    dn1.Text = res.Split('|')[0];
                    dn2.Text = res.Split('|')[1];
                }catch(Exception ex)
                {
                    MessageBox.Show("Cannot load duty config:\n" + ex.Message + "\n" + ex.StackTrace, "Fatal Error!!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(2);
                }

                try
                {
                    defaultScale = Double.Parse(sr.ReadLine());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Default scale failed:\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            //Try to get current date and load given file
            Console.WriteLine("Loading Date Configuration File");
            DateTime now = DateTime.Now;
            DateTime date = DateTime.Today;
            Console.WriteLine(now + " " + date);
            gkTimer.Text = gk.Subtract(date).Days+"";

            try
            {
                string name = "Config_" + now.DayOfWeek.ToString() + ".txt";
                Console.WriteLine(name);
                if (!File.Exists(name))
                {
                    string user=Interaction.InputBox("Cannot find configuration file:" + name + "\nPlease specify another:", "No such file", "");
                    name = "Config_" + user + ".txt";
                }

                ReadTimetable(name);
            }catch(Exception e)
            {
                MessageBox.Show(e+"\n"+e.StackTrace, "Fatal Error!!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(e+"\n"+e.StackTrace);
                Environment.Exit(1);
            }

            //perform scale
            ScaleAll(defaultScale);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            nowTimeLabel.Text = now.ToString();

            bool found = false;

            for(int i = 0; i < lessons.Count; i++)
            {
                Lesson l = lessons[i];

                if (l.begin<=now && l.end>=now) //in this class
                {
                    timeLabel.Text = "还有" + (l.end.Subtract(now).Hours * 60 + l.end.Subtract(now).Minutes) + "分";
                    subjectLabel.Text = l.subject;

                    for (int j = 0; j < lessons.Count; j++)
                    {
                        if (j < i)
                        {
                            lessonLabels[j].ForeColor = colorFinish;
                        }
                        else if (j == i)
                        {
                            lessonLabels[j].ForeColor = colorNow;
                        }
                        else
                        {
                            lessonLabels[j].ForeColor = colorNext;
                        }
                    }
                    found = true;
                    break;
                }else if (l.begin > now)
                {
                    timeLabel.Text = (l.begin.Subtract(now).Hours * 60 + l.begin.Subtract(now).Minutes) + "分后";
                    subjectLabel.Text = l.subject;

                    for (int j = 0; j < lessons.Count; j++)
                    {
                        if (j < i)
                        {
                            lessonLabels[j].ForeColor = colorFinish;
                        }
                        else if (j == i)
                        {
                            lessonLabels[j].ForeColor = colorSoon;
                        }
                        else
                        {
                            lessonLabels[j].ForeColor = colorNext;
                        }
                    }
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                for (int j = 0; j < lessons.Count; j++)
                {
                    lessonLabels[j].ForeColor = colorDayEnd;
                }
                timeLabel.Text = "结束";
                subjectLabel.Text = "回家";
            }

            //fix wrong transparency
            if (BackgroundImage != null)
            {
                foreach (Control i in Controls)
                {
                    try
                    {
                        if (i is Separator)
                        {
                            i.Visible = false;
                            continue;
                        }
                        if (i.BackColor != Color.Transparent)
                        {
                            Console.WriteLine("Fixed:"+i.Name);
                            i.BackColor = Color.Transparent;
                        }
                    }
                    catch (Exception _)
                    {

                    }
                }
            }
            
        }

        private void 无边框ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form.ActiveForm.FormBorderStyle = FormBorderStyle.None;
        }

        private void 有边框ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            form.ActiveForm.FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        public void SetTransparent()
        {
            if (this.Parent != null)
            {
            }
        }

        private Bitmap TakeComponentScreenShot(Control control)
        {
            Rectangle rect = control.RectangleToScreen(this.Bounds);
            if (rect.Width == 0 || rect.Height == 0)
            {
                return null;
            }
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);

            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

            return bmp;
        }
        
        public bool LockMenu()
        {
            if(MessageBox.Show("Are you sure to lock? After locking the menu & window border will be hidden.", "Lock?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)==DialogResult.Yes)
            {
                form.ActiveForm.FormBorderStyle = FormBorderStyle.None;
                form.ActiveForm.MainMenuStrip.Visible = false;
                return true;
            }
            return false;
        }
        private void 置底ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = false;

            if (!LockMenu())
            {
                return;
            }

            //TransparencyKey = Color.FromArgb(255, 255, 192);
            //SendToBack();

            // 通过类名查找一个窗口，返回窗口句柄。
            programIntPtr = Win32.FindWindow("Progman", null);

            // 窗口句柄有效
            if (programIntPtr != IntPtr.Zero)
            {

                IntPtr result = IntPtr.Zero;

                // 向 Program Manager 窗口发送 0x52c 的一个消息，超时设置为0x3e8（1秒）。
                Win32.SendMessageTimeout(programIntPtr, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 0x3e8, result);

                // 遍历顶级窗口
                Win32.EnumWindows((hwnd, lParam) =>
                {
                    // 找到包含 SHELLDLL_DefView 这个窗口句柄的 WorkerW
                    if (Win32.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
                    {
                        // 找到当前 WorkerW 窗口的，后一个 WorkerW 窗口。
                        IntPtr tempHwnd = Win32.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);

                        // 隐藏这个窗口
                        Win32.ShowWindow(tempHwnd, 0);
                    }
                    return true;
                }, IntPtr.Zero);

                Win32.SetParent(Handle, programIntPtr);
                
            }
        }

        private void form_Paint(object sender, PaintEventArgs e)
        {

        }

        private void form_Activated(object sender, EventArgs e)
        {
        }

        private void form_Load(object sender, EventArgs e)
        {

        }


        private void 重载时刻表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            
            string user = Interaction.InputBox("Please specify configuration:", "No such file", "");
            if(user=="" || user == null)
            {
                return;
            }
            string name = "Config_" + user + ".txt";
            

            ReadTimetable(name);
        }

        private void ClearBG()
        {
            if (BackgroundImage != null)
            {
                BackgroundImage.Dispose();
                BackgroundImage = null;
                backgroundImagePath = null;
            }
        }

        private void 背景色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            ClearBG();
            BackColor = colorDialog1.Color;

            foreach (Control i in Controls)
            {
                try
                {
                    i.BackColor = BackColor;
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void 锁定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LockMenu();
        }

        private void creditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This project is brought to you by:\nXGN & Zzzyt\nOpen source at:https://github.com/XiaoGeNintendo/GKHelper \nHell Hole Studios 2022", "Credit!!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void 置顶ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            MessageBox.Show("Now Topmost=" + TopMost);
        }

        private void 改字体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("[Instructions]\nThe system will show up a series of dialogs for every font that is present on the interface. Please perform changes according to the font preloaded into the dialog. Note that the UI may break after a font change.","Instruction",MessageBoxButtons.OK,MessageBoxIcon.Information);

            Dictionary<Font, Font> dict = new Dictionary<Font, Font>();
            foreach(Control i in Controls)
            {
                if(i is Label j && i.Visible)
                {
                    if (!dict.ContainsKey(j.Font))
                    {
                        var tmp = j.BackColor;
                        j.BackColor = (this.BackColor == Color.Red ? Color.Green : Color.Red);
                        fontDialog1.Font = j.Font;
                        fontDialog1.ShowDialog();
                        dict[j.Font] = fontDialog1.Font;
                        j.BackColor = tmp;
                    }

                    j.Font = dict[j.Font];
                }
            }

            //reset lesson label font
            if (lessonLabels.Count >= 1) { 
                lessonLabelFont = lessonLabels[0].Font;
            }
        }

        private void 保存UI设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var x=saveFileDialog1.ShowDialog();
                string fn = saveFileDialog1.FileName;
                if (fn == "" || x==DialogResult.Cancel)
                {
                    return;
                }
                using (StreamWriter sw = new StreamWriter(fn))
                {
                    //background image
                    sw.WriteLine("V21C");
                    sw.WriteLine(backgroundImagePath);
                    sw.WriteLine((int)BackgroundImageLayout);

                    //colors
                    sw.WriteLine(BackColor.ToArgb());
                    sw.WriteLine(dn1.ForeColor.ToArgb()); //candidate label
                    sw.WriteLine(colorDayEnd.ToArgb());
                    sw.WriteLine(colorFinish.ToArgb());
                    sw.WriteLine(colorNow.ToArgb());
                    sw.WriteLine(colorSoon.ToArgb());
                    sw.WriteLine(colorNext.ToArgb());
                    //fonts
                    sw.WriteLine(JsonConvert.SerializeObject(lessonLabelFont));
                    foreach (Control i in Controls)
                    {
                        if (i is Label j && i.Visible && !lessonLabels.Contains(j))
                        {
                            sw.WriteLine(j.Name);
                            sw.WriteLine(JsonConvert.SerializeObject(j.Font));
                        }
                    }
                }
                MessageBox.Show("Done!","OK",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }catch(Exception ex)
            {
                MessageBox.Show("Error occurred when saving:" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void 载入UI设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var x=openFileDialog1.ShowDialog();
                string fn=openFileDialog1.FileName;
                if (fn == "" || x==DialogResult.Cancel)
                {
                    return;
                }
                using(StreamReader sr=new StreamReader(fn))
                {

                    string ver = sr.ReadLine();

                    if (ver != "V21C")
                    {
                        MessageBox.Show("Out-dated or Corrupted Configuration File!\n" +
                            "Configuration verification failed\n" +
                            "Your configuration file is created with an earlier version of the application,\n" +
                            "or is corrupted and cannot be loaded anymore.\n"+
                            "If it was created with V2.0, please update the file by appending:\n" +
                            "V21C at line 1\n" +
                            "null at line 2\n" +
                            "0 at line 3", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //read bgimg
                    backgroundImagePath = sr.ReadLine();
                    if (backgroundImagePath == "")
                    {
                        ClearBG();
                        sr.ReadLine();
                    }
                    else
                    {
                        ChangeBGImage(backgroundImagePath, (ImageLayout)int.Parse(sr.ReadLine()));
                    }

                    //read color
                    BackColor = Color.FromArgb(Int32.Parse(sr.ReadLine()));
                    
                    var tmp = Color.FromArgb(Int32.Parse(sr.ReadLine()));
                    foreach (Control c in Controls)
                    {
                        c.ForeColor = tmp;
                        c.BackColor = BackColor;
                    }

                    
                    colorDayEnd = Color.FromArgb(Int32.Parse(sr.ReadLine()));
                    colorFinish = Color.FromArgb(Int32.Parse(sr.ReadLine()));
                    colorNow = Color.FromArgb(Int32.Parse(sr.ReadLine()));
                    colorSoon = Color.FromArgb(Int32.Parse(sr.ReadLine()));
                    colorNext = Color.FromArgb(Int32.Parse(sr.ReadLine()));

                    lessonLabelFont = JsonConvert.DeserializeObject<Font>(sr.ReadLine());
                    foreach(Control i in lessonLabels)
                    {
                        i.Font = lessonLabelFont;
                    }

                    while (true)
                    {
                        var name = sr.ReadLine();
                        if (name == null)
                        {
                            break;
                        }
                        var font = JsonConvert.DeserializeObject<Font>(sr.ReadLine()); 
                        foreach(Control i in Controls) { 
                            if (i is Label j && i.Visible && !lessonLabels.Contains(j) && name==i.Name)
                            {
                                if (name != j.Name)
                                {
                                    throw new Exception("Failed to verify component name: Expected " + name + " but found " + j.Name+"!");
                                }
                                j.Font = font;
                            }
                        }
                    }
                }

                MessageBox.Show("Done!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error occurred when loading:" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeBGImage(string fn, ImageLayout layout)
        {
            ClearBG();

            backgroundImagePath = fn;
            this.BackgroundImage = Image.FromFile(fn);
            this.BackgroundImageLayout = layout;
            foreach (Control i in Controls)
            {
                try
                {
                    if (i is Separator)
                    {
                        i.Visible = false;
                        continue;
                    }
                    Console.WriteLine(i.Name);
                    i.BackColor = Color.Transparent;
                }
                catch (Exception _)
                {

                }
            }
        }

        private void 图片背景ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var x = openFileDialog1.ShowDialog();
                string fn = openFileDialog1.FileName;
                if (fn == "" || x == DialogResult.Cancel)
                {
                    return;
                }


                var xd = Interaction.InputBox("Specify Image Layout:\nNone=0;Tile=1;Center=2;Stretch=3;Zoom=4", "Image Layout", "1");
                if (xd == "")
                {
                    ChangeBGImage(fn, ImageLayout.Tile);
                }
                else
                {
                    ChangeBGImage(fn, (ImageLayout)int.Parse(xd));
                }


                

                MessageBox.Show("Done!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred when loading:" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScaleAll(double d)
        {
            if (d != 1.0)
            {
                重载时刻表ToolStripMenuItem.Enabled = false;
            }

            Width = (int)(Width * d);
            Height = (int)(Height * d);

            foreach (Control c in Controls)
            {
                c.Width = (int)(c.Width * d);
                c.Height = (int)(c.Height * d);
                c.Top = (int)(c.Top * d);
                c.Left = (int)(c.Left * d);
                if (c is Label l)
                {
                    l.Font = new Font(l.Font.Name, (float)(l.Font.Size * d));
                }
            }
        }

        private void 缩放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string res=Interaction.InputBox("[Buggy scaling]\nScaling may corrupt the current layout. Restart the program if something goes wrong.\nScale by:", "Scaling", "1.0");
            if (res == "")
            {
                return;
            }
            try
            {
                double d = Double.Parse(res);

                ScaleAll(d);
            }catch(FormatException ex)
            {
                MessageBox.Show("Cannot parse input string as double!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 更改值日岗位责任ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string res = Interaction.InputBox("Enter new positions for duty display, seperated with '|':\n Fun fact: You can change it in Config.txt permanently!", "Enter", "擦黑板|倒垃圾");
            try {
                if (res == "" || res==null)
                {
                    return;
                }
                dn1.Text = res.Split('|')[0];
                dn2.Text = res.Split('|')[1];
            }catch(Exception ex)
            {
                MessageBox.Show(ex + "");
            }
        }

        private void 改前景色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("[Instructions]\nThe system will show up a series of dialogs for every color that is present on the interface. Please perform changes according to the color preloaded into the dialog.", "Instruction", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Dictionary<Color, Color> dc = new Dictionary<Color, Color>();
 
            colorDialog1.Color = dn1.ForeColor; //delegate
            colorDialog1.ShowDialog();
            Console.WriteLine("Replace " + dn1.ForeColor + "->" + colorDialog1.Color);

            foreach (Control c in Controls)
            {
                c.ForeColor = colorDialog1.Color;
            }

            colorDialog1.Color = colorDayEnd;
            colorDialog1.ShowDialog();
            colorDayEnd = colorDialog1.Color;

            colorDialog1.Color = colorFinish;
            colorDialog1.ShowDialog();
            colorFinish = colorDialog1.Color;

            colorDialog1.Color = colorNow;
            colorDialog1.ShowDialog();
            colorNow = colorDialog1.Color;

            colorDialog1.Color = colorSoon;
            colorDialog1.ShowDialog();
            colorSoon = colorDialog1.Color;

            colorDialog1.Color = colorNext;
            colorDialog1.ShowDialog();
            colorNext = colorDialog1.Color;
        }
    }
}
