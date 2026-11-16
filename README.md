<p align="center">
  <img src="assets/logo/logo-128.png" width="96" alt="Watch Lock logo">
</p>

<h1 align="center">Watch Lock</h1>

<p align="center">
  <b>Your screen stays visible. Your keyboard and mouse stay locked.</b><br>
  A tiny Windows app that lets you walk away while long-running work keeps going.
</p>

<p align="center">
  <b>English</b> · <a href="README.tr.md">Türkçe</a>
</p>

---

## Why?

Something long is running on your PC: an AI coding agent (Claude Code, Codex, etc.) is working, a project is building, a big file is downloading. You want to step away for a coffee, but:

- **Win+L** locks the screen and hides it, so you can't check on your progress.
- **Leaving it unlocked** means someone (a kid, a cat, a coworker) can press a key, type into your terminal or close a window.

Watch Lock sits in between: **the screen stays exactly as it is, but nobody can touch the keyboard or mouse until your password is typed.** Your work keeps running in the background and the PC won't go to sleep.

## Features

- 🔒 **See-through lock:** A slightly dimmed, transparent layer covers the screen. You can still see everything underneath.
- ⌨️ **Keyboard and mouse lock:** Keys and clicks only reach the password box. Alt+Tab, the Windows key, Alt+F4 and Ctrl+Shift+Esc are blocked.
- 🛡️ **Falls back to the Windows lock on Ctrl+Alt+Del:** No app can block Ctrl+Alt+Del. When Watch Lock detects it, it immediately triggers the real Windows lock, so the Windows password is needed from then on.
- ☕ **Stays awake:** While locked, the PC won't sleep or start the screensaver.
- 🖥️ **Multi-monitor:** Every screen is locked; the password box is on the main one.
- ⚡ **Hotkey:** Lock instantly with `Ctrl+AltGr+F` (`Ctrl+Right Alt+F`).
- 🌍 **7 languages:** English, Türkçe, Deutsch, Español, Français, Português (Brasil), Русский.
- 🪶 **Very light:** Uses about **2.5 MB of RAM** while waiting in the system tray, and practically no CPU.
- 🙈 **No data collection:** It never connects to the internet. Details below.

## Use cases

- Watching an AI agent's progress from across the room while it works on a long task
- Stepping away while a long build, test run, render or upload is in progress
- Keeping a presentation, dashboard or live stream on screen without anyone touching it
- Leaving the PC on with kids or pets around
- Protecting your screen from fiddling during a short break at the office

## Download

Grab one of the two builds from the [**Releases**](../../releases/latest) page:

| File | Size | When to use |
|---|---|---|
| `WatchLock-<version>-win-x64.exe` | ~60 MB | **Recommended.** Runs without installing anything. |
| `WatchLock-<version>-win-x64-dotnet.exe` | ~270 KB | If you already have the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0). |

There's no installer: put the exe in any folder and run it.

> **"Windows protected your PC" warning:** The app isn't code-signed (signing certificates cost money), so Windows SmartScreen may warn you. Click **More info → Run anyway**. If you'd rather be sure, the full source is in this repo and you can build it yourself (see below).

## How to use

1. **First launch:** Set a lock password (at least 4 characters). It can be different from your Windows password.
2. **Lock:** Click **Lock** in the window, or press **`Ctrl+AltGr+F`** from anywhere.
3. **Unlock:** Type your password and press **Enter**.
4. **Leave it in the background:** Closing the window doesn't quit the app; it moves to the **system tray** next to the clock.
   - Left-click the icon: opens the window.
   - Right-click: Open / Lock / Exit.
5. **Language:** Pick it from the box in the top-right corner of the window.

### Test mode

On first launch, **Test mode** is checked: the lock **opens by itself after 60 seconds**. It's there so you can't lock yourself out while getting to know the app. Once you've seen how it works, uncheck it, or the lock will keep opening after one minute.

## I forgot my password

Don't panic. As long as you can sign in to Windows, there's always a way out:

1. Press **Ctrl+Alt+Del**. Watch Lock immediately switches to the Windows lock screen.
2. Sign in with your **Windows password or PIN**.
3. Once you're back in Windows, Watch Lock **unlocks automatically**. The logic: whoever knows the Windows password already owns the PC.

**To reset the password:** Right-click the tray icon and choose **Exit**. Then delete this file:

```
%APPDATA%\WatchLock\password.json
```

The next time you open the app, it asks for a new password. (If you know your password, use the **Change password** link in the window instead.)

## Privacy and data collection

**Watch Lock collects no data and sends nothing anywhere.**

- **No internet:** There is not a single network call in the code. No analytics, no ads, no update checks, no telemetry.
- **Your password is never stored in plain text:** Only a one-way hash is kept (PBKDF2-SHA256, 200,000 iterations, random salt). The password can't be recovered from it.
- **Files it keeps on your PC** (all in `%APPDATA%\WatchLock\`):
  - `password.json`: password hash and salt
  - `settings.json`: language, whether test mode is on, whether the tray tip was shown
- **About keyboard monitoring:** To work, the app listens to the keyboard at a low level (the same Windows feature keyloggers use). While unlocked, it only watches for the `Ctrl+AltGr+F` combination; while locked, it blocks forbidden keys. **Keystrokes are never written down, stored or sent anywhere.** If you want to verify this, the relevant code is a few dozen lines in [`TrayIcon.cs`](src/WatchLock/TrayIcon.cs) and [`LockSession.cs`](src/WatchLock/LockSession.cs).
- **No registry entries, no autostart.** To uninstall, delete the exe and the `%APPDATA%\WatchLock` folder.

## Security: what it does and doesn't do

Watch Lock is **a practical lock, not a security product.** Its goal is to stop someone who sits at your PC from messing things up "by accident or out of curiosity". Against a determined person with physical access, use the Windows lock (Win+L) and disk encryption (BitLocker).

Things to know:

- **Ctrl+Alt+Del can't be blocked:** Windows does this on purpose. Watch Lock switches to the real Windows lock in that case, so the lock isn't broken, it's handed over to Windows. The screen is no longer visible at that point, but your work keeps running.
- **Power button:** No software can stop someone from pressing the physical power button.
- **Admin (UAC) prompts:** If a program asks for admin permission while locked, Windows switches to the secure desktop. Watch Lock treats this like Ctrl+Alt+Del and switches to the Windows lock.
- **Wrong password attempts:** The wait after each wrong attempt grows (up to 2 seconds), but there's no attempt limit yet. Pick a somewhat longer password rather than a short PIN.
- **Antivirus warnings:** Because it listens to the keyboard at a low level, some antivirus programs may find the app suspicious. All the code is open; feel free to build it yourself.

## Performance

| State | RAM (Task Manager) | CPU |
|---|---|---|
| Waiting in the tray | ~2.5 MB | ~0% |
| Window open | ~25-80 MB* | ~0% |

\* Higher for the standalone (60 MB) build. A few seconds after the window or lock screen closes, the app releases memory and drops back to ~2-3 MB.

How: when the window closes it's fully released from memory, the garbage collector runs, and unused memory is handed back to Windows. The tray icon talks to the Windows API directly instead of loading a heavy UI library.

## Translations

All texts live in one file: [`Strings.cs`](src/WatchLock/Strings.cs). To add a language, add it to the `Languages` list and copy the English table with your translations. Missing texts fall back to English. Pull requests and fixes from native speakers are welcome; the non-English translations besides Turkish haven't been reviewed by native speakers yet.

## Building from source

Requirements: Windows 10/11, [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
git clone https://github.com/GetAwayWithIt/watch-lock.git
cd watch-lock
dotnet run --project src/WatchLock
```

To produce a single exe:

```powershell
# Small (requires .NET 10 Desktop Runtime)
dotnet publish src/WatchLock -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=none -o publish

# Standalone (requires nothing)
dotnet publish src/WatchLock -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=none -o publish/tam
```

The logo is drawn in code: `tools/draw-icon.ps1` → `assets/logo/` and `src/WatchLock/app.ico`.

## Upgrading from v0.2 ("Ekran Kilidi")

Watch Lock was called "Ekran Kilidi" up to v0.2. On first launch, the new version moves your existing password over from `%APPDATA%\EkranKilidi` automatically and keeps the interface in Turkish. You can delete the old `EkranKilidi.exe`.

## Roadmap

- [ ] Switch to the Windows lock after a number of wrong passwords
- [ ] Log of wrong attempts (with times)
- [ ] Start with Windows option
- [ ] Notification when an agent's job is done
- [ ] More languages

## License

[MIT](LICENSE): use it, change it, share it.
