using System.ComponentModel.DataAnnotations;
using Leaderboard.Service.Customers;
using Leaderboard.Service.Customers.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.WebApi.Controllers;

[ApiController]
[Route("leaderboard")]
public class LeaderboardController(ICustomerLeaderboardService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), 200)]
    public IActionResult GetCustomersByRank([Required, Range(1, int.MaxValue)] int start, [Required, Range(1, int.MaxValue)] int end)
    {
        return Ok(service.GetCustomersByRank(start, end));
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), 200)]
    public IActionResult GetCustomersById(ulong customerId, [Range(0, int.MaxValue)] int high = 0, [Range(0, int.MaxValue)] int low = 0)
    {
        return Ok(service.GetCustomersById(customerId, high, low));
    }
}