using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GKHelper
{
    public partial class TestSegmentBar : UserControl
    {
        public Dictionary<string, Brush> brushes=new Dictionary<string, Brush>();

        public List<Lesson> lessons = new List<Lesson>();

        public TestSegmentBar(List<Lesson> lessons)
        {
            InitializeComponent();
            this.lessons = lessons;
        }

        public TestSegmentBar()
        {
            InitializeComponent();
            brushes["default"] = new SolidBrush(Color.Blue);
        }

        private Brush Fetch(string s)
        {
            if(brushes.Keys.Contains(s)){
                return brushes[s];
            }
            else
            {
                return brushes["default"];
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pe)
        {
            base.OnPaintBackground(pe);

            pe.Graphics.FillRectangle(Fetch("Back"), new Rectangle(0, 0, this.Width, this.Height));

            if (lessons!=null && lessons.Count > 0)
            {
                var allStart = lessons[0].begin - new TimeSpan(0, 30, 0);
                var allEnd = lessons.Last().end + new TimeSpan(0, 30, 0);
                foreach(Lesson lesson in lessons)
                {
                    float percentStart = (float)((lesson.begin - allStart).TotalSeconds / (allEnd - allStart).TotalSeconds);
                    float percentEnd = (float)((lesson.end - allStart).TotalSeconds / (allEnd - allStart).TotalSeconds);
                    //Console.WriteLine(percentStart + "-" + percentEnd + "-2" + lesson);
                    pe.Graphics.FillRectangle(Fetch(lesson.subject), new RectangleF((percentStart * this.Width), 0, ((percentEnd - percentStart) * this.Width), this.Height));
                }

                float nowPos = (float)((DateTime.Now - allStart).TotalSeconds / (allEnd - allStart).TotalSeconds);
                pe.Graphics.FillRectangle(Fetch("Past"), new RectangleF(0, 0, (nowPos * this.Width), this.Height));
            }

            
        }
    }
}
