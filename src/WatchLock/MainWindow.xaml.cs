using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WatchLock;

public partial class MainWindow : Window
{
    private bool _loadingLanguage;

    public MainWindow()
    {
        InitializeComponent();

        _loadingLanguage = true;
        LanguageBox.ItemsSource = Strings.Languages;
        LanguageBox.SelectedItem = Strings.Languages.FirstOrDefault(l => l.Code == Strings.Current) ?? Strings.Languages[0];
        _loadingLanguage = false;

        TestMode.IsChecked = App.Current.Settings.TestMode;
        HotkeyInfo.Text = App.Current.HotkeyActive ? Strings.HotkeyLabel : Strings.HotkeyFailed;

        if (PasswordStore.HasPassword) ShowLockPanel();
        else ShowSetup(changing: false);
    }

    private void ShowSetup(bool changing)
    {
        SetupPanel.Visibility = Visibility.Visible;
        LockPanel.Visibility = Visibility.Collapsed;
        CurrentPasswordPanel.Visibility = changing ? Visibility.Visible : Visibility.Collapsed;
        CancelButton.Visibility = changing ? Visibility.Visible : Visibility.Collapsed;
        SetupTitle.Text = changing ? Strings.ChangeTitle : Strings.SetupTitle;
        CurrentPassword.Clear(); NewPassword1.Clear(); NewPassword2.Clear();
        StatusText.Text = "";
        (changing ? CurrentPassword : NewPassword1).Focus();
    }

    private void ShowLockPanel()
    {
        SetupPanel.Visibility = Visibility.Collapsed;
        LockPanel.Visibility = Visibility.Visible;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        ShowError("");
        bool changing = CurrentPasswordPanel.Visibility == Visibility.Visible;
        string current = CurrentPassword.Password, p1 = NewPassword1.Password, p2 = NewPassword2.Password;

        if (p1.Length < 4) { ShowError(Strings.PwTooShort); return; }
        if (p1 != p2) { ShowError(Strings.PwMismatch); return; }

        SaveButton.IsEnabled = false;
        try
        {
            if (changing && !await Task.Run(() => PasswordStore.Verify(current)))
            {
                ShowError(Strings.PwWrongCurrent);
                return;
            }
            await Task.Run(() => PasswordStore.Save(p1));
            ShowLockPanel();
            ShowInfo(changing ? Strings.PwChanged : Strings.PwSaved);
        }
        finally { SaveButton.IsEnabled = true; }
    }

    private void NewPassword2_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Save_Click(sender, e);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        ShowLockPanel();
        StatusText.Text = "";
    }

    private void ChangePassword_Click(object sender, RoutedEventArgs e) => ShowSetup(changing: true);

    private void Lock_Click(object sender, RoutedEventArgs e) => App.Current.Lock(fromWindow: true);

    private void TestMode_Changed(object sender, RoutedEventArgs e)
    {
        var s = App.Current.Settings;
        bool value = TestMode.IsChecked == true;
        if (s.TestMode == value) return;
        s.TestMode = value;
        s.Save();
    }

    private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loadingLanguage || LanguageBox.SelectedItem is not Strings.LanguageInfo lang) return;
        if (lang.Code == Strings.Current) return;
        // Texts are read when a window is created, so reopen the window in the new language.
        Dispatcher.BeginInvoke(() => App.Current.ChangeLanguage(lang.Code));
    }

    private void ShowError(string m) { StatusText.Foreground = (Brush)FindResource("Danger"); StatusText.Text = m; }
    public void ShowInfo(string m) { StatusText.Foreground = (Brush)FindResource("Muted"); StatusText.Text = m; }
}
