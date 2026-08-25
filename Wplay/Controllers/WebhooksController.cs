using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wplay.Data;

namespace Wplay.Controllers;

[Route("webhooks")]
public class WebhooksController(AppDbContext db) : Controller
{
	// GET: /webhooks/{id}
	[HttpGet("{id:int}")]
	public async Task<IActionResult> Show(int id)
	{
		var request = await db.WebhookRequests
			.FirstOrDefaultAsync(w => w.Id == id);

		return request is null ? NotFound() : PartialView("_WebhookDetailsPartial", request);
	}

	// DELETE: /webhooks/{id}
	[HttpDelete("{id:int}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(int id)
	{
		var request = await db.WebhookRequests.FindAsync(id);
		if (request is not null)
		{
			db.WebhookRequests.Remove(request);
			await db.SaveChangesAsync();
		}

		return Content(""); // HTMX removes the target container
	}

	// DELETE: /endpoints/{endpointId}/webhooks
	[HttpDelete("/endpoints/{endpointId:int}/webhooks")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteAll(int endpointId)
	{
		await db.WebhookRequests
			.Where(w => w.EndpointId == endpointId)
			.ExecuteDeleteAsync();

		return PartialView("~/Views/Endpoints/_RequestListPartial.cshtml", new List<Wplay.Models.WebhookRequest>());
	}
}
