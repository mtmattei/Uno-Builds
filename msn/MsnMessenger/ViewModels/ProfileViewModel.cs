using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsnMessenger.Models;
using MsnMessenger.Services;

namespace MsnMessenger.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IMsnDataService _dataService;

    [ObservableProperty]
    private bool _isEditingName;

    [ObservableProperty]
    private bool _isEditingMessage;

    [ObservableProperty]
    private string _tempDisplayName = string.Empty;

    [ObservableProperty]
    private string _tempPersonalMessage = string.Empty;

    [ObservableProperty]
    private bool _showStatusPicker;

    public UserProfile CurrentUser => _dataService.CurrentUser;

    public string DisplayName
    {
        get => CurrentUser.DisplayName;
        set
        {
            if (CurrentUser.DisplayName != value)
            {
                _dataService.UpdateDisplayName(value);
                OnPropertyChanged();
            }
        }
    }

    public string PersonalMessage
    {
        get => CurrentUser.PersonalMessage;
        set
        {
            if (CurrentUser.PersonalMessage != value)
            {
                _dataService.UpdatePersonalMessage(value);
                OnPropertyChanged();
            }
        }
    }

    public PresenceStatus Status
    {
        get => CurrentUser.Status;
        set
        {
            if (CurrentUser.Status != value)
            {
                _dataService.UpdateUserStatus(value);
                OnPropertyChanged();
            }
        }
    }

    public ProfileViewModel(IMsnDataService dataService)
    {
        _dataService = dataService;
        _tempDisplayName = CurrentUser.DisplayName;
        _tempPersonalMessage = CurrentUser.PersonalMessage;
    }

    [RelayCommand]
    private void StartEditName()
    {
        TempDisplayName = DisplayName;
        IsEditingName = true;
    }

    [RelayCommand]
    private void SaveName()
    {
        DisplayName = TempDisplayName;
        IsEditingName = false;
    }

    [RelayCommand]
    private void CancelEditName()
    {
        TempDisplayName = DisplayName;
        IsEditingName = false;
    }

    [RelayCommand]
    private void StartEditMessage()
    {
        TempPersonalMessage = PersonalMessage;
        IsEditingMessage = true;
    }

    [RelayCommand]
    private void SaveMessage()
    {
        PersonalMessage = TempPersonalMessage;
        IsEditingMessage = false;
    }

    [RelayCommand]
    private void CancelEditMessage()
    {
        TempPersonalMessage = PersonalMessage;
        IsEditingMessage = false;
    }

    [RelayCommand]
    private void SetStatus(PresenceStatus status)
    {
        Status = status;
        ShowStatusPicker = false;
    }

    [RelayCommand]
    private void ToggleStatusPicker()
    {
        ShowStatusPicker = !ShowStatusPicker;
    }

    [RelayCommand]
    private void SetQuickMessage(string message)
    {
        PersonalMessage = message;
        TempPersonalMessage = message;
    }
}
