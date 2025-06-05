using Frederikskaj2.Reservations.Core;
using LanguageExt;
using PhoneNumbers;
using System;
using System.Collections.Immutable;
using System.Net;
using static Frederikskaj2.Reservations.Core.Validator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class Validator
{
    static readonly Roles allRoles = Enum.GetValues<Roles>().Fold(default(Roles), (state, role) => state | role);
    static readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public static Either<Failure<SignUpError>, SignUpCommand> ValidateSignUp(IDateProvider dateProvider, SignUpRequest request)
    {
        var either =
            from email in ValidateEmail(request.Email)
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from apartmentId in ValidateApartmentIdOptional(request.ApartmentId)
            from password in ValidatePassword(request.Password)
            select new SignUpCommand(dateProvider.Now, email, fullName, phone, apartmentId, password);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, SignUpError.InvalidRequest);
    }

    public static Either<Failure<SignInError>, SignInCommand> ValidateSignIn(IDateProvider dateProvider, SignInRequest request, Option<DeviceId> deviceId)
    {
        var either =
            from email in ValidateEmail(request.Email)
            from password in ValidatePassword(request.Password)
            select new SignInCommand(dateProvider.Now, email, password, request.IsPersistent, deviceId);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, SignInError.InvalidRequest);
    }

    public static Either<Failure<PasswordError>, UpdatePasswordCommand> ValidateUpdatePassword(
        IDateProvider dateProvider, UpdatePasswordRequest request, ParsedRefreshToken parsedToken)
    {
        var either =
            from currentPassword in ValidatePassword(request.CurrentPassword)
            from newPassword in ValidatePassword(request.NewPassword)
            select new UpdatePasswordCommand(dateProvider.Now, currentPassword, newPassword, parsedToken.UserId, parsedToken.TokenId);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity, PasswordError.InvalidRequest);
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

    public static Either<Failure<Unit>, SendNewPasswordEmailCommand> ValidateSendNewPasswordEmail(
        IDateProvider dateProvider, SendNewPasswordEmailRequest request)
    {
        var either =
            from email in ValidateEmail(request.Email)
            select new SendNewPasswordEmailCommand(dateProvider.Now, email);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
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

    public static Either<Failure<Unit>, UpdateMyUserCommand> ValidateUpdateMyUser(IDateProvider dateProvider, UpdateMyUserRequest request, UserId userId)
    {
        var either =
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            select new UpdateMyUserCommand(dateProvider.Now, userId, fullName, phone, request.EmailSubscriptions);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    static Either<string, EmailAddress> ValidateEmail(string? email) =>
        from emailNotNull in IsNotNullOrEmpty(email, "Email")
        let trimmedEmail = emailNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedEmail, ValidationRule.MaximumEmailLength, "Email")
        from _2 in IsMatching(trimmedEmail, ValidationRule.EmailRegex, "Email")
        select EmailAddress.FromString(trimmedEmail);

    static Either<string, Option<ApartmentId>> ValidateApartmentIdOptional(ApartmentId? apartmentId) =>
        apartmentId.HasValue
            ? Some(ValidateApartmentId(apartmentId)).Sequence()
            : (Option<ApartmentId>) None;

    public static Either<string, ApartmentId> ValidateApartmentId(ApartmentId? apartmentId) =>
        apartmentId.HasValue && Apartments.IsValid(apartmentId.Value) ? apartmentId.Value : "Invalid apartment ID.";

    static Either<string, string> ValidatePassword(string? password) =>
        from nonEmptyPassword in IsNotNullOrEmpty(password, "Password")
        from validPassword in IsNotLongerThan(nonEmptyPassword, ValidationRule.MaximumPasswordLength, "Password")
        select validPassword;

    static Either<string, ImmutableArray<byte>> IsBase64(string value, string context)
    {
        try
        {
            return Convert.FromBase64String(value).UnsafeNoCopyToImmutableArray();
        }
        catch (FormatException)
        {
            return $"{context} is not valid base64.";
        }
    }

    public static Either<Failure<Unit>, UpdateUserCommand> ValidateUpdateUser(
        IDateProvider dateProvider, UserId userId, UpdateUserRequest request, UserId createdByUserId)
    {
        var either =
            from fullName in ValidateFullName(request.FullName)
            from phone in ValidatePhone(request.Phone)
            from roles in ValidateRoles(request.Roles)
            select new UpdateUserCommand(dateProvider.Now, createdByUserId, userId, fullName, phone, roles, request.IsPendingDelete);
        return either.MapFailure(HttpStatusCode.UnprocessableEntity);
    }

    public static Either<string, string> ValidateFullName(string? fullName) =>
        from fullNameNotNull in IsNotNullOrEmpty(fullName, "Full name")
        let trimmedFullName = fullNameNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedFullName, ValidationRule.MaximumFullNameLength, "Full name")
        from _2 in IsMatching(trimmedFullName, ValidationRule.FullNameRegex, "Full name")
        select trimmedFullName;

    public static Either<string, string> ValidatePhone(string? phone) =>
        from phoneNotNull in IsNotNullOrEmpty(phone, "Phone")
        let trimmedPhone = phoneNotNull.Trim()
        from _1 in IsNotLongerThan(trimmedPhone, ValidationRule.MaximumPhoneLength, "Phone")
        from _2 in IsMatching(trimmedPhone, ValidationRule.PhoneRegex, "Phone")
        select SanitizePhoneNumber(trimmedPhone);

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

    static Either<string, Roles> ValidateRoles(Roles roles) =>
        (roles & ~allRoles) != default ? "Roles are invalid." : roles;
}
