#include "views/mainwindow.h"
#include <libnick/app/appinfo.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/notifications/shellnotification.h>
#include <libnick/localization/gettext.h>
#include <libnick/localization/documentation.h>
#include "helpers/builder.h"
#include "helpers/dialogptr.h"
#include "views/newaccountdialog.h"
#include "views/preferencesdialog.h"

using namespace Nickvision;
using namespace Nickvision::App;
using namespace Nickvision::Events;
using namespace Nickvision::Localization;
using namespace Nickvision::Money::GNOME::Controls;
using namespace Nickvision::Money::GNOME::Helpers;
using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;
using namespace Nickvision::Notifications;

namespace Nickvision::Money::GNOME::Views
{
    enum Pages
    {
        Home = 0,
        CurrencyConverter = 1,
        Dashboard = 3
    };

    MainWindow::MainWindow(const std::shared_ptr<MainWindowController>& controller, GtkApplication* app)
        : m_controller{ controller },
        m_app{ app },
        m_builder{ BuilderHelpers::fromBlueprint("main_window") },
        m_window{ ADW_APPLICATION_WINDOW(gtk_builder_get_object(m_builder, "root")) },
        m_currencyConverterPage{ GTK_WINDOW(m_window) }
    {
        //Setup Window
        if(m_window == nullptr)
        {
            throw std::runtime_error("Failed to setup window"); // Panic and exit.
        }
        gtk_application_add_window(GTK_APPLICATION(app), GTK_WINDOW(m_window));
        gtk_window_set_title(GTK_WINDOW(m_window), m_controller->getAppInfo().getShortName().c_str());
        gtk_window_set_icon_name(GTK_WINDOW(m_window), m_controller->getAppInfo().getId().c_str());
        if(m_controller->isDevVersion())
        {
            gtk_widget_add_css_class(GTK_WIDGET(m_window), "devel");
        }
        adw_navigation_page_set_title(ADW_NAVIGATION_PAGE(gtk_builder_get_object(m_builder, "navPageSidebar")), m_controller->getAppInfo().getShortName().c_str());
        adw_status_page_set_title(ADW_STATUS_PAGE(gtk_builder_get_object(m_builder, "statusPageHome")), m_controller->getGreeting().c_str());
        //Register Events
        g_signal_connect(m_window, "close_request", G_CALLBACK(+[](GtkWindow*, gpointer data) -> bool { return reinterpret_cast<MainWindow*>(data)->onCloseRequested(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "listNavItems"), "row-activated", G_CALLBACK(+[](GtkListBox*, GtkListBoxRow*, gpointer data) { adw_navigation_split_view_set_show_content(ADW_NAVIGATION_SPLIT_VIEW(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "navView")), true); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "listNavItems"), "row-selected", G_CALLBACK(+[](GtkListBox* self, GtkListBoxRow* row, gpointer data) { reinterpret_cast<MainWindow*>(data)->onNavItemSelected(self, row); }), this);
        m_controller->notificationSent() += [&](const NotificationSentEventArgs& args) { onNotificationSent(args); };
        m_controller->shellNotificationSent() += [&](const ShellNotificationSentEventArgs& args) { onShellNotificationSent(args); };
        m_controller->recentAccountsChanged() += [&](const ParamEventArgs<std::vector<RecentAccount>>& args) { loadRecentAccounts(args); };
        m_controller->accountAdded() += [&](const ParamEventArgs<const std::shared_ptr<AccountViewController>&>& args) { onAccountAdded(args); };
        //Drop Target
        GtkDropTarget* dropTarget{ gtk_drop_target_new(G_TYPE_FILE, GDK_ACTION_COPY) };
        g_signal_connect(dropTarget, "drop", G_CALLBACK(+[](GtkDropTarget*, const GValue* value, double, double, gpointer data) -> bool { return reinterpret_cast<MainWindow*>(data)->onDrop(value); }), this);
        gtk_widget_add_controller(GTK_WIDGET(m_window), GTK_EVENT_CONTROLLER(dropTarget));
        //Quit Action
        GSimpleAction* actQuit{ g_simple_action_new("quit", nullptr) };
        g_signal_connect(actQuit, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->quit(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actQuit));
        SET_ACCEL_FOR_ACTION(m_app, "win.quit", "<Ctrl>Q");
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
        //Preferences Action
        GSimpleAction* actHelp{ g_simple_action_new("help", nullptr) };
        g_signal_connect(actHelp, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->help(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actHelp));
        SET_ACCEL_FOR_ACTION(m_app, "win.help", "F1");
        //About Action
        GSimpleAction* actAbout{ g_simple_action_new("about", nullptr) };
        g_signal_connect(actAbout, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->about(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actAbout));
        SET_ACCEL_FOR_ACTION(m_app, "win.about", "F1");
        //New Account Action
        GSimpleAction* actNewAccount{ g_simple_action_new("newAccount", nullptr) };
        g_signal_connect(actNewAccount, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->newAccount(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actNewAccount));
        SET_ACCEL_FOR_ACTION(m_app, "win.newAccount", "<Ctrl>N");
        //Open Account Action
        GSimpleAction* actOpenAccount{ g_simple_action_new("openAccount", nullptr) };
        g_signal_connect(actOpenAccount, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_window), G_ACTION(actOpenAccount));
        SET_ACCEL_FOR_ACTION(m_app, "win.openAccount", "<Ctrl>O");
        //Recent Account Callbacks
        g_signal_connect(gtk_builder_get_object(m_builder, "recentAccount1PopoverRow"), "activated", G_CALLBACK(+[](AdwActionRow* row, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(row))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removeRecentAccount1PopoverButton"), "clicked", G_CALLBACK(+[](GtkButton* button, gpointer data){ reinterpret_cast<MainWindow*>(data)->removeRecentAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "recentAccount1PopoverRow")))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "recentAccount2PopoverRow"), "activated", G_CALLBACK(+[](AdwActionRow* row, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(row))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removeRecentAccount2PopoverButton"), "clicked", G_CALLBACK(+[](GtkButton* button, gpointer data){ reinterpret_cast<MainWindow*>(data)->removeRecentAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "recentAccount2PopoverRow")))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "recentAccount3PopoverRow"), "activated", G_CALLBACK(+[](AdwActionRow* row, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(row))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removeRecentAccount3PopoverButton"), "clicked", G_CALLBACK(+[](GtkButton* button, gpointer data){ reinterpret_cast<MainWindow*>(data)->removeRecentAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "recentAccount3PopoverRow")))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "recentAccount1Row"), "activated", G_CALLBACK(+[](AdwActionRow* row, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(row))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removeRecentAccount1Button"), "clicked", G_CALLBACK(+[](GtkButton* button, gpointer data){ reinterpret_cast<MainWindow*>(data)->removeRecentAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "recentAccount1Row")))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "recentAccount2Row"), "activated", G_CALLBACK(+[](AdwActionRow* row, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(row))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removeRecentAccount2Button"), "clicked", G_CALLBACK(+[](GtkButton* button, gpointer data){ reinterpret_cast<MainWindow*>(data)->removeRecentAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "recentAccount2Row")))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "recentAccount3Row"), "activated", G_CALLBACK(+[](AdwActionRow* row, gpointer data){ reinterpret_cast<MainWindow*>(data)->openAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(row))); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removeRecentAccount3Button"), "clicked", G_CALLBACK(+[](GtkButton* button, gpointer data){ reinterpret_cast<MainWindow*>(data)->removeRecentAccount(gtk_widget_get_tooltip_text(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<MainWindow*>(data)->m_builder, "recentAccount3Row")))); }), this);
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
        gtk_list_box_select_row(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "listNavItems")), gtk_list_box_get_row_at_index(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "listNavItems")), 0));
    }

    bool MainWindow::onCloseRequested()
    {
        return false;
    }

    bool MainWindow::onDrop(const GValue* value)
    {
        if(G_VALUE_HOLDS(value, G_TYPE_FILE))
        {
            openAccount(g_file_get_path(G_FILE(g_value_get_object(value))));
            return true;
        }
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

    void MainWindow::preferences()
    {
        DialogPtr<PreferencesDialog> dialog{ m_controller->createPreferencesViewController(), GTK_WINDOW(m_window) };
        dialog->present();
    }

    void MainWindow::keyboardShortcuts()
    {
        GtkBuilder* builderHelp{ BuilderHelpers::fromBlueprint("shortcuts_dialog") };
        GtkShortcutsWindow* shortcuts{ GTK_SHORTCUTS_WINDOW(gtk_builder_get_object(builderHelp, "root")) };
        gtk_window_set_transient_for(GTK_WINDOW(shortcuts), GTK_WINDOW(m_window));
        gtk_window_set_icon_name(GTK_WINDOW(shortcuts), m_controller->getAppInfo().getId().c_str());
        g_signal_connect(shortcuts, "close-request", G_CALLBACK(+[](GtkWindow*, gpointer data){ g_object_unref(reinterpret_cast<GtkBuilder*>(data)); }), builderHelp);
        gtk_window_present(GTK_WINDOW(shortcuts));
    }

    void MainWindow::help()
    {
        std::string helpUrl{ Documentation::getHelpUrl("index") };
        GtkUriLauncher* launcher{ gtk_uri_launcher_new(helpUrl.c_str()) };
        gtk_uri_launcher_launch(launcher, GTK_WINDOW(m_window), nullptr, GAsyncReadyCallback(+[](GObject* source, GAsyncResult* res, gpointer) { gtk_uri_launcher_launch_finish(GTK_URI_LAUNCHER(source), res, nullptr); }), nullptr);
    }

    void MainWindow::about()
    {
        std::string extraDebug;
        extraDebug += "GTK " + std::to_string(gtk_get_major_version()) + "." + std::to_string(gtk_get_minor_version()) + "." + std::to_string(gtk_get_micro_version()) + "\n";
        extraDebug += "libadwaita " + std::to_string(adw_get_major_version()) + "." + std::to_string(adw_get_minor_version()) + "." + std::to_string(adw_get_micro_version());
        AdwAboutDialog* dialog{ ADW_ABOUT_DIALOG(adw_about_dialog_new()) };
        adw_about_dialog_set_application_name(dialog, m_controller->getAppInfo().getShortName().c_str());
        adw_about_dialog_set_application_icon(dialog, std::string(m_controller->getAppInfo().getId() + (m_controller->isDevVersion() ? "-devel" : "")).c_str());
        adw_about_dialog_set_developer_name(dialog, "Nickvision");
        adw_about_dialog_set_version(dialog, m_controller->getAppInfo().getVersion().toString().c_str());
        adw_about_dialog_set_release_notes(dialog, m_controller->getAppInfo().getHtmlChangelog().c_str());
        adw_about_dialog_set_debug_info(dialog, m_controller->getDebugInformation(extraDebug).c_str());
        adw_about_dialog_set_comments(dialog, m_controller->getAppInfo().getDescription().c_str());
        adw_about_dialog_set_license_type(dialog, GTK_LICENSE_GPL_3_0);
        adw_about_dialog_set_copyright(dialog, "© Nickvision 2021-2024");
        adw_about_dialog_set_website(dialog, "https://nickvision.org/");
        adw_about_dialog_set_issue_url(dialog, m_controller->getAppInfo().getIssueTracker().c_str());
        adw_about_dialog_set_support_url(dialog, m_controller->getAppInfo().getSupportUrl().c_str());
        adw_about_dialog_add_link(dialog, _("GitHub Repo"), m_controller->getAppInfo().getSourceRepo().c_str());
        for(const std::pair<std::string, std::string>& pair : m_controller->getAppInfo().getExtraLinks())
        {
            adw_about_dialog_add_link(dialog, pair.first.c_str(), pair.second.c_str());
        }
        std::vector<const char*> urls;
        std::vector<std::string> developers{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getDevelopers()) };
        for(const std::string& developer : developers)
        {
            urls.push_back(developer.c_str());
        }
        urls.push_back(nullptr);
        adw_about_dialog_set_developers(dialog, &urls[0]);
        urls.clear();
        std::vector<std::string> designers{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getDesigners()) };
        for(const std::string& designer : designers)
        {
            urls.push_back(designer.c_str());
        }
        urls.push_back(nullptr);
        adw_about_dialog_set_designers(dialog, &urls[0]);
        urls.clear();
        std::vector<std::string> artists{ AppInfo::convertUrlMapToVector(m_controller->getAppInfo().getArtists()) };
        for(const std::string& artist : artists)
        {
            urls.push_back(artist.c_str());
        }
        urls.push_back(nullptr);
        adw_about_dialog_set_artists(dialog, &urls[0]);
        adw_about_dialog_set_translator_credits(dialog, m_controller->getAppInfo().getTranslatorCredits().c_str());
        adw_dialog_present(ADW_DIALOG(dialog), GTK_WIDGET(m_window));
    }

    void MainWindow::onNavItemSelected(GtkListBox* box, GtkListBoxRow* row)
    {
        adw_navigation_split_view_set_show_content(ADW_NAVIGATION_SPLIT_VIEW(gtk_builder_get_object(m_builder, "navView")), true);
        //Reset content header and switcher bar
        adw_header_bar_set_title_widget(ADW_HEADER_BAR(gtk_builder_get_object(m_builder, "contentHeaderBar")), nullptr);
        adw_view_switcher_bar_set_stack(ADW_VIEW_SWITCHER_BAR(gtk_builder_get_object(m_builder, "contentSwitcherBar")), nullptr);
        if(row == gtk_list_box_get_row_at_index(box, Pages::Home))
        {
            adw_navigation_page_set_title(ADW_NAVIGATION_PAGE(gtk_builder_get_object(m_builder, "navPageContent")), _("Home"));
            adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "viewStack")), "home");
        }
        else if(row == gtk_list_box_get_row_at_index(box, Pages::CurrencyConverter))
        {
            adw_navigation_page_set_title(ADW_NAVIGATION_PAGE(gtk_builder_get_object(m_builder, "navPageContent")), _("Currency Converter"));
            adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "viewStack")), "custom");
            adw_bin_set_child(ADW_BIN(gtk_builder_get_object(m_builder, "customBin")), GTK_WIDGET(m_currencyConverterPage.gobj()));
        }
        else if(row == gtk_list_box_get_row_at_index(box, Pages::Dashboard))
        {
            adw_navigation_page_set_title(ADW_NAVIGATION_PAGE(gtk_builder_get_object(m_builder, "navPageContent")), _("Dashboard"));
            if(m_controller->hasOpenAccounts())
            {
                adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "viewStack")), "custom");
                adw_bin_set_child(ADW_BIN(gtk_builder_get_object(m_builder, "customBin")), nullptr);
            }
            else
            {
                adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "viewStack")), "noAccounts");
            }
        }
        else //Account
        {
            const std::shared_ptr<AccountPage>& page{ m_accountPages.at(gtk_widget_get_tooltip_text(GTK_WIDGET(row))) };
            //Set AccountPage as child
            adw_navigation_page_set_title(ADW_NAVIGATION_PAGE(gtk_builder_get_object(m_builder, "navPageContent")), page->getTitle().c_str());
            adw_view_stack_set_visible_child_name(ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "viewStack")), "custom");
            adw_bin_set_child(ADW_BIN(gtk_builder_get_object(m_builder, "customBin")), GTK_WIDGET(page->gobj()));
            //Set content header and switcher bar for AccountPage's viewstack
            if(!adw_navigation_split_view_get_collapsed(ADW_NAVIGATION_SPLIT_VIEW(gtk_builder_get_object(m_builder, "navView"))))
            {
                AdwViewSwitcher* viewSwitcher{ ADW_VIEW_SWITCHER(adw_view_switcher_new()) };
                adw_view_switcher_set_policy(viewSwitcher, ADW_VIEW_SWITCHER_POLICY_WIDE);
                adw_view_switcher_set_stack(viewSwitcher, page->gobj());
                adw_header_bar_set_title_widget(ADW_HEADER_BAR(gtk_builder_get_object(m_builder, "contentHeaderBar")), GTK_WIDGET(viewSwitcher));
            }
            adw_view_switcher_bar_set_stack(ADW_VIEW_SWITCHER_BAR(gtk_builder_get_object(m_builder, "contentSwitcherBar")), page->gobj());
        }
    }

    void MainWindow::onAccountAdded(const ParamEventArgs<const std::shared_ptr<AccountViewController>&>& args)
    {
        //Create sidebar item for account
        GtkBox* row{ GTK_BOX(gtk_box_new(GTK_ORIENTATION_HORIZONTAL, 12)) };
        gtk_widget_set_margin_start(GTK_WIDGET(row), 6);
        gtk_widget_set_margin_top(GTK_WIDGET(row), 6);
        gtk_widget_set_margin_end(GTK_WIDGET(row), 6);
        gtk_widget_set_margin_bottom(GTK_WIDGET(row), 6);
        GtkImage* img{ GTK_IMAGE(gtk_image_new_from_icon_name("wallet2-symbolic")) };
        gtk_box_append(row, GTK_WIDGET(img));
        GtkLabel* lbl{ GTK_LABEL(gtk_label_new(args.getParam()->getMetadata().getName().c_str())) };
        gtk_box_append(row, GTK_WIDGET(lbl));
        gtk_list_box_append(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "listNavItems")), GTK_WIDGET(row));
        //Handle account name change event
        args.getParam()->accountNameChanged() += [lbl](const ParamEventArgs<std::string>& args){ gtk_label_set_text(lbl, args.getParam().c_str()); };
        //Create view for account
        m_accountPages[args.getParam()->getPath()] = std::make_shared<AccountPage>(args.getParam(), GTK_WINDOW(m_window));
        GtkListBoxRow* listRow{ gtk_list_box_get_row_at_index(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "listNavItems")), m_accountPages.size() + Pages::Dashboard) };
        gtk_widget_set_tooltip_text(GTK_WIDGET(listRow), args.getParam()->getPath().string().c_str());
        gtk_list_box_select_row(GTK_LIST_BOX(gtk_builder_get_object(m_builder, "listNavItems")), listRow);
    }

    void MainWindow::newAccount()
    {
        gtk_popover_popdown(GTK_POPOVER(gtk_builder_get_object(m_builder, "accountMenuPopover")));
        DialogPtr<NewAccountDialog> newAccountDialog{ m_controller->createNewAccountDialogController(), GTK_WINDOW(m_window) };
        newAccountDialog->created() += [&](const ParamEventArgs<std::shared_ptr<NewAccountDialogController>>& args)
        {
            m_controller->newAccount(args.getParam());
        };
        newAccountDialog->present();
    }

    void MainWindow::openAccount()
    {
        gtk_popover_popdown(GTK_POPOVER(gtk_builder_get_object(m_builder, "accountMenuPopover")));
        GtkFileDialog* fileDialog{ gtk_file_dialog_new() };
        gtk_file_dialog_set_title(fileDialog, _("Open Account"));
        GtkFileFilter* filter{ gtk_file_filter_new() };
        gtk_file_filter_set_name(filter, _("Nickvision Denaro Account (*.nmoney)"));
        gtk_file_filter_add_pattern(filter, "*.nmoney");
        gtk_file_filter_add_pattern(filter, "*.NMONEY");
        GListStore* filters{ g_list_store_new(gtk_file_filter_get_type()) };
        g_list_store_append(filters, G_OBJECT(filter));
        gtk_file_dialog_set_filters(fileDialog, G_LIST_MODEL(filters));
        gtk_file_dialog_open(fileDialog, GTK_WINDOW(m_window), nullptr, GAsyncReadyCallback(+[](GObject* self, GAsyncResult* res, gpointer data) 
        {
            GFile* file{ gtk_file_dialog_open_finish(GTK_FILE_DIALOG(self), res, nullptr) };
            if(file)
            {
                reinterpret_cast<MainWindow*>(data)->openAccount(g_file_get_path(file));
            }
        }), this);
    }

    void MainWindow::loadRecentAccounts(const ParamEventArgs<std::vector<RecentAccount>>& args)
    {
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "noRecentAccountsPopoverRow")), args.getParam().size() == 0);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "recentAccount1PopoverRow")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "recentAccount2PopoverRow")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "recentAccount3PopoverRow")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "noRecentAccountsRow")), args.getParam().size() == 0);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "recentAccount1Row")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "recentAccount2Row")), false);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "recentAccount3Row")), false);
        for(size_t i = 0; i < args.getParam().size(); i++)
        {
            const RecentAccount& recentAccount{ args.getParam().at(i) };
            std::string popoverRowName{ "recentAccount" + std::to_string(i + 1) + "PopoverRow" };
            std::string rowName{ "recentAccount" + std::to_string(i + 1) + "Row" };
            AdwActionRow* popoverRow{ ADW_ACTION_ROW(gtk_builder_get_object(m_builder, popoverRowName.c_str())) };
            AdwActionRow* row{ ADW_ACTION_ROW(gtk_builder_get_object(m_builder, rowName.c_str())) };
            gtk_widget_set_visible(GTK_WIDGET(popoverRow), true);
            gtk_widget_set_visible(GTK_WIDGET(row), true);
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(popoverRow), recentAccount.getName().c_str());
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), recentAccount.getName().c_str());
            switch(recentAccount.getType())
            {
            case AccountType::Checking:
                adw_action_row_set_subtitle(popoverRow, _("Checking"));
                adw_action_row_set_subtitle(row, _("Checking"));
                break;
            case AccountType::Savings:
                adw_action_row_set_subtitle(popoverRow, _("Savings"));
                adw_action_row_set_subtitle(row, _("Savings"));
                break;
            case AccountType::Business:
                adw_action_row_set_subtitle(popoverRow, _("Business"));
                adw_action_row_set_subtitle(row, _("Business"));
                break;
            }
            gtk_widget_set_tooltip_text(GTK_WIDGET(popoverRow), recentAccount.getPath().string().c_str());
            gtk_widget_set_tooltip_text(GTK_WIDGET(row), recentAccount.getPath().string().c_str());
        }
    }

    void MainWindow::removeRecentAccount(const std::filesystem::path& path)
    {
        m_controller->removeRecentAccount(path);
    }

    void MainWindow::openAccount(const std::filesystem::path& path)
    {
        gtk_popover_popdown(GTK_POPOVER(gtk_builder_get_object(m_builder, "accountMenuPopover")));
        if(StringHelpers::toLower(path.extension().string()) != ".nmoney")
        {
            return;
        }
        std::string password;
        //Get password if needed
        if(m_controller->isAccountPasswordProtected(path))
        {
            bool done{ false };
            GValue valTrue;
            g_value_init(&valTrue, G_TYPE_BOOLEAN);
            g_value_set_boolean(&valTrue, true);
            GValue valPlaceholderText;
            g_value_init(&valPlaceholderText, G_TYPE_STRING);
            g_value_set_string(&valPlaceholderText, _("Enter password here"));
            GtkPasswordEntry* passwordEntry{ GTK_PASSWORD_ENTRY(gtk_password_entry_new()) };
            gtk_password_entry_set_show_peek_icon(passwordEntry, true);
            g_object_set_property(G_OBJECT(passwordEntry), "activates-default", &valTrue);
            g_object_set_property(G_OBJECT(passwordEntry), "placeholder-text", &valPlaceholderText);
            g_signal_connect(passwordEntry, "changed", G_CALLBACK(+[](GtkEditable* self, gpointer data){ *(reinterpret_cast<std::string*>(data)) = gtk_editable_get_text(self); }), &password);
            AdwAlertDialog* passwordDialog{ ADW_ALERT_DIALOG(adw_alert_dialog_new(path.filename().string().c_str(), nullptr)) };
            adw_alert_dialog_set_extra_child(passwordDialog, GTK_WIDGET(passwordEntry));
            adw_alert_dialog_add_responses(passwordDialog, "cancel", _("Cancel"), "unlock", _("Unlock"), nullptr);
            adw_alert_dialog_set_default_response(passwordDialog, "unlock");
            adw_alert_dialog_set_close_response(passwordDialog, "cancel");
            adw_alert_dialog_set_response_appearance(passwordDialog, "unlock", ADW_RESPONSE_SUGGESTED);
            g_signal_connect(passwordDialog, "response", G_CALLBACK(+[](AdwAlertDialog* self, const char* response, gpointer data)
            {
                if(std::string(response) != "unlock")
                {
                    *(reinterpret_cast<std::string*>(data)) = "";
                }
                adw_dialog_force_close(ADW_DIALOG(self));
            }), &password);
            g_signal_connect(passwordDialog, "closed", G_CALLBACK(+[](AdwDialog*,gpointer data){ *(reinterpret_cast<bool*>(data)) = true; }), &done);
            adw_dialog_present(ADW_DIALOG(passwordDialog), GTK_WIDGET(m_window));
            while(!done)
            {
                g_main_context_iteration(g_main_context_default(), false);
            }
        }
        //Open account
        m_controller->openAccount(path, password);
    }
}
