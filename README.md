# AdminDiskApp 🧹

![GitHub Release](https://img.shields.io/github/v/release/jbasta74/AdminDiskApp?color=blue&label=aktuální%20verze)
![.NET Version](https://img.shields.io/badge/.NET-10.0-blueviolet)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

**AdminDiskApp** je lehký, ale výkonný nástroj pro automatizovanou údržbu disků a čištění starých souborů (záloh, logů, tempů) na Windows serverech. Byl navržen s důrazem na stabilitu, minimální nároky na systém a snadnou přenositelnost.

## Hlavní funkce
- ✅ **Automatizované čištění:** Možnost nastavit konkrétní čas (např. 03:00) pro každodenní úklid.
- ⚙️ **Windows Integrace:** Snadná registrace do Plánovače úloh přímo z aplikace.
- 📊 **Statistiky:** Přehled o tom, kolik MB/GB místa bylo při posledním úklidu ušetřeno.
- 📝 **Inteligentní logování:** Ukládání historie do souboru `cleanup.log` s automatickou rotací (soubor nikdy nepřesáhne 2 MB).
- 🚀 **Native Single-File:** Aplikace je distribuována jako jeden `.exe` soubor, který v sobě nese vše potřebné (není nutná instalace .NETu na cílovém serveru).

## Jak začít
1. Stáhněte si nejnovější verzi z [sekce Releases](https://github.com/jbasta74/AdminDiskApp/releases).
2. Spusťte `AdminDiskApp.exe` jako správce (vyžadováno pro zápis do Plánovače úloh).
3. Pomocí tlačítka **➕ Přidat složku** definujte cesty, které se mají promazávat.
4. Nastavte parametry (filtr souborů, stáří souborů ve dnech, podadresáře).
5. Klikněte na **Zaregistrovat do Plánovače úloh Windows**.

## Technické detaily
- **Jazyk:** C# / WPF
- **Runtime:** .NET 10 (Self-contained)
- **Architektura:** x64
- **Logování:** Implementována rotace `cleanup.log` -> `cleanup.log.old`.

## Náhled aplikace
<img width="739" height="558" alt="image" src="https://github.com/user-attachments/assets/f72eb61c-0210-4c3a-b361-db4bdf57aa71" />



## Autor
- **Jiří Bašta** (jbasta74) - [GitHub Profil](https://github.com/jbasta74)

---
© 2026 - AdminDiskApp
