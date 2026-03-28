namespace HycoatApi.Services.Shared;

public static class NumberToWordsConverter
{
    private static readonly string[] Ones =
    [
        "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
        "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
        "Seventeen", "Eighteen", "Nineteen"
    ];

    private static readonly string[] Tens =
    [
        "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
    ];

    public static string Convert(decimal amount)
    {
        var rupees = (long)Math.Floor(Math.Abs(amount));
        var paise = (int)Math.Round((Math.Abs(amount) - rupees) * 100);

        if (rupees == 0 && paise == 0)
            return "Zero Rupees Only";

        var result = "";

        if (rupees > 0)
        {
            result = ConvertToIndianWords(rupees) + " Rupees";
        }

        if (paise > 0)
        {
            if (result.Length > 0) result += " and ";
            result += ConvertToIndianWords(paise) + " Paise";
        }

        return result + " Only";
    }

    private static string ConvertToIndianWords(long number)
    {
        if (number == 0) return "";
        if (number < 0) return "Minus " + ConvertToIndianWords(-number);

        var parts = new List<string>();

        // Crore (10,000,000)
        if (number >= 10000000)
        {
            parts.Add(ConvertToIndianWords(number / 10000000) + " Crore");
            number %= 10000000;
        }

        // Lakh (100,000)
        if (number >= 100000)
        {
            parts.Add(ConvertToIndianWords(number / 100000) + " Lakh");
            number %= 100000;
        }

        // Thousand
        if (number >= 1000)
        {
            parts.Add(ConvertToIndianWords(number / 1000) + " Thousand");
            number %= 1000;
        }

        // Hundred
        if (number >= 100)
        {
            parts.Add(Ones[number / 100] + " Hundred");
            number %= 100;
        }

        // Tens and ones
        if (number > 0)
        {
            if (number < 20)
            {
                parts.Add(Ones[number]);
            }
            else
            {
                var word = Tens[number / 10];
                if (number % 10 > 0)
                    word += "-" + Ones[number % 10];
                parts.Add(word);
            }
        }

        return string.Join(" ", parts);
    }
}
