using System.IO;
using System.Text.Encodings.Web;

namespace Frederikskaj2.Reservations.EmailSender;

public interface IHtmlContent
{
    void WriteTo(TextWriter writer, HtmlEncoder encoder);
}
