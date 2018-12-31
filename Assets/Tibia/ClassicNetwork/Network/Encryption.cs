using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClient.Network
{
    public class Rsa
    {
        public static BigInteger N;
        protected static BigInteger D;
        protected static BigInteger Me = new BigInteger("65537", 10);

        public static void Encrypt(ref byte[] buffer, int index)
        {
            byte[] temp = new byte[128];
            Array.Copy(buffer, index, temp, 0, 128);

            BigInteger input = new BigInteger(temp);
            BigInteger output = input.ModPow(Me, N);

            Array.Copy(GetPaddedValue(output), 0, buffer, index, 128);
        }
        private static byte[] GetPaddedValue(BigInteger value)
        {
            byte[] result = value.GetBytes();

            const int length = (1024 >> 3);
            if (result.Length >= length)
                return result;

            // left-pad 0x00 value on the result (same integer, correct length)
            byte[] padded = new byte[length];
            Buffer.BlockCopy(result, 0, padded, (length - result.Length), result.Length);
            // temporary result may contain decrypted (plaintext) data, clear it
            Array.Clear(result, 0, result.Length);
            return padded;
        }
        public static int RsaGetSize()
        {
            return 128;
        }
    }

    public static class Tools
    {
        public static uint Adler32(byte[] data, int index, int length)
        {
            const ushort adler = 65521;

            uint a = 1, b = 0;

            while (length > 0)
            {
                int tmp = (length > 5552) ? 5552 : length;
                length -= tmp;

                do
                {
                    a += data[index++];
                    b += a;
                } while (--tmp > 0);

                a %= adler;
                b %= adler;
            }

            return (b << 16) | a;
        }

        public static double round(double r)
        {
            return (r > 0.0) ? Math.Floor(r + 0.5) : Math.Ceiling(r - 0.5);
        }
    }
}
