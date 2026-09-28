using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UbuhlebethuConnectPro.Web.Data;
using UbuhlebethuConnectPro.Web.Hubs;
using UbuhlebethuConnectPro.Web.Models;

namespace UbuhlebethuConnectPro.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class QuotationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SyncHub> _hubContext;

        public QuotationsController(ApplicationDbContext context, IHubContext<SyncHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/Quotations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuoteRequest>>> GetQuotations()
        {
            return await _context.QuoteRequests.ToListAsync();
        }

        // POST: api/Quotations
        [HttpPost]
        public async Task<ActionResult<QuoteRequest>> PostQuotation(QuoteRequest quoteRequest)
        {
            _context.QuoteRequests.Add(quoteRequest);
            await _context.SaveChangesAsync();

            // Broadcast real-time update to all connected clients
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "Quotation", $"New quotation {quoteRequest.Reference} was created.");

            return CreatedAtAction("GetQuotation", new { id = quoteRequest.Id }, quoteRequest);
        }

        // PUT: api/Quotations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuotation(int id, QuoteRequest quoteRequest)
        {
            if (id != quoteRequest.Id)
            {
                return BadRequest();
            }

            _context.Entry(quoteRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                
                // Broadcast real-time update
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "Quotation", $"Quotation {quoteRequest.Reference} was updated.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuoteRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool QuoteRequestExists(int id)
        {
            return _context.QuoteRequests.Any(e => e.Id == id);
        }
    }
}
