namespace Frederikskaj2.Reservations.EmailSender;

public static class Html
{
    public static IHtmlContent Raw(string html) => new HtmlString(html);
}