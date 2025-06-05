using Frederikskaj2.Reservations.LockBox;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Emails.Messages.LockBoxCodesOverview;

partial class Body
{
    const string baseStyle = "padding: 8px; border-top: solid 1px #CCC; border-collapse: collapse";
    const string leftStyle = $"{baseStyle}; text-align: left";
    const string rightStyle = $"{baseStyle}; text-align: right";
    const string leftBottomBorderStyle = $"{leftStyle}; border-bottom: solid 2px #CCC";
    const string centerBottomBorderStyle = $"{baseStyle}; text-align: center; border-bottom: solid 2px #CCC";

    IEnumerable<Resource> GetOrderedResources() => Model.Data.Resources.OrderBy(resource => resource.Sequence);
}
