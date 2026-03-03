using System.Collections.ObjectModel;
using ZaraApp.Models;

namespace ZaraApp.ViewModels;

public class MainViewModel
{
    public ObservableCollection<Product> StraightProducts { get; set; } = new();
    public ObservableCollection<Product> FlareProducts { get; set; } = new();

    public MainViewModel()
    {
        LoadStaticData();
    }

    private void LoadStaticData()
    {
        StraightProducts = new ObservableCollection<Product>
        {
            new Product
            {
                Name = "STRAIGHT FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/pants1.jpg",
                Price = "35.90 GBP",
                ColorIndicator = "□+7",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsComingSoon = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsComingSoon = true }
                }
            },
            new Product
            {
                Name = "STRAIGHT FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/pants2.jpg",
                Price = "35.99 GBP",
                ColorIndicator = "□+5",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "STRAIGHT FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/pants3.jpg",
                Price = "35.90 GBP",
                ColorIndicator = "□+4",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "STRAIGHT FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/pants4.jpg",
                Price = "35.90 GBP",
                ColorIndicator = "□+6",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "STRAIGHT FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/pants5.jpg",
                Price = "35.90 GBP",
                ColorIndicator = "□+3",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "STRAIGHT FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/pants6.jpg",
                Price = "35.99 GBP",
                ColorIndicator = "□+4",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "CARGO FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/cargo1.jpg",
                Price = "42.90 GBP",
                ColorIndicator = "□+5",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "CARGO FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/cargo2.jpg",
                Price = "42.90 GBP",
                ColorIndicator = "□+6",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "CARGO FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/cargo3.jpg",
                Price = "42.90 GBP",
                ColorIndicator = "□+3",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            }
        };

        FlareProducts = new ObservableCollection<Product>
        {
            new Product
            {
                Name = "FLARE FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/flare1.jpg",
                Price = "35.90 GBP",
                ColorIndicator = "□+3",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "BOOT CUT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/flare2.jpg",
                Price = "35.99 GBP",
                ColorIndicator = "□+5",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            },
            new Product
            {
                Name = "FLARE FIT JEANS",
                ImageUrl = "ms-appx:///Assets/Images/flare 3.jpg",
                Price = "35.90 GBP",
                ColorIndicator = "□+4",
                AvailableSizes = new List<SizeOption>
                {
                    new SizeOption { Size = "30", IsAvailable = true },
                    new SizeOption { Size = "31", IsAvailable = true },
                    new SizeOption { Size = "32", IsAvailable = true },
                    new SizeOption { Size = "34", IsAvailable = true },
                    new SizeOption { Size = "36", IsAvailable = true }
                }
            }
        };
    }
}
