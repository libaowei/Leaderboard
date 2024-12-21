using System.Diagnostics;

namespace Leaderboard.Service.Customers.Dto;

[DebuggerDisplay("{CustomerId},{Score}")]
public class Customer : IComparable<Customer>
{
    public ulong CustomerId { get; set; }
    public decimal Score { get; set; }

    public int CompareTo(Customer? other)
    {
        int scoreDescComparison = other!.Score.CompareTo(Score);
        return scoreDescComparison != 0 ? scoreDescComparison : CustomerId.CompareTo(other.CustomerId);
    }
}

public class CustomerDto
{
    public ulong CustomerId { get; set; }
    public decimal Score { get; set; }
    public int Rank { get; set; }
}