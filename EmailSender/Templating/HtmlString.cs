using System.IO;
using System.Text.Encodings.Web;

namespace Frederikskaj2.Reservations.EmailSender;

class HtmlString : IHtmlContent
{
    readonly string html;

    public HtmlString(string html) => this.html = html;

    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => writer.Write(html);
}
