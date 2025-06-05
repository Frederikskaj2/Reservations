using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Frederikskaj2.Reservations.Users;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder builder, bool isDevelopment)
    {
        builder.MapGet("user", GetMyUserEndpoint.Handle);
        builder.MapPatch("user", UpdateMyUserEndpoint.Handle);
        builder.MapDelete("user", DeleteMyUserEndpoint.Handle);
        builder.MapPost("user/confirm-email", ConfirmEmailEndpoint.Handle);
        builder.MapPost("user/create-access-token", CreateAccessTokenEndpoint.Handle);
        builder.MapPost("user/new-password", NewPasswordEndpoint.Handle);
        builder.MapPost("user/resend-confirm-email-email", ResendConfirmEmailEmailEndpoint.Handle);
        builder.MapPost("user/send-new-password-email", SendNewPasswordEmailEndpoint.Handle);
        builder.MapPost("user/sign-in", SignInEndpoint.Handle);
        builder.MapPost("user/sign-out", SignOutEndpoint.Handle);
        builder.MapPost("user/sign-out-everywhere-else", SignOutEverywhereElseEndpoint.Handle);
        builder.MapPost("user/sign-up", SignUpEndpoint.Handle);
        builder.MapPost("user/update-password", UpdatePasswordEndpoint.Handle);
        builder.MapGet("users", GetUsersEndpoint.Handle);
        builder.MapGet("users/{userId:int}", GetUserEndpoint.Handle);
        builder.MapPatch("users/{userId:int}", UpdateUserEndpoint.Handle);
        if (isDevelopment)
            builder.MapPost("jobs/delete-users/run", DeleteUsersEndpoint.Handle);
        return builder;
    }
}
