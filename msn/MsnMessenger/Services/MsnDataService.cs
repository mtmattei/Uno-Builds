using MsnMessenger.Models;
using System.Collections.ObjectModel;

namespace MsnMessenger.Services;

public class MsnDataService : IMsnDataService
{
    private readonly Dictionary<string, List<Message>> _messageStore = new();
    private readonly Dictionary<string, LiveActivity> _activityStore = new();

    public UserProfile CurrentUser { get; } = new()
    {
        DisplayName = "~*You*~ 🦋",
        Status = PresenceStatus.Online,
        PersonalMessage = "Building something cool...",
        Email = "you@msn.com",
        BuddyCount = 12,
        MessageCount = 1247,
        NudgeCount = 89
    };

    public ObservableCollection<ContactGroup> Groups { get; } = new();
    public ObservableCollection<Chat> RecentChats { get; } = new();

    public MsnDataService()
    {
        InitializeSampleData();
        InitializeActivityData();
    }

    private void InitializeSampleData()
    {
        var sarah = new Contact
        {
            Id = "1",
            DisplayName = "~*Sarah*~ ♪",
            Status = PresenceStatus.Online,
            PersonalMessage = "🎵 Listening to Taylor Swift",
            FrameColor = "#E91E8C",
            NowPlaying = new NowPlaying
            {
                TrackName = "Anti-Hero",
                ArtistName = "Taylor Swift",
                AlbumName = "Midnights",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(20)),
                Progress = TimeSpan.FromMinutes(1).Add(TimeSpan.FromSeconds(45)),
                IsPlaying = true
            }
        };

        var mike = new Contact
        {
            Id = "2",
            DisplayName = "Mike 🎮",
            Status = PresenceStatus.Online,
            PersonalMessage = "Playing Valorant",
            FrameColor = "#9B59B6",
            NowPlaying = new NowPlaying
            {
                TrackName = "RISE",
                ArtistName = "League of Legends",
                AlbumName = "Worlds 2018",
                Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(1)),
                Progress = TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(10)),
                IsPlaying = true
            }
        };

        var emma = new Contact
        {
            Id = "3",
            DisplayName = "Emma ★彡",
            Status = PresenceStatus.Away,
            PersonalMessage = "brb, grabbing coffee ☕",
            FrameColor = "#FF6B00"
        };

        var alex = new Contact
        {
            Id = "4",
            DisplayName = "xX_Alex_Xx",
            Status = PresenceStatus.Busy,
            PersonalMessage = "In a meeting - do not disturb",
            FrameColor = "#0078D4"
        };

        var mom = new Contact
        {
            Id = "5",
            DisplayName = "Mom ❤️",
            Status = PresenceStatus.Online,
            PersonalMessage = "Call me when you can!",
            FrameColor = "#00B377"
        };

        var dad = new Contact
        {
            Id = "6",
            DisplayName = "Dad 👨",
            Status = PresenceStatus.Offline,
            PersonalMessage = string.Empty
        };

        var jake = new Contact
        {
            Id = "7",
            DisplayName = "[AFK] Jake",
            Status = PresenceStatus.Offline,
            PersonalMessage = "Gone fishing 🎣"
        };

        var lisa = new Contact
        {
            Id = "8",
            DisplayName = "✿ Lisa ✿",
            Status = PresenceStatus.Online,
            PersonalMessage = "New job, who dis? 💼"
        };

        Groups.Add(new ContactGroup
        {
            Name = "Best Friends",
            Emoji = "⭐",
            Contacts = new ObservableCollection<Contact> { sarah, mike }
        });

        Groups.Add(new ContactGroup
        {
            Name = "Gaming Crew",
            Emoji = "🎮",
            Contacts = new ObservableCollection<Contact> { mike, alex, jake }
        });

        Groups.Add(new ContactGroup
        {
            Name = "Family",
            Emoji = "👨‍👩‍👧",
            Contacts = new ObservableCollection<Contact> { mom, dad }
        });

        Groups.Add(new ContactGroup
        {
            Name = "Work",
            Emoji = "💼",
            Contacts = new ObservableCollection<Contact> { emma, lisa }
        });

        RecentChats.Add(new Chat
        {
            Contact = sarah,
            LastMessageTime = DateTime.Now.AddMinutes(-2),
            UnreadCount = 2
        });

        RecentChats.Add(new Chat
        {
            Contact = mike,
            LastMessageTime = DateTime.Now.AddMinutes(-15),
            UnreadCount = 0
        });

        RecentChats.Add(new Chat
        {
            Contact = mom,
            LastMessageTime = DateTime.Now.AddHours(-2),
            UnreadCount = 1
        });

        _messageStore["1"] = new List<Message>
        {
            new() { Text = "Hey! Are you coming to the party tonight?", SenderId = "1", Timestamp = DateTime.Now.AddMinutes(-10) },
            new() { Text = "Yes! Can't wait 🎉", SenderId = "me", Timestamp = DateTime.Now.AddMinutes(-8) },
            new() { Text = "Awesome! See you at 8!", SenderId = "1", Timestamp = DateTime.Now.AddMinutes(-5) },
            new() { Text = "Should I bring anything?", SenderId = "me", Timestamp = DateTime.Now.AddMinutes(-2) }
        };

        _messageStore["2"] = new List<Message>
        {
            new() { Text = "gg bro", SenderId = "2", Timestamp = DateTime.Now.AddMinutes(-20) },
            new() { Text = "That was insane! Another round?", SenderId = "me", Timestamp = DateTime.Now.AddMinutes(-15) }
        };
    }

    public void UpdateUserProfile(UserProfile profile)
    {
        CurrentUser.DisplayName = profile.DisplayName;
        CurrentUser.Status = profile.Status;
        CurrentUser.PersonalMessage = profile.PersonalMessage;
    }

    public void UpdateUserStatus(PresenceStatus status)
    {
        CurrentUser.Status = status;
    }

    public void UpdateDisplayName(string name)
    {
        CurrentUser.DisplayName = name;
    }

    public void UpdatePersonalMessage(string message)
    {
        CurrentUser.PersonalMessage = message;
    }

    public List<Message> GetMessagesForContact(string contactId)
    {
        return _messageStore.TryGetValue(contactId, out var messages)
            ? messages
            : new List<Message>();
    }

    public void SendMessage(string contactId, string text)
    {
        if (!_messageStore.ContainsKey(contactId))
            _messageStore[contactId] = new List<Message>();

        _messageStore[contactId].Add(new Message
        {
            Text = text,
            SenderId = "me",
            Type = MessageType.Text
        });

        UpdateRecentChat(contactId);
    }

    public void SendWink(string contactId, string emoji)
    {
        if (!_messageStore.ContainsKey(contactId))
            _messageStore[contactId] = new List<Message>();

        _messageStore[contactId].Add(new Message
        {
            Text = emoji,
            SenderId = "me",
            Type = MessageType.Wink,
            WinkEmoji = emoji
        });

        UpdateRecentChat(contactId);
    }

    public void SendNudge(string contactId)
    {
        if (!_messageStore.ContainsKey(contactId))
            _messageStore[contactId] = new List<Message>();

        _messageStore[contactId].Add(new Message
        {
            Text = "👊 Nudge!",
            SenderId = "me",
            Type = MessageType.Nudge
        });

        CurrentUser.NudgeCount++;
        UpdateRecentChat(contactId);
    }

    private void UpdateRecentChat(string contactId)
    {
        var existingChat = RecentChats.FirstOrDefault(c => c.Contact.Id == contactId);
        if (existingChat != null)
        {
            existingChat.LastMessageTime = DateTime.Now;
            var index = RecentChats.IndexOf(existingChat);
            if (index > 0)
            {
                RecentChats.Move(index, 0);
            }
        }
    }

    private void InitializeActivityData()
    {
        // Sarah is listening to Spotify
        _activityStore["1"] = new LiveActivity
        {
            Type = ActivityType.Spotify,
            Title = "Blinding Lights",
            Artist = "The Weeknd",
            Album = "After Hours",
            Progress = 65,
            Duration = "3:20",
            AccentColor = "#1DB954",
            ActionLabel = "Listen Along",
            ActionUrl = "spotify:track:0VjIjW4GlUZAMYd2vXMi3b",
            StartedAt = DateTime.Now.AddMinutes(-2)
        };

        // Mike is playing Valorant
        _activityStore["2"] = new LiveActivity
        {
            Type = ActivityType.Gaming,
            Title = "Valorant",
            Platform = "PC",
            Status = "In Match",
            PartySize = "3/5",
            Joinable = true,
            AccentColor = "#ff4655",
            ActionLabel = "Ask to Join",
            StartedAt = DateTime.Now.AddMinutes(-24)
        };

        // Lisa is watching Netflix
        _activityStore["8"] = new LiveActivity
        {
            Type = ActivityType.Video,
            Title = "Stranger Things",
            Subtitle = "Season 4, Episode 7",
            Service = "Netflix",
            AccentColor = "#e50914",
            ActionLabel = "Watch Together",
            StartedAt = DateTime.Now.AddMinutes(-45)
        };
    }

    public LiveActivity? GetActivityForContact(string contactId)
    {
        return _activityStore.TryGetValue(contactId, out var activity) ? activity : null;
    }
}
