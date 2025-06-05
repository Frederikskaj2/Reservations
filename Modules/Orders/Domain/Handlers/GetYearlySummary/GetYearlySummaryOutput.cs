using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

record GetYearlySummaryOutput(int Year, Seq<ResourceSummary> ResourceSummaries);
