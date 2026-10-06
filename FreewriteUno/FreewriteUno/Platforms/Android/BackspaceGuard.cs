#if __ANDROID__
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using Microsoft.UI.Xaml.Controls;

namespace FreewriteUno.Platforms.Android;

// HACK: Entry point for hooking the NoDeleteInputConnection wrapper onto the native
// AppCompatEditText that backs Uno's TextBox. Two integration approaches per
// Architecture Brief §6.5:
//
//   (a) Subclass AppCompatEditText and register into the Uno TextBox template — clean,
//       requires modifying the Uno template at runtime.
//   (b) Walk the native view tree from the TextBox's Loaded handler and intercept
//       OnCreateInputConnection via reflection on the existing EditText — fragile.
//
// This file scaffolds approach (b) as the lower-risk starting point. The real Install
// implementation needs to:
//   1. Find the AppCompatEditText backing `textBox` in the visual tree.
//   2. Either subclass it dynamically (Java side, not feasible from C#) or swap it for
//      a FreewriteEditText that overrides OnCreateInputConnection to wrap with
//      NoDeleteInputConnection.
//
// Until verified on Gboard / Samsung Keyboard / SwiftKey, the chrome toggle stays
// disabled (see MainPage.ConfigureBackspaceToggleForPlatform).
public static class BackspaceGuard
{
    public static void Install(TextBox textBox, Func<bool> isLocked)
    {
        // TODO(v1.1): real implementation.
        // 1. Hook textBox.Loaded to wait for the native AppCompatEditText to exist.
        // 2. Replace the EditText in the parent view group with a FreewriteEditText
        //    subclass whose OnCreateInputConnection returns
        //    new NoDeleteInputConnection(base.OnCreateInputConnection(outAttrs), true, isLocked).
        // 3. Re-enable BackspaceButton in MainPage.ConfigureBackspaceToggleForPlatform.
        _ = textBox;
        _ = isLocked;
    }
}
#endif
