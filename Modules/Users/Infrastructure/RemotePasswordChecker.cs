using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Users;

class RemotePasswordChecker(ILogger<RemotePasswordChecker> logger, IOptions<PasswordOptions> options, IMemoryCache cache, IHttpClientFactory httpClientFactory)
{
    const int hashByteCount = 20;
    const int hashLength = 2*hashByteCount;
    const int hashPrefixLength = 5;
    const int hashSuffixLength = hashLength - hashPrefixLength;
    const int veryExposedCount = 200; // All the 1,000,000 locally stored hashes are exposed at least this number of times.
    static readonly char[] hexDigits = Enumerable.Range(0, 16).Select(i => (char) (i < 10 ? i + '0' : i - 10 + 'A')).ToArray();
    readonly AsyncLazy<HashSet<byte[]>> exposedPasswordHashes = new(() => GetExposedPasswordHashes(logger));
    readonly RemotePasswordCheckerOptions options = options.Value.RemoteChecker;

    public async Task<int> GetPasswordExposedCount(string password)
    {
        if (!options.IsEnabled)
        {
            logger.LogDebug("Remote password checker is disabled");
            return 0;
        }

        var hash = GetHash(password);
        var hexString = GetHexString(hash);

        return await cache.GetOrCreateAsync(hexString, async entry =>
        {
            entry.SetSize(1);
            entry.SetAbsoluteExpiration(options.CacheExpiration);
            if (!options.AlwaysUseLocalData)
            {
                var exposedCount = await GetPasswordExposedCount(hexString.AsMemory());
                if (exposedCount.HasValue)
                    return exposedCount.Value;
            }

            var localExposedPasswordHashes = await exposedPasswordHashes.Value;
            return localExposedPasswordHashes.Contains(hash) ? veryExposedCount : 0;
        });
    }

    [SuppressMessage("Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "The 'Have I Been Pwned' API uses SHA1.")]
    static byte[] GetHash(string password) => SHA1.HashData(Encoding.UTF8.GetBytes(password));

    static string GetHexString(byte[] hash)
    {
        return string.Create(2*hash.Length, hash, SpanAction);

        static void SpanAction(Span<char> span, byte[] bytes)
        {
            for (var i = 0; i < bytes.Length; i += 1)
            {
                span[2*i] = hexDigits[(bytes[i] & 0xF0) >> 4];
                span[2*i + 1] = hexDigits[bytes[i] & 0xF];
            }
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpClient instances should not be disposed.")]
    async Task<int?> GetPasswordExposedCount(ReadOnlyMemory<char> hexString)
    {
        var httpClient = GetHttpClient();
        var path = new Uri(hexString[..hashPrefixLength].ToString(), UriKind.Relative);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var suffixes = await httpClient.GetStringAsync(path);
            stopwatch.Stop();
            logger.LogInformation("Remote password checker returned {Characters} characters in {Duration}", suffixes.Length, stopwatch.Elapsed);
            var lines = SplitLines(suffixes.AsMemory());
            var suffix = hexString[hashPrefixLength..];
            var matchingLine = lines.FirstOrDefault(line => line[..hashSuffixLength].Span.SequenceEqual(suffix.Span));
            return matchingLine.Length is not 0 ? int.Parse(matchingLine[(hashSuffixLength + 1)..].Span, NumberStyles.None, CultureInfo.InvariantCulture) : 0;
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
                logger.LogWarning("Remote password checker did not respond before the timeout {Timeout} expired", httpClient.Timeout);
            else
                logger.LogWarning(exception, "Remote password checker failed");

            return null;
        }
    }

    HttpClient GetHttpClient()
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new("https://api.pwnedpasswords.com/range/");
        httpClient.Timeout = options.Timeout;
        httpClient.DefaultRequestHeaders.UserAgent.Add(new(options.ProductName, null));
        httpClient.DefaultRequestHeaders.Accept.Add(new("application/vnd.haveibeenpwned.v2+json"));
        return httpClient;
    }

    static IEnumerable<ReadOnlyMemory<char>> SplitLines(ReadOnlyMemory<char> memory)
    {
        var start = 0;
        for (var i = 0; i < memory.Length; i += 1)
        {
            if (memory.Span[i] is not '\r' and not '\n')
                continue;
            if (i > start)
                yield return memory[start..i];
            start = i + 1;
        }

        if (start < memory.Length)
            yield return memory[start..];
    }

    static async Task<HashSet<byte[]>> GetExposedPasswordHashes(ILogger logger)
    {
        var passwordHashes = new HashSet<byte[]>(StructuralEqualityComparer<byte[]>.Default);

        var stopwatch = Stopwatch.StartNew();
        var type = typeof(RemotePasswordChecker);
        await using var stream = type.Assembly.GetManifestResourceStream(type.Namespace + ".WeakPasswords.bin");
        Debug.Assert(stream is not null);
        while (true)
        {
            var hash = new byte[hashByteCount];
            var bytesRead = await stream.ReadAsync(hash);
            if (bytesRead is not hashByteCount)
            {
                logger.LogInformation("Loaded offline list with {Count} passwords in {Elapsed}", passwordHashes.Count, stopwatch.Elapsed);
                return passwordHashes;
            }

            passwordHashes.Add(hash);
        }
    }

    class StructuralEqualityComparer<T> : IEqualityComparer<T> where T : notnull
    {
        public static readonly IEqualityComparer<T> Default = new StructuralEqualityComparer<T>();

        public bool Equals(T? x, T? y) => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

        public int GetHashCode(T obj) => StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
    }
}
