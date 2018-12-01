using System;
using System.Diagnostics;
using System.Web;

namespace DiplomMSSQLApp.WEB.Modules {
    public class TimerModule : IHttpModule {
        static private TimeSpan time;
        private Stopwatch timer;

        public void Init(HttpApplication app) {
            app.BeginRequest += HandleEvent;
            app.EndRequest += HandleEvent;
        }

        private void HandleEvent(object src, EventArgs args) {
            HttpContext ctx = HttpContext.Current;
            if (ctx.CurrentNotification == RequestNotification.BeginRequest) {
                timer = Stopwatch.StartNew();
            }
            else {
                Debug.WriteLine("Elapsed: " + ((float)timer.ElapsedTicks) / Stopwatch.Frequency + " seconds");
                //ctx.Response.Write($"<div class='alert alert-success'>Elapsed: {((float)timer.ElapsedTicks) / Stopwatch.Frequency:F5} seconds</div>");
            }
        }

        public void Dispose() {
        }
    }
}
