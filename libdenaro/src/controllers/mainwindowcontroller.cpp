﻿#include "controllers/mainwindowcontroller.h"
#include <ctime>
#include <locale>
#include <sstream>
#include <thread>
#include <libnick/app/aura.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/localization/gettext.h>
#include "models/account.h"
#include "models/configuration.h"
#ifdef _WIN32
#include <windows.h>
#endif

using namespace Nickvision::Money::Shared::Models;
using namespace Nickvision::App;
using namespace Nickvision::Events;
using namespace Nickvision::Notifications;
using namespace Nickvision::Update;

namespace Nickvision::Money::Shared::Controllers
{
    MainWindowController::MainWindowController()
        : m_started{ false }
    {
#ifdef DEBUG
        Aura::getActive().init("org.nickvision.money", "Nickvision Denaro", "Denaro", Logging::LogLevel::Debug);
#else
        Aura::getActive().init("org.nickvision.money", "Nickvision Denaro", "Denaro", Logging::LogLevel::Info);
#endif
        AppInfo& appInfo{ Aura::getActive().getAppInfo() };
        appInfo.setVersion({ "2024.5.0-next" });
        appInfo.setShortName(_("Denaro"));
        appInfo.setDescription(_("Manage your personal finances"));
        appInfo.setSourceRepo("https://github.com/NickvisionApps/Denaro");
        appInfo.setIssueTracker("https://github.com/NickvisionApps/Denaro/issues/new");
        appInfo.setSupportUrl("https://github.com/NickvisionApps/Denaro/discussions");
        appInfo.getExtraLinks()[_("Matrix Chat")] = "https://matrix.to/#/#nickvision:matrix.org";
        appInfo.getDevelopers()["Nicholas Logozzo"] = "https://github.com/nlogozzo";
        appInfo.getDevelopers()[_("Contributors on GitHub")] = "https://github.com/NickvisionApps/Denaro/graphs/contributors";
        appInfo.getDesigners()["Nicholas Logozzo"] = "https://github.com/nlogozzo";
        appInfo.getDesigners()[_("Fyodor Sobolev")] = "https://github.com/fsobolev";
        appInfo.getDesigners()["DaPigGuy"] = "https://github.com/DaPigGuy";
        appInfo.getDesigners()["JoseBritto"] = "https://github.com/JoseBritto";
        appInfo.getArtists()[_("David Lapshin")] = "https://github.com/daudix";
        appInfo.getArtists()["Tobias Bernard"] = "https://github.com/bertob";
        appInfo.setTranslatorCredits(_("translator-credits"));
    }

    AppInfo& MainWindowController::getAppInfo() const
    {
        return Aura::getActive().getAppInfo();
    }

    bool MainWindowController::isDevVersion() const
    {
        return Aura::getActive().getAppInfo().getVersion().getVersionType() == VersionType::Preview;
    }

    Theme MainWindowController::getTheme() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getTheme();
    }

    Event<EventArgs>& MainWindowController::configurationSaved()
    {
        return Aura::getActive().getConfig<Configuration>("config").saved();
    }

    Event<NotificationSentEventArgs>& MainWindowController::notificationSent()
    {
        return m_notificationSent;
    }

    Event<ShellNotificationSentEventArgs>& MainWindowController::shellNotificationSent()
    {
        return m_shellNotificationSent;
    }

    Event<ParamEventArgs<std::vector<RecentAccount>>>& MainWindowController::recentAccountsChanged()
    {
        return m_recentAccountsChanged;
    }

    Event<ParamEventArgs<const std::shared_ptr<AccountViewController>&>>& MainWindowController::accountAdded()
    {
        return m_accountAdded;
    }

    std::string MainWindowController::getDebugInformation(const std::string& extraInformation) const
    {
        std::stringstream builder;
        //App ID and Ver
        builder << Aura::getActive().getAppInfo().getId();
#ifdef _WIN32
        builder << ".winui" << std::endl;
#elif defined(__linux__)
        builder << ".gnome" << std::endl;
#endif
        builder << Aura::getActive().getAppInfo().getVersion().toString() << std::endl << std::endl;
        //System Information
        if(Aura::getActive().isRunningViaFlatpak())
        {
            builder << "Running under Flatpak" << std::endl;
        }
        else if(Aura::getActive().isRunningViaSnap())
        {
            builder << "Running under Snap" << std::endl;
        }
        else
        {
            builder << "Running locally" << std::endl;
        }
#ifdef _WIN32
        LCID lcid = GetThreadLocale();
        wchar_t name[LOCALE_NAME_MAX_LENGTH];
        if(LCIDToLocaleName(lcid, name, LOCALE_NAME_MAX_LENGTH, 0) > 0)
        {
            builder << StringHelpers::toString(name) << std::endl;
        }
#elif defined(__linux__)
        try
        {
            builder << std::locale("").name() << std::endl;
        }
        catch(...)
        {
            builder << "Locale not set" << std::endl;
        }
#endif
        //Gnuplot
        builder << std::endl << Aura::getActive().sysExec("gnuplot --version");
        //Extra Information
        if (!extraInformation.empty())
        {
            builder << std::endl << extraInformation << std::endl;
        }
        return builder.str();
    }

    std::string MainWindowController::getGreeting() const
    {
        std::time_t now{ std::time(nullptr) };
        std::tm* cal{ std::localtime(&now) };
        if (cal->tm_hour >= 0 && cal->tm_hour < 6)
        {
            return _p("Night", "Good Morning!");
        }
        else if (cal->tm_hour < 12)
        {
            return _p("Morning", "Good Morning!");
        }
        else if (cal->tm_hour < 18)
        {
            return _("Good Afternoon!");
        }
        else if (cal->tm_hour < 24)
        {
            return _("Good Evening!");
        }
        return _("Good Day!");
    }

    std::shared_ptr<PreferencesViewController> MainWindowController::createPreferencesViewController() const
    {
        return std::make_shared<PreferencesViewController>();
    }

    std::shared_ptr<NewAccountDialogController> MainWindowController::createNewAccountDialogController() const
    {
        return std::make_shared<NewAccountDialogController>();
    }

    std::shared_ptr<DashboardViewController> MainWindowController::createDashboardViewController() const
    {
        std::vector<std::shared_ptr<AccountViewController>> openAccounts;
        for(const std::pair<const std::filesystem::path, std::shared_ptr<AccountViewController>>& pair : m_accountViewControllers)
        {
            openAccounts.push_back(pair.second);
        }
        return std::make_shared<DashboardViewController>(openAccounts);
    }

    const std::shared_ptr<AccountViewController>& MainWindowController::getAccountViewController(const std::filesystem::path& path) const
    {
        return m_accountViewControllers.at(path);
    }

    void MainWindowController::startup()
    {
        if (!m_started)
        {
#ifdef _WIN32
            try
            {
                m_updater = std::make_shared<Updater>(Aura::getActive().getAppInfo().getSourceRepo());
            }
            catch(...)
            {
                m_updater = nullptr;
            }
            if (Aura::getActive().getConfig<Configuration>("config").getAutomaticallyCheckForUpdates())
            {
                checkForUpdates();
            }
#endif
            m_recentAccountsChanged.invoke({ Aura::getActive().getConfig<Configuration>("config").getRecentAccounts() });
            m_started = true;
            Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "MainWindow started.");
        }
    }

    void MainWindowController::checkForUpdates()
    {
        if(m_updater)
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Checking for updates...");
            std::thread worker{ [&]()
            {
                Version latest{ m_updater->fetchCurrentStableVersion() };
                if (!latest.empty())
                {
                    if (latest > Aura::getActive().getAppInfo().getVersion())
                    {
                        Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Update found: " + latest.toString());
                        m_notificationSent.invoke({ _("New update available"), NotificationSeverity::Success, "update" });
                    }
                    else
                    {
                        Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "No updates found.");
                    }
                }
                else
                {
                    Aura::getActive().getLogger().log(Logging::LogLevel::Warning, "Unable to fetch latest app version.");
                }
            } };
            worker.detach();
        }
    }

#ifdef _WIN32
    void MainWindowController::windowsUpdate()
    {
        if(m_updater)
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Fetching Windows app update...");
            std::thread worker{ [&]()
            {
                bool res{ m_updater->windowsUpdate(VersionType::Stable) };
                if (!res)
                {
                    Aura::getActive().getLogger().log(Logging::LogLevel::Error, "Unbale to fetch Windows app update.");
                    m_notificationSent.invoke({ _("Unable to download and install update"), NotificationSeverity::Error, "error" });
                }
            } };
            worker.detach();
        }
    }

    void MainWindowController::connectTaskbar(HWND hwnd)
    {
        if(m_taskbar.connect(hwnd))
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Connected to Windows taskbar.");
        }
        else
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Error, "Unable to connect to Windows taskbar.");
        }
    }
#elif defined(__linux__)
    void MainWindowController::connectTaskbar(const std::string& desktopFile)
    {
        if(m_taskbar.connect(desktopFile))
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Connected to Linux taskbar.");
        }
        else
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Error, "Unable to connect to Linux taskbar.");
        }
    }
#endif

    bool MainWindowController::isAccountPasswordProtected(const std::filesystem::path& path) const
    {
        if(StringHelpers::toLower(path.extension().string()) != ".nmoney")
        {
            return false;
        }
        Account a{ path };
        return a.isEncrypted();
    }

    bool MainWindowController::hasOpenAccounts() const
    {
        return !m_accountViewControllers.empty();
    }

    void MainWindowController::newAccount(const std::shared_ptr<NewAccountDialogController>& newAccountDialogController)
    {
        if(std::filesystem::exists(newAccountDialogController->getFilePath()))
        {
            //Check if overwrite is allowed
            if(!newAccountDialogController->getOverwriteExisting())
            {
                Aura::getActive().getLogger().log(Logging::LogLevel::Error, "Account exists and overwrite is disabled. (" + newAccountDialogController->getFilePath().string() + ")");
                m_notificationSent.invoke({ _("This account already exists."), NotificationSeverity::Error });
                return;
            }
            //Check if the account is open in the app (cannot delete if open)
            if(m_accountViewControllers.contains(newAccountDialogController->getFilePath()))
            {
                Aura::getActive().getLogger().log(Logging::LogLevel::Error, "Cannot overwrite. Account is opened. (" + newAccountDialogController->getFilePath().string() + ")");
                m_notificationSent.invoke({ _("This account cannot be overwritten."), NotificationSeverity::Error });
                return;
            }
            Aura::getActive().getLogger().log(Logging::LogLevel::Warning, "Overwriting existing account file. (" + newAccountDialogController->getFilePath().string() + ")");
            std::filesystem::remove(newAccountDialogController->getFilePath());
        }
        Configuration& config{ Aura::getActive().getConfig<Configuration>("config") };
        //Create the new account
        std::unique_ptr<Account> account{ std::make_unique<Account>(newAccountDialogController->getFilePath()) };
        account->login("");
        account->setMetadata(newAccountDialogController->getMetadata());
        account->importFromFile(newAccountDialogController->getImportFile(), config.getTransactionDefaultColor(), config.getGroupDefaultColor());
        account->changePassword(newAccountDialogController->getPassword());
        account.reset();
        //Create the controller and open the account
        std::shared_ptr<AccountViewController> controller{ nullptr };
        try
        {
            controller = std::make_shared<AccountViewController>(newAccountDialogController->getFilePath(), newAccountDialogController->getPassword());
        }
        catch(const std::exception& e)
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Error, std::string(e.what()) + ". (" + newAccountDialogController->getFilePath().string() + ")");
            m_notificationSent.invoke({ e.what(), NotificationSeverity::Error });
        }
        if(controller)
        {
            m_accountViewControllers[newAccountDialogController->getFilePath()] = controller;
            config.addRecentAccount(controller->toRecentAccount());
            config.save();
            Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Config saved.");
            m_recentAccountsChanged.invoke({ config.getRecentAccounts() });
            m_accountAdded.invoke({ m_accountViewControllers[newAccountDialogController->getFilePath()] });
            Aura::getActive().getLogger().log(Logging::LogLevel::Info, "Account created. (" + newAccountDialogController->getFilePath().string() + ")");
        }
    }

    void MainWindowController::openAccount(std::filesystem::path path, const std::string& password)
    {
        //Check if the file is a Denaro account file
        if(StringHelpers::toLower(path.extension().string()) != ".nmoney")
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Error, "Invalid file extension. (" + path.string() + ")");
            m_notificationSent.invoke({ _("The file is not a Denaro account file."), NotificationSeverity::Error });
        }
        //Check if the account is already open
        else if(m_accountViewControllers.contains(path))
        {
            Aura::getActive().getLogger().log(Logging::LogLevel::Warning, "Account already open. (" + path.string() + ")");
            m_notificationSent.invoke({ _("The account is already open."), NotificationSeverity::Warning });
        }
        //Create the controller and open the account
        else
        {
            std::shared_ptr<AccountViewController> controller{ nullptr };
            try
            {
                controller = std::make_shared<AccountViewController>(path, password);
            }
            catch(const std::exception& e)
            {
                Aura::getActive().getLogger().log(Logging::LogLevel::Error, std::string(e.what()) + ". (" + path.string() + ")");
                m_notificationSent.invoke({ e.what(), NotificationSeverity::Error });
            }
            if(controller)
            {
                m_accountViewControllers[path] = controller;
                Configuration& config{ Aura::getActive().getConfig<Configuration>("config") };
                config.addRecentAccount(controller->toRecentAccount());
                config.save();
                Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Config saved.");
                m_recentAccountsChanged.invoke({ config.getRecentAccounts() });
                m_accountAdded.invoke({ m_accountViewControllers[path] });
                Aura::getActive().getLogger().log(Logging::LogLevel::Info, "Account opened. (" + path.string() + ")");
            }
        }
    }

    void MainWindowController::removeRecentAccount(const RecentAccount& account)
    {
        Configuration& config{ Aura::getActive().getConfig<Configuration>("config") };
        config.removeRecentAccount(account);
        config.save();
        Aura::getActive().getLogger().log(Logging::LogLevel::Debug, "Config saved.");
        m_recentAccountsChanged.invoke({ config.getRecentAccounts() });
    }
}
