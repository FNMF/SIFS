using System.Security.Cryptography;

namespace SIFS.Shared.Helpers
{
    public class UuidV7
    {
        public static Guid NewUuidV7()          //按照UuidV7生成Guid
        {
            var bytes = NewUuidV7ToBtyes();
            return new Guid(bytes);
        }
        public static byte[] NewUuidV7ToBtyes()         //按照UuidV7生成对应的Bytes
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // 48 bits
            var random = RandomNumberGenerator.GetBytes(10); // 80 bits (128 - 48)

            Span<byte> bytes = stackalloc byte[16];

            // Put timestamp (big endian)
            bytes[0] = (byte)(timestamp >> 40);
            bytes[1] = (byte)(timestamp >> 32);
            bytes[2] = (byte)(timestamp >> 24);
            bytes[3] = (byte)(timestamp >> 16);
            bytes[4] = (byte)(timestamp >> 8);
            bytes[5] = (byte)(timestamp);

            // Version 7
            bytes[6] = (byte)(0x70 | ((random[0] & 0x0F))); // 0111 xxxx
            bytes[7] = random[1];

            // Variant (10xx)
            bytes[8] = (byte)((random[2] & 0x3F) | 0x80);
            bytes[9] = random[3];

            // Remaining random bytes
            for (int i = 10; i < 16; i++)
                bytes[i] = random[i - 6];

            return bytes.ToArray();
        }
    }
}
