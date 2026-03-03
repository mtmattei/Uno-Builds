# Zara Shopping App - Uno Platform Implementation

A pixel-perfect clone of the Zara shopping app built with Uno Platform, targeting iOS, Android, Web (WASM), Windows, macOS, and Linux.

## Project Structure

```
Zara/
├── Zara/                                    # Main application project
│   ├── Models/
│   │   ├── Product.cs                       # Product data model
│   │   ├── Category.cs                      # Category model with selection state
│   │   └── ProductFit.cs                    # Fit category model
│   ├── ViewModels/
│   │   └── ProductCatalogViewModel.cs       # Main catalog ViewModel with MVVM
│   ├── Services/
│   │   └── MockProductService.cs            # Mock data service (9 products, 6 categories, 4 fits)
│   ├── Views/
│   │   ├── ProductCatalogPage.xaml          # Main product catalog page
│   │   └── Controls/
│   │       ├── HeaderControl.xaml           # Header with menu and action icons
│   │       ├── CategoryTabBar.xaml          # Horizontal scrolling category tabs
│   │       ├── FitTileControl.xaml          # Fit category tile (3:4 aspect ratio)
│   │       ├── ProductCard.xaml             # Product card (180px × 240px)
│   │       ├── ProductListSection.xaml      # Horizontal scrolling product list
│   │       └── FitsGridSection.xaml         # 2×2 grid of fit categories
│   ├── Styles/
│   │   ├── Colors.xaml                      # Zara color palette
│   │   ├── Typography.xaml                  # Font styles and text styles
│   │   └── ZaraStyles.xaml                  # Button and control styles
│   ├── Assets/
│   │   └── Images/
│   │       └── Placeholders/                # 13 placeholder images
│   │           ├── product-1.png through product-9.png
│   │           ├── fit-baggy.png, fit-flare.png, fit-slim.png, fit-straight.png
│   │           ├── generate-placeholders.ps1 # PowerShell script to regenerate
│   │           └── README.md                # Image specifications
│   ├── App.xaml                             # Application resources
│   ├── MainPage.xaml                        # Root page with Frame navigation
│   └── Zara.csproj                          # Project file
└── README.md                                # This file
```

## Features Implemented

### ✅ Core UI Components
- **HeaderControl**: Fixed header with hamburger menu, filters, search, and cart icons
- **CategoryTabBar**: Horizontal scrollable category tabs (VIEW ALL, RAW DENIM, FLARE, BAGGY, BALLOON, SLIM)
- **FitsGridSection**: 2×2 grid showing fit categories with overlay text
- **ProductCard**: Pixel-perfect product card with image, name, price, bookmark, and color variants
- **ProductListSection**: Horizontal scrolling list of products organized by fit type

### ✅ Design System
- **Colors**: Exact Zara color palette (#FFFFFF, #1A1A1A, #757575, #F5F5F5, #ECECEC)
- **Typography**: Sans-serif font family with proper sizing (11-16px) and character spacing
- **Layout**: Precise spacing (8-16px gaps) and padding matching the specification

### ✅ Data & Architecture
- **MVVM Pattern**: Using CommunityToolkit.Mvvm for observable objects and commands
- **Mock Data Service**: 9 products across 4 fit types (Straight, Flare, Baggy, Slim)
- **Categories**: 6 category tabs with selection state
- **Product Fits**: 4 fit categories with images and names

### ✅ Platform Support
- Windows Desktop (tested and built successfully)
- iOS (project configured)
- Android (project configured)
- Web/WASM (project configured, requires more memory for build)
- macOS (project configured)
- Linux (project configured)

## Build & Run

### Prerequisites
- .NET 10.0 SDK
- Uno Platform Workload installed
- Visual Studio 2022 or JetBrains Rider (recommended for development)

### Build Commands

```bash
# Restore packages
cd Zara
dotnet restore

# Build for specific platform
dotnet build -f net10.0-desktop      # Windows
dotnet build -f net10.0-android      # Android
dotnet build -f net10.0-ios          # iOS
dotnet build -f net10.0-browserwasm  # Web (requires significant memory)

# Run on Windows
dotnet run -f net10.0-desktop
```

### Visual Studio / Rider
1. Open `Zara.sln`
2. Select target platform (Windows, Android, iOS, etc.)
3. Press F5 to build and run

## Placeholder Images

The app includes 13 auto-generated placeholder images:

### Product Images (360×480px)
- Light gray background (#F5F5F5)
- Centered text labels
- Ready to be replaced with actual product photos

### Fit Category Images (400×533px)
- Gray background (#ECECEC)
- Centered fit type labels
- 3:4 aspect ratio

### Regenerating Placeholders
```powershell
cd Zara/Assets/Images/Placeholders
powershell -ExecutionPolicy Bypass -File generate-placeholders.ps1
```

## Design Specifications

### Color Palette
| Color Name | Hex Code | Usage |
|------------|----------|-------|
| Primary Background | #FFFFFF | Main background |
| Text Primary | #1A1A1A | Headings, prices |
| Text Secondary | #757575 | Product names, labels |
| Accent Gray | #F5F5F5 | Product card backgrounds |
| Accent Gray Dark | #ECECEC | Fit tile backgrounds |
| Icon Color | #1A1A1A | Navigation icons |

### Typography
| Style | Font Size | Weight | Usage |
|-------|-----------|--------|-------|
| Header | 16px | SemiBold | Section headers |
| Product Name | 11px | Normal | Product titles |
| Price | 12px | Normal | Product prices |
| Tab | 12px | Normal | Category tabs |
| Fit Overlay | 16px | Bold | Fit category text |

### Component Sizing
| Component | Width | Height | Aspect Ratio |
|-----------|-------|--------|--------------|
| HeaderControl | Full width | 60px | - |
| CategoryTabBar | Full width | 45px | - |
| ProductCard | 180px | 240px (image) | 3:4 |
| FitTileControl | Flexible | Flexible | 3:4 |

## Mock Data

### Products (9 total)
1. **Straight Fit** (4 products): Basic category
   - STRAIGHT FIT JEANS - £35.90
   - STRAIGHT FIT JEANS VINTAGE - £39.90
   - STRAIGHT FIT JEANS RAW DENIM - £35.90
   - STRAIGHT FIT JEANS BLACK - £35.90

2. **Flare Fit** (3 products): Boot Cut category
   - FLARE FIT JEANS - £39.90
   - FLARE FIT JEANS VINTAGE BLUE - £42.90
   - BOOT CUT JEANS LIGHT WASH - £39.90

3. **Baggy Fit** (1 product): Relaxed category
   - BAGGY FIT JEANS - £45.90

4. **Slim Fit** (1 product): Essential category
   - SLIM FIT JEANS - £35.90

### Categories (6 total)
- VIEW ALL (default selected)
- RAW DENIM
- FLARE
- BAGGY
- BALLOON
- SLIM

## Next Steps

### To Complete the App
1. **Replace Placeholder Images**: Add real product photos to `Assets/Images/Placeholders/`
2. **Add Product Detail Page**: Create ProductDetailPage.xaml with full product information
3. **Implement Navigation**: Add proper page navigation using Uno.Extensions.Navigation
4. **Add Filtering**: Implement category filtering logic in ViewModel
5. **Platform-Specific Adjustments**:
   - iOS: Safe area insets for notch/island
   - Android: Status bar styling
   - Web: Touch scrolling optimization

### Optional Enhancements
1. Add pull-to-refresh functionality
2. Implement infinite scrolling for product lists
3. Add product search functionality
4. Create shopping cart page
5. Add product filtering and sorting
6. Implement image lazy loading
7. Add animations and transitions

## Known Issues

1. **WASM Build**: Requires significant memory (out of memory error during build)
   - **Workaround**: Build for specific platforms only
   - **Solution**: Increase system memory or use build agents with more RAM

2. **Color Indicator Binding**: Color strings need proper converter
   - Currently using direct binding which may not work on all platforms
   - Add a StringToBrushConverter if needed

## Technical Notes

### Dependencies
- **Uno.SDK**: Latest version (defined in global.json)
- **CommunityToolkit.Mvvm**: 8.4.0 (for MVVM pattern)
- **Uno.UI.HotDesign**: Included for hot reload support

### Central Package Management
The project uses Central Package Management (CPM) with package versions defined in `Directory.Packages.props`.

### Single Project Architecture
Uno Platform's Single Project feature is enabled, allowing a unified project structure across all platforms.

## Resources

- [Uno Platform Documentation](https://platform.uno/docs/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Zara Design Specification](./DESIGN_SPEC.md) (from original UI description)

## License

This is a sample application for educational and demonstration purposes.

## Contributing

This is a sample implementation. Feel free to:
1. Replace placeholder images with actual product photos
2. Add new features and pages
3. Improve the UI/UX
4. Optimize performance
5. Add unit and UI tests

---

**Built with Uno Platform** - Build native mobile, desktop and WebAssembly apps with C# and XAML from a single codebase.
