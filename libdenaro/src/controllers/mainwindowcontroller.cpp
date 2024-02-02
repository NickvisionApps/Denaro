#include "controllers/mainwindowcontroller.h"
#include <ctime>
#include <format>
#include <sstream>
#include <thread>
#include <boost/locale.hpp>
#include <libnick/app/aura.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/localization/gettext.h>
#include "models/configuration.h"

using namespace Nickvision::Money::Shared::Models;
using namespace Nickvision::App;
using namespace Nickvision::Events;
using namespace Nickvision::Notifications;
using namespace Nickvision::Update;

namespace Nickvision::Money::Shared::Controllers
{
    MainWindowController::MainWindowController()
    {
        Aura::getActive().init("org.nickvision.money", "Nickvision Denaro", "Denaro");
        AppInfo& appInfo{ Aura::getActive().getAppInfo() };
        appInfo.setVersion({ "2024.2.1-next" });
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

    std::string MainWindowController::getDebugInformation(const std::string& extraInformation) const
    {
        std::stringstream builder;
        builder << Aura::getActive().getAppInfo().getId();
#ifdef _WIN32
        builder << ".winui" << std::endl;
#elif defined(__linux__)
        builder << ".gnome" << std::endl;
#endif
        builder << Aura::getActive().getAppInfo().getVersion().toString() << std::endl << std::endl;
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
        builder << StringHelpers::split(boost::locale::util::get_system_locale(), ".")[0] << std::endl;
        if (!extraInformation.empty())
        {
            builder << extraInformation << std::endl;
        }
        return builder.str();
    }

    std::shared_ptr<PreferencesViewController> MainWindowController::createPreferencesViewController() const
    {
        return std::make_shared<PreferencesViewController>();
    }

    void MainWindowController::startup()
    {
        static bool started{ false };
        if (!started)
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
            started = true;
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
}