//using System.Collections.Concurrent;
//using Leaderboard.Service.Customers.Dto;

//namespace Leaderboard.Service.Customers;

//public class CustomerLeaderboardServiceRBTree : ICustomerLeaderboardService
//{
//    private readonly ConcurrentDictionary<ulong, Customer> _customers = new();
//    private readonly SortedSet<Customer> _leaderboard = new();
//    private readonly ReaderWriterLockSlim _rwlock = new();

//    public decimal UpdateScore(ulong customerId, decimal scoreChange)
//    {
//        //Note: AddOrUpdate 的更新委托并不能保证执行次数，如果更新委托的时间成本较高，就不能用AddOrUpdate
//        _customers.AddOrUpdate(customerId,
//            new Customer { CustomerId = customerId, Score = scoreChange },
//            (id, item) =>
//            {
//                item.Score += scoreChange;
//                return item;
//            });
//        var customer = _customers[customerId];
//        try
//        {
//            _rwlock.EnterWriteLock();
//            _leaderboard.RemoveWhere(x => x.CustomerId == customerId);
//            if (customer.Score > 0)
//            {
//                _leaderboard.Add(customer);
//            }
//            return customer.Score;
//        }
//        finally
//        {
//            _rwlock.ExitWriteLock();
//        }
//    }

//    public List<CustomerWithRank> GetCustomersByRank(int start, int end)
//    {
//        return [];
//    }

//    public List<CustomerWithRank> GetCustomersById(ulong customerId, int high = 0, int low = 0)
//    {
//        return [];
//    }
//}