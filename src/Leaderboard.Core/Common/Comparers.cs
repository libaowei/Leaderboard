namespace Leaderboard.Core.Common;

public static class Comparers
{
    public static bool IsGreaterThan<T>(this T firstValue, T secondValue) where T : IComparable<T>
    {
        return firstValue.CompareTo(secondValue) > 0;
    }

    public static bool IsLessThan<T>(this T firstValue, T secondValue) where T : IComparable<T>
    {
        return firstValue.CompareTo(secondValue) < 0;
    }

    public static bool IsGreaterThanOrEqualTo<T>(this T firstValue, T secondValue) where T : IComparable<T>
    {
        return firstValue.CompareTo(secondValue) >= 0;
    }

    public static bool IsLessThanOrEqualTo<T>(this T firstValue, T secondValue) where T : IComparable<T>
    {
        return firstValue.CompareTo(secondValue) <= 0;
    }
}