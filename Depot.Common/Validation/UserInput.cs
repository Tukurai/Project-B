using System.Text;

namespace Depot.Common.Validation
{
    public static class UserInput
    {
        /// <summary>
        /// Asks the user for a number. If the number is not within the given range, the user will be asked again.
        /// </summary>
        /// <param name="message">The message to show the user when asking for the time.</param>
        /// <param name="min">Minimum to pass, null means no lower bound.</param>
        /// <param name="max">Maximum to pass, null means no upper bound.</param>
        /// <returns>The given number parsed as an integer.</returns>
        public static long? GetNumber(string message, long? min = null, long? max = null)
        {
            Console.WriteLine(message);
            while (true)
            {
                if (!CancelableReadline(out string amountString))
                {
                    return null;
                }

                if (long.TryParse(amountString, out long amount)
                    && (max == null || amount <= max)
                    && (min == null || amount >= min)
                )
                {
                    return amount;
                }
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine(Localization.Ongeldige_invoer);
            }
        }


        /// <summary>
        /// Asks the user for a time in the format 'hours:minutes'.
        /// </summary>
        /// <param name="message">The message to show the user when asking for the time.</param>
        /// <returns>The given time parsed as a timespan.</returns>
        public static TimeSpan? GetTime(string message)
        {
            Console.WriteLine(message);
            while (true)
            {
                if (!CancelableReadline(out string beginTijd))
                {
                    return null;
                }

                if (TimeSpan.TryParseExact(beginTijd, "h\\:m", null, out TimeSpan time))
                {
                    return time;
                }
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine(Localization.Ongeldige_invoer_tijd);
            }
        }


        public static bool CancelableReadline(out string value)
        {
            value = string.Empty;
            var buffer = new StringBuilder();
            var key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Backspace && Console.CursorLeft > 0)
                {
                    var cli = --Console.CursorLeft;
                    buffer.Remove(cli, 1);
                    Console.CursorLeft = 0;
                    Console.Write(new string(' ', buffer.Length + 1));
                    Console.CursorLeft = 0;
                    Console.Write(buffer.ToString());
                    Console.CursorLeft = cli;
                }
                else if (char.IsLetterOrDigit(key.KeyChar) || char.IsWhiteSpace(key.KeyChar) || key.KeyChar == ':')
                {
                    var cli = Console.CursorLeft;
                    buffer.Insert(cli, key.KeyChar);
                    Console.CursorLeft = 0;
                    Console.Write(buffer.ToString());
                    Console.CursorLeft = cli + 1;
                }
                else if (key.Key == ConsoleKey.LeftArrow && Console.CursorLeft > 0)
                {
                    Console.CursorLeft--;
                }
                else if (key.Key == ConsoleKey.RightArrow && Console.CursorLeft < buffer.Length)
                {
                    Console.CursorLeft++;
                }
                key = Console.ReadKey(true);
            }

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                value = buffer.ToString();
                return true;
            }
            return false;
        }
    }
}
