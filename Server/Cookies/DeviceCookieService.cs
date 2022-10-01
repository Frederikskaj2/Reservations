using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Server;

public class DeviceCookieService
{
    const string cookieName = "Device";
    readonly ILogger logger;
    readonly CookieOptions options;
    readonly CookieValueSerializer serializer;

    public DeviceCookieService(ILogger<DeviceCookieService> logger, IOptionsSnapshot<CookieOptions> options, CookieValueSerializer serializer)
    {
        this.logger = logger;
        this.serializer = serializer;
        this.options = options.Value;
    }

    public Cookie CreateCookie(DeviceId deviceId)
    {
        var cookie = new DeviceCookie(deviceId);
        logger.LogTrace("Creating device cookie {Cookie}", cookie);
        return CookieFactory.CreateCookie(cookieName, serializer.Serialize(cookie), options.DeviceCookieDuration);
    }

    public Option<DeviceId> ParseCookie(string? cookie)
    {
        var deviceId = GetDeviceCookieOption(serializer.Deserialize<DeviceCookie>(CookieFactory.ParseCookie(cookieName, cookie)));
        deviceId.Match(
            Some: id => logger.LogTrace("Received device ID {Device} from cookie", id),
            None: () => logger.LogTrace("Received no device ID from cookie"));
        return deviceId;
    }

    static Option<DeviceId> GetDeviceCookieOption(DeviceCookie? cookie) =>
        cookie is { DeviceId: { } } ? cookie.DeviceId.Value : None;
}
