using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Util {
    public class InternationalizationAttribute : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
        }
    }
}
