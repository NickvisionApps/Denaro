#include "application.hpp"
#include "../controllers/mainwindowcontroller.hpp"
#include "../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;
using namespace NickvisionMoney::UI;
using namespace NickvisionMoney::UI::Views;

Application::Application(const std::string& id, GApplicationFlags flags) : m_adwApp{ adw_application_new(id.c_str(), flags) }
{
    //AppInfo
    m_appInfo.setId(id);
    m_appInfo.setName("Nickvision Money");
    m_appInfo.setShortName("Money");
    m_appInfo.setDescription(_("A personal finance manager."));
    m_appInfo.setVersion("2022.11.0-beta5");
    m_appInfo.setChangelog("<ul><li>Introducing Groups: Add groups to an account and associate transactions with groups for a more precise finance management system</li><li>Money will automatically obtain the user's currency symbol and format from their locale</li><li>Money will remember up to 3 recently opened accounts for quick and easy access</li><li>Fixed an issue where a new account was not created if overwriting an old one</li><li>Added translation support</li><li>Added Croatian translation (Thanks @milotype!)</li><li>Added Dutch translation (Thanks @Vistaus!)</li><li>Added French translation (Thanks @zothma!)</li><li>Added Hindi translation (Thanks @securearth!)</li><li>Added Italian translation (Thanks @bordam!)</li><li>Added Polish translation (Thanks @martin-desktops!)</li><li>Added Russian translation (Thanks @fsobolev!)</li></ul>");
    m_appInfo.setGitHubRepo("https://github.com/nlogozzo/NickvisionMoney");
    m_appInfo.setIssueTracker("https://github.com/nlogozzo/NickvisionMoney/issues/new");
    m_appInfo.setSupportUrl("https://github.com/nlogozzo/NickvisionMoney/discussions");
    //Signals
    g_signal_connect(m_adwApp, "activate", G_CALLBACK((void (*)(GtkApplication*, gpointer))[](GtkApplication* app, gpointer data) { reinterpret_cast<Application*>(data)->onActivate(app); }), this);
}

int Application::run(int argc, char* argv[])
{
    return g_application_run(G_APPLICATION(m_adwApp), argc, argv);
}

void Application::onActivate(GtkApplication* app)
{
    if(m_configuration.getTheme() == Theme::System)
    {
         adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_PREFER_LIGHT);
    }
    else if(m_configuration.getTheme() == Theme::Light)
    {
         adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_LIGHT);
    }
    else if(m_configuration.getTheme() == Theme::Dark)
    {
         adw_style_manager_set_color_scheme(adw_style_manager_get_default(), ADW_COLOR_SCHEME_FORCE_DARK);
    }
    m_mainWindow = std::make_shared<MainWindow>(app, MainWindowController(m_appInfo, m_configuration));
    gtk_application_add_window(app, GTK_WINDOW(m_mainWindow->gobj()));
    m_mainWindow->start();
}
