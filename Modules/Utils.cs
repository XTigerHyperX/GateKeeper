using System;
using System.Linq;
using System.Text;

namespace Gatekeeper.Modules
{
    public static class Utils
    {
        private static readonly Random _RANDOM = new Random();
        private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const char InvisibleChar = (char) 0xAD;
        private const int PasswordLength = 5;

        public static (string, string) GeneratePasteProofString()
        {
            var sb = new StringBuilder[2]
            {
                new StringBuilder(), // encrypted
                new StringBuilder() // decrypted
            };

            var firstChar = AllowedChars[_RANDOM.Next(0, AllowedChars.Length)];
            sb[0].Append(firstChar); // first char should always be a letter
            sb[1].Append(firstChar);

            var charCount = 1;
            while (charCount < PasswordLength)
            {
                if (AllowedChars.Contains(sb[0][^1]) || _RANDOM.Next(2) == 0) // force an invisible char if previous char was a letter
                {
                    sb[0].Append(InvisibleChar);
                }
                else
                {
                    charCount++;
                    var actualChar = AllowedChars[_RANDOM.Next(0, AllowedChars.Length)];
                    sb[0].Append(actualChar);
                    sb[1].Append(actualChar);
                }
            }

            return (sb[0].ToString(), sb[1].ToString());
        }

        public static bool VerifyPasteProofString(string code, string userInput)
        {
            if (userInput.Length != code.Length || userInput.Contains(InvisibleChar))
                return false;

            return !userInput.Where((t, i) => !t.Equals(code[i])).Any();
        }
    }
}