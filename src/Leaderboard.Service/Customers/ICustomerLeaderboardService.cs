using Leaderboard.Service.Customers.Dto;

namespace Leaderboard.Service.Customers;

public interface ICustomerLeaderboardService
{
    IEnumerable<CustomerDto> GetCustomersById(ulong customerId, int high = 0, int low = 0);
    IEnumerable<CustomerDto> GetCustomersByRank(int start, int end);
    decimal UpdateScore(ulong customerId, decimal scoreChange);
}