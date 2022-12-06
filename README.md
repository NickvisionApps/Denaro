# Money
<img src="src/resources/org.nickvision.money.svg" width="100" height="100"/>

 **A personal finance manager**

# Features
- Manage multiple accounts at a time, with a familiar tab interface
- Easily filter transactions by type, group, or date
- Easily repeat transactions, such as bills that occur every month
- Transfer money from one account to another
- Export an account as a CSV file and import a CSV file to bulk add transactions to an account

# Installation
<a href='https://flathub.org/apps/details/org.nickvision.money'><img width='140' alt='Download on Flathub' src='https://flathub.org/assets/badges/flathub-badge-en.png'/></a>

# Chat
<a href='https://matrix.to/#/#nickvision:matrix.org'><img width='140' alt='Join our room' src='https://user-images.githubusercontent.com/17648453/196094077-c896527d-af6d-4b43-a5d8-e34a00ffd8f6.png'/></a>

# Screenshots
![MainWindow](https://user-images.githubusercontent.com/17648453/202083015-1a48ec33-84b9-476d-ab66-db9ff507d692.png)
![OpenAccount](https://user-images.githubusercontent.com/17648453/204042093-d01d5aaa-3a82-49fd-b579-afd48d20936e.png)
![DarkMode](https://user-images.githubusercontent.com/17648453/204042100-a56c28e2-2a83-4c12-9629-acf8079fd89a.png)
![TransactionDialog](https://user-images.githubusercontent.com/17648453/204042104-6f0e3019-c476-40aa-8a2b-2a5e539f7b27.png)

[![Please do not theme this app](https://stopthemingmy.app/badge.svg)](https://stopthemingmy.app) 
This app is designed for GNOME and optimized for the default Adwaita theme. If you customized your system look, it can negatively affect Money. Hovewer, in case of a breakage, we provide a way to customize some elements using CSS so you can make it look as you need. The CSS code should be in `~/.var/app/org.nickvision.money/config/gtk-4.0/gtk.css` if you installed the app using Flatpak or in `~/.config/gtk-4.0/gtk.css` otherwise. An example:

```
.money-total {
	background-color: @warning_color;
	color: #fff;
}

.money-income {
  color: @purple_2;
}

.money-expense {
  background: linear-gradient(to right, #000, @blue_4);
  color: #fff;
}
```

# Translating
Everyone is welcome to translate this app into their native or known languages, so that the application is accessible to everyone.

To translate the app, fork the repository and clone it locally. Make sure that `meson` is installed. Run the commands in your shell while in the directory of repository:
```bash
meson build
cd build
meson compile org.nickvision.money-pot
```
Or, if you are using GNOME Builder, build the app and then run in the Builder's terminal:
```bash
flatpak run --command=sh org.gnome.Builder
cd _build
meson compile org.nickvision.money-pot
```
This would generate a `NickvisionMoney/po/org.nickvision.money.pot` file, now you can use this file to translate the strings into your target language. You may use [Gtranslator](https://flathub.org/apps/details/org.gnome.Gtranslator) or [Poedit](https://poedit.net) if you do not know how to translate manually in text itself. After translating (either through tools or directly in text editor), make sure to include the required metadata on the top of translation file (see existing files in `NickvisionMoney/po/` directory.)

One particular thing you should keep in mind is that some strings in this project are bifurcated into multiple strings to cater to responsiveness of the application, like:
```
msgid ""
"If checked, the currency symbol will be displayed on the right of a monetary "
"value."
```
You should use the same format for translated strings as well. But, because all languages do not have the same sentence structure, you may not need to follow this word-by-word, rather you should bifurcate the string in about the same ratio. (For examples, look into translations of languages which do not have a English-like structure in `NickvisionMoney/po/`)

Put your translated file in `NickvisionMoney/po` directory in format `<LANG>.po` where `<LANG>` is the language code.

Put the language code of your language in `NickvisionMoney/po/LINGUAS` (this file, as a convention, should remain in alphabetical order.)

Add information in `NickvisionMoney/po/CREDITS.json` so your name will appear in the app's About dialog:
```
"Jango Fett": {
    "lang": "Mandalorian",
    "email": "jango@galaxyfarfar.away"
}
```
If you made multiple translations, use an array to list all languages:
```
"C-3PO": {
    "lang": ["Ewokese", "Wookieespeak", "Jawaese"],
    "url": "https://free.droids"
}
```

To test your translation in GNOME Builder, press Ctrl+Alt+T to open a terminal inside the app's environment and then run:
```
LC_ALL=<LOCALE> /app/bin/org.nickvision.money
```
where `<LOCALE>` is your locale (e.g. `it_IT.UTF-8`.)

Commit these changes, and then create a pull request to the project.

As more strings may be added in the application in future, the following command needs to be ran to update all the `.po` files, which would add new strings to be translated without altering the already translated strings. But, because running this command would do this for all the languages, generally a maintainer would do that.

```bash
meson compile org.nickvision.money-update-po
```

The upper command needs to be run in `build` directory generated by `meson`.

# Dependencies
- [C++20](https://en.cppreference.com/w/cpp/20)
- [GTK 4](https://www.gtk.org/)
- [libadwaita](https://gnome.pages.gitlab.gnome.org/libadwaita/)
- [jsoncpp](https://github.com/open-source-parsers/jsoncpp)
- [sqlitecpp](https://github.com/SRombauts/SQLiteCpp)
- [boost](https://www.boost.org/)

# Special Thanks
- [daudix-UFO](https://github.com/daudix-UFO) for our application icons
