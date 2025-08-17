using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record SendDebtReminderOutput(Seq<User> UsersToUpdate, Seq<User> UsersToRemind, DebtReminder DebtReminder);
