using SmartNotes.Models;

namespace SmartNotes;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        TestDatabase();
    }

    private void TestDatabase()
    {
        // Create a test note
        var testNote = new Note
        {
            Title = "Test Note",
            Content = "This is a test note to verify the database works!",
            CreatedAt = DateTime.Now
        };

        // Insert it
        int newId = App.Database.InsertNote(testNote);

        // Read it back
        var retrievedNote = App.Database.GetNoteById(newId);

        // Verify
        if (retrievedNote != null)
        {
            System.Diagnostics.Debug.WriteLine($"✅ Database works! Note ID: {newId}, Title: {retrievedNote.Title}");
        }
    }
}
