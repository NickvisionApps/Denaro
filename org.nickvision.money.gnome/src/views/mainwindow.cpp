#include "views/mainwindow.h"
#include <format>
#include <libnick/app/appinfo.h>
#include <libnick/notifications/shellnotification.h>
#include <libnick/localization/gettext.h>
#include "controls/currencyconverterdialog.h"
#include "helpers/builder.h"
#include "views/preferencesdialog.h"

using namespace Nickvision::Money::GNOME::Controls;
using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::App;
using namespace Nickvision::Events;
using namespace Nickvision::Notifications;

namespace Nickvision::Money::GNOME::Views
{
    MainWindow::MainWindow(const std::shared_ptr<MainWindowController>& controller, GtkApplication* app)
        : m_controller{ controller },
        m_app{ app },
        m_builder{ BuilderHelpers::fromBlueprint("main_window") },
        m_window{ ADW_APPLICATION_WINDOW(gtk_builder_get_object(m_builder, "root")) }
    {
        //Setup Window
        gtk_application_add_window(GTK_APPLICATION(app), GTK_WINDOW(m_window));
        gtk_window_set_title(GTK_WINDOW(m_window), m_controller->getAppInfo().getShortName().c_str());
        gtk_window_set_icon_name(GTK_WINDOW(m_window), m_controller->getAppInfo().getId().c_str());
        if(m_controller->isDevVersion())
        {
            gtk_widget_add_css_class(GTK_WIDGET(m_window), "devel");
        }
        adw_window_title_set_title(ADW_WINDOW_TITLE(gtk_builder_get_object(m_builder, "title")), m_controller->getAppInfo().getShortName().c_str());
        //Register Events
        g_signal_connect(m_window, "close_request", G_CALLBACK(+[](GtkWindow*, gpointer data) -> bool { return reinterpret_cast<MainWindow*>(data)->onCloseRequested(); }), this);
        m_controller->notificationSent() += [&](const NotificationSentEventArgs& args) { onNotificationSent(args); };
        m_controller->shellNotificationSent() += [&](const ShellNotificationSentEventArgs& args) { onShellNotificationSent(args); };
        //Quit Action
        GSimpleAction* actQuit{ g_simple_action_new("quit", nullptr) };
        g_signal_connect(actQuit, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->quit(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actQuit));
        SET_ACCEL_FOR_ACTION(m_app, "win.quit", "<Ctrl>Q");
        //Currency Converter Action
        GSimpleAction* actCurrencyConverter{ g_simple_action_new("currencyConverter", nullptr) };
        g_signal_connect(actCurrencyConverter, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->currencyConverter(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actCurrencyConverter));
        //Preferences Action
        GSimpleAction* actPreferences{ g_simple_action_new("preferences", nullptr) };
        g_signal_connect(actPreferences, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->preferences(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actPreferences));
        SET_ACCEL_FOR_ACTION(m_app, "win.preferences", "<Ctrl>comma");
        //Keyboard Shortcuts Action
        GSimpleAction* actKeyboardShortcuts{ g_simple_action_new("keyboardShortcuts", nullptr) };
        g_signal_connect(actKeyboardShortcuts, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->keyboardShortcuts(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actKeyboardShortcuts));
        SET_ACCEL_FOR_ACTION(m_app, "win.keyboardShortcuts", "<Ctrl>question");
        //About Action
        GSimpleAction* actAbout{ g_simple_action_new("about", nullptr) };
        g_signal_connect(actAbout, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->about(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actAbout));
        SET_ACCEL_FOR_ACTION(m_app, "win.about", "F1");
    }

    MainWindow::~MainWindow()
    {
        gtk_window_destroy(GTK_WINDOW(m_window));
        g_object_unref(m_builder);
    }

    GObject* MainWindow::gobj() const
    {
        return G_OBJECT(m_window);
    }

    void MainWindow::show()
    {
        gtk_window_present(GTK_WINDOW(m_window));
        m_controller->connectTaskbar(m_controller->getAppInfo().getId() + ".desktop");
        m_controller->startup();
    }

    bool MainWindow::onCloseRequested()
    {
        return false;
    }

    void MainWindow::onNotificationSent(const NotificationSentEventArgs& args)
    {
        AdwToast* toast{ adw_toast_new(args.getMessage().c_str()) };
        adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(gtk_builder_get_object(m_builder, "toastOverlay")), toast);
    }

    void MainWindow::onShellNotificationSent(const ShellNotificationSentEventArgs& args)
    {
        ShellNotification::send(args, _("Open"));
    }

    void MainWindow::quit()
    {
        if(!onCloseRequested())
        {
            g_application_quit(G_APPLICATION(m_app));
        }
    }

    void MainWindow::currencyConverter()
    {
        CurrencyConverterDialog currencyConverter{ GTK_WINDOW(m_window), m_controller->getAppInfo().getId() };
        currencyConverter.run();
    }

    void MainWindow::preferences()
    {
        PreferencesDialog preferences{ m_controller->createPreferencesViewController(), GTK_WINDOW(m_window) };
        preferences.run();
    }

    void MainWindow::keyboardShortcuts()
    {
        GtkBuilder* builderHelp{ BuilderHelpers::fromBlueprint("shortcuts_dialog") };
        GtkShortcutsWindow* shortcuts{ GTK_SHORTCUTS_WINDOW(gtk_builder_get_object(builderHelp, "root")) };
        gtk_window_set_transient_for(GTK_WINDOW(shortcuts), GTK_WINDOW(m_window));
        gtk_window_set_icon_name(GTK_WINDOW(shortcuts), m_controller->getAppInfo().getId().c_str());
        gtk_window_present(GTK_WINDOW(shortcuts));
    }

    void MainWindow::about()
    {
        std::string extraDebug;
        extraDebug += "GTK " + std::to_string(gtk_get_major_version()) + "." + std::to_string(gtk_get_minor_version()) + "." + std::to_string(gtk_get_micro_version()) + "\n";
        extraDebug += "libadwaita " + std::to_string(adw_get_major_version()) + "." + std::to_string(adw_get_minor_version()) + "." + std::to_string(adw_get_micro_version());
        AdwAboutWindow* dialog{ ADW_ABOUT_WINDOW(adw_about_window_new()) };
        gtk_window_set_transient_for(GTK_WINDOW(dialog), GTK_WINDOW(m_window));
        gtk_window_set_icon_name(GTK_WINDOW(dialog), m_controller->getAppInfo().getId().c_str());
        adw_about_window_set_application_name(dialog, m_controller->getAppInfo().getShortName().c_str());
        adw_about_window_set_application_icon(dialog, std::string(m_controller->getAppInfo().getId() + (m_controller->isDevVersion() ? "-devel" : "")).c_str());
        adw_about_window_set_developer_name(dialog, "Nickvision");
        adw_about_window_set_version(dialog, m_controller->getAppInfo().getVersion().toString().c_str());
        adw_about_window_set_release_notes(dialog, m_controller->getAppInfo().getHtmlChangelog().c_str());
        adw_about_window_set_debug_info(dialog, m_controller->getDebugInformation(extraDebug).c_str());
        adw_about_window_set_comments(dialog, m_controller->getAppInfo().getDescription().c_str());
        adw_about_window_set_license_type(dialog, GTK_LICENSE_GPL_3_0);
        adw_about_window_set_copyright(dialog, "© Nickvision 2021-2024");
        adw_about_window_set_website(dialog, "https://nickvision.org/");
        adw_about_window_set_issue_url(dialog, m_controller->getAppInfo().getIssueTracker().c_str());
        adw_about_window_set_support_url(dialog, m_controller->getAppInfo().getSupportUrl().c_str());
        adw_about_window_add_link(dialog, _("GitHub Repo"), m_controller->getAppInfo().getSourceRepo().c_str());
        for(const std::pair<std::string, std::string>& pair : m_controller->getAppInfo().getExtraLinks())
        {
            adw_about_window_add_link(dialog, pair.first.c_str(), pair.second.c_str());
        }
        std::vector<const char*> urls;
        std::vector<std::string> developers{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getDevelopers()) };
        for(const std::string& developer : developers)
        {
            urls.push_back(developer.c_str());
        }
        urls.push_back(nullptr);
        adw_about_window_set_developers(dialog, &urls[0]);
        urls.clear();
        std::vector<std::string> designers{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getDesigners()) };
        for(const std::string& designer : designers)
        {
            urls.push_back(designer.c_str());
        }
        urls.push_back(nullptr);
        adw_about_window_set_designers(dialog, &urls[0]);
        urls.clear();
        std::vector<std::string> artists{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getArtists()) };
        for(const std::string& artist : artists)
        {
            urls.push_back(artist.c_str());
        }
        urls.push_back(nullptr);
        adw_about_window_set_artists(dialog, &urls[0]);
        adw_about_window_set_translator_credits(dialog, m_controller->getAppInfo().getTranslatorCredits().c_str());
        gtk_window_present(GTK_WINDOW(dialog));
    }
}