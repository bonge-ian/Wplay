using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wplay.Data;

namespace Wplay.Controllers;

public class EndpointsController(AppDbContext dbContext) : Controller
{
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create()
	{
		var endpoint = new Models.Endpoint
		{
			Uuid = Guid.NewGuid(),
			Name = "Temporary Bucket",
			DefaultStatusCode = 200,
			DefaultResponseBody = "{\"status\":\"received\"}"
		};

		dbContext.Endpoints.Add(endpoint);
		await dbContext.SaveChangesAsync();

		return RedirectToAction(nameof(Show), new { uuid = endpoint.Uuid });
	}

	// GET: /endpoints/{uuid}
	[HttpGet("endpoints/{uuid:guid}")]
	public async Task<IActionResult> Show(Guid uuid)
	{
		var endpoint = await dbContext.Endpoints
			.FirstOrDefaultAsync(e => e.Uuid == uuid);

		if (endpoint is null)
		{
			return NotFound();
		}

		var webhooks = await dbContext.WebhookRequests
			.Where(w => w.EndpointId == endpoint.Id)
			.OrderByDescending(w => w.CreatedAt)
			.Take(50)
			.ToListAsync();

		ViewBag.InitialWebhooks = webhooks;

		return View(endpoint);
	}

	// GET: /endpoints/{id}/feed (HTMX Polling Partial)
	[HttpGet("endpoints/{id:int}/feed")]
	public async Task<IActionResult> Feed(int id)
	{
		var webhooks = await dbContext.WebhookRequests
			.Where(w => w.EndpointId == id)
			.OrderByDescending(w => w.CreatedAt)
			.Take(50)
			.ToListAsync();

		return PartialView("_RequestListPartial", webhooks);
	}
}
