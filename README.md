# Installing DoughTrack on another computer

DoughTrack needs two things on the target PC: **MySQL Server** (the database) and the **DoughTrack program**.

## Part 1. Before you start (on your own PC)

1. Open `.gitignore` in the solution folder. Delete any line that looks like spaced-out letters (`D o u g h T r a c k ...`). Make sure these lines exist, each on its own line: `DoughTrack/appsettings.json`, `bin/`, `obj/`, `.vs/`.
2. In the Terminal: `git ls-files`. `appsettings.json` must NOT be listed. `appsettings.example.json` and the `Database/Setup` files must be listed.
3. Delete the leftover `Form1.vb`, `Form1.Designer.vb`, `Form1.resx` from the project, then Rebuild.
4. Commit and push to GitHub.
5. **Never put a real password in any file you commit.**

## Part 2. Install MySQL on the other PC (once)

1. Install **MySQL Server 8.0.16 or newer** (MySQL Installer for Windows, choose "Server only"; MySQL Workbench is also handy). Keep port **3306**, run it as a Windows Service, and write down the **root** password you choose.
2. Open **MySQL Workbench** -> click the local connection -> enter the root password.

## Part 3. Create the database

Run these three files from `Database/Setup`, in order. For each: **File -> Open SQL Script...**, pick the file, press **Ctrl+Shift+Enter**, and check the Output panel has no red errors.

1. `01_InitialSetup.sql` creates `doughtrack_db` and the 9 empty tables. It is safe to run twice and never deletes anything.
2. `02_CreateAppUser.sql`: first replace `CHOOSE_A_STRONG_PASSWORD` with a password you choose, then run it. **Do not save or commit that edit.** It creates the login `doughtrack_app`, which can only read and change data inside `doughtrack_db`.
3. `03_VerifyInstall.sql` is a read-only check. Expected: `tables_found` = 9, and every count = 0.

## Part 4. Install the program (pick ONE option)

**Option A. Build it on the other PC.** Install Git and Visual Studio 2026 Community with ".NET desktop development". Run `git clone <your repo URL> C:\DoughTrack`, open `DoughTrack.slnx`, press F5.

**Option B. Copy a ready-to-run folder (no Visual Studio needed).** On YOUR PC, in the solution folder:

```
dotnet publish DoughTrack\DoughTrack.vbproj -c Release -r win-x64 --self-contained true -o C:\DoughTrackApp
```

Then delete `C:\DoughTrackApp\appsettings.json` (it contains YOUR development password). Copy the whole `DoughTrackApp` folder to the other PC, for example to `C:\DoughTrack`. Do not put it in `Program Files`: menu images are saved in a `MenuImages` folder next to the program and need a writable location.

## Part 5. Connect the program to the database

1. In the program's folder (next to `DoughTrack.exe`), copy `appsettings.example.json` to a new file named exactly `appsettings.json`. In File Explorer turn on **View -> Show -> File name extensions** so it is not saved as `appsettings.json.txt`.
2. Edit it. Set `"User": "doughtrack_app"` and `"Password"` to the password from Part 3. Keep `"Name": "doughtrack_db"`, `"Server": "localhost"`, `"Port": 3306`. Save as UTF-8.

## Part 6. First launch

Start DoughTrack. Because the database has no users, a **First-time setup** window appears. Type the Admin username, full name, and a password of 8 or more characters, then click **Create Admin**. Log in with it. This window never appears again once an account exists.

## Troubleshooting

| What you see | Cause and fix |
|---|---|
| "Unable to reach the database..." | MySQL service not running (Services -> MySQL -> Start), or `appsettings.json` is missing or has the wrong user/password. |
| App closes at start or says it cannot find appsettings.json | The file is missing or misnamed (`appsettings.json.txt`). |
| "Access denied" in the Visual Studio Output window | Password in `appsettings.json` differs from the one used in `02_CreateAppUser.sql`. |
| Script error about CHECK | MySQL is older than 8.0.16. Install a newer version. |
| First-time window does not appear but login fails | An account already exists. Use that account. |
