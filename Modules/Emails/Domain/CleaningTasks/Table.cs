using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

record Table(Row Header, IReadOnlyList<Row> Body);