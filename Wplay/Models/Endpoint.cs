using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;
using Wplay.Enums;

namespace Wplay.Models;

public class Endpoint : BaseModel
{
	public int? UserId { get; set; } = null;

	public Guid Uuid { get; set; } = Guid.NewGuid();

	public EndpointType Type { get; set; } = EndpointType.Webhook;

	[MaxLength(length: 200)]
	public string Name { get; set; } = string.Empty;

	public int? DefaultStatusCode { get; set; } = 200;

	public string? DefaultResponseHeaders { get; set; } = null;

	public string? DefaultResponseBody { get; set; }

	public int ResponseDelay { get; set; } = 0;

	public bool IsProtected { get; set; } = false;

	public string? AuthCredentials { get; set; } = null;

	public DateTime? ExpiresAt { get; set; } = null;

	[NotMapped]
	public JsonNode? ResponseHeadersDocument
	{
		get => string.IsNullOrWhiteSpace(DefaultResponseHeaders) ? null : JsonNode.Parse(DefaultResponseHeaders);
		set => DefaultResponseHeaders = value?.ToJsonString();
	}

	[NotMapped]
	public JsonNode? AuthCredentialsDocument
	{
		get => string.IsNullOrWhiteSpace(AuthCredentials) ? null : JsonNode.Parse(AuthCredentials);
		set => AuthCredentials = value?.ToJsonString();
	}
}
