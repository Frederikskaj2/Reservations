using System;
using System.Net;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using LanguageExt;
using PhoneNumbers;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

static partial class Validator
{
    static readonly Roles allRoles = Enum.GetValues<Roles>().Fold(default(Roles), (state, role) => state | role);
    static readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public static Either<Failure<SignInError>, SignInCommand> ValidateSignIn(IDateProvider dateProvider, SignInRequest? body, Option<DeviceId> deviceId)
    {
        var either =
            from request in HasValue(body, "Request body")
            from email in ValidateEmail(request.Email)
            from password in ValidatePassword(request.Password)
            select new SignInCommand(dateProvider.Now, email, password, request.IsPersistent, deviceId);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, SignInError.InvalidRequest);
    }

    public static Either<Failure<SignUpError>, SignUpCommand> ValidateSignUp(
        Func<ApartmentId, bool> isValidApartmentId, IDateProvider dateProvider, SignUpRequest request)
    {
        var either =
            from email in ValidateEmail(request.Email)
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from apartmentId in ValidateApartmentIdOptional(isValidApartmentId, request.ApartmentId)
            from password in ValidatePassword(request.Password)
            select new SignUpCommand(dateProvider.Now, email, fullName, phone, apartmentId, password);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, SignUpError.InvalidRequest);
    }

    public static Either<Failure, UpdateMyUserCommand> ValidateUpdateMyUser(IDateProvider dateProvider, UpdateMyUserRequest request, UserId userId)
    {
        var either =
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            select new UpdateMyUserCommand(dateProvider.Now, userId, fullName, phone, request.EmailSubscriptions);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure, UpdateUserCommand> ValidateUpdateUser(
        IDateProvider dateProvider, UserId userId, UpdateUserRequest? body, UserId administratorUserId)
    {
        var either =
            from request in HasValue(body, "Request body")
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from roles in ValidateRoles(request.Roles)
            select new UpdateUserCommand(dateProvider.Now, administratorUserId, userId, fullName, phone, roles, request.IsPendingDelete);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<Failure<PasswordError>, UpdatePasswordCommand> ValidateUpdatePassword(
        IDateProvider dateProvider, UpdatePasswordRequest? body, ParsedRefreshToken parsedToken)
    {
        var either =
            from request in HasValue(body, "Request body")
            from currentPassword in ValidatePassword(request.CurrentPassword)
            from newPassword in ValidatePassword(request.NewPassword)
            select new UpdatePasswordCommand(dateProvider.Now, currentPassword, newPassword, parsedToken);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, PasswordError.InvalidRequest);
    }

    public static Either<Failure<NewPasswordError>, NewPasswordCommand> ValidateNewPassword(IDateProvider dateProvider, NewPasswordRequest request)
    {
        var either =
            from email in ValidateEmail(request.Email)
            from newPassword in ValidatePassword(request.NewPassword)
            from tokenNotNull in IsNotNullOrEmpty(request.Token, "Token")
            from token in IsBase64(tokenNotNull, "Token")
            select new NewPasswordCommand(dateProvider.Now, email, newPassword, token);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, NewPasswordError.InvalidRequest);
    }

    public static Either<Failure<ConfirmEmailError>, ConfirmEmailCommand> ValidateConfirmEmail(IDateProvider dateProvider, ConfirmEmailRequest request)
    {
        var either =
            from email in ValidateEmail(request.Email)
            from tokenNotNull in IsNotNullOrEmpty(request.Token, "Token")
            from token in IsBase64(tokenNotNull, "Token")
            select new ConfirmEmailCommand(dateProvider.Now, email, token);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, ConfirmEmailError.InvalidRequest);
    }

    public static Either<Failure, SendNewPasswordEmailCommand> ValidateSendNewPasswordEmail(IDateProvider dateProvider, SendNewPasswordEmailRequest request)
    {
        var either =
            from email in ValidateEmail(request.Email)
            select new SendNewPasswordEmailCommand(dateProvider.Now, email);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, EmailAddress> ValidateEmail(string? email) =>
        from emailNotNull in IsNotNullOrEmpty(email, "Email")
        let trimmedEmail = emailNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedEmail, ValidationRules.MaximumEmailLength, "Email")
        from _2 in IsMatching(trimmedEmail, ValidationRules.EmailRegex, "Email")
        select EmailAddress.FromString(trimmedEmail);

    static Either<string, string> ValidateFullName(string? fullName) =>
        from fullNameNotNull in IsNotNullOrEmpty(fullName, "Full name")
        let trimmedFullName = fullNameNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedFullName, ValidationRules.MaximumFullNameLength, "Full name")
        from _2 in IsMatching(trimmedFullName, ValidationRules.FullNameRegex, "Full name")
        select trimmedFullName;

    static Either<string, string> ValidatePhone(string? phone) =>
        from phoneNotNull in IsNotNullOrEmpty(phone, "Phone")
        let trimmedPhone = phoneNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedPhone, ValidationRules.MaximumPhoneLength, "Phone")
        from _2 in IsMatching(trimmedPhone, ValidationRules.PhoneRegex, "Phone")
        select SanitizePhoneNumber(trimmedPhone);

    static Either<string, Roles> ValidateRoles(Roles roles) =>
        (roles & ~allRoles) != default ? "Roles are invalid." : roles;

    static Either<string, Option<string>> ValidateOptionalOwnerOrderDescription(string? description) =>
        description is null ? Option<string>.None : Some(ValidateOwnerOrderDescription(description)).Sequence();

    static Either<string, string> ValidateOwnerOrderDescription(string? description) =>
        from descriptionNotNull in IsNotNullOrEmpty(description, "Description")
        let trimmedDescription = descriptionNotNull.Trim()
        from _ in IsNotLongerThan(trimmedDescription, ValidationRules.MaximumOwnerOrderDescriptionLength, "Description")
        select trimmedDescription;

    static Either<string, Option<ApartmentId>> ValidateApartmentIdOptional(Func<ApartmentId, bool> isValidApartmentId, ApartmentId? apartmentId) =>
        apartmentId.HasValue
            ? Some(ValidateApartmentId(isValidApartmentId, apartmentId)).Sequence()
            : (Option<ApartmentId>) None;

    static Either<string, ApartmentId> ValidateApartmentId(Func<ApartmentId, bool> isValidApartmentId, ApartmentId? apartmentId) =>
        apartmentId.HasValue && isValidApartmentId(apartmentId.Value) ? apartmentId.Value : "Invalid apartment ID.";

    static Either<string, string> ValidatePassword(string? password) =>
        IsNotNullOrEmpty(password, "Password");

    static string SanitizePhoneNumber(string phoneNumber)
    {
        try
        {
            var parsed = phoneNumberUtil.Parse(phoneNumber, "DK");
            var format = parsed.CountryCode is 45 ? PhoneNumberFormat.NATIONAL : PhoneNumberFormat.INTERNATIONAL;
            return phoneNumberUtil.Format(parsed, format);
        }
        catch (NumberParseException)
        {
            return phoneNumber;
        }
    }
}
