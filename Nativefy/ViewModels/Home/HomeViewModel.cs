using CommunityToolkit.Mvvm.ComponentModel;
using Nativefy.Models.Home;
using Nativefy.ViewModels.Home;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class HomeViewModel : BaseViewModel
{
    public ObservableCollection<HomeSection> Sections { get; } = [];

    [ObservableProperty]
    public partial HeroAdapter HeroAdapter { get; set; } = new();

    public async Task LoadHomeFeedAsync()
    {
        Sections.Add(new()
        {
            ViewType = Enums.ViewType.Standard,
            Title = "Recents",
            SectionUri = "Recents",
            Items =
            [
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
            ],
        });
        Sections.Add(new()
        {
            ViewType = Enums.ViewType.HeroPrompt,
            Title = "Recents",
            SectionUri = "Recents",
            Items =
            [
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
                new()
                {
                    Title = "Song",
                    Artist = "Me",
                    ImageUrl = "https://picsum.photos/800/600"
                },
            ],
        });

        HeroAdapter.HeroSlides.Add(new()
        {
            Accent = new(255, 255, 255, 255),
            Eyebrow = "Song",
            ImageUri = new("https://picsum.photos/800/600"),
            PrimaryCtaText = "Play",
            SecondaryCtaText = "Open",
            Tag = "Song",
            Title = "SONG",
            UseImageAsBackground = true,
            Tagline = "Hello",
            TaglineIconColor = new(255, 0, 0, 0),
        });

        HeroAdapter.HeroSlides.Add(new()
        {
            Accent = new(255, 255, 255, 255),
            Eyebrow = "Song 1",
            ImageUri = new("https://picsum.photos/800/600"),
            PrimaryCtaText = "Play",
            SecondaryCtaText = "Open",
            Tag = "Song",
            Title = "SONG 1",
            UseImageAsBackground = true,
            Tagline = "Hello 1",
            TaglineIconColor = new(255, 0, 0, 0),
        });

        HeroAdapter.Card0 = new()
        {
            Eyebrow = "recommended",
            Label = "Song 1",
            Accent = new Color(0, 255, 255, 255).ToHex(),
            ImageUri = new("https://picsum.photos/800/600"),
            NavigationUri = "Song 1",
        };
        HeroAdapter.Card1 = new()
        {
            Eyebrow = "Played recently",
            Label = "Song",
            Accent = new Color(0, 255, 255, 255).ToHex(),
            ImageUri = new("https://picsum.photos/800/600"),
            NavigationUri = "Song",
        };
        HeroAdapter.Card2 = new()
        {
            Eyebrow = "Played a lot",
            Label = "Song",
            Accent = new Color(0, 255, 255, 255).ToHex(),
            ImageUri = new("https://picsum.photos/800/600"),
            NavigationUri = "Song",
        };
    }
}
