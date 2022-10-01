using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;

namespace Frederikskaj2.Reservations.Application;

public static class GetMyUserHandler
{
    public static EitherAsync<Failure, MyUser> Handle(IPersistenceContextFactory contextFactory, UserId userId) =>
        from user in ReadUser(CreateContext(contextFactory), userId)
        select UserFactory.CreateMyUser(user);
}
