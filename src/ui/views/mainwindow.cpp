#include "mainwindow.hpp"
#include <filesystem>
#include <regex>
#include "preferencesdialog.hpp"
#include "shortcutsdialog.hpp"
#include "../../helpers/translation.hpp"
#include "../../helpers/stringhelpers.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Helpers;
using namespace NickvisionMoney::UI::Views;

MainWindow::MainWindow(GtkApplication* application, const MainWindowController& controller) : m_controller{ controller }, m_gobj{ adw_application_window_new(application) }
{
    //Window Settings
    gtk_window_set_default_size(GTK_WINDOW(m_gobj), 900, 700);
    if(m_controller.getIsDevVersion())
    {
        gtk_widget_add_css_class(m_gobj, "devel");
    }
    //Header Bar
    m_headerBar = adw_header_bar_new();
    m_adwTitle = adw_window_title_new(m_controller.getAppInfo().getShortName().c_str(), nullptr);
    adw_header_bar_set_title_widget(ADW_HEADER_BAR(m_headerBar), m_adwTitle);
    //Account Popover
    m_popoverAccount = gtk_popover_new();
    //Label Recents
    m_lblRecents = gtk_label_new(_("Recents"));
    gtk_widget_add_css_class(m_lblRecents, "title-4");
    gtk_widget_set_hexpand(m_lblRecents, true);
    gtk_widget_set_halign(m_lblRecents, GTK_ALIGN_START);
    //Account Popover Buttons Box
    m_popBoxButtons = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 0);
    gtk_widget_add_css_class(m_popBoxButtons, "linked");
    gtk_widget_set_halign(m_popBoxButtons, GTK_ALIGN_CENTER);
    gtk_widget_set_valign(m_popBoxButtons, GTK_ALIGN_CENTER);
    //Account Popover New Account Button
    m_popBtnNewAccount = gtk_button_new();
    gtk_widget_add_css_class(m_popBtnNewAccount, "suggested-action");
    GtkWidget* popBtnNewAccountContent{ adw_button_content_new() };
    adw_button_content_set_label(ADW_BUTTON_CONTENT(popBtnNewAccountContent), _("New"));
    adw_button_content_set_icon_name(ADW_BUTTON_CONTENT(popBtnNewAccountContent), "document-new-symbolic");
    gtk_button_set_child(GTK_BUTTON(m_popBtnNewAccount), popBtnNewAccountContent);
    gtk_widget_set_tooltip_text(m_popBtnNewAccount, _("New Account (Ctrl+N)"));
    gtk_actionable_set_detailed_action_name(GTK_ACTIONABLE(m_popBtnNewAccount), "win.newAccount");
    gtk_box_append(GTK_BOX(m_popBoxButtons), m_popBtnNewAccount);
    //Account Popover Open Account Button
    m_popBtnOpenAccount = gtk_button_new();
    gtk_button_set_icon_name(GTK_BUTTON(m_popBtnOpenAccount), "document-open-symbolic");
    gtk_widget_set_tooltip_text(m_popBtnOpenAccount, _("Open Account (Ctrl+O)"));
    gtk_actionable_set_detailed_action_name(GTK_ACTIONABLE(m_popBtnOpenAccount), "win.openAccount");
    gtk_box_append(GTK_BOX(m_popBoxButtons), m_popBtnOpenAccount);
    //Account Popover Header Box
    m_popBoxHeader = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 10);
    gtk_box_append(GTK_BOX(m_popBoxHeader), m_lblRecents);
    gtk_box_append(GTK_BOX(m_popBoxHeader), m_popBoxButtons);
    //List Recent Accounts
    m_listRecentAccounts = gtk_list_box_new();
    gtk_widget_add_css_class(m_listRecentAccounts, "boxed-list");
    gtk_widget_set_size_request(m_listRecentAccounts, 200, 55);
    g_signal_connect(m_listRecentAccounts, "selected-rows-changed", G_CALLBACK((void (*)(GtkListBox*, gpointer))[](GtkListBox*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onListRecentAccountsSelectionChanged(); }), this);
    //Account Popover Box
    m_popBoxAccount = gtk_box_new(GTK_ORIENTATION_VERTICAL, 10);
    gtk_widget_set_margin_start(m_popBoxAccount, 5);
    gtk_widget_set_margin_top(m_popBoxAccount, 5);
    gtk_widget_set_margin_end(m_popBoxAccount, 5);
    gtk_widget_set_margin_bottom(m_popBoxAccount, 5);
    gtk_box_append(GTK_BOX(m_popBoxAccount), m_popBoxHeader);
    gtk_box_append(GTK_BOX(m_popBoxAccount), m_listRecentAccounts);
    gtk_popover_set_child(GTK_POPOVER(m_popoverAccount), m_popBoxAccount);
    //Menu Account Button
    m_btnMenuAccount = gtk_menu_button_new();
    gtk_widget_set_visible(m_btnMenuAccount, false);
    gtk_menu_button_set_icon_name(GTK_MENU_BUTTON(m_btnMenuAccount), "bank-symbolic");
    gtk_menu_button_set_popover(GTK_MENU_BUTTON(m_btnMenuAccount), m_popoverAccount);
    gtk_widget_set_tooltip_text(m_btnMenuAccount, _("Account Menu"));
    adw_header_bar_pack_start(ADW_HEADER_BAR(m_headerBar), m_btnMenuAccount);
    //Menu Help Button
    m_btnMenuHelp = gtk_menu_button_new();
    GMenu* menuHelp{ g_menu_new() };
    g_menu_append(menuHelp, _("Preferences"), "win.preferences");
    g_menu_append(menuHelp, _("Keyboard Shortcuts"), "win.keyboardShortcuts");
    g_menu_append(menuHelp, std::string(StringHelpers::format(_("About %s"), m_controller.getAppInfo().getShortName().c_str())).c_str(), "win.about");
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
    m_boxStatusButtons = gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 12);
    gtk_widget_set_hexpand(m_boxStatusButtons, true);
    gtk_widget_set_halign(m_boxStatusButtons, GTK_ALIGN_CENTER);
    //New Account Button
    m_btnNewAccount = gtk_button_new();
    gtk_widget_set_halign(m_btnNewAccount, GTK_ALIGN_CENTER);
    gtk_widget_set_size_request(m_btnNewAccount, 200, 50);
    gtk_widget_add_css_class(m_btnNewAccount, "circular");
    gtk_widget_add_css_class(m_btnNewAccount, "suggested-action");
    gtk_button_set_label(GTK_BUTTON(m_btnNewAccount), _("New Account"));
    gtk_actionable_set_detailed_action_name(GTK_ACTIONABLE(m_btnNewAccount), "win.newAccount");
    gtk_box_append(GTK_BOX(m_boxStatusButtons), m_btnNewAccount);
    //Open Account Button
    m_btnOpenAccount = gtk_button_new();
    gtk_widget_set_halign(m_btnOpenAccount, GTK_ALIGN_CENTER);
    gtk_widget_set_size_request(m_btnOpenAccount, 200, 50);
    gtk_widget_add_css_class(m_btnOpenAccount, "circular");
    gtk_button_set_label(GTK_BUTTON(m_btnOpenAccount), _("Open Account"));
    gtk_actionable_set_detailed_action_name(GTK_ACTIONABLE(m_btnOpenAccount), "win.openAccount");
    gtk_box_append(GTK_BOX(m_boxStatusButtons), m_btnOpenAccount);
    //Drag Label
    m_lblDrag = gtk_label_new(_("You may also drag in a file from your file browser to open."));
    gtk_widget_add_css_class(m_lblDrag, "dim-label");
    gtk_label_set_wrap(GTK_LABEL(m_lblDrag), true);
    gtk_label_set_justify(GTK_LABEL(m_lblDrag), GTK_JUSTIFY_CENTER);
    //Status Page Box
    m_boxStatusPage = gtk_box_new(GTK_ORIENTATION_VERTICAL, 12);
    gtk_widget_set_hexpand(m_boxStatusPage, false);
    gtk_widget_set_halign(m_boxStatusPage, GTK_ALIGN_CENTER);
    gtk_box_append(GTK_BOX(m_boxStatusPage), m_boxStatusButtons);
    gtk_box_append(GTK_BOX(m_boxStatusPage), m_lblDrag);
    //Recent Accounts Label
    m_lblRecentAccounts = gtk_label_new(_("Recent Accounts"));
    gtk_widget_add_css_class(m_lblRecentAccounts, "title-4");
    gtk_widget_set_hexpand(m_lblRecentAccounts, true);
    gtk_widget_set_halign(m_lblRecentAccounts, GTK_ALIGN_START);
    //List Recent Accounts On The Start Screen
    m_listRecentAccountsOnStart = gtk_list_box_new();
    gtk_widget_add_css_class(m_listRecentAccountsOnStart, "boxed-list");
    gtk_widget_set_size_request(m_listRecentAccountsOnStart, 200, 55);
    gtk_widget_set_margin_bottom(m_listRecentAccountsOnStart, 24);
    g_signal_connect(m_listRecentAccountsOnStart, "selected-rows-changed", G_CALLBACK((void (*)(GtkListBox*, gpointer))[](GtkListBox*, gpointer data) { reinterpret_cast<MainWindow*>(data)->onListRecentAccountsOnStartSelectionChanged(); }), this);
    //Page No Accounts
    m_pageStatusNoAccounts = adw_status_page_new();
    adw_status_page_set_icon_name(ADW_STATUS_PAGE(m_pageStatusNoAccounts), "org.nickvision.money-symbolic");
    adw_status_page_set_title(ADW_STATUS_PAGE(m_pageStatusNoAccounts), m_controller.getWelcomeMessage().c_str());
    adw_status_page_set_description(ADW_STATUS_PAGE(m_pageStatusNoAccounts), _("Open or create an account to get started."));
    adw_status_page_set_child(ADW_STATUS_PAGE(m_pageStatusNoAccounts), m_boxStatusPage);
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
    updateRecentAccounts();
    if (m_controller.getRecentAccounts().size() > 0)
    {
        for(const std::string& recentAccountPath : m_controller.getRecentAccounts())
        {
            GtkWidget* row{ adw_action_row_new() };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), std::filesystem::path(recentAccountPath).filename().c_str());
            adw_action_row_set_subtitle(ADW_ACTION_ROW(row), std::regex_replace(recentAccountPath, std::regex("\\&"), "&amp;").c_str());
            adw_action_row_add_prefix(ADW_ACTION_ROW(row), gtk_image_new_from_icon_name("folder-documents-symbolic"));
            gtk_list_box_append(GTK_LIST_BOX(m_listRecentAccountsOnStart), row);
        }
        adw_status_page_set_description(ADW_STATUS_PAGE(m_pageStatusNoAccounts), "");
        gtk_box_prepend(GTK_BOX(m_boxStatusPage), m_listRecentAccountsOnStart);
        gtk_box_prepend(GTK_BOX(m_boxStatusPage), m_lblRecentAccounts);
        gtk_widget_set_margin_top(m_boxStatusPage, 24);
    }
}

void MainWindow::onAccountAdded()
{
    g_simple_action_set_enabled(m_actCloseAccount, true);
    adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(m_viewStack), "pageTabs");
    std::unique_ptr<AccountView> newAccountView{ std::make_unique<AccountView>(GTK_WINDOW(m_gobj), m_tabView, m_controller.createAccountViewControllerForLatestAccount()) };
    adw_tab_view_set_selected_page(m_tabView, newAccountView->gobj());
    m_accountViews.push_back(std::move(newAccountView));
    adw_window_title_set_subtitle(ADW_WINDOW_TITLE(m_adwTitle), m_controller.getNumberOfOpenAccounts() == 1 ? m_controller.getFirstOpenAccountPath().c_str() : nullptr);
    updateRecentAccounts();
    gtk_widget_set_visible(m_btnMenuAccount, true);
}

void MainWindow::onNewAccount()
{
    gtk_popover_popdown(GTK_POPOVER(m_popoverAccount));
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
            if(mainWindow->m_controller.isAccountOpened(path))
            {
                adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(mainWindow->m_toastOverlay), adw_toast_new(_("Unable to override an opened account.")));
            }
            else
            {
                if(std::filesystem::exists(path))
                {
                    std::filesystem::remove(path);
                }
                mainWindow->m_controller.addAccount(path);
            }
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(saveFileDialog));
}

void MainWindow::onOpenAccount()
{
    gtk_popover_popdown(GTK_POPOVER(m_popoverAccount));
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
    gtk_popover_popdown(GTK_POPOVER(m_popoverAccount));
    adw_tab_view_close_page(m_tabView, adw_tab_view_get_selected_page(m_tabView));
}

void MainWindow::onPreferences()
{
    PreferencesDialog preferencesDialog{ GTK_WINDOW(m_gobj), m_controller.createPreferencesDialogController() };
    preferencesDialog.run();
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

void MainWindow::updateRecentAccounts()
{
    for(GtkWidget* row : m_listRecentAccountsRows)
    {
        gtk_list_box_remove(GTK_LIST_BOX(m_listRecentAccounts), row);
    }
    m_listRecentAccountsRows.clear();
    for(const std::string& recentAccountPath : m_controller.getRecentAccounts())
    {
        GtkWidget* row{ adw_action_row_new() };
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), std::filesystem::path(recentAccountPath).filename().c_str());
        adw_action_row_set_subtitle(ADW_ACTION_ROW(row), std::regex_replace(recentAccountPath, std::regex("\\&"), "&amp;").c_str());
        adw_action_row_add_prefix(ADW_ACTION_ROW(row), gtk_image_new_from_icon_name("folder-documents-symbolic"));
        gtk_list_box_append(GTK_LIST_BOX(m_listRecentAccounts), row);
        m_listRecentAccountsRows.push_back(row);
    }
}

void MainWindow::onListRecentAccountsSelectionChanged()
{
    GtkListBoxRow* selectedRow{ gtk_list_box_get_selected_row(GTK_LIST_BOX(m_listRecentAccounts)) };
    if(selectedRow)
    {
        gtk_popover_popdown(GTK_POPOVER(m_popoverAccount));
        std::string path{ adw_action_row_get_subtitle(ADW_ACTION_ROW(selectedRow)) };
        m_controller.addAccount(path);
        gtk_list_box_unselect_all(GTK_LIST_BOX(m_listRecentAccounts));
    }
}

void MainWindow::onListRecentAccountsOnStartSelectionChanged()
{
    GtkListBoxRow* selectedRow{ gtk_list_box_get_selected_row(GTK_LIST_BOX(m_listRecentAccountsOnStart)) };
    if(selectedRow)
    {
        std::string path{ adw_action_row_get_subtitle(ADW_ACTION_ROW(selectedRow)) };
        m_controller.addAccount(path);
        gtk_list_box_unselect_all(GTK_LIST_BOX(m_listRecentAccountsOnStart));
    }
}

