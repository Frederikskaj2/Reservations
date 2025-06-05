using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

class DeviceCookieService(ILogger<DeviceCookieService> logger, IOptionsSnapshot<CookieOptions> options, CookieValueSerializer serializer)
    : IDeviceCookieService
{
    const string cookieName = "Device";
    readonly CookieOptions options = options.Value;

    public Cookie CreateCookie(DeviceId deviceId)
    {
        var cookie = new DeviceCookie(deviceId);
        logger.LogTrace("Creating device cookie {Cookie}", cookie);
        return CookieProvider.CreateCookie(cookieName, serializer.Serialize(cookie), options.DeviceCookieDuration);
    }

    public Option<DeviceId> ParseCookie(string? cookie)
    {
        var deviceId = GetDeviceCookieOption(serializer.Deserialize<DeviceCookie>(CookieProvider.ParseCookie(cookieName, cookie)));
        deviceId.Match(
            Some: id => logger.LogTrace("Received device ID {Device} from cookie", id),
            None: () => logger.LogTrace("Device ID cookie is invalid or missing"));
        return deviceId;
    }

    static Option<DeviceId> GetDeviceCookieOption(DeviceCookie? cookie) =>
        cookie is { DeviceId: not null } ? cookie.DeviceId.Value : None;
}
