namespace UnoVox.Models;

/// <summary>
/// Manages undo/redo operations with a 10-command limit
/// </summary>
public class UndoStack
{
    private readonly Stack<IVoxelCommand> _undoStack = new();
    private readonly Stack<IVoxelCommand> _redoStack = new();
    private const int MaxHistorySize = 10;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Executes a command and adds it to the undo stack
    /// </summary>
    public void ExecuteCommand(IVoxelCommand command, VoxelGrid grid)
    {
        command.Execute(grid);
        
        _undoStack.Push(command);
        
        // Limit stack size to 10 commands
        while (_undoStack.Count > MaxHistorySize)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = 0; i < MaxHistorySize; i++)
            {
                _undoStack.Push(items[i]);
            }
        }
        
        // Clear redo stack when new command is executed
        _redoStack.Clear();
    }

    /// <summary>
    /// Undoes the last command
    /// </summary>
    public void Undo(VoxelGrid grid)
    {
        if (!CanUndo) return;

        var command = _undoStack.Pop();
        command.Undo(grid);
        _redoStack.Push(command);
    }

    /// <summary>
    /// Redoes the last undone command
    /// </summary>
    public void Redo(VoxelGrid grid)
    {
        if (!CanRedo) return;

        var command = _redoStack.Pop();
        command.Execute(grid);
        _undoStack.Push(command);
    }

    /// <summary>
    /// Clears all undo/redo history
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
