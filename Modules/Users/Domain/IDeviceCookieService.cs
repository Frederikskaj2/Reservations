using LanguageExt;

namespace Frederikskaj2.Reservations.Users;

public interface IDeviceCookieService
{
    Cookie CreateCookie(DeviceId deviceId);
    Option<DeviceId> ParseCookie(string? cookie);
}
