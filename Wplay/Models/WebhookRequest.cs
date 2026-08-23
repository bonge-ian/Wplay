using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace Wplay.Models;

public class WebhookRequest : BaseModel
{
	public int EndpointId { get; set; }

	public Guid Uuid { get; set; } = Guid.NewGuid();

	public string Method { get; set; } = "POST";

	public string Url { get; set; } = string.Empty;

	public string? QueryParameters { get; set; } = null;

	public string? Headers { get; set; }

	public string? Body { get; set; }

	public string? IpAddress { get; set; } = null;

	public string? ContentType { get; set; }

	public int ResponseCode { get; set; } = 200;

	public Endpoint? Endpoint { get; set; }

	[NotMapped]
	public JsonNode? HeadersNode
	{
		get => string.IsNullOrWhiteSpace(Headers) ? null : JsonNode.Parse(Headers);
		set => Headers = value?.ToJsonString();
	}

	[NotMapped]
	public JsonNode? QueryParametersNode
	{
		get => string.IsNullOrWhiteSpace(QueryParameters) ? null : JsonNode.Parse(QueryParameters);
		set => QueryParameters = value?.ToJsonString();
	}
}
