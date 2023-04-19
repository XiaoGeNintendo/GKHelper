using Microsoft.Toolkit.Uwp.Notifications;
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

        Dictionary<string, Brush> brushes = new Dictionary<string, Brush>();
        Dictionary<string, Color> brushColors = new Dictionary<string, Color>();

        /**
         * The date for Gaokao
         */
        DateTime gk;
        bool wallpaper = false;
        IntPtr programIntPtr;
        List<Label> lessonLabels = new List<Label>();
        List<Label> lessonLabels2 = new List<Label>();
        string backgroundImagePath = null;
        int tick;

        /**
         * Task Scheduler for toasts
         */
        TaskScheduler taskScheduler;
        bool doToast = true;

        /**
         * Whether to open rtf files by wordpad
         */
        bool doWordpad = true;

        /**
         * Whether to set no border soon
         */
        bool doNoBorder = false;

        /**
         * Whether to show the announcement richtext
         */
        bool expand = true;

        /**
        * The theme file to be loaded next tick
        * 
        * Should be set only once
        */
        string themeFile = null;

        Font lessonLabelFont = new Font("华文中宋", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));

        Color colorDayEnd=Color.Gray, colorFinish=Color.Red, colorNext=Color.Green,colorNow=Color.Blue,colorSoon=Color.Lime;

        public DateTime AppendTime(DateTime date, string str)
        {
            string[] info = str.Split(':');
            return date.AddHours(Double.Parse(info[0])).AddMinutes(Double.Parse(info[1]));
        }

        public string GetSubjectHeroImage(string subject)
        {
            string pic = "hero/default.png";
            if (File.Exists("hero/" + subject + ".png"))
            {
                pic = "hero/" + subject + ".png";
            }

            return pic;
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

            //Preparing Toasts
            taskScheduler = new TaskScheduler();
            for(int i=0;i<lessons.Count;i++)
            {
                Lesson l = lessons[i];


                taskScheduler.Append(new Toast(l.begin-TimeSpan.FromMinutes(2), l.subject + " 即将开始！",
                    "共 " + l.Length() + " 分长",
                    GetSubjectHeroImage(l.subject)));

                taskScheduler.Append(new Toast(l.begin, l.subject + " 开始了！", 
                    "共 "+l.Length()+" 分长", 
                    GetSubjectHeroImage(l.subject)));
                if (i != lessons.Count - 1)
                {
                    taskScheduler.Append(new Toast(l.end, l.subject + " 结束了！",
                        (lessons[i + 1].begin - lessons[i].end).TotalMinutes.ToString() + " 分后是" + lessons[i + 1].subject + "！",
                        GetSubjectHeroImage(lessons[i + 1].subject)));
                }
                else
                {
                    taskScheduler.Append(new Toast(l.end, l.subject + " 结束了！",
                        "结束撒花！(゜▽゜*)♪",
                        GetSubjectHeroImage("结束")));
                }
                
            }
        }

        public void LoadColorPalette(string fn)
        {
            brushes = new Dictionary<string, Brush>();
            using(StreamReader sr=new StreamReader(fn))
            {
                //skip first line
                sr.ReadLine();
                while (true)
                {
                    string[] para=sr.ReadLine().Trim().Split(' ');

                    var color = Color.FromArgb(
                                (para[0] == "Past" ? 200 : 255),
                                int.Parse(para[1]),
                                int.Parse(para[2]),
                                int.Parse(para[3])
                            );
                    brushes[para[0]] = new SolidBrush(color);
                    brushColors[para[0]] = color;

                    Console.WriteLine("Read from palette:" + para[0] + " " + para[1] + " " + para[2] + " " + para[3]);
                    if (para[0] == "default")
                    {
                        break;
                    }
                }
            }

            tsb.brushes = brushes;
        }

        public form()
        {
            InitializeComponent();

            timer1.Enabled = false;

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

                try
                {
                    string[] inp = sr.ReadLine().Split(' ');
                    int x = int.Parse(inp[0]);
                    int y = int.Parse(inp[1]);
                    if(x>=0 && y>=0)
                    {
                        Console.WriteLine("Set:" + x + " " + y);
                        StartPosition = FormStartPosition.Manual;
                        Location = new Point(x, y);
                    }
                    else
                    {
                        Console.WriteLine("Did not set location");
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show("Default location failed:\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                try
                {
                    themeFile = sr.ReadLine();
                }catch(Exception ex)
                {
                    MessageBox.Show("Default UI file failed:\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                try
                {
                    string tagExpand = sr.ReadLine();
                    if (tagExpand == "DoExpand")
                    {
                        Console.WriteLine("#DoExpand");
                        ToggleExpand();
                    }
                    string tagToast = sr.ReadLine();
                    if (tagToast == "NoToast")
                    {
                        Console.WriteLine("#NoToast");
                        doToast = false;
                    }
                    string tagWritepad = sr.ReadLine();
                    if (tagWritepad == "NoWordpad")
                    {
                        Console.WriteLine("#NoWordpad");
                        doWordpad = false;
                    }
                    string startupLockLevel = sr.ReadLine();
                    if (startupLockLevel == "NoBorder")
                    {
                        Console.WriteLine("#NoBorder");
                        doNoBorder = true;
                    }
                    string useColorBar = sr.ReadLine();
                    if (useColorBar == "NoColorBar")
                    {
                        Console.WriteLine("#NoColorBar");
                        tsb.Visible = false;
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Config Tag failed:\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            //defaultly no expand & read rtf file
            ToggleExpand();

            if (!File.Exists(GetTodayAnnouncementFile()))
            {
                CompileAnnouncementTemplate();
            }

            //load segment bar
            LoadColorPalette("DefaultColor.txt");
            tsb.lessons = lessons;
            tsb.BringToFront();

            //run last
            timer1.Enabled = true;

        }


        public void CompileAnnouncementTemplate()
        {
            Console.WriteLine("Creating Announcement File");
            using (StreamReader sr = new StreamReader("default.rtf"))
            {
                using (StreamWriter sw = new StreamWriter(GetTodayAnnouncementFile()))
                {
                    var nw = sr.ReadToEnd().Replace("\\{DATE\\}", DateTime.Today.ToShortDateString())
                        .Replace("\\{FILE\\}", GetTodayAnnouncementFile())
                        .Replace("\\{TIME\\}", DateTime.Now.ToLongTimeString());
                    Console.WriteLine(nw);
                    sw.WriteLine(nw);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tick++;

            //refresh Segment Bar
            tsb.Refresh();

            //default theme file
            try
            {
                if (themeFile != "null" && themeFile != null)
                {
                    
                    Console.WriteLine("Loading default themeFile:" + themeFile);
                    var tmp = themeFile;
                    themeFile = null;
                    LoadUIFrom(tmp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Default UI file failed:\n" + ex.Message + "\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //parse configurations
            if (doNoBorder)
            {
                form.ActiveForm.FormBorderStyle = FormBorderStyle.None;
                doNoBorder = false;
            }

            if (doToast)
            {
                taskScheduler.Tick(DateTime.Now);
            }

            DateTime now = DateTime.Now;
            nowTimeLabel.Text = now.ToString();

            bool found = false;

            for(int i = 0; i < lessons.Count; i++)
            {
                Lesson l = lessons[i];

                if (l.begin<=now && l.end>=now) //in this class
                {
                    timeLabel.Text = "还有" + (int)l.end.Subtract(now).TotalMinutes + "分";
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
                    timeLabel.Text = (int)l.begin.Subtract(now).TotalMinutes + "分后";
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
                        if (!(i is RichTextBox) && i.BackColor != Color.Transparent)
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

        public string GetTodayAnnouncementFile()
        {
            DateTime today = DateTime.Today;
            return "anno_" + today.ToShortDateString().Replace('/','-') + ".rtf";
        }

        private void form_Activated(object sender, EventArgs e)
        {
            if (expand)
            {
                LoadRTF();
            }
        }

        public void LoadRTF()
        {
            try
            {
                richTextBox1.LoadFile(GetTodayAnnouncementFile());
            }
            catch (Exception ex)
            {
                richTextBox1.Text = "无法读取" + GetTodayAnnouncementFile() + "：" + ex.Message;
                Console.WriteLine("Error while loading rich text:" + ex.Message + " " + ex.StackTrace);
            }
        }

        private void form_Load(object sender, EventArgs e)
        {

        }


        private void 重载时刻表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            
            string user = Interaction.InputBox("Please specify configuration:", "Reload Timetable", "");
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
            colorDialog1.Color = BackColor;
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
            MessageBox.Show("This project is brought to you by:\n" +
                "XGN & Zzzyt\n" +
                "Open source at:https://github.com/XiaoGeNintendo/GKHelper \n" +
                "Hell Hole Studios 2022\n"+
                "Picture By: Pixabay\n" +
                "Novel AI", "Credit!!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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
                    sw.WriteLine("V22C");

                    //palette
                    foreach(var y in brushColors)
                    {
                        if (y.Key != "default")
                        {
                            sw.WriteLine(y.Key + " " + y.Value.R + " " + y.Value.G + " " + y.Value.B);
                        }
                    }
                    sw.WriteLine("default " + brushColors["default"].R + " " + brushColors["default"].G + " " + brushColors["default"].B);

                    //background image
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

        public void LoadUIFrom(string fn)
        {
            using (StreamReader sr = new StreamReader(fn))
            {

                string ver = sr.ReadLine();

                if (ver != "V22C")
                {
                    if (ver == "V21C")
                    {
                        MessageBox.Show("Out-dated Configuration File!\n" +
                       "Configuration verification failed\n" +
                       "Your configuration file is created with an earlier version of the application,\n" +
                       "please update it soon!\n" +
                       "The program will load it anyway.\n", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Out-dated or Corrupted Configuration File!\n" +
                        "Configuration verification failed\n" +
                        "Your configuration file is created with an earlier version of the application,\n" +
                        "or is corrupted and cannot be loaded anymore.\n", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (ver == "V22C") {
                    LoadColorPalette(fn);
                    //skip till default
                    while (true)
                    {
                        string[] para=sr.ReadLine().Split(' ');
                        if (para.Length >= 4 && para[0] == "default") 
                        {
                            break;
                        }
                    }
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
                foreach (Control i in lessonLabels)
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
                    foreach (Control i in Controls)
                    {
                        if (i is Label j && i.Visible && !lessonLabels.Contains(j) && name == i.Name)
                        {
                            if (name != j.Name)
                            {
                                throw new Exception("Failed to verify component name: Expected " + name + " but found " + j.Name + "!");
                            }
                            j.Font = font;
                        }
                    }
                }
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

                LoadUIFrom(fn);

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

        private void 防睡眠ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Kernel.SetThreadExecutionState(EXECUTION_STATE.ES_AWAYMODE_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
            MessageBox.Show("Sleep prevention on!");
        }

        private void 展开收缩ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleExpand();
        }

        public void ToggleExpand()
        {
            if (expand)
            {
                expand = false;
                Width /= 2;
            }
            else
            {
                Width *= 2;
                expand = true;
                LoadRTF();
            }
        }

        private void 重新生成公告模板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will replace the current announcement file:" + GetTodayAnnouncementFile() + "\nAre you sure?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                CompileAnnouncementTemplate();
                LoadRTF();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (doWordpad)
            {
                System.Diagnostics.Process.Start("wordpad", GetTodayAnnouncementFile());
            }
            else
            {
                System.Diagnostics.Process.Start(GetTodayAnnouncementFile());
            }
        }

        private void form_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        private void form_FormClosing(object sender, FormClosingEventArgs e)
        {
            ToastNotificationManagerCompat.Uninstall();
            Console.WriteLine("Uninstalling Toast");
        }

        private void 启用吐司ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doToast = !doToast;
            MessageBox.Show("Toast=" + doToast);
        }

        private void 创建安装Band脚本ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(StreamWriter sw=new StreamWriter("BandSync.bat"))
            {
                foreach (string s in Directory.GetFiles(Directory.GetCurrentDirectory()))
                {
                    var p = Path.GetFileName(s);
                    Console.WriteLine(s);
                    if (p.StartsWith("Config") || p.EndsWith(".js"))
                    {
                        Console.WriteLine(s);
                        sw.WriteLine("copy \"" + s + "\" \"C:/GKB/" + p + "\"");
                    }
                }
                sw.WriteLine("taskkill /f /IM explorer.exe\nstart explorer.exe\npause");
            }
            using(StreamWriter sw=new StreamWriter("BandInstall.bat"))
            {
                sw.WriteLine("mkdir \"C:/GKB\"");
                foreach(string s in Directory.GetFiles(Directory.GetCurrentDirectory()))
                {
                    var p = Path.GetFileName(s);
                    Console.WriteLine(s);
                    if (p.StartsWith("Config") || p.EndsWith(".js"))
                    {
                        Console.WriteLine(s);
                        sw.WriteLine("copy \"" + s + "\" \"C:/GKB/" + p + "\"");
                    }
                }
                sw.WriteLine("C:/WINDOWS/Microsoft.NET/Framework64/v4.0.30319/regasm.exe /codebase \"" + Directory.GetCurrentDirectory() + "/GKHelperBand.dll\"");
                sw.WriteLine("taskkill /f /IM explorer.exe\nstart explorer.exe\npause");
            }
            using(StreamWriter sw=new StreamWriter("BandUninstall.bat"))
            {
                sw.WriteLine("del C:\\GKB\\");
                sw.WriteLine("C:/WINDOWS/Microsoft.NET/Framework64/v4.0.30319/regasm.exe /u \"" + Directory.GetCurrentDirectory() + "/GKHelperBand.dll\"");
                sw.WriteLine("taskkill /f /IM explorer.exe\nstart explorer.exe\npause");
            }

            MessageBox.Show("Done! Please run BandInstall.bat with admin privilege!\nRun BandSync.bat when configs are changed!", "Done!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 系统调试工具箱ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string debugInfo = "系统调试信息显示：\n" +
                "吐司：" + doToast + "\n" +
                "写字板：" + doWordpad + "\n" +
                "位置：" + Location + "\n";
            MessageBox.Show(debugInfo);
        }

        private void 改ColorBar颜色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("[Instructions]\nThe system will show up a series of dialogs for every color that is present in the DefaultColor.txt. Please perform changes according to the color preloaded into the dialog", "Instruction", MessageBoxButtons.OK, MessageBoxIcon.Information);
            var list = brushColors.Keys.ToList();
            for(int i = 0; i < list.Count; i++)
            {
                var key = list[i];
                var value = brushColors[key];

                MessageBox.Show("Next:" + key);
                colorDialog1.Color = value;
                colorDialog1.ShowDialog();
                var finalColor = colorDialog1.Color;
                if (key == "Past")
                {
                    finalColor = Color.FromArgb(
                        200,
                        finalColor.R,
                        finalColor.G,
                        finalColor.B);
                }

                brushColors[key] = finalColor;
                brushes[key] = new SolidBrush(finalColor);
            }

            tsb.brushes = brushes;
        }

        private void 窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ScaleAll(double d)
        {
            if (d != 1.0)
            {
                重载时刻表ToolStripMenuItem.Enabled = false;
            }

            lessonLabelFont = new Font(lessonLabelFont.Name, (float)(lessonLabelFont.Size * d));

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
            MessageBox.Show("This function has been deprecated. Please change the scaling in Config.txt.", "Deprecated!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
