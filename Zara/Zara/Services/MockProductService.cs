using Zara.Models;

namespace Zara.Services;

public class MockProductService
{
    private static readonly List<Product> _allProducts = new()
    {
        // Straight Fit Products - Basic Category
        new Product
        {
            Id = "1",
            Name = "STRAIGHT FIT JEANS",
            Price = 35.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants1.jpg",
            ColorIndicator = "#1A1A1A",
            VariantCount = 7,
            Fit = "Straight",
            Category = "Basic"
        },
        new Product
        {
            Id = "2",
            Name = "STRAIGHT FIT JEANS VINTAGE",
            Price = 39.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants2.jpg",
            ColorIndicator = "#4A6FA5",
            VariantCount = 5,
            Fit = "Straight",
            Category = "Basic"
        },
        new Product
        {
            Id = "3",
            Name = "STRAIGHT FIT JEANS RAW DENIM",
            Price = 42.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants3.jpg",
            ColorIndicator = "#2B4B7C",
            VariantCount = 4,
            Fit = "Straight",
            Category = "Basic"
        },
        new Product
        {
            Id = "4",
            Name = "STRAIGHT FIT JEANS BLACK",
            Price = 37.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants4.jpg",
            ColorIndicator = "#000000",
            VariantCount = 6,
            Fit = "Straight",
            Category = "Basic"
        },
        new Product
        {
            Id = "5",
            Name = "STRAIGHT FIT JEANS LIGHT BLUE",
            Price = 44.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants5.jpg",
            ColorIndicator = "#7A9BBF",
            VariantCount = 5,
            Fit = "Straight",
            Category = "Basic"
        },
        new Product
        {
            Id = "6",
            Name = "STRAIGHT FIT JEANS GREY",
            Price = 38.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants6.jpg",
            ColorIndicator = "#6B6B6B",
            VariantCount = 4,
            Fit = "Straight",
            Category = "Basic"
        },

        // Flare Fit Products - Boot Cut Category
        new Product
        {
            Id = "7",
            Name = "FLARE FIT JEANS",
            Price = 41.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/flare1.jpg",
            ColorIndicator = "#1A1A1A",
            VariantCount = 5,
            Fit = "Flare",
            Category = "Boot Cut"
        },
        new Product
        {
            Id = "8",
            Name = "FLARE FIT JEANS VINTAGE BLUE",
            Price = 45.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/flare2.jpg",
            ColorIndicator = "#5B7FA8",
            VariantCount = 4,
            Fit = "Flare",
            Category = "Boot Cut"
        },
        new Product
        {
            Id = "9",
            Name = "BOOT CUT JEANS LIGHT WASH",
            Price = 43.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/flare 3.jpg",
            ColorIndicator = "#7A9BBF",
            VariantCount = 6,
            Fit = "Flare",
            Category = "Boot Cut"
        },

        // Baggy Fit Products - Relaxed Category
        new Product
        {
            Id = "10",
            Name = "BAGGY FIT JEANS",
            Price = 47.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants2.jpg",
            ColorIndicator = "#2B2B2B",
            VariantCount = 5,
            Fit = "Baggy",
            Category = "Relaxed"
        },
        new Product
        {
            Id = "11",
            Name = "BAGGY FIT JEANS DARK WASH",
            Price = 49.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants5.jpg",
            ColorIndicator = "#1A1A1A",
            VariantCount = 4,
            Fit = "Baggy",
            Category = "Relaxed"
        },

        // Slim Fit Products - Essential Category
        new Product
        {
            Id = "12",
            Name = "SLIM FIT JEANS",
            Price = 36.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants3.jpg",
            ColorIndicator = "#1A1A1A",
            VariantCount = 8,
            Fit = "Slim",
            Category = "Essential"
        },
        new Product
        {
            Id = "13",
            Name = "SLIM FIT JEANS CHARCOAL",
            Price = 40.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants6.jpg",
            ColorIndicator = "#4A4A4A",
            VariantCount = 6,
            Fit = "Slim",
            Category = "Essential"
        },

        // Cargo Pants - Utility Category
        new Product
        {
            Id = "14",
            Name = "CARGO PANTS REGULAR FIT",
            Price = 52.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/cargo1.jpg",
            ColorIndicator = "#3D5A45",
            VariantCount = 5,
            Fit = "Cargo",
            Category = "Utility"
        },
        new Product
        {
            Id = "15",
            Name = "CARGO PANTS TAPERED",
            Price = 54.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/cargo2.jpg",
            ColorIndicator = "#4A4A3A",
            VariantCount = 4,
            Fit = "Cargo",
            Category = "Utility"
        },
        new Product
        {
            Id = "16",
            Name = "CARGO PANTS TACTICAL",
            Price = 58.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/cargo3.jpg",
            ColorIndicator = "#2D2D2D",
            VariantCount = 6,
            Fit = "Cargo",
            Category = "Utility"
        },

        // Balloon Fit Products - Contemporary Category
        new Product
        {
            Id = "17",
            Name = "BALLOON FIT JEANS",
            Price = 48.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants1.jpg",
            ColorIndicator = "#5B7FA8",
            VariantCount = 4,
            Fit = "Balloon",
            Category = "Contemporary"
        },
        new Product
        {
            Id = "18",
            Name = "BALLOON FIT JEANS WIDE",
            Price = 51.90m,
            Currency = "GBP",
            ImageUrl = "ms-appx:///Assets/Images/pants4.jpg",
            ColorIndicator = "#4A6FA5",
            VariantCount = 5,
            Fit = "Balloon",
            Category = "Contemporary"
        }
    };

    private static readonly List<Category> _categories = new()
    {
        new Category { Id = "all", Name = "VIEW ALL", IsSelected = true },
        new Category { Id = "straight", Name = "STRAIGHT", IsSelected = false },
        new Category { Id = "flare", Name = "FLARE", IsSelected = false },
        new Category { Id = "baggy", Name = "BAGGY", IsSelected = false },
        new Category { Id = "balloon", Name = "BALLOON", IsSelected = false },
        new Category { Id = "slim", Name = "SLIM", IsSelected = false },
        new Category { Id = "cargo", Name = "CARGO", IsSelected = false }
    };

    private static readonly List<ProductFit> _fits = new()
    {
        new ProductFit
        {
            Id = "baggy",
            Name = "BAGGY FIT",
            ImageUrl = "ms-appx:///Assets/Images/hero1.jpg"
        },
        new ProductFit
        {
            Id = "flare",
            Name = "FLARE FIT",
            ImageUrl = "ms-appx:///Assets/Images/hero2.jpg"
        },
        new ProductFit
        {
            Id = "slim",
            Name = "SLIM FIT",
            ImageUrl = "ms-appx:///Assets/Images/hero3.jpg"
        },
        new ProductFit
        {
            Id = "straight",
            Name = "STRAIGHT FIT",
            ImageUrl = "ms-appx:///Assets/Images/hero4.jpg"
        }
    };

    public async Task<IEnumerable<Product>> GetProductsAsync(string? fit = null)
    {
        await Task.Delay(300); // Simulate network delay

        var products = string.IsNullOrEmpty(fit) || fit == "all"
            ? _allProducts
            : _allProducts.Where(p => p.Fit.Equals(fit, StringComparison.OrdinalIgnoreCase));

        return products.AsEnumerable();
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        await Task.Delay(300);

        if (category.Equals("all", StringComparison.OrdinalIgnoreCase))
            return _allProducts.AsEnumerable();

        var products = _allProducts
            .Where(p => p.Fit.Equals(category, StringComparison.OrdinalIgnoreCase));

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        await Task.Delay(200);
        return _allProducts.FirstOrDefault(p => p.Id == id);
    }

    public Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return Task.FromResult(_categories.AsEnumerable());
    }

    public Task<IEnumerable<ProductFit>> GetFitsAsync()
    {
        return Task.FromResult(_fits.AsEnumerable());
    }

    public IEnumerable<Product> GetStraightBasicProducts()
    {
        return _allProducts
            .Where(p => p.Fit == "Straight" && p.Category == "Basic")
            .Take(6);
    }

    public IEnumerable<Product> GetFlareBootCutProducts()
    {
        return _allProducts
            .Where(p => p.Fit == "Flare" && p.Category == "Boot Cut")
            .Take(4);
    }

    public IEnumerable<Product> GetCargoUtilityProducts()
    {
        return _allProducts
            .Where(p => p.Fit == "Cargo" && p.Category == "Utility")
            .Take(4);
    }

    public IEnumerable<Product> GetBalloonContemporaryProducts()
    {
        return _allProducts
            .Where(p => p.Fit == "Balloon" && p.Category == "Contemporary")
            .Take(4);
    }
}
