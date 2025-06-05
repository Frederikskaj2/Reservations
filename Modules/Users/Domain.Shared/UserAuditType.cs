namespace Frederikskaj2.Reservations.Users;

public enum UserAuditType
{
    None,
    Import,
    SignUp,
    ConfirmEmail,
    RequestResendConfirmEmail,
    RequestNewPassword,
    UpdatePassword,
    UpdateApartmentId,
    UpdateFullName,
    UpdatePhone,
    SetAccountNumber,
    RemoveAccountNumber,
    UpdateEmailSubscriptions,
    UpdateRoles,
    PlaceOrder,
    PlaceOwnerOrder,
    PayIn,
    PayOut,
    RequestDelete,
    Delete,
    Reimburse,
    Charge, // "Opkrævning"
}
