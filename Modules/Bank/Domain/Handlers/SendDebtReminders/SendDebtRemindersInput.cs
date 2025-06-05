using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Bank;

record SendDebtRemindersInput(SendDebtRemindersCommand Command, Seq<User> UsersWithReminder);
