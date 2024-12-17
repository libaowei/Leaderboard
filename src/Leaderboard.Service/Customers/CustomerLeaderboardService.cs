﻿using Leaderboard.Core.DataStructures;
using Leaderboard.Service.Customers.Dto;

namespace Leaderboard.Service.Customers;

public class CustomerLeaderboardService : ICustomerLeaderboardService
{
    private readonly Dictionary<ulong, Customer> _customers = [];
    private readonly SkipList<Customer> _leaderboard = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public decimal UpdateScore(ulong customerId, decimal scoreChange)
    {
        if (scoreChange == 0) throw new ArgumentException("scoreChange cannot be zero.");
        try
        {
            _lock.EnterWriteLock();
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
                _leaderboard.Insert(customer);
            }

            return customer.Score;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IEnumerable<CustomerDto> GetCustomersByRank(int start, int end)
    {
        if (end >= start || start <= _leaderboard.Count)
        {
            if (end > _leaderboard.Count) end = _leaderboard.Count;
            try
            {
                _lock.EnterReadLock();
                var customers = _leaderboard.GetByRange(start, end);
                var list = new List<CustomerDto>(customers.Count);
                foreach (var (rank, item) in customers)
                {
                    list.Add(new CustomerDto
                    {
                        CustomerId = item.CustomerId,
                        Score = item.Score,
                        Rank = rank
                    });
                }
                return list;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        return [];
    }

    public IEnumerable<CustomerDto> GetCustomersById(ulong customerId, int high = 0, int low = 0)
    {
        try
        {
            _lock.EnterReadLock();
            if (_customers.TryGetValue(customerId, out var customer))
            {
                var customers = _leaderboard.GetNeighbors(customer, high, low);
                var list = new List<CustomerDto>(customers.Count);
                foreach (var (rank, item) in customers)
                {
                    list.Add(new CustomerDto
                    {
                        CustomerId = item.CustomerId,
                        Score = item.Score,
                        Rank = rank
                    });
                }
                return list;
                //var rank = (int)_leaderboard.GetRank(customer);
                //var begin = rank - high;
                //var end = rank + low;
                //if (begin < 1) begin = 1;
                //if (end > _leaderboard.Count) end = _leaderboard.Count;
                //return GetLeaderboard(begin, end);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }
        return [];
    }
}