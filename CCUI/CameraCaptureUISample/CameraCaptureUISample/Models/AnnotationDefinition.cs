using System.Collections.Immutable;
using Windows.UI;

namespace CameraCaptureUISample.Models;

public record PlatformDetail(
	string ApiName,
	string Note
);

public partial record AnnotationDefinition(
	string Key,
	string Label,
	string IconGlyph,
	Color BadgeColor,
	string Summary,
	string CodeSnippet,
	ImmutableDictionary<string, PlatformDetail> Platforms
);
