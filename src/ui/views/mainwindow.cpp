#include "mainwindow.hpp"
#include <chrono>
#include <thread>
#include <utility>
#include "preferencesdialog.hpp"
#include "shortcutsdialog.hpp"
#include "../controls/progressdialog.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Controls;
using namespace NickvisionMoney::UI::Views;

MainWindow::MainWindow(GtkApplication* application, const MainWindowController& controller) : m_controller{ controller }, m_gobj{ adw_application_window_new(application) }
{
    //Window Settings
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 900, 700);
    if(m_controller.getIsDevVersion())
    {
        gtk_style_context_add_class(gtk_widget_get_style_context(m_gobj), "devel");
    }
    //Header Bar
    m_headerBar = adw_header_bar_new();
    m_adwTitle = adw_window_title_new(m_controller.getAppInfo().getShortName().c_str(), nullptr);
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), m_adwTitle);
    //Menu Account Button
    m_btnMenuAccount = gtk_menu_button_new();
    GMenu* menuAccount{ g_menu_new() };
    g_menu_append(menuAccount, "New Account", "win.newAccount");
    g_menu_append(menuAccount, "Open Account", "win.openAccount");
    g_menu_append(menuAccount, "Close Account", "win.closeAccount");
    gtk_menu_button_set_icon_name(GTK_MENU_BUTTON(m_btnMenuAccount), "emblem-documents-symbolic");
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuAccount), G_MENU_MODEL(menuAccount));
    gtk_widget_set_tooltip_text(m_btnMenuAccount, "Account Menu");
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnMenuAccount);
    g_object_unref(menuAccount);
    //Menu Help Button
    m_btnMenuHelp = gtk_menu_button_new();
    GMenu* menuHelp{ g_menu_new() };
    g_menu_append(menuHelp, "Preferences", "win.preferences");
    g_menu_append(menuHelp, "Keyboard Shortcuts", "win.keyboardShortcuts");
    g_menu_append(menuHelp, std::string("About " + m_controller.getAppInfo().getShortName()).c_str(), "win.about");
    gtk_menu_button_set_direction(GTK_MENU_BUTTON(m_btnMenuHelp), GTK_ARROW_NONE);
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuHelp), G_MENU_MODEL(menuHelp));
    gtk_widget_set_tooltip_text(m_btnMenuHelp, "Main Menu");
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnMenuHelp);
    g_object_unref(menuHelp);
    //Toast Overlay
    m_toastOverlay = adw_toast_overlay_new();
    gtk_widget_set_hexpand(m_toastOverlay, true);
    gtk_widget_set_vexpand(m_toastOverlay, true);
    //Page No Downloads
    m_pageStatusNoAccounts = adw_status_page_new();
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_pageStatusNoAccounts), "org.nickvision.money-symbolic");
    adw_status_page_set_title(ADW_STATUS_PAGE(m_pageStatusNoAccounts), "No Accounts Open");
    adw_status_page_set_description(ADW_STATUS_PAGE(m_pageStatusNoAccounts), "Open or create an account to get started.");
    //View Stack
    m_viewStack = adw_view_stack_new();
    adw_view_stack_add_named(ADW_VIEW_STACK(m_viewStack), m_pageStatusNoAccounts, "pageNoAccounts");
    adw_toast_overlay_set_child(ADW_TOAST_OVERLAY(m_toastOverlay), m_viewStack);
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_toastOverlay);
    adw_application_window_set_content(ADW_APPLICATION_WINDOW(m_gobj), m_mainBox);
    //Send Toast Callback
    m_controller.registerSendToastCallback([&](const std::string& message) { adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(m_toastOverlay), adw_toast_new(message.c_str())); });
    //New Account Action
    m_actNewAccount = g_simple_action_new("newAccount", nullptr);
    g_signal_connect(m_actNewAccount, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onNewAccount(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actNewAccount));
    gtk_application_set_accels_for_action(application, "win.newAccount", new const char*[2]{ "<Ctrl>N", nullptr });
    //Open Account Action
    m_actOpenAccount = g_simple_action_new("openAccount", nullptr);
    g_signal_connect(m_actOpenAccount, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onOpenAccount(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actOpenAccount));
    gtk_application_set_accels_for_action(application, "win.openAccount", new const char*[2]{ "<Ctrl>O", nullptr });
    //Close Account Action
    m_actCloseAccount = g_simple_action_new("closeAccount", nullptr);
    g_signal_connect(m_actCloseAccount, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onCloseAccount(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actCloseAccount));
    gtk_application_set_accels_for_action(application, "win.closeAccount", new const char*[2]{ "<Ctrl>W", nullptr });
    //Preferences Action
    m_actPreferences = g_simple_action_new("preferences", nullptr);
    g_signal_connect(m_actPreferences, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onPreferences(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actPreferences));
    gtk_application_set_accels_for_action(application, "win.preferences", new const char*[2]{ "<Ctrl>comma", nullptr });
    //Keyboard Shortcuts Action
    m_actKeyboardShortcuts = g_simple_action_new("keyboardShortcuts", nullptr);
    g_signal_connect(m_actKeyboardShortcuts, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onKeyboardShortcuts(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actKeyboardShortcuts));
    gtk_application_set_accels_for_action(application, "win.keyboardShortcuts", new const char*[2]{ "<Ctrl>question", nullptr });
    //About Action
    m_actAbout = g_simple_action_new("about", nullptr);
    g_signal_connect(m_actAbout, "activate", G_CALLBACK((void (*)(GSimpleAction*, GVariant*, gpointer))[](GSimpleAction*, GVariant*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onAbout(); }), this);
    g_action_map_add_action(G_ACTION_MAP(m_gobj), G_ACTION(m_actAbout));
    gtk_application_set_accels_for_action(application, "win.about", new const char*[2]{ "F1", nullptr });
}

GtkWidget* MainWindow::gobj()
{
    return m_gobj;
}

void MainWindow::start()
{
    gtk_widget_show(m_gobj);
    m_controller.startup();
}

void MainWindow::onNewAccount()
{

}

void MainWindow::onOpenAccount()
{

}

void MainWindow::onCloseAccount()
{

}

void MainWindow::onPreferences()
{
    PreferencesDialog preferencesDialog{ GTK_WINDOW(m_gobj), m_controller.createPreferencesDialogController() };
    preferencesDialog.run();
    m_controller.onConfigurationChanged();
}

void MainWindow::onKeyboardShortcuts()
{
    ShortcutsDialog shortcutsDialog{ GTK_WINDOW(m_gobj) };
    shortcutsDialog.run();
}

void MainWindow::onAbout()
{
    adw_show_about_window(GTK_WINDOW(m_gobj),
                          "application-name", m_controller.getAppInfo().getShortName().c_str(),
                          "application-icon", (m_controller.getAppInfo().getId() + (m_controller.getIsDevVersion() ? "-devel" : "")).c_str(),
                          "version", m_controller.getAppInfo().getVersion().c_str(),
                          "comments", m_controller.getAppInfo().getDescription().c_str(),
                          "developer-name", "Nickvision",
                          "license-type", GTK_LICENSE_GPL_3_0,
                          "copyright", "(C) Nickvision 2021-2022",
                          "website", m_controller.getAppInfo().getGitHubRepo().c_str(),
                          "issue-url", m_controller.getAppInfo().getIssueTracker().c_str(),
                          "support-url", m_controller.getAppInfo().getSupportUrl().c_str(),
                          "developers", new const char*[3]{ "Nicholas Logozzo https://github.com/nlogozzo", "Contributors on GitHub ❤️ https://github.com/nlogozzo/NickvisionMoney/graphs/contributors", nullptr },
                          "designers", new const char*[2]{ "Nicholas Logozzo https://github.com/nlogozzo", nullptr },
                          "artists", new const char*[2]{ "David Lapshin https://github.com/daudix-UFO", nullptr },
                          "release-notes", m_controller.getAppInfo().getChangelog().c_str(),
                          nullptr);
}
