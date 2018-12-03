using DiplomMSSQLApp.BLL.BusinessModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Helpers {
    public static class PagingHelpers {
        public static MvcHtmlString PageLinks(this HtmlHelper html, PageInfo pageInfo, Func<int, string> pageUrl) {
            StringBuilder result = new StringBuilder();
            TagBuilder tag;

            // Кнопка перехода к первой странице
            tag = new TagBuilder("a");
            tag.MergeAttributes(new Dictionary<string, object> {
                { "id", "firstpagebutton" },
                { "class", "btn btn-default" },
                { "data-ajax", "true" },
                { "data-ajax-success", "processData" },
                { "data-ajax-url", pageUrl(1) },
                { "href", pageUrl(1) }
            });
            tag.InnerHtml = "<<";
            result.Append(tag.ToString());

            // Кнопка перехода на 3 страницы влево
            int numPage = pageInfo.PageNumber > 3 ? pageInfo.PageNumber - 3 : 1;
            tag = new TagBuilder("a");
            tag.MergeAttributes(new Dictionary<string, object> {
                { "id", "leftbutton" },
                { "class", "btn btn-default" },
                { "data-ajax", "true" },
                { "data-ajax-success", "processData" },
                { "data-ajax-url", pageUrl(numPage) },
                { "href", pageUrl(numPage) }
            });
            tag.InnerHtml = "<";
            result.Append(tag.ToString());

            // Кнопки перехода с цифрами
            // Показываем только номера текущей и соседних страниц
            int minPage = pageInfo.PageNumber > 1 ? pageInfo.PageNumber - 1 : 1;
            int maxPage = pageInfo.PageNumber < pageInfo.TotalPages - 1 ? pageInfo.PageNumber + 1 : pageInfo.TotalPages;
            for (numPage = minPage; numPage <= maxPage; numPage++) {
                tag = new TagBuilder("a");
                tag.MergeAttributes(new Dictionary<string, object> {
                    { "data-ajax", "true" },
                    { "data-ajax-success", "processData" },
                    { "data-ajax-url", pageUrl(numPage) },
                    { "href", pageUrl(numPage) }
                });
                tag.InnerHtml = numPage.ToString();
                // если текущая страница, то выделяем ее
                if (numPage == pageInfo.PageNumber)
                    tag.AddCssClass("centerbuttons btn btn-primary selected");
                else
                    tag.AddCssClass("centerbuttons btn btn-default");
                result.Append(tag.ToString());
            }

            // Кнопка перехода на 3 страницы вправо
            numPage = pageInfo.PageNumber < pageInfo.TotalPages - 3 ? pageInfo.PageNumber + 3 : pageInfo.TotalPages;
            tag = new TagBuilder("a");
            tag.MergeAttributes(new Dictionary<string, object> {
                { "id", "rightbutton" },
                { "class", "btn btn-default" },
                { "data-ajax", "true" },
                { "data-ajax-success", "processData" },
                { "data-ajax-url", pageUrl(numPage) },
                { "href", pageUrl(numPage) }
            });
            tag.InnerHtml = ">";
            result.Append(tag.ToString());

            // Кнопка перехода к последней странице
            numPage = pageInfo.TotalPages;
            tag = new TagBuilder("a");
            tag.MergeAttributes(new Dictionary<string, object> {
                { "id", "lastpagebutton" },
                { "class", "btn btn-default" },
                { "data-ajax", "true" },
                { "data-ajax-success", "processData" },
                { "data-ajax-url", pageUrl(numPage) },
                { "href", pageUrl(numPage) }
            });
            tag.InnerHtml = ">>";
            result.Append(tag.ToString());

            return MvcHtmlString.Create(result.ToString());
        }
    }
}
