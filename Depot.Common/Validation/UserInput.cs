using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static int GetNumber(string message, int? min = null, int? max = null)
        {
            Console.WriteLine(message);
            do
            {
                string amountString = Console.ReadLine() ?? "";

                if (!int.TryParse(amountString, out int amount)
                    || (max != null && amount > max)
                    || (min != null && amount < min)
                )
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine("Ongeldige invoer.");
                }

                return amount;
            } while (true);
        }

        /// <summary>
        /// Asks the user for a time in the format 'hours:minutes'.
        /// </summary>
        /// <param name="message">The message to show the user when asking for the time.</param>
        /// <returns>The given time parsed as a timespan.</returns>
        public static TimeSpan GetTime(string message)
        {
            Console.WriteLine(message);
            do
            {
                string beginTijd = Console.ReadLine() ?? "";

                if (TimeSpan.TryParseExact(beginTijd, "h\\:m", null, out TimeSpan time))
                {
                    return time;
                }
                else
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine("Ongeldige tijd. Hou het formaat 'uren:minuten' aan.");
                }
            } while (true);
        }
    }
}
