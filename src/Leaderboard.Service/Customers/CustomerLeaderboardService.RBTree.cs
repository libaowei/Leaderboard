using Leaderboard.Service.Customers.Dto;

namespace Leaderboard.Service.Customers;

public class CustomerLeaderboardServiceRBTree : ICustomerLeaderboardService
{
    private readonly Dictionary<ulong, Customer> _customers = new();
    private readonly SortedSet<Customer> _leaderboard = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public decimal UpdateScore(ulong customerId, decimal scoreChange)
    {
        if (scoreChange == 0) throw new ArgumentException("scoreChange cannot be zero.");
        _lock.EnterWriteLock();
        try
        {
            var exist = _customers.TryGetValue(customerId, out Customer? customer);
            customer ??= new Customer
            {
                CustomerId = customerId,
                Score = 0
            };
            _customers[customerId] = customer;

            var oldScore = customer.Score;
            var newScore = oldScore + scoreChange;
            if (exist && oldScore > 0)
            {
                _leaderboard.Remove(customer);
            }

            customer.Score = newScore;
            if (customer.Score > 0)
            {
                _leaderboard.Add(customer);
            }

            return customer.Score;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 时间复杂度O(end)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IEnumerable<CustomerDto> GetCustomersByRank(int start, int end)
    {
        if (end > _leaderboard.Count) end = _leaderboard.Count;
        if (start <= end)
        {
            var rank = 1;
            var list = new List<CustomerDto>(end - start + 1);
            _lock.EnterReadLock();
            try
            {
                foreach (var customer in _leaderboard)
                {
                    if (rank >= start)
                    {
                        list.Add(new CustomerDto
                        {
                            CustomerId = customer.CustomerId,
                            Score = customer.Score,
                            Rank = rank
                        });
                    }
                    if (rank == end) break;
                    rank++;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        return [];
    }

    /// <summary>
    /// 时间复杂度不确定，最小O(1), 最大O(N)
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="high"></param>
    /// <param name="low"></param>
    /// <returns></returns>
    public IEnumerable<CustomerDto> GetCustomersById(ulong customerId, int high = 0, int low = 0)
    {
        _lock.EnterReadLock();
        try
        {
            var list = new List<Customer>();
            var matched = false;
            var index = 0;
            var rank = 0;
            foreach (var customer in _leaderboard)
            {
                list.Add(customer);
                if (customer.CustomerId == customerId)
                {
                    matched = true;
                    rank = index;
                }
                index++;
                if (matched) low--;
                if (low == -1) break;
            }
            var start = rank - high;
            if (start < 0) start = 0;
            var siblings = new List<CustomerDto>(index - start);
            for (var i = start; i < index; i++)
            {
                siblings.Add(new CustomerDto
                {
                    CustomerId = list[i].CustomerId,
                    Score = list[i].Score,
                    Rank = i + 1
                });
            }
            return siblings;
        }
        finally
        {
            _lock?.ExitReadLock();
        }
    }
}