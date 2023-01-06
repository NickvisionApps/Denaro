# Money
<img src="NickvisionMoney.Shared/Resources/org.nickvision.money.svg" width="100" height="100"/>

 **A personal finance manager**
 
 [![Translation status](https://hosted.weblate.org/widgets/nickvision-money/-/app/svg-badge.svg)](https://hosted.weblate.org/engage/nickvision-money/)

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

# Translating
Everyone is welcome to translate this app into their native or known languages, so that the application is accessible to everyone.

## Via Weblate
Money is available to translate on [Weblate](https://hosted.weblate.org/engage/nickvision-money/)!

## Manually
To start translating the app, fork the repository and clone it locally.

In the `NickvisionMoney.Shared/Resources` folder you will see a file called `String.resx`. This is a C# resource file that contains all the strings for the application. Simply copy that file and rename it `String.<lang-code>.resx`. For example, if I'm creating an Italian translation, the copied file would be called `Strings.it.resx`. Once you have your copied file, simply replace each `<value>` block of each `<data>` string block with your language's appropriate translation.

To check your translation file, make sure your system is in the locale of the language you are translating and run the app. You should see your translated strings!

Once all changes to your translated file are made, make sure the file is in the path `NickvisionMoney.Shared/Resources/String.<lang-code>.resx`, commit these changes and create a pull request to the project.

# GNOME Screenshots
![GNOMELight](NickvisionMoney.GNOME/Screenshots/OpenAccount.png)
![GNOMEDark](NickvisionMoney.GNOME/Screenshots/OpenAccountDark.png)
<p align='center'><img src='NickvisionMoney.GNOME/Screenshots/Transaction.png' alt='GNOMETransaction' width='350px'><img src='NickvisionMoney.GNOME/Screenshots/CompactMode.png' alt='GNOMECompactMode' width='350px'></p>

# WinUI Screenshots
![HomePage](NickvisionMoney.WinUI/Screenshots/HomePage.png)
![OpenAccount](NickvisionMoney.WinUI/Screenshots/OpenAccount.png)
![DarkMode](NickvisionMoney.WinUI/Screenshots/DarkMode.png)
![TransactionDialog](NickvisionMoney.WinUI/Screenshots/TransactionDialog.png)

# GNOME Theming

[![Please do not theme this app](https://stopthemingmy.app/badge.svg)](https://stopthemingmy.app) 

The Linux version of this app is designed for GNOME and optimized for the default Adwaita theme. If you customized your system look, it can negatively affect Money. However, in case of a breakage, we provide a way to customize some elements using CSS so you can make it look as you need. The CSS code should be added to `~/.config/gtk-4.0/gtk.css`. An example (not really pleasant-looking, it's just to show what modifications you can apply):

```
.denaro-total {
    background-color: @warning_color;
    color: #fff;
}

.denaro-income {
    color: @purple_2;
}

.denaro-expense {
    background: linear-gradient(to right, #000, @blue_4);
    color: #fff;
}

@define-color denaro_calendar_today_bg_color @blue_5;
@define-color denaro_calendar_today_fg_color #ff0000;
@define-color denaro_calendar_marked_day_fg_color @success_color;
@define-color denaro_calendar_selected_day_bg_color @card_bg_color;
@define-color denaro_calendar_selected_day_fg_color #55cc10;
@define-color denaro_calendar_other_month_fg_color @dark_5;
```

# Dependencies
- [.NET 7](https://dotnet.microsoft.com/en-us/)

# Special Thanks
- [daudix-UFO](https://github.com/daudix-UFO) for our application icons

# Code of Conduct
This project follows the [GNOME Code of Conduct](https://wiki.gnome.org/Foundation/CodeOfConduct).
