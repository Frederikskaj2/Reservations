using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

record DeleteUsersInput(DeleteUsersCommand Command, Seq<User> Users);