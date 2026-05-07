using Frederikskaj2.Reservations.Bank;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record PostingsForMonthDto(LocalDate FromMonth, LocalDate ToMonth, IEnumerable<AccountName> AccountNames, IEnumerable<PostingDto> Postings);
