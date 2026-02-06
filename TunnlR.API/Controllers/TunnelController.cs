using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TunnlR.Application.Interfaces.IService;

namespace TunnlR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TunnelController : ControllerBase
    {
        private readonly ITunnelService _tunnelService;

        public TunnelController(ITunnelService tunnelService)
        {
            _tunnelService = tunnelService;
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateTunnel([FromBody] TunnelCreateRequest request)
        //{
        //    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //    var response = await _tunnelService.CreateTunnelAsync(userId, request);
        //    return Ok(response);
        //}

        [HttpDelete("tunnel/deativate/{tunnelId}")]
        public async Task<IActionResult> DeactivateTunnel(Guid tunnelId)
        {
            await _tunnelService.DeactivateTunnelAsync(tunnelId);
            return NoContent();
        }

        [HttpGet("{tunnelId}/status")]
        public async Task<IActionResult> GetStatus(Guid tunnelId)
        {
            var response = await _tunnelService.GetTunnelStatusAsync(tunnelId);
            return Ok(response);
        }

        [HttpDelete("{tunnelId}")]
        public async Task<IActionResult> Deactivate(Guid tunnelId)
        {
            await _tunnelService.DeactivateTunnelAsync(tunnelId);
            return NoContent();
        }
    }
}