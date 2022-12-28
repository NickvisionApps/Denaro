# Money
<img src="NickvisionMoney.Shared/Resources/org.nickvision.money.svg" width="100" height="100"/>

 **A personal finance manager**

# Features
- A cross-platform C# application
  - Windows UI in Windows App SDK (WinUI 3)
  - GNOME UI in gir.core (Gtk4/Libadwaita)
- Manage multiple accounts at a time, with a familiar tab interface
- Easily filter transactions by type, group, or date
- Easily repeat transactions, such as bills that occur every month
- Transfer money from one account to another
- Export an account as a CSV file and import a CSV, OFX or QIF file to bulk add transactions to an account

# Installation

<a href='https://flathub.org/apps/details/org.nickvision.money'><img width='140' alt='Download on Flathub' src='https://flathub.org/assets/badges/flathub-badge-en.png'/></a>

# Chat
<a href='https://matrix.to/#/#nickvision:matrix.org'><img width='140' alt='Join our room' src='https://user-images.githubusercontent.com/17648453/196094077-c896527d-af6d-4b43-a5d8-e34a00ffd8f6.png'/></a>

# Screenshots
<!--![WinUILight](https://user-images.githubusercontent.com/17648453/207794202-2ec536e0-106c-451d-b380-2091c2db96bf.png)
![WinUIDark](https://user-images.githubusercontent.com/17648453/207794094-f6e371b0-9c0e-4356-b9ea-7cdca7eb3b05.png)-->
![GNOMELight](NickvisionMoney.GNOME/Screenshots/OpenAccount.png)
![GNOMEDark](NickvisionMoney.GNOME/Screenshots/OpenAccountDark.png)
<p align='center'><img src='NickvisionMoney.GNOME/Screenshots/Transaction.png' alt='GNOMETransaction' width='350px'><img src='NickvisionMoney.GNOME/Screenshots/CompactMode.png' alt='GNOMECompactMode' width='350px'></p>

# GNOME Theming

[![Please do not theme this app](https://stopthemingmy.app/badge.svg)](https://stopthemingmy.app) 

The Linux version of this app is designed for GNOME and optimized for the default Adwaita theme. If you customized your system look, it can negatively affect Money. Hovewer, in case of a breakage, we provide a way to customize some elements using CSS so you can make it look as you need. The CSS code should be in `~/.var/app/org.nickvision.money/config/gtk-4.0/gtk.css` if you installed the app using Flatpak or in `~/.config/gtk-4.0/gtk.css` otherwise. An example:

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

To start translating the app, fork the repository and clone it locally.

In the `NickvisionMoney.Shared/Resources` folder you will see a file called `String.resx`. This is a C# resource file that contains all the strings for the application. Simply copy that file and rename it `String.<lang-code>.resx`. For example, if I'm creating an Italian translation, the copied file would be called `Strings.it.resx`. Once you have your copied file, simply replace each `<value>` block of each `<data>` string block with your language's appropriate translation.

To check your translation file, make sure your system is in the locale of the language you are translating and run the app. You should see your translated strings!

Once all changes to your translated file are made, make sure the file is in the path `NickvisionMoney.Shared/Resources/String.<lang-code>.resx` and commit these changes.

Even if you're running Windows, we ask you to also translate metadata for GNOME (Linux) version of the app. There are 2 places that require changes when a new translation is added:
- `NickvisionMoney.GNOME/org.nickvision.money.desktop`: `Comment[lang-code]` line
- `NickvisionMoney.GNOME/org.nickvision.money.metainfo.xml`: `<description>` section

When you're done, create a pull request to the project.

# Dependencies
- [.NET 7](https://dotnet.microsoft.com/en-us/)

# Special Thanks
- [daudix-UFO](https://github.com/daudix-UFO) for our application icons

# Code of Conduct

This project follows the [GNOME Code of Conduct](https://wiki.gnome.org/Foundation/CodeOfConduct).
