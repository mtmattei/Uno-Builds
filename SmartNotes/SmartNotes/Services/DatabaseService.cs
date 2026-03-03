using LiteDB;
using LiteDB.Vector;
using SmartNotes.Models;

namespace SmartNotes.Services;
public class DatabaseService
{
    private readonly LiteDatabase _database;

    public DatabaseService(string databasePath)
    {
        _database = new LiteDatabase(databasePath);
    }

    public int InsertNote(Note note)
    {
        var notesCollection = _database.GetCollection<Note>("notes"); //here?
        return notesCollection.Insert(note);
    }

    public List<Note> GetAllNotes()
    {
        var notesCollection = _database.GetCollection<Note>("notes");
        return notesCollection.FindAll().ToList();
    }

    public Note? GetNotebyID(int id)
    {
        var notesCollection = _database.GetCollection<Note>("notes");
        return notesCollection.FindById(id);
    }

    public bool UpdateNote(Note note)
    {
        var notesCollection = _database.GetCollection<Note>("notes");
        return notesCollection.Update(note);
    }

    public bool DeleteNote(int id)
    {
        var notesCollection = _database.GetCollection<Note>("notes");
        return notesCollection.Delete(id);
    }

    public void EnsureVectorIndex(int dimensions)
    {
        var notesCollection = _database.GetCollection<Note>("notes");
        notesCollection.EnsureIndex(x => x.Embedding, new VectorIndexOptions(dimensions));
    }

    public List<Note> SearchSimilarNotes(float[] queryEmbedding, int limit = 10)
    {
        var notesCollection = _database.GetCollection<Note>("notes");

        return notesCollection.Query()
            .OrderBySimilarity(x => x.Embedding, queryEmbedding)
            .Limit(limit)
            .ToList();
    }
}


