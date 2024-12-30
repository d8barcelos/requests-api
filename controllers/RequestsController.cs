using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestsApi.Models;
using RequestsApi.services;
using StackExchange.Redis;

namespace RequestsApi.controllers
{
    [ApiController]
    [Authorize]
    [Route("api/requests")]
    public class RequestsController : ControllerBase
    {
        private readonly RequestServices _requestService;
        private readonly IDatabase _redisDatabase;

        public RequestsController(RequestServices requestService, IConnectionMultiplexer redisConnection)
        {
            _requestService = requestService;
            _redisDatabase = redisConnection.GetDatabase();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _requestService.GetAllAsync();
            return Ok(requests);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var request = await _requestService.GetByIdAsync(id);
            if (request == null) return NotFound();
            return Ok(request);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Request request)
        {
            await _requestService.CreateAsync(request);
            await _redisDatabase.KeyDeleteAsync("items");
            return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
        }
        
        [HttpPut("{requestId}")]
        public async Task<IActionResult> Update(Request request, string requestId)
        {
            try
            {
                await _requestService.UpdateAsync(request, requestId);
                await _redisDatabase.KeyDeleteAsync("items");
                
                return Ok(new { message = "Request updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _requestService.DeleteAsync(id);
                return Ok(new { message = "Request deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
        
        [HttpPatch("{id}/update-status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            try
            {
                await _requestService.UpdateStatusAsync(id, status);
                await _redisDatabase.KeyDeleteAsync("items");

                return Ok(new { message = "Status updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
    }
}
