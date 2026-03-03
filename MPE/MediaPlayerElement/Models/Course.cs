namespace MediaPlayerElement.Models;

public record Course(
    string Title,
    string Category,
    string Instructor,
    string VideoSource,
    string Emoji,
    string BackgroundColor = "#FFF0F0F0"
);

public record TechBite(
    string Title,
    string Emoji,
    string BackgroundColor
);
