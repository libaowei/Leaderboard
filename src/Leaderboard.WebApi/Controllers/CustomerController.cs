using System.ComponentModel.DataAnnotations;
using Leaderboard.Service.Customers;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.WebApi.Controllers;

[ApiController]
[Route("customer")]
public class CustomerController(ICustomerLeaderboardService service) : ControllerBase
{
    [HttpPost("{customerId}/score/{score}")]
    [ProducesResponseType(typeof(decimal), 200)]
    public IActionResult UpdateScore(ulong customerId, [Range(-1000, 1000)] decimal score)
    {
        return Ok(service.UpdateScore(customerId, score));
    }
}