using System;
using System.Text;

namespace src.Modules
{
    public static class Utils
    {
        private static readonly Random _RANDOM = new Random();
        private const string _ALLOWED_CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const char _INVISIBLE_CHAR = (char)0xAD;
        private const int _PASSWORD_LENGTH = 5;

        public static (string, string) GeneratePasteProofString()
        {
            StringBuilder[] sb = new StringBuilder[2]
            {
                new StringBuilder(), // encrypted
                new StringBuilder()  // decrypted
            };

            char firstChar = _ALLOWED_CHARS[_RANDOM.Next(0, _ALLOWED_CHARS.Length)];
            sb[0].Append(firstChar); // first char should always be a letter
            sb[1].Append(firstChar);

            int charCount = 1;
            while (charCount < _PASSWORD_LENGTH)
            {
                if (_ALLOWED_CHARS.Contains(sb[0][^1]) || _RANDOM.Next(2) == 0) // force an invisible char if previous char was a letter
                {
                    sb[0].Append(_INVISIBLE_CHAR);
                }
                else
                {
                    charCount++;
                    char actualChar = _ALLOWED_CHARS[_RANDOM.Next(0, _ALLOWED_CHARS.Length)];
                    sb[0].Append(actualChar);
                    sb[1].Append(actualChar);
                }
            }

            return (sb[0].ToString(), sb[1].ToString());
        }

        public static bool VerifyPasteProofString(string code, string userInput)
        {
            if (userInput.Length != code.Length || userInput.Contains(_INVISIBLE_CHAR))
                return false;

            for (int i = 0; i < userInput.Length; i++)
            {
                if (!userInput[i].Equals(code[i]))
                    return false;
            }

            return true;
        }
    }
}