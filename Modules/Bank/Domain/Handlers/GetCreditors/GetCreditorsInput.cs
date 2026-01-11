using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record GetCreditorsInput(Seq<User> Creditors, Seq<PayOut> InProgressPayOuts);