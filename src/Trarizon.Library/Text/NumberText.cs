namespace Trarizon.Library.Text;

public static partial class NumberText
{
    private static readonly string[] s_romanNumbers = ["I", "IV", "V", "IX", "X", "XL", "L", "XC", "C", "CD", "D", "CM", "M"];

    public static string NumberToRoman(int number)
    {
        var sb = (stackalloc char[15]);
        NumberToRoman(number, sb, out var length);
        return sb[..length].ToString();
    }

    public static void NumberToRoman(int number, Span<char> span, out int length)
    {
        ReadOnlySpan<int> values = [1, 4, 5, 9, 10, 40, 50, 90, 100, 400, 500, 900, 1000];

        var isb = 0;
        for (int i = values.Length - 1; i >= 0; i--)
        {
            while (number >= values[i])
            {
                span[isb++] = s_romanNumbers[i][0];
                number -= values[i];
            }
        }
        length = isb;
    }
}
