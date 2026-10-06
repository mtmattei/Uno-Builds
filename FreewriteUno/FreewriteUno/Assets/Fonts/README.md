# Bundled fonts (placeholder)

Per Design Brief §3.2, v1 ships with three bundled fonts. Drop the `.ttf` files in this directory and they will be picked up by the `FontFamily` resources in `App.xaml`:

| File | URI in `App.xaml` |
|------|-------------------|
| `Lato-Regular.ttf` | `ms-appx:///Assets/Fonts/Lato-Regular.ttf#Lato` |
| `Newsreader-Regular.ttf` | `ms-appx:///Assets/Fonts/Newsreader-Regular.ttf#Newsreader` |
| `JetBrainsMono-Regular.ttf` | `ms-appx:///Assets/Fonts/JetBrainsMono-Regular.ttf#JetBrains Mono` |

Until the files are present, the resources in `App.xaml` reference the system family names directly (`Lato`, `Newsreader`, `JetBrains Mono`). On systems where the family is not installed, the OS substitutes a default sans-serif (or monospace) — layout works, but the typographic personality from §3.2 will not show until the assets are bundled.

License notes when adding the files:
- **Lato**: SIL Open Font License.
- **Newsreader**: SIL Open Font License.
- **JetBrains Mono**: SIL Open Font License.
