using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

static class UserExtensions
{
    public static async ValueTask SignUp(this SessionFixture session)
    {
        var request = Generate.SignUpRequest();
        await session.SignUpRaw(request);
        var accountNumber = Generate.AccountNumber();
        session.User = new(request.Email!, request.Password!, request.FullName!, request.Phone!, request.ApartmentId!.Value, accountNumber);
    }

    public static async ValueTask SignUpRaw(this SessionFixture session, SignUpRequest request)
    {
        var response = await session.PostAnonymous("user/sign-up", request);
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask SignUpAgain(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var request = new SignUpRequest(email: session.User.Email, fullName: session.User.FullName, phone: session.User.Phone,
            apartmentId: session.User.ApartmentId, password: session.User.Password);
        var response = await session.PostAnonymous("user/sign-up", request);
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask SignIn(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var response = await session.SignInRaw(session.User.Email, session.User.Password);
        response.EnsureSuccessStatusCode();
        session.Cookies = response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : null;
        var tokensResponse  = await session.Deserialize<TokensResponse>(response);
        session.AccessToken = tokensResponse.AccessToken;
    }

    public static ValueTask<HttpResponseMessage> SignInRaw(this SessionFixture session, string? email, string? password)
    {
        var request = new SignInRequest(email, password, IsPersistent: false);
        return session.PostAnonymous("user/sign-in", request);
    }

    public static async ValueTask<TokensResponse> CreateAccessToken(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var response = await session.Post("user/create-access-token");
        response.EnsureSuccessStatusCode();
        session.Cookies = response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : null;
        var tokensResponse = await session.Deserialize<TokensResponse>(response);
        session.AccessToken = tokensResponse.AccessToken;
        return tokensResponse;
    }

    public static ValueTask<HttpResponseMessage> CreateAccessTokenRaw(this SessionFixture session, IEnumerable<string>? cookies = null)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        return session.Post("user/create-access-token", cookies ?? session.Cookies);
    }

    public static async ValueTask SignOutEverywhereElse(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var response = await session.Post("user/sign-out-everywhere-else");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask ConfirmEmail(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var email = await session.DequeueConfirmEmailEmail();
        var query = QueryParser.GetQuery(email.ConfirmEmail!.Url.ToString());
        var request = new ConfirmEmailRequest(Email: email.ToEmail.ToString(), Token: query["token"].First());
        var response = await session.PostAnonymous("user/confirm-email", request);
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask ResendConfirmEmailEmail(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var response = await session.Post("user/resend-confirm-email-email");
        response.EnsureSuccessStatusCode();
    }

    public static async ValueTask SendNewPasswordEmail(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var response = await session.SendNewPasswordEmailRaw(session.User.Email);
        response.EnsureSuccessStatusCode();
    }

    public static ValueTask<HttpResponseMessage> SendNewPasswordEmailRaw(this SessionFixture session, string email)
    {
        var request = new SendNewPasswordEmailRequest(Email: email);
        return session.PostAnonymous("user/send-new-password-email", request);
    }

    public static async ValueTask NewPasswordFromNewPasswordEmail(this SessionFixture session)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var email = await session.DequeueNewPasswordEmail();
        var newPassword = Generate.Password();
        var query = QueryParser.GetQuery(email.NewPassword!.Url.ToString());
        var request = new NewPasswordRequest(Email: email.ToEmail.ToString(), Token: query["token"].First(), NewPassword: newPassword);
        var response = await session.PostAnonymous("user/new-password", request);
        response.EnsureSuccessStatusCode();
        session.User = session.User with { Password = newPassword };
    }

    public static async ValueTask UpdatePassword(this SessionFixture session)
    {
        var newPassword = Generate.Password();
        var response = await session.UpdatePasswordRaw(newPassword);
        response.EnsureSuccessStatusCode();
        session.User = session.User! with { Password = newPassword };
    }

    public static ValueTask<HttpResponseMessage> UpdatePasswordRaw(this SessionFixture session, string? newPassword = null)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var request = new UpdatePasswordRequest(CurrentPassword: session.User.Password, NewPassword: newPassword ?? Generate.Password());
        return session.Post("user/update-password", request);
    }

    public static async ValueTask SignUpAndSignIn(this SessionFixture session)
    {
        await session.SignUp();
        await session.ConfirmEmail();
        await session.SignIn();
    }

    public static async ValueTask<GetMyUserResponse> GetMyUser(this SessionFixture session) =>
        await session.Deserialize<GetMyUserResponse>(await session.Get("user"));

    public static async ValueTask UpdateMyUser(this SessionFixture session, string fullName, string phone)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var request = new UpdateMyUserRequest(fullName, phone, EmailSubscriptions.None);
        var response = await session.Patch("user", request);
        response.EnsureSuccessStatusCode();
        var myUser = await session.Deserialize<GetMyUserResponse>(response);
        session.User = session.User with { FullName = myUser.Identity.FullName, Phone = myUser.Identity.Phone };
    }

    public static ValueTask<HttpResponseMessage> UpdateMyEmailSubscriptionsRaw(this SessionFixture session, EmailSubscriptions emailSubscriptions)
    {
        if (session.User is null)
            throw new InvalidOperationException();
        var request = new UpdateMyUserRequest(session.User.FullName, session.User.Phone, emailSubscriptions);
        return session.Patch("user", request);
    }

    public static UserId UserId(this SessionFixture session)
    {
        if (session.AccessToken is null)
            throw new InvalidOperationException();
        return Users.UserId.FromInt32(
            int.Parse(
                JwtTokenParser.Parse(session.AccessToken)![ClaimTypes.NameIdentifier].ToString(),
                NumberStyles.None,
                CultureInfo.InvariantCulture));
    }

    public static async ValueTask<DeleteUserResponse> DeleteUser(this SessionFixture session)
    {
        if (session.AccessToken is null)
            throw new InvalidOperationException();
        return await session.Deserialize<DeleteUserResponse>(await session.Delete("user"));
    }
}
