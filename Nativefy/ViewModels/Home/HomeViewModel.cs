using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Nativefy.Engine;
using Nativefy.Messages;
using Nativefy.Services.Feed;
using Nativefy.ViewModels.Home;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nativefy.ViewModels;

public sealed partial class HomeViewModel(IFeedService feedService) : BaseViewModel
{
    private List<FeedResult> _feed = [];

    public ObservableCollection<FeedResult> Feed { get; } = [];
    
    [ObservableProperty]
    public partial HeroAdapter HeroAdapter { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFeedNotEmpty))]
    public partial bool IsFeedEmpty { get; set; } = false;

    public bool IsFeedNotEmpty => !IsFeedEmpty;

    public async Task LoadHomeFeedAsync()
    {
        IsLoading = true;
        try
        {
            var feed = await feedService.GetFeedAsync();

            IsFeedEmpty = feed.Count == 0;

            if (feed.Count < 8)
            {
                IsFeedEmpty = true;
                return;
            }

            _feed = feed;

            for (int i = 0; i < 5; i++)
            {
                var item = feed[i];
                HeroAdapter.HeroSlides.Add(new(item.Title, item.Author, "Play", new(item.ImageUrl), PlayCommand, item.Id));
            }
            
            for (int i = 5; i < 8; i++)
            {
                var card = feed[i];
                HeroAdapter.Cards.Add(new(card.Id, card.ImageUrl, "You might like this instead", card.Title, card.Author));
            }

            for (int i = 8; i < feed.Count; i++)
            {
                var item = feed[i];
                Feed.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void Play(string id)
    {
        var selectedItem = _feed.FirstOrDefault(f => f.Id == id) ?? throw new InvalidDataException();

        WeakReferenceMessenger.Default.Send(new PlayItemMessage(new()
        {
            Id = id,
            Author = selectedItem.Author,
            ImageUrl = selectedItem.ImageUrl,
            Title = selectedItem.Title,
            Url = selectedItem.Url,
            Position = 0,
            Duration = selectedItem.Duration,
            AddedDate = DateTime.Now,
        }));
    }
}