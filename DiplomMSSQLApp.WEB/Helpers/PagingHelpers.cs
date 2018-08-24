using DiplomMSSQLApp.BLL.BusinessModels;
using System;
using System.Text;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Helpers
{
    public static class PagingHelpers
    {
        // Хелпер пагинации
        public static MvcHtmlString PageLinks(this HtmlHelper html,
            PageInfo pageInfo, Func<int, string> pageUrl)
        {
            StringBuilder result = new StringBuilder();
            TagBuilder tag;
            // Кнопка перехода к первой странице
            tag = new TagBuilder("a");
            tag.MergeAttribute("href", pageUrl(1));
            tag.InnerHtml = "<<";
            tag.AddCssClass("btn btn-default");
            result.Append(tag.ToString());
            // Кнопка перехода на 3 страницы влево
            tag = new TagBuilder("a");
            int numPage = pageInfo.PageNumber > 3 ? pageInfo.PageNumber - 3 : 1;
            tag.MergeAttribute("href", pageUrl(numPage));
            tag.InnerHtml = "<";
            tag.AddCssClass("btn btn-default");
            result.Append(tag.ToString());
            // Кнопки перехода с цифрами
            // Показываем только номера текущей и соседних страниц
            int minPage = pageInfo.PageNumber > 1 ? pageInfo.PageNumber - 1 : 1;
            int maxPage = pageInfo.PageNumber < pageInfo.TotalPages - 1 ? pageInfo.PageNumber + 1 : pageInfo.TotalPages;
            for (int i = minPage; i <= maxPage; i++)
            {
                tag = new TagBuilder("a");
                tag.MergeAttribute("href", pageUrl(i));
                tag.InnerHtml = i.ToString();
                // если текущая страница, то выделяем ее
                if (i == pageInfo.PageNumber)
                {
                    tag.AddCssClass("selected");
                    tag.AddCssClass("btn-primary");
                }
                tag.AddCssClass("btn btn-default");
                result.Append(tag.ToString());
            }
            // Кнопка перехода на 3 страницы вправо
            tag = new TagBuilder("a");
            numPage = pageInfo.PageNumber < pageInfo.TotalPages - 3 ? pageInfo.PageNumber + 3 : pageInfo.TotalPages;
            tag.MergeAttribute("href", pageUrl(numPage));
            tag.InnerHtml = ">";
            tag.AddCssClass("btn btn-default");
            result.Append(tag.ToString());
            // Кнопка перехода к последней странице
            tag = new TagBuilder("a");
            tag.MergeAttribute("href", pageUrl(pageInfo.TotalPages));
            tag.InnerHtml = ">>";
            tag.AddCssClass("btn btn-default");
            result.Append(tag.ToString());

            return MvcHtmlString.Create(result.ToString());
        }
    }
}