using Bogus;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class UserExtensions
{
    static readonly Faker dataFaker = new();

    static readonly Faker<SignUpRequest> signUpRequestFaker = new Faker<SignUpRequest>()
        .RuleFor(request => request.Email, faker => faker.Internet.Email())
        .RuleFor(request => request.FullName, faker => faker.Name.FullName())
        .RuleFor(request => request.Phone, faker => faker.Phone.PhoneNumber("########"))
        .RuleFor(request => request.ApartmentId, faker => ApartmentId.FromInt32(faker.Random.Int(1, 157)))
        .RuleFor(request => request.Password, faker => faker.Internet.Password());

    public static async ValueTask SignUpAsync(this SessionFixture session)
    {
        var request = signUpRequestFaker.Generate();
        var response = await session.PostAnonymousAsync("user/sign-up", request);
        response.EnsureSuccessStatusCode();
        var accountNumber = $"{dataFaker.Finance.Account(4)}-{dataFaker.Finance.Account()}";
        session.User = new User(request.Email!, request.Password!, request.FullName!, request.Phone!, request.ApartmentId!.Value, accountNumber);
    }

    public static async ValueTask SignUpAgainAsync(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var request = new SignUpRequest
        {
            Email = session.User.Email,
            FullName = session.User.FullName,
            Phone = session.User.Phone,
            ApartmentId = session.User.ApartmentId,
            Password = session.User.Password
        };
        var response = await session.PostAnonymousAsync("user/sign-up", request);
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask SignInAsync(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var request = new SignInRequest
        {
            Email = session.User.Email,
            Password = session.User.Password
        };
        var response = await session.PostAnonymousAsync("user/sign-in", request);
        response.EnsureSuccessStatusCode();
        session.Cookies = response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : null;
        session.Tokens = await session.DeserializeAsync<Tokens>(response);
    }

    public static async ValueTask ConfirmEmailAsync(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var email = await session.DequeueConfirmEmailEmailAsync();
        var query = QueryParser.GetQuery(email.Url.ToString());
        var request = new ConfirmEmailRequest
        {
            Email = email.Email.ToString(),
            Token = query["token"].First()
        };
        var response = await session.PostAnonymousAsync("user/confirm-email", request);
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask NewPasswordEmailAsync(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var email = await session.DequeueNewPasswordEmailAsync();
        var newPassword = dataFaker.Internet.Password();
        var query = QueryParser.GetQuery(email.Url.ToString());
        var request = new NewPasswordRequest
        {
            Email = email.Email.ToString(),
            Token = query["token"].First(),
            NewPassword = newPassword
        };
        var response = await session.PostAnonymousAsync("user/new-password", request);
        response.EnsureSuccessStatusCode();
        session.User = session.User with { Password = newPassword };
    }

    public static async ValueTask UpdatePassword(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var newPassword = dataFaker.Internet.Password();
        var request = new UpdatePasswordRequest
        {
            CurrentPassword = session.User.Password,
            NewPassword = newPassword
        };
        var response = await session.PostAsync("user/update-password", request);
        response.EnsureSuccessStatusCode();
        session.User = session.User with { Password = newPassword };
    }

    public static async ValueTask SignUpAndSignInAsync(this SessionFixture session)
    {
        await session.SignUpAsync();
        await session.ConfirmEmailAsync();
        await session.SignInAsync();
    }

    public static async ValueTask<MyTransactions> GetMyTransactionsAsync(this SessionFixture session) =>
        await session.DeserializeAsync<MyTransactions>(await session.GetAsync("transactions"));

    public static UserId UserId(this SessionFixture session)
    {
        if (session.Tokens is null)
            throw new InvalidOperationException();
        return Shared.Core.UserId.FromInt32(
            int.Parse(
                JwtTokenParser.Parse(session.Tokens.AccessToken)![ClaimTypes.NameIdentifier].ToString()!,
                NumberStyles.None,
                CultureInfo.InvariantCulture));
    }

    public static async ValueTask<DeleteUserResponse> DeleteUserAsync(this SessionFixture session)
    {
        if (session.Tokens is null)
            throw new InvalidOperationException();
        return await session.DeserializeAsync<DeleteUserResponse>(await session.DeleteAsync("user"));
    }
}
