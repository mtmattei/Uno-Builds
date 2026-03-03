namespace ListHold.Models;

public record ListItemModel(
    string Id,
    string Title,
    string Preview,
    string Details,
    Dictionary<string, string> Meta,
    List<string> Actions
);
