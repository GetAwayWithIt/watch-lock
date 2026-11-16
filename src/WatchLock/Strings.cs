namespace WatchLock;

/// <summary>
/// All user-facing text. English is the default and the fallback for any missing translation.
/// To add a language: add it to <see cref="Languages"/> and add a table with the same keys.
/// </summary>
public static class Strings
{
    public const string HotkeyText = "Ctrl+AltGr+F";

    public sealed record LanguageInfo(string Code, string NativeName)
    {
        public override string ToString() => NativeName;
    }

    public static readonly LanguageInfo[] Languages =
    {
        new("en", "English"),
        new("tr", "Türkçe"),
        new("de", "Deutsch"),
        new("es", "Español"),
        new("fr", "Français"),
        new("pt", "Português (Brasil)"),
        new("ru", "Русский"),
    };

    public static string Current { get; private set; } = "en";

    public static void SetLanguage(string? code) =>
        Current = code is not null && Table.ContainsKey(code) ? code : "en";

    private static string T(string key) =>
        Table[Current].TryGetValue(key, out var s) ? s
        : Table["en"].TryGetValue(key, out var e) ? e
        : key;

    // ---- Main window ----
    public static string AppName => T(nameof(AppName));
    public static string Tagline => T(nameof(Tagline));
    public static string SetupTitle => T(nameof(SetupTitle));
    public static string ChangeTitle => T(nameof(ChangeTitle));
    public static string CurrentPassword => T(nameof(CurrentPassword));
    public static string NewPassword => T(nameof(NewPassword));
    public static string Repeat => T(nameof(Repeat));
    public static string Save => T(nameof(Save));
    public static string Cancel => T(nameof(Cancel));
    public static string Lock => T(nameof(Lock));
    public static string HotkeyLabel => string.Format(T(nameof(HotkeyLabel)), HotkeyText);
    public static string HotkeyFailed => T(nameof(HotkeyFailed));
    public static string TestMode => T(nameof(TestMode));
    public static string TestModeHint => T(nameof(TestModeHint));
    public static string ChangePassword => T(nameof(ChangePassword));
    public static string TrayHint => T(nameof(TrayHint));
    public static string Language => T(nameof(Language));
    public static string PwTooShort => T(nameof(PwTooShort));
    public static string PwMismatch => T(nameof(PwMismatch));
    public static string PwWrongCurrent => T(nameof(PwWrongCurrent));
    public static string PwChanged => T(nameof(PwChanged));
    public static string PwSaved => T(nameof(PwSaved));

    // ---- Lock screen ----
    public static string LockedBadge => T(nameof(LockedBadge));
    public static string LockedBadgeSecondary => T(nameof(LockedBadgeSecondary));
    public static string ComputerLocked => T(nameof(ComputerLocked));
    public static string LockHelp => T(nameof(LockHelp));
    public static string WrongPassword(int n) => string.Format(T(nameof(WrongPassword)), n);
    public static string TestCountdown(int seconds) => string.Format(T(nameof(TestCountdown)), seconds);
    public static string LockedFor(string duration) => string.Format(T(nameof(LockedFor)), duration);
    public static string Unlocked => T(nameof(Unlocked));
    public static string UnlockedWithAttempts(int n) => string.Format(T(nameof(UnlockedWithAttempts)), n);
    public static string TestExpired => T(nameof(TestExpired));
    public static string UnlockedViaWindows => T(nameof(UnlockedViaWindows));

    // ---- Tray ----
    public static string TrayTipUnlocked => string.Format(T(nameof(TrayTipUnlocked)), HotkeyText);
    public static string TrayTipLocked => T(nameof(TrayTipLocked));
    public static string MenuOpen => T(nameof(MenuOpen));
    public static string MenuLock => T(nameof(MenuLock));
    public static string MenuExit => T(nameof(MenuExit));
    public static string BalloonBgTitle => T(nameof(BalloonBgTitle));
    public static string BalloonBgText => string.Format(T(nameof(BalloonBgText)), HotkeyText);

    private static readonly Dictionary<string, Dictionary<string, string>> Table = new()
    {
        ["en"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Your screen stays visible while the keyboard and mouse are locked. Your work keeps running in the background.",
            [nameof(SetupTitle)] = "Set a lock password",
            [nameof(ChangeTitle)] = "Change password",
            [nameof(CurrentPassword)] = "Current password",
            [nameof(NewPassword)] = "New password",
            [nameof(Repeat)] = "Repeat",
            [nameof(Save)] = "Save",
            [nameof(Cancel)] = "Cancel",
            [nameof(Lock)] = "Lock",
            [nameof(HotkeyLabel)] = "Hotkey: {0}",
            [nameof(HotkeyFailed)] = "The hotkey couldn't be started. Use the Lock button instead.",
            [nameof(TestMode)] = "Test mode: unlock automatically after 60 seconds",
            [nameof(TestModeHint)] = "Keep this on for your first tries, turn it off once you're confident.",
            [nameof(ChangePassword)] = "Change password",
            [nameof(TrayHint)] = "Closing this window keeps the app running in the system tray, next to the clock.",
            [nameof(Language)] = "Language",
            [nameof(PwTooShort)] = "The password must be at least 4 characters.",
            [nameof(PwMismatch)] = "The two passwords don't match.",
            [nameof(PwWrongCurrent)] = "The current password is wrong.",
            [nameof(PwChanged)] = "Password changed.",
            [nameof(PwSaved)] = "Password saved. You're all set.",
            [nameof(LockedBadge)] = "🔒 Locked",
            [nameof(LockedBadgeSecondary)] = "🔒 Locked: the password box is on the main screen",
            [nameof(ComputerLocked)] = "🔒 This computer is locked",
            [nameof(LockHelp)] = "Type the password and press Enter. Pressing Ctrl+Alt+Del and signing in with your Windows password also unlocks it.",
            [nameof(WrongPassword)] = "Wrong password ({0})",
            [nameof(TestCountdown)] = "Test mode: unlocks automatically in {0} s",
            [nameof(LockedFor)] = "Locked for {0}",
            [nameof(Unlocked)] = "Unlocked.",
            [nameof(UnlockedWithAttempts)] = "Unlocked. Wrong password attempts while you were away: {0}.",
            [nameof(TestExpired)] = "Test mode: time's up, the lock opened automatically.",
            [nameof(UnlockedViaWindows)] = "Unlocked with your Windows password.",
            [nameof(TrayTipUnlocked)] = "Watch Lock: press {0} to lock",
            [nameof(TrayTipLocked)] = "Watch Lock: locked",
            [nameof(MenuOpen)] = "Open Watch Lock",
            [nameof(MenuLock)] = "Lock",
            [nameof(MenuExit)] = "Exit",
            [nameof(BalloonBgTitle)] = "Watch Lock is running in the background",
            [nameof(BalloonBgText)] = "Press {0} to lock. Right-click the tray icon to exit.",
        },
        ["tr"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Ekran görünür kalır, klavye ve fare kilitlenir. İşlerin arkada çalışmaya devam eder.",
            [nameof(SetupTitle)] = "Bir kilit şifresi belirle",
            [nameof(ChangeTitle)] = "Şifreyi değiştir",
            [nameof(CurrentPassword)] = "Mevcut şifre",
            [nameof(NewPassword)] = "Yeni şifre",
            [nameof(Repeat)] = "Tekrar",
            [nameof(Save)] = "Kaydet",
            [nameof(Cancel)] = "Vazgeç",
            [nameof(Lock)] = "Kilitle",
            [nameof(HotkeyLabel)] = "Kısayol: {0}",
            [nameof(HotkeyFailed)] = "Kısayol tuşu başlatılamadı, Kilitle butonunu kullan.",
            [nameof(TestMode)] = "Deneme modu: 60 saniye sonra kendiliğinden açılsın",
            [nameof(TestModeHint)] = "İlk denemelerde açık bırak, emin olunca kapat.",
            [nameof(ChangePassword)] = "Şifreyi değiştir",
            [nameof(TrayHint)] = "Pencereyi kapatınca uygulama saatin yanındaki tepside çalışmaya devam eder.",
            [nameof(Language)] = "Dil",
            [nameof(PwTooShort)] = "Şifre en az 4 karakter olsun.",
            [nameof(PwMismatch)] = "İki şifre aynı değil.",
            [nameof(PwWrongCurrent)] = "Mevcut şifre yanlış.",
            [nameof(PwChanged)] = "Şifre değişti.",
            [nameof(PwSaved)] = "Şifre kaydedildi. Hazırsın.",
            [nameof(LockedBadge)] = "🔒 Kilitli",
            [nameof(LockedBadgeSecondary)] = "🔒 Kilitli: şifre ana ekranda",
            [nameof(ComputerLocked)] = "🔒 Bilgisayar kilitli",
            [nameof(LockHelp)] = "Şifreyi yaz, Enter'a bas. Ctrl+Alt+Del'e basıp Windows şifrenle girersen de açılır.",
            [nameof(WrongPassword)] = "Yanlış şifre ({0})",
            [nameof(TestCountdown)] = "Deneme modu: {0} sn sonra kendiliğinden açılacak",
            [nameof(LockedFor)] = "{0} süredir kilitli",
            [nameof(Unlocked)] = "Kilit açıldı.",
            [nameof(UnlockedWithAttempts)] = "Kilit açıldı. Bu sırada denenen yanlış şifre sayısı: {0}.",
            [nameof(TestExpired)] = "Deneme modu: süre doldu, kilit kendiliğinden açıldı.",
            [nameof(UnlockedViaWindows)] = "Windows şifresiyle açıldı.",
            [nameof(TrayTipUnlocked)] = "Watch Lock: kilitlemek için {0}",
            [nameof(TrayTipLocked)] = "Watch Lock: kilitli",
            [nameof(MenuOpen)] = "Watch Lock'u aç",
            [nameof(MenuLock)] = "Kilitle",
            [nameof(MenuExit)] = "Çıkış",
            [nameof(BalloonBgTitle)] = "Watch Lock arka planda çalışıyor",
            [nameof(BalloonBgText)] = "Kilitlemek için {0}. Kapatmak için tepsideki simgeye sağ tıkla.",
        },
        ["de"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Dein Bildschirm bleibt sichtbar, Tastatur und Maus werden gesperrt. Deine Arbeit läuft im Hintergrund weiter.",
            [nameof(SetupTitle)] = "Lege ein Sperrpasswort fest",
            [nameof(ChangeTitle)] = "Passwort ändern",
            [nameof(CurrentPassword)] = "Aktuelles Passwort",
            [nameof(NewPassword)] = "Neues Passwort",
            [nameof(Repeat)] = "Wiederholen",
            [nameof(Save)] = "Speichern",
            [nameof(Cancel)] = "Abbrechen",
            [nameof(Lock)] = "Sperren",
            [nameof(HotkeyLabel)] = "Tastenkürzel: {0}",
            [nameof(HotkeyFailed)] = "Das Tastenkürzel konnte nicht aktiviert werden. Nutze stattdessen „Sperren“.",
            [nameof(TestMode)] = "Testmodus: nach 60 Sekunden automatisch entsperren",
            [nameof(TestModeHint)] = "Lass ihn bei den ersten Versuchen an und schalte ihn aus, sobald du sicher bist.",
            [nameof(ChangePassword)] = "Passwort ändern",
            [nameof(TrayHint)] = "Wenn du dieses Fenster schließt, läuft die App im Infobereich neben der Uhr weiter.",
            [nameof(Language)] = "Sprache",
            [nameof(PwTooShort)] = "Das Passwort muss mindestens 4 Zeichen lang sein.",
            [nameof(PwMismatch)] = "Die beiden Passwörter stimmen nicht überein.",
            [nameof(PwWrongCurrent)] = "Das aktuelle Passwort ist falsch.",
            [nameof(PwChanged)] = "Passwort geändert.",
            [nameof(PwSaved)] = "Passwort gespeichert. Alles bereit.",
            [nameof(LockedBadge)] = "🔒 Gesperrt",
            [nameof(LockedBadgeSecondary)] = "🔒 Gesperrt: Das Passwortfeld ist auf dem Hauptbildschirm",
            [nameof(ComputerLocked)] = "🔒 Dieser Computer ist gesperrt",
            [nameof(LockHelp)] = "Passwort eingeben und Enter drücken. Auch Strg+Alt+Entf und die Anmeldung mit deinem Windows-Passwort entsperren ihn.",
            [nameof(WrongPassword)] = "Falsches Passwort ({0})",
            [nameof(TestCountdown)] = "Testmodus: entsperrt automatisch in {0} s",
            [nameof(LockedFor)] = "Gesperrt: {0}",
            [nameof(Unlocked)] = "Entsperrt.",
            [nameof(UnlockedWithAttempts)] = "Entsperrt. Fehlversuche während deiner Abwesenheit: {0}.",
            [nameof(TestExpired)] = "Testmodus: Zeit abgelaufen, die Sperre wurde automatisch aufgehoben.",
            [nameof(UnlockedViaWindows)] = "Mit deinem Windows-Passwort entsperrt.",
            [nameof(TrayTipUnlocked)] = "Watch Lock: {0} zum Sperren",
            [nameof(TrayTipLocked)] = "Watch Lock: gesperrt",
            [nameof(MenuOpen)] = "Watch Lock öffnen",
            [nameof(MenuLock)] = "Sperren",
            [nameof(MenuExit)] = "Beenden",
            [nameof(BalloonBgTitle)] = "Watch Lock läuft im Hintergrund",
            [nameof(BalloonBgText)] = "Drücke {0} zum Sperren. Zum Beenden mit der rechten Maustaste auf das Symbol im Infobereich klicken.",
        },
        ["es"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Tu pantalla sigue visible mientras el teclado y el ratón quedan bloqueados. Tu trabajo sigue en segundo plano.",
            [nameof(SetupTitle)] = "Elige una contraseña de bloqueo",
            [nameof(ChangeTitle)] = "Cambiar contraseña",
            [nameof(CurrentPassword)] = "Contraseña actual",
            [nameof(NewPassword)] = "Nueva contraseña",
            [nameof(Repeat)] = "Repetir",
            [nameof(Save)] = "Guardar",
            [nameof(Cancel)] = "Cancelar",
            [nameof(Lock)] = "Bloquear",
            [nameof(HotkeyLabel)] = "Atajo: {0}",
            [nameof(HotkeyFailed)] = "No se pudo activar el atajo. Usa el botón Bloquear.",
            [nameof(TestMode)] = "Modo de prueba: desbloquear automáticamente a los 60 segundos",
            [nameof(TestModeHint)] = "Déjalo activado en tus primeras pruebas y desactívalo cuando estés seguro.",
            [nameof(ChangePassword)] = "Cambiar contraseña",
            [nameof(TrayHint)] = "Al cerrar esta ventana, la aplicación sigue funcionando en la bandeja del sistema, junto al reloj.",
            [nameof(Language)] = "Idioma",
            [nameof(PwTooShort)] = "La contraseña debe tener al menos 4 caracteres.",
            [nameof(PwMismatch)] = "Las dos contraseñas no coinciden.",
            [nameof(PwWrongCurrent)] = "La contraseña actual es incorrecta.",
            [nameof(PwChanged)] = "Contraseña cambiada.",
            [nameof(PwSaved)] = "Contraseña guardada. Todo listo.",
            [nameof(LockedBadge)] = "🔒 Bloqueado",
            [nameof(LockedBadgeSecondary)] = "🔒 Bloqueado: la contraseña está en la pantalla principal",
            [nameof(ComputerLocked)] = "🔒 Este equipo está bloqueado",
            [nameof(LockHelp)] = "Escribe la contraseña y pulsa Enter. También se desbloquea con Ctrl+Alt+Supr e iniciando sesión con tu contraseña de Windows.",
            [nameof(WrongPassword)] = "Contraseña incorrecta ({0})",
            [nameof(TestCountdown)] = "Modo de prueba: se desbloqueará en {0} s",
            [nameof(LockedFor)] = "Bloqueado: {0}",
            [nameof(Unlocked)] = "Desbloqueado.",
            [nameof(UnlockedWithAttempts)] = "Desbloqueado. Intentos fallidos mientras no estabas: {0}.",
            [nameof(TestExpired)] = "Modo de prueba: se acabó el tiempo, el bloqueo se abrió solo.",
            [nameof(UnlockedViaWindows)] = "Desbloqueado con tu contraseña de Windows.",
            [nameof(TrayTipUnlocked)] = "Watch Lock: pulsa {0} para bloquear",
            [nameof(TrayTipLocked)] = "Watch Lock: bloqueado",
            [nameof(MenuOpen)] = "Abrir Watch Lock",
            [nameof(MenuLock)] = "Bloquear",
            [nameof(MenuExit)] = "Salir",
            [nameof(BalloonBgTitle)] = "Watch Lock sigue en segundo plano",
            [nameof(BalloonBgText)] = "Pulsa {0} para bloquear. Haz clic derecho en el icono de la bandeja para salir.",
        },
        ["fr"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Votre écran reste visible pendant que le clavier et la souris sont verrouillés. Votre travail continue en arrière-plan.",
            [nameof(SetupTitle)] = "Choisissez un mot de passe de verrouillage",
            [nameof(ChangeTitle)] = "Changer le mot de passe",
            [nameof(CurrentPassword)] = "Mot de passe actuel",
            [nameof(NewPassword)] = "Nouveau mot de passe",
            [nameof(Repeat)] = "Confirmer",
            [nameof(Save)] = "Enregistrer",
            [nameof(Cancel)] = "Annuler",
            [nameof(Lock)] = "Verrouiller",
            [nameof(HotkeyLabel)] = "Raccourci : {0}",
            [nameof(HotkeyFailed)] = "Impossible d’activer le raccourci. Utilisez le bouton Verrouiller.",
            [nameof(TestMode)] = "Mode test : déverrouiller automatiquement après 60 secondes",
            [nameof(TestModeHint)] = "Laissez-le activé pour vos premiers essais, désactivez-le quand vous êtes sûr.",
            [nameof(ChangePassword)] = "Changer le mot de passe",
            [nameof(TrayHint)] = "Si vous fermez cette fenêtre, l’application continue de tourner dans la zone de notification, près de l’horloge.",
            [nameof(Language)] = "Langue",
            [nameof(PwTooShort)] = "Le mot de passe doit contenir au moins 4 caractères.",
            [nameof(PwMismatch)] = "Les deux mots de passe ne correspondent pas.",
            [nameof(PwWrongCurrent)] = "Le mot de passe actuel est incorrect.",
            [nameof(PwChanged)] = "Mot de passe modifié.",
            [nameof(PwSaved)] = "Mot de passe enregistré. Tout est prêt.",
            [nameof(LockedBadge)] = "🔒 Verrouillé",
            [nameof(LockedBadgeSecondary)] = "🔒 Verrouillé : le mot de passe se saisit sur l’écran principal",
            [nameof(ComputerLocked)] = "🔒 Cet ordinateur est verrouillé",
            [nameof(LockHelp)] = "Saisissez le mot de passe et appuyez sur Entrée. Ctrl+Alt+Suppr puis une connexion avec votre mot de passe Windows le déverrouille aussi.",
            [nameof(WrongPassword)] = "Mot de passe incorrect ({0})",
            [nameof(TestCountdown)] = "Mode test : déverrouillage automatique dans {0} s",
            [nameof(LockedFor)] = "Verrouillé : {0}",
            [nameof(Unlocked)] = "Déverrouillé.",
            [nameof(UnlockedWithAttempts)] = "Déverrouillé. Tentatives échouées pendant votre absence : {0}.",
            [nameof(TestExpired)] = "Mode test : temps écoulé, le verrou s’est ouvert automatiquement.",
            [nameof(UnlockedViaWindows)] = "Déverrouillé avec votre mot de passe Windows.",
            [nameof(TrayTipUnlocked)] = "Watch Lock : {0} pour verrouiller",
            [nameof(TrayTipLocked)] = "Watch Lock : verrouillé",
            [nameof(MenuOpen)] = "Ouvrir Watch Lock",
            [nameof(MenuLock)] = "Verrouiller",
            [nameof(MenuExit)] = "Quitter",
            [nameof(BalloonBgTitle)] = "Watch Lock tourne en arrière-plan",
            [nameof(BalloonBgText)] = "Appuyez sur {0} pour verrouiller. Clic droit sur l’icône de la zone de notification pour quitter.",
        },
        ["pt"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Sua tela continua visível enquanto o teclado e o mouse ficam bloqueados. Seu trabalho continua em segundo plano.",
            [nameof(SetupTitle)] = "Defina uma senha de bloqueio",
            [nameof(ChangeTitle)] = "Alterar senha",
            [nameof(CurrentPassword)] = "Senha atual",
            [nameof(NewPassword)] = "Nova senha",
            [nameof(Repeat)] = "Repetir",
            [nameof(Save)] = "Salvar",
            [nameof(Cancel)] = "Cancelar",
            [nameof(Lock)] = "Bloquear",
            [nameof(HotkeyLabel)] = "Atalho: {0}",
            [nameof(HotkeyFailed)] = "Não foi possível ativar o atalho. Use o botão Bloquear.",
            [nameof(TestMode)] = "Modo de teste: desbloquear automaticamente após 60 segundos",
            [nameof(TestModeHint)] = "Deixe ativado nas primeiras tentativas e desative quando tiver certeza.",
            [nameof(ChangePassword)] = "Alterar senha",
            [nameof(TrayHint)] = "Ao fechar esta janela, o app continua rodando na bandeja do sistema, ao lado do relógio.",
            [nameof(Language)] = "Idioma",
            [nameof(PwTooShort)] = "A senha deve ter pelo menos 4 caracteres.",
            [nameof(PwMismatch)] = "As duas senhas não coincidem.",
            [nameof(PwWrongCurrent)] = "A senha atual está incorreta.",
            [nameof(PwChanged)] = "Senha alterada.",
            [nameof(PwSaved)] = "Senha salva. Tudo pronto.",
            [nameof(LockedBadge)] = "🔒 Bloqueado",
            [nameof(LockedBadgeSecondary)] = "🔒 Bloqueado: a senha fica na tela principal",
            [nameof(ComputerLocked)] = "🔒 Este computador está bloqueado",
            [nameof(LockHelp)] = "Digite a senha e pressione Enter. Pressionar Ctrl+Alt+Del e entrar com sua senha do Windows também desbloqueia.",
            [nameof(WrongPassword)] = "Senha incorreta ({0})",
            [nameof(TestCountdown)] = "Modo de teste: desbloqueia automaticamente em {0} s",
            [nameof(LockedFor)] = "Bloqueado: {0}",
            [nameof(Unlocked)] = "Desbloqueado.",
            [nameof(UnlockedWithAttempts)] = "Desbloqueado. Tentativas erradas enquanto você estava fora: {0}.",
            [nameof(TestExpired)] = "Modo de teste: o tempo acabou, o bloqueio abriu sozinho.",
            [nameof(UnlockedViaWindows)] = "Desbloqueado com sua senha do Windows.",
            [nameof(TrayTipUnlocked)] = "Watch Lock: pressione {0} para bloquear",
            [nameof(TrayTipLocked)] = "Watch Lock: bloqueado",
            [nameof(MenuOpen)] = "Abrir o Watch Lock",
            [nameof(MenuLock)] = "Bloquear",
            [nameof(MenuExit)] = "Sair",
            [nameof(BalloonBgTitle)] = "O Watch Lock está rodando em segundo plano",
            [nameof(BalloonBgText)] = "Pressione {0} para bloquear. Clique com o botão direito no ícone da bandeja para sair.",
        },
        ["ru"] = new()
        {
            [nameof(AppName)] = "Watch Lock",
            [nameof(Tagline)] = "Экран остаётся видимым, а клавиатура и мышь блокируются. Ваша работа продолжается в фоне.",
            [nameof(SetupTitle)] = "Задайте пароль блокировки",
            [nameof(ChangeTitle)] = "Сменить пароль",
            [nameof(CurrentPassword)] = "Текущий пароль",
            [nameof(NewPassword)] = "Новый пароль",
            [nameof(Repeat)] = "Повторите",
            [nameof(Save)] = "Сохранить",
            [nameof(Cancel)] = "Отмена",
            [nameof(Lock)] = "Заблокировать",
            [nameof(HotkeyLabel)] = "Горячая клавиша: {0}",
            [nameof(HotkeyFailed)] = "Не удалось включить горячую клавишу. Используйте кнопку «Заблокировать».",
            [nameof(TestMode)] = "Тестовый режим: разблокировать автоматически через 60 секунд",
            [nameof(TestModeHint)] = "Оставьте включённым на первых попытках и отключите, когда освоитесь.",
            [nameof(ChangePassword)] = "Сменить пароль",
            [nameof(TrayHint)] = "Если закрыть это окно, приложение продолжит работать в области уведомлений рядом с часами.",
            [nameof(Language)] = "Язык",
            [nameof(PwTooShort)] = "Пароль должен содержать не менее 4 символов.",
            [nameof(PwMismatch)] = "Пароли не совпадают.",
            [nameof(PwWrongCurrent)] = "Текущий пароль неверен.",
            [nameof(PwChanged)] = "Пароль изменён.",
            [nameof(PwSaved)] = "Пароль сохранён. Всё готово.",
            [nameof(LockedBadge)] = "🔒 Заблокировано",
            [nameof(LockedBadgeSecondary)] = "🔒 Заблокировано: пароль вводится на основном экране",
            [nameof(ComputerLocked)] = "🔒 Компьютер заблокирован",
            [nameof(LockHelp)] = "Введите пароль и нажмите Enter. Также можно нажать Ctrl+Alt+Del и войти с паролем Windows.",
            [nameof(WrongPassword)] = "Неверный пароль ({0})",
            [nameof(TestCountdown)] = "Тестовый режим: разблокировка через {0} с",
            [nameof(LockedFor)] = "Заблокировано: {0}",
            [nameof(Unlocked)] = "Разблокировано.",
            [nameof(UnlockedWithAttempts)] = "Разблокировано. Неудачных попыток за время вашего отсутствия: {0}.",
            [nameof(TestExpired)] = "Тестовый режим: время вышло, блокировка снята автоматически.",
            [nameof(UnlockedViaWindows)] = "Разблокировано паролем Windows.",
            [nameof(TrayTipUnlocked)] = "Watch Lock: {0} — заблокировать",
            [nameof(TrayTipLocked)] = "Watch Lock: заблокировано",
            [nameof(MenuOpen)] = "Открыть Watch Lock",
            [nameof(MenuLock)] = "Заблокировать",
            [nameof(MenuExit)] = "Выход",
            [nameof(BalloonBgTitle)] = "Watch Lock работает в фоне",
            [nameof(BalloonBgText)] = "Нажмите {0}, чтобы заблокировать. Чтобы выйти, щёлкните правой кнопкой по значку в области уведомлений.",
        },
    };
}
