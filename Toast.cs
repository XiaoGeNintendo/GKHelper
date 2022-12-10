using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKHelper
{
    class Toast: IComparable<Toast>
    {
        public DateTime time;
        public string title;
        public string content;
        public string pic;


        public override string ToString()
        {
            return title + ">>" + content + "[" + pic + "]" + "@" + time;
        }

        public int CompareTo(Toast obj)
        {
            return time.CompareTo(obj.time);
        }

        public static bool operator <(Toast x, Toast y)
        {
            return x.time < y.time;
        }

        public static bool operator >(Toast x, Toast y)
        {
            return x.time > y.time;
        }

        public Toast(DateTime time, string title, string content, string pic)
        {
            this.time = time;
            this.title = title;
            this.content = content;
            this.pic = pic;
        }
    }
}
