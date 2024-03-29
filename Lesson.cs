﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKHelper
{
    public class Lesson
    {
        public DateTime begin;
        public DateTime end;
        public string subject;

        public int Length()
        {
            return (int)((end - begin).TotalMinutes);
        }

        public override string ToString()
        {
            return "Lesson "+subject+" "+begin+"-"+end;
        }

        public Lesson(DateTime begin, DateTime end, string subject)
        {
            this.begin = begin;
            this.end = end;
            this.subject = subject;
        }
    }
}
