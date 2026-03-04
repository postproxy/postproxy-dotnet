using System.Security.Cryptography;
using System.Text;

namespace PostProxy;

public static class WebhookSignature
{
    public static bool Verify(string payload, string signatureHeader, string secret)
    {
        try
        {
            string? timestamp = null;
            string? expected = null;

            foreach (var part in signatureHeader.Split(','))
            {
                var idx = part.IndexOf('=');
                if (idx < 0) continue;
                var key = part[..idx];
                var value = part[(idx + 1)..];

                if (key == "t") timestamp = value;
                else if (key == "v1") expected = value;
            }

            if (timestamp is null || expected is null)
                return false;

            var signedPayload = $"{timestamp}.{payload}";
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

            var hash = HMACSHA256.HashData(keyBytes, payloadBytes);
            var computed = Convert.ToHexStringLower(hash);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(expected));
        }
        catch
        {
            return false;
        }
    }
}
