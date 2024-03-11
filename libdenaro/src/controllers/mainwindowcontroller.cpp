#include "controllers/mainwindowcontroller.h"
#include <ctime>
#include <format>
#include <locale>
#include <stdexcept>
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
        Aura::getActive().init("org.nickvision.money", "Nickvision Denaro", "Denaro");
        AppInfo& appInfo{ Aura::getActive().getAppInfo() };
        appInfo.setVersion({ "2024.3.0-next" });
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

    std::vector<Models::RecentAccount> MainWindowController::getRecentAccounts() const
    {
        return Aura::getActive().getConfig<Configuration>("config").getRecentAccounts();
    }

    Color MainWindowController::getAccountTypeColor(AccountType accountType) const
    {
        switch(accountType)
        {
        case AccountType::Checking:
            return Aura::getActive().getConfig<Configuration>("config").getAccountCheckingColor();
        case AccountType::Savings:
            return Aura::getActive().getConfig<Configuration>("config").getAccountSavingsColor();
        case AccountType::Business:
            return Aura::getActive().getConfig<Configuration>("config").getAccountBusinessColor();
        default:
            return {};
        }
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

    Event<ParamEventArgs<std::shared_ptr<AccountViewController>>>& MainWindowController::accountAdded()
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
        builder << std::locale("").name() << std::endl;
#endif
        //Gnuplot
        builder << std::endl << Aura::getActive().sysExec("gnuplot --version") << std::endl;
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
        std::vector<std::filesystem::path> openAccounts;
        for(const std::pair<const std::filesystem::path, std::shared_ptr<AccountViewController>>& pair : m_accountViewControllers)
        {
            openAccounts.push_back(pair.first);
        }
        return std::make_shared<NewAccountDialogController>(openAccounts);
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
            m_started = true;
        }
    }

    void MainWindowController::checkForUpdates()
    {
        if(m_updater)
        {
            std::thread worker{ [&]()
            {
                Version latest{ m_updater->fetchCurrentStableVersion() };
                if (!latest.empty())
                {
                    if (latest > Aura::getActive().getAppInfo().getVersion())
                    {
                        m_notificationSent.invoke({ _("New update available"), NotificationSeverity::Success, "update" });
                    }
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
            std::thread worker{ [&]()
            {
                bool res{ m_updater->windowsUpdate(VersionType::Stable) };
                if (!res)
                {
                    m_notificationSent.invoke({ _("Unable to download and install update"), NotificationSeverity::Error, "error" });
                }
            } };
            worker.detach();
        }
    }

    bool MainWindowController::connectTaskbar(HWND hwnd)
    {
        return m_taskbar.connect(hwnd);
    }
#elif defined(__linux__)
    bool MainWindowController::connectTaskbar(const std::string& desktopFile)
    {
        return m_taskbar.connect(desktopFile);
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

    void MainWindowController::openAccount(std::filesystem::path path, const std::string& password)
    {
        //Check if the file is a Denaro account file
        if(StringHelpers::toLower(path.extension().string()) != ".nmoney")
        {
            m_notificationSent.invoke({ _("The file is not a Denaro account file."), NotificationSeverity::Error });
        }
        //Check if the account is already open
        else if(m_accountViewControllers.contains(path))
        {
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
                m_notificationSent.invoke({ e.what(), NotificationSeverity::Error });
            }
            if(controller)
            {
                m_accountViewControllers.emplace(std::make_pair(path, controller));
                Configuration& config{ Aura::getActive().getConfig<Configuration>("config") };
                config.addRecentAccount(controller->toRecentAccount());
                config.save();
                m_accountAdded.invoke(controller);
            }
        }
    }

    void MainWindowController::removeRecentAccount(const RecentAccount& account)
    {
        Configuration& config{ Aura::getActive().getConfig<Configuration>("config") };
        config.removeRecentAccount(account);
        config.save();
    }
}