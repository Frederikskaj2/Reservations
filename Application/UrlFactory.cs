using Frederikskaj2.Reservations.Shared.Core;
using System;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Application;

static class UrlFactory
{
    public static Uri GetMyOrderUrl(Uri baseUrl, OrderId orderId)
    {
        var uriBuilder = new UriBuilder(baseUrl!)
        {
            Path = $"{Urls.MyOrders}/{orderId}"
        };
        return uriBuilder.Uri;
    }

    public static Uri GetOrderUrl(Uri baseUrl, OrderId orderId)
    {
        var uriBuilder = new UriBuilder(baseUrl!)
        {
            Path = $"{Urls.Orders}/{orderId}"
        };
        return uriBuilder.Uri;
    }

    public static Uri GetConfirmEmailUrl(Uri baseUrl, EmailAddress email, ImmutableArray<byte> token)
    {
        var uriBuilder = new UriBuilder(baseUrl!)
        {
            Path = Urls.ConfirmEmail,
            Query = $"?email={Uri.EscapeDataString(email.ToString())}&token={Uri.EscapeDataString(Convert.ToBase64String(token.UnsafeNoCopyToArray()))}"
        };
        return uriBuilder.Uri;
    }

    public static Uri GetNewPasswordUrl(Uri baseUrl, EmailAddress email, ImmutableArray<byte> token)
    {
        var uriBuilder = new UriBuilder(baseUrl!)
        {
            Path = Urls.NewPassword,
            Query = $"?email={Uri.EscapeDataString(email.ToString())}&token={Uri.EscapeDataString(Convert.ToBase64String(token.UnsafeNoCopyToArray()))}"
        };
        return uriBuilder.Uri;
    }

    public static Uri GetRulesUrl(Uri baseUrl, ResourceId resourceId)
    {
        var uriBuilder = new UriBuilder(baseUrl!)
        {
            Path = Resources.GetResourceType(resourceId).Case switch
            {
                ResourceType.BanquetFacilities => Urls.RulesBanquetFacilities,
                ResourceType.Bedroom => Urls.RulesBedrooms,
                _ => throw new ArgumentException($"Invalid resource ID {resourceId}.", nameof(resourceId))
            }
        };
        return uriBuilder.Uri;
    }
}
