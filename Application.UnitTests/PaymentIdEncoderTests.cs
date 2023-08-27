using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Frederikskaj2.Reservations.Application.UnitTests;

public class PaymentIdEncoderTests
{
    [Fact]
    public void AllValidPaymentIdsCanRoundtrip()
    {
        for (var i = 0; i < 0x8000; i += 1)
        {
            var userId = UserId.FromInt32(i);
            var paymentId = PaymentIdEncoder.FromUserId(userId);
            var roundtripUserId = PaymentIdEncoder.ToUserId(paymentId);
            Assert.Equal(roundtripUserId, userId);
        }
    }

    [Fact]
    public void NoValidPaymentIdsOutOfRange()
    {
        var characterSet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();
        var allPossiblePaymentIds = CreateAllPossiblePaymentIds();
        Assert.Equal(0x8000, allPossiblePaymentIds.Count());
        return;

        IEnumerable<PaymentId> CreateAllPossiblePaymentIds()
        {
            for (var i = 0; i < characterSet.Length; i += 1)
            for (var j = 0; j < characterSet.Length; j += 1)
            for (var k = 0; k < characterSet.Length; k += 1)
            for (var l = 0; l < characterSet.Length; l += 1)
            {
                var paymentId = CreatePaymentId(i, j, k, l);
                if (PaymentIdEncoder.IsValid(paymentId))
                    yield return paymentId;
            }
        }

        PaymentId CreatePaymentId(int i, int j, int k, int l) =>
            PaymentId.FromString($"B-{characterSet[i]}{characterSet[j]}{characterSet[k]}{characterSet[l]}");
    }
}
