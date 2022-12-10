using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GKHelper
{
    class TaskScheduler
    {
        public List<Toast> toasts = new List<Toast>();
        
        public void Append(Toast t)
        {
            toasts.Add(t);
            toasts.Sort();
            Console.WriteLine("New Toast:" + t + "\n"+toasts.ToArray());
        }

        public void Tick(DateTime now)
        {
            while (toasts.Count>0)
            {
                if (toasts[0].time < now - TimeSpan.FromSeconds(300))
                {
                    Console.WriteLine("Cleared Out-dated Toast:" + toasts[0].title);
                    toasts.RemoveAt(0);
                } else if (toasts[0].time <= now)
                {
                    Console.WriteLine("Activated Toast:" + toasts[0].title);
                    new ToastContentBuilder()
                        .AddText(toasts[0].title, AdaptiveTextStyle.Header)
                        .AddText(toasts[0].content, AdaptiveTextStyle.Body)
                        .AddHeroImage(new Uri(Path.GetFullPath(toasts[0].pic), UriKind.Absolute))
                        .Show(toast =>
                        {
                            toast.ExpirationTime = DateTime.Now.AddMinutes(5);
                        }
                        );
                    toasts.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
