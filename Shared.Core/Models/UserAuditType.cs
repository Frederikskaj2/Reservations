namespace Frederikskaj2.Reservations.Shared.Core;

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
    CreateOrder,
    CreateOwnerOrder,
    PayIn,
    PayOut,
    RequestDelete,
    Delete,
    Reimburse,
    Charge // TODO: "Opkrævning"
}
