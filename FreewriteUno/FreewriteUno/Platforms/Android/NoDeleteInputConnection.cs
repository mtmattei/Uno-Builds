#if __ANDROID__
using System.Runtime.Versioning;
using Android.Views;
using Android.Views.InputMethods;

namespace FreewriteUno.Platforms.Android;

// HACK: Modern Android IMEs (Gboard, Samsung Keyboard, SwiftKey) don't always route
// backspace through KEYCODE_DEL. They use InputConnection.deleteSurroundingText and
// zero-length commitText calls. This wrapper intercepts those when the lock is engaged.
// See Architecture Brief §6.5.
internal sealed class NoDeleteInputConnection : InputConnectionWrapper
{
    private readonly Func<bool> _isLocked;

    public NoDeleteInputConnection(IInputConnection target, bool mutable, Func<bool> isLocked)
        : base(target, mutable)
    {
        _isLocked = isLocked;
    }

    public override bool DeleteSurroundingText(int beforeLength, int afterLength)
    {
        if (_isLocked()) return true; // pretend success — text is unchanged
        return base.DeleteSurroundingText(beforeLength, afterLength);
    }

    // Override only invoked by the system on API 24+. Body and `base` call are both
    // API-24-only; annotate so CA1416 reflects reality instead of warning at minSdk 21.
    [SupportedOSPlatform("android24.0")]
    public override bool DeleteSurroundingTextInCodePoints(int beforeLength, int afterLength)
    {
        if (_isLocked()) return true;
        return base.DeleteSurroundingTextInCodePoints(beforeLength, afterLength);
    }

    public override bool CommitText(Java.Lang.ICharSequence? text, int newCursorPosition)
    {
        // Zero-length commits are how some IMEs apply deletes via composing-text replacement.
        if (_isLocked() && (text is null || text.Length() == 0)) return true;
        return base.CommitText(text, newCursorPosition);
    }

    public override bool SetComposingText(Java.Lang.ICharSequence? text, int newCursorPosition)
    {
        if (_isLocked() && (text is null || text.Length() == 0)) return true;
        return base.SetComposingText(text, newCursorPosition);
    }

    public override bool SendKeyEvent(KeyEvent? e)
    {
        if (_isLocked() && e is not null &&
            (e.KeyCode == Keycode.Del || e.KeyCode == Keycode.ForwardDel))
        {
            return true;
        }
        return base.SendKeyEvent(e);
    }
}
#endif
