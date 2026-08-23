using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace Wplay.Models;

public class EmailRequest : BaseModel
{
	public int EndpointId { get; set; }

	public Guid Uuid { get; set; } = Guid.NewGuid();

	public string Sender { get; set; } = string.Empty;

	public string Recipient { get; set; } = string.Empty;

	public string? Subject { get; set; }

	public string? TextBody { get; set; }

	public string? HtmlBody { get; set; }

	public string? RawHeaders { get; set; }

	public string? AttachmentsMetadata { get; set; }

	// EF Core Navigation Property
	public Endpoint? Endpoint { get; set; }

	[NotMapped]
	public JsonNode? RawHeadersNode
	{
		get => string.IsNullOrWhiteSpace(RawHeaders) ? null : JsonNode.Parse(RawHeaders);
		set => RawHeaders = value?.ToJsonString();
	}

	[NotMapped]
	public JsonNode? AttachmentsMetadataNode
	{
		get => string.IsNullOrWhiteSpace(AttachmentsMetadata) ? null : JsonNode.Parse(AttachmentsMetadata);
		set => AttachmentsMetadata = value?.ToJsonString();
	}
}
