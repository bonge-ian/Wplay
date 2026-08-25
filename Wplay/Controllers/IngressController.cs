using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Wplay.Data;
using Wplay.Models;

namespace Wplay.Controllers;

[ApiController]
public class IngressController(AppDbContext db) : ControllerBase
{
	private const string ForcePrefix = "force-";

	[AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS", "HEAD")]
	[Route("hp/{uuid:guid}/{codeOrMode?}")]
	public async Task<IActionResult> Handle(Guid uuid, string? codeOrMode)
	{
		// 1. Fetch and validate the target endpoint bucket
		Models.Endpoint? endpoint = await db.Endpoints.FirstOrDefaultAsync(e => e.Uuid == uuid);

		if (endpoint is null)
		{
			return NotFound(new { error = "Endpoint bucket not found." });
		}

		if (endpoint.ExpiresAt.HasValue && endpoint.ExpiresAt.Value < DateTime.UtcNow)
		{
			return StatusCode(statusCode: 410, value: new { error = "Endpoint has expired." });
		}

		// 2. Read and format raw HTTP request components
		(string? bodyText, string? headersJson, string? queryJson) = await ParseRequestPayloadAsync(Request);

		// 3. Inject simulated response latency if configured
		await ApplyConfiguredDelayAsync(Request, endpoint.ResponseDelay);

		// 4. Calculate response status code and body (handling URL overrides and failure lotteries)
		(int statusCode, string? responseBody) = EvaluateResponse(Request, endpoint, codeOrMode);

		// 5. Persist the captured webhook request to the database
		await SaveWebhookLogAsync(endpoint.Id, Request, bodyText, headersJson, queryJson, statusCode);

		// 6. Return configured HTTP response back to the caller
		return StatusCode(statusCode, responseBody);
	}

	/// <summary>
	/// Reads the raw body string and serializes headers and query parameters to JSON strings.
	/// </summary>
	private static async Task<(string Body, string HeadersJson, string? QueryJson)> ParseRequestPayloadAsync(
		HttpRequest request
	)
	{
		using StreamReader reader = new(request.Body);
		string bodyText = await reader.ReadToEndAsync();

		Dictionary<string, string> headersDict = request.Headers
			.ToDictionary(h => h.Key, h => h.Value.ToString());
		string headersJson = JsonSerializer.Serialize(headersDict);

		Dictionary<string, string> queryDict = request.Query
			.ToDictionary(q => q.Key, q => q.Value.ToString());
		string? queryJson = queryDict.Count > 0 ? JsonSerializer.Serialize(queryDict) : null;

		return (bodyText, headersJson, queryJson);
	}

	/// <summary>
	/// Applies simulated latency based on query parameters (?delay=2000) 
	/// or bucket configuration (capped at 10 seconds max).
	/// </summary>
	private static async Task ApplyConfiguredDelayAsync(HttpRequest request, int defaultDelayMs)
	{
		int delayMs = defaultDelayMs;

		if (
			request.Query.TryGetValue("delay", out StringValues delayVal) &&
			int.TryParse(delayVal, out int parsedDelay))
		{
			delayMs = parsedDelay;
		}

		if (delayMs > 0)
		{
			await Task.Delay(Math.Min(delayMs, 10000));
		}
	}

	/// <summary>
	/// Computes the HTTP status code and response payload by checking route overrides (/force-500, /404),
	/// percentage-based failure lotteries (?lottery=30), or falling back to the endpoint defaults.
	/// </summary>
	private static (int StatusCode, string ResponseBody) EvaluateResponse(
		HttpRequest request,
		Models.Endpoint endpoint,
		string? codeOrMode
	)
	{
		int statusCode = endpoint.DefaultStatusCode ?? 200;
		string responseBody = endpoint.DefaultResponseBody ?? "{\"status\":\"received\"}";

		// Handle URL forced status codes: e.g., /force-500 or /404
		//if (!string.IsNullOrEmpty(codeOrMode))
		//{
		//	if (codeOrMode.StartsWith("force-", StringComparison.OrdinalIgnoreCase) &&
		//		int.TryParse(codeOrMode.Replace(oldValue: "force-", newValue: "", comparisonType: StringComparison.OrdinalIgnoreCase), out int forcedCode))
		//	{
		//		statusCode = forcedCode;
		//	}
		//	else if (int.TryParse(codeOrMode, out int directCode))
		//	{
		//		statusCode = directCode;
		//	}
		//}
		if (codeOrMode is { Length: > 0 })
		{
			string rawCode = codeOrMode.StartsWith(ForcePrefix, StringComparison.OrdinalIgnoreCase)
				? codeOrMode[ForcePrefix.Length..] // get substring after ForcePrefix..
				: codeOrMode;

			if (int.TryParse(rawCode, out int parsedCode))
			{
				statusCode = parsedCode;
			}
		}

		// Handle failure lotteries: e.g., ?lottery=30 (30% chance of returning a 500 error)
		if (
			request.Query.TryGetValue("lottery", out StringValues lotteryVal) &&
			int.TryParse(lotteryVal, out int failureChance))
		{
			int roll = Random.Shared.Next(1, 101);
			if (roll <= failureChance)
			{
				statusCode = 500;
				responseBody = JsonSerializer.Serialize(new
				{
					error = "Lottery failure triggered",
					chance = $"{failureChance}%",
					roll
				});
			}
		}

		return (statusCode, responseBody);
	}

	/// <summary>
	/// Creates and persists a WebhookRequest entity log into the database.
	/// </summary>
	private async Task SaveWebhookLogAsync(
		int endpointId,
		HttpRequest request,
		string body,
		string headersJson,
		string? queryJson,
		int statusCode)
	{
		WebhookRequest webhookRequest = new()
		{
			EndpointId = endpointId,
			Uuid = Guid.NewGuid(),
			Method = request.Method,
			Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
			QueryParameters = queryJson,
			Headers = headersJson,
			Body = body,
			IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
			ContentType = request.ContentType,
			ResponseCode = statusCode
		};

		db.WebhookRequests.Add(webhookRequest);
		await db.SaveChangesAsync();
	}
}
