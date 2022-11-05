#include "mainwindow.hpp"
#include <filesystem>
#include "preferencesdialog.hpp"
#include "shortcutsdialog.hpp"

using namespace NickvisionMoney::Controllers;
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
    g_menu_append(menuAccount, _("New Account"), "win.newAccount");
    g_menu_append(menuAccount, _("Open Account"), "win.openAccount");
    g_menu_append(menuAccount, _("Close Account"), "win.closeAccount");
    gtk_menu_button_set_icon_name(GTK_MENU_BUTTON(m_btnMenuAccount), "bank-symbolic");
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuAccount), G_MENU_MODEL(menuAccount));
    gtk_widget_set_tooltip_text(m_btnMenuAccount, _("Account Menu"));
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnMenuAccount);
    g_object_unref(menuAccount);
    //Menu Help Button
    m_btnMenuHelp = gtk_menu_button_new();
    GMenu* menuHelp{ g_menu_new() };
    g_menu_append(menuHelp, _("Preferences"), "win.preferences");
    g_menu_append(menuHelp, _("Keyboard Shortcuts"), "win.keyboardShortcuts");
    g_menu_append(menuHelp, std::string(_("About ") + m_controller.getAppInfo().getShortName()).c_str(), "win.about");
    gtk_menu_button_set_direction(GTK_MENU_BUTTON(m_btnMenuHelp), GTK_ARROW_NONE);
    gtk_menu_button_set_menu_model(GTK_MENU_BUTTON(m_btnMenuHelp), G_MENU_MODEL(menuHelp));
    gtk_widget_set_tooltip_text(m_btnMenuHelp, _("Main Menu"));
    adw_header_bar_pack_end(ADW_HEADER_BAR(m_headerBar), m_btnMenuHelp);
    g_object_unref(menuHelp);
    //Toast Overlay
    m_toastOverlay = adw_toast_overlay_new();
    gtk_widget_set_hexpand(m_toastOverlay, true);
    gtk_widget_set_vexpand(m_toastOverlay, true);
    //Status Buttons
    m_boxStatusButtons = gtk_box_new(GTK_ORIENTATION_VERTICAL, 12);
    //New Account Button
    m_btnNewAccount = gtk_button_new();
    gtk_widget_set_halign(m_btnNewAccount, GTK_ALIGN_CENTER);
    gtk_widget_set_size_request(m_btnNewAccount, 200, 50);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnNewAccount), "circular");
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnNewAccount), "suggested-action");
    gtk_button_set_label(GTK_BUTTON(m_btnNewAccount), _("New Account"));
    gtk_actionable_set_detailed_action_name(GTK_ACTIONABLE(m_btnNewAccount), "win.newAccount");
    gtk_box_append(GTK_BOX(m_boxStatusButtons), m_btnNewAccount);
    //Open Account Button
    m_btnOpenAccount = gtk_button_new();
    gtk_widget_set_halign(m_btnOpenAccount, GTK_ALIGN_CENTER);
    gtk_widget_set_size_request(m_btnOpenAccount, 200, 50);
    gtk_style_context_add_class(gtk_widget_get_style_context(m_btnOpenAccount), "circular");
    gtk_button_set_label(GTK_BUTTON(m_btnOpenAccount), _("Open Account"));
    gtk_actionable_set_detailed_action_name(GTK_ACTIONABLE(m_btnOpenAccount), "win.openAccount");
    gtk_box_append(GTK_BOX(m_boxStatusButtons), m_btnOpenAccount);
    //Page No Downloads
    m_pageStatusNoAccounts = adw_status_page_new();
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_pageStatusNoAccounts), "org.nickvision.money-symbolic");
    adw_status_page_set_title(ADW_STATUS_PAGE(m_pageStatusNoAccounts), _("No Accounts Open"));
    adw_status_page_set_description(ADW_STATUS_PAGE(m_pageStatusNoAccounts), _("Open or create an account to get started."));
    adw_status_page_set_child(ADW_STATUS_PAGE(m_pageStatusNoAccounts), m_boxStatusButtons);
    //Page Tabs
    m_pageTabs = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    m_tabView = adw_tab_view_new();
    g_signal_connect(m_tabView, "close-page", G_CALLBACK((bool (*)(AdwTabView*, AdwTabPage*, gpointer))[](AdwTabView*, AdwTabPage* page, gpointer data) -> bool { return reinterpret_cast<MainWindow*>(data)->onCloseAccountPage(page); }), this);
    m_tabBar = adw_tab_bar_new();
    adw_tab_bar_set_view(m_tabBar, m_tabView);
    gtk_box_append(GTK_BOX(m_pageTabs), GTK_WIDGET(m_tabBar));
    gtk_box_append(GTK_BOX(m_pageTabs), GTK_WIDGET(m_tabView));
    //View Stack
    m_viewStack = adw_view_stack_new();
    adw_view_stack_add_named(ADW_VIEW_STACK(m_viewStack), m_pageStatusNoAccounts, "pageNoAccounts");
    adw_view_stack_add_named(ADW_VIEW_STACK(m_viewStack), m_pageTabs, "pageTabs");
    adw_toast_overlay_set_child(ADW_TOAST_OVERLAY(m_toastOverlay), m_viewStack);
    //Main Box
    m_mainBox = gtk_box_new(GTK_ORIENTATION_VERTICAL, 0);
    gtk_box_append(GTK_BOX(m_mainBox), m_headerBar);
    gtk_box_append(GTK_BOX(m_mainBox), m_toastOverlay);
    adw_application_window_set_content(ADW_APPLICATION_WINDOW(m_gobj), m_mainBox);
    //Send Toast Callback
    m_controller.registerSendToastCallback([&](const std::string& message) { adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(m_toastOverlay), adw_toast_new(message.c_str())); });
    //Account Added Callback
    m_controller.registerAccountAddedCallback([&]() { onAccountAdded(); });
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
    g_simple_action_set_enabled(m_actCloseAccount, false);
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
    //Drop Target
    m_dropTarget = gtk_drop_target_new(G_TYPE_FILE, GDK_ACTION_COPY);
    g_signal_connect(m_dropTarget, "drop", G_CALLBACK((int (*)(GtkDropTarget*, const GValue*, gdouble, gdouble, gpointer))[](GtkDropTarget*, const GValue* value, gdouble, gdouble, gpointer data) -> int { return reinterpret_cast<MainWindow*>(data)->onDrop(value); }), this);
    gtk_widget_add_controller(m_gobj, GTK_EVENT_CONTROLLER(m_dropTarget));
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

void MainWindow::onAccountAdded()
{
    g_simple_action_set_enabled(m_actCloseAccount, true);
    adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_viewStack), "pageTabs");
    std::unique_ptr<AccountView> newAccountView{ std::make_unique<AccountView>(GTK_WINDOW(m_gobj), m_tabView, m_controller.createAccountViewControllerForLatestAccount()) };
    adw_tab_view_set_selected_page(m_tabView, newAccountView->gobj());
    m_accountViews.push_back(std::move(newAccountView));
    adw_window_title_set_subtitle(ADW_WINDOW_TITLE(m_adwTitle), m_controller.getNumberOfOpenAccounts() == 1 ? m_controller.getFirstOpenAccountPath().c_str() : nullptr);
}

void MainWindow::onNewAccount()
{
    GtkFileChooserNative* saveFileDialog{ gtk_file_chooser_native_new(_("Open Account"), GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_SAVE, _("_Save"), _("_Cancel")) };
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(saveFileDialog), true);
    GtkFileFilter* filter{ gtk_file_filter_new() };
    gtk_file_filter_set_name(filter, _("Money Account (*.nmoney)"));
    gtk_file_filter_add_pattern(filter, "*.nmoney");
    gtk_file_chooser_add_filter(GTK_FILE_CHOOSER(saveFileDialog), filter);
    g_object_unref(filter);
    g_signal_connect(saveFileDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer))([](GtkNativeDialog* dialog, gint response_id, gpointer data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{ reinterpret_cast<MainWindow*>(data) };
            GFile* file{ gtk_file_chooser_get_file(GTK_FILE_CHOOSER(dialog)) };
            std::string path{ g_file_get_path(file) };
            mainWindow->m_controller.addAccount(path);
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(saveFileDialog));
}

void MainWindow::onOpenAccount()
{
    GtkFileChooserNative* openFileDialog{ gtk_file_chooser_native_new(_("Open Account"), GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_OPEN, _("_Open"), _("_Cancel")) };
    gtk_native_dialog_set_modal(GTK_NATIVE_DIALOG(openFileDialog), true);
    GtkFileFilter* filter{ gtk_file_filter_new() };
    gtk_file_filter_set_name(filter, _("Money Account (*.nmoney)"));
    gtk_file_filter_add_pattern(filter, "*.nmoney");
    gtk_file_chooser_add_filter(GTK_FILE_CHOOSER(openFileDialog), filter);
    g_object_unref(filter);
    g_signal_connect(openFileDialog, "response", G_CALLBACK((void (*)(GtkNativeDialog*, gint, gpointer))([](GtkNativeDialog* dialog, gint response_id, gpointer data)
    {
        if(response_id == GTK_RESPONSE_ACCEPT)
        {
            MainWindow* mainWindow{ reinterpret_cast<MainWindow*>(data) };
            GFile* file{ gtk_file_chooser_get_file(GTK_FILE_CHOOSER(dialog)) };
            std::string path{ g_file_get_path(file) };
            mainWindow->m_controller.addAccount(path);
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openFileDialog));
}

void MainWindow::onCloseAccount()
{
    adw_tab_view_close_page(m_tabView, adw_tab_view_get_selected_page(m_tabView));
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


bool MainWindow::onDrop(const GValue* value)
{
    void* file{ g_value_get_object(value) };
    std::string path{ g_file_get_path(G_FILE(file)) };
    if(std::filesystem::path(path).extension() == ".nmoney")
    {
        m_controller.addAccount(path);
        return true;
    }
    return false;
}

bool MainWindow::onCloseAccountPage(AdwTabPage* page)
{
    int indexPage{ adw_tab_view_get_page_position(m_tabView, page) };
    m_controller.closeAccount(indexPage);
    m_accountViews.erase(m_accountViews.begin() + indexPage);
    adw_tab_view_close_page_finish(m_tabView, page, true);
    adw_window_title_set_subtitle(ADW_WINDOW_TITLE(m_adwTitle), m_controller.getNumberOfOpenAccounts() == 1 ? m_controller.getFirstOpenAccountPath().c_str() : nullptr);
    if(m_controller.getNumberOfOpenAccounts() == 0)
    {
        g_simple_action_set_enabled(m_actCloseAccount, false);
        adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_viewStack), "pageNoAccounts");
    }
    return GDK_EVENT_STOP;
}
