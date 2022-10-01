using System.Collections.Generic;

namespace Frederikskaj2.Reservations.EmailSender;

record Table(Row Header, IReadOnlyList<Row> Body);