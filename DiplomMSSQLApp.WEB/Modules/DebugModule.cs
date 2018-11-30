using System.Collections.Generic;
using System.Web;

namespace DiplomMSSQLApp.WEB.Modules {
    public class DebugModule : IHttpModule {
        private static List<string> requestUrls = new List<string>();
        private static object lockObject = new object();

        public void Init(HttpApplication app) {
            app.BeginRequest += (src, args) => {
                lock (lockObject) {
                    if (app.Request.RawUrl == "/Stats") {
                        app.Response.Write($"<div>There have been {requestUrls.Count} requests</div>");
                        app.Response.Write("<table><tr><th>ID</th><th>URL</th></tr>");
                        for (int i = 0; i < requestUrls.Count; i++) {
                            app.Response.Write($"<tr><td>{i}</td><td>{requestUrls[i]}</td></tr>");
                        }
                        app.CompleteRequest();
                    }
                    else {
                        requestUrls.Add(app.Request.RawUrl);
                    }
                }
            };
        }

        public void Dispose() {
        }
    }
}
