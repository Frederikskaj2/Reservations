using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Bank;

public record GetPostingsResponse(IEnumerable<PostingDto> Postings);
