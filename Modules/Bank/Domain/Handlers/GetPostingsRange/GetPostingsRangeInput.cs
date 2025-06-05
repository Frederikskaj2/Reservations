using Frederikskaj2.Reservations.Orders;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record GetPostingsRangeInput(GetPostingsRangeQuery Query, Option<Transaction> EarliestTransaction, Option<Transaction> LatestTransaction);