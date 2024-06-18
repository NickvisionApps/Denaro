#include "views/accountpage.h"
#include <libnick/localization/gettext.h>
#include "helpers/builder.h"
#include "helpers/dialogptr.h"
#include "views/accountsettingsdialog.h"

using namespace Nickvision::Events;
using namespace Nickvision::Money::GNOME::Helpers;
using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Views
{
    AccountPage::AccountPage(const std::shared_ptr<AccountViewController>& controller, GtkWindow* parent)
        : m_controller(controller),
        m_builder{ BuilderHelpers::fromBlueprint("account_page") },
        m_parent{ parent },
        m_page{ ADW_VIEW_STACK(gtk_builder_get_object(m_builder, "root")) },
        m_actionGroup{ g_simple_action_group_new() }
    {
        gtk_widget_insert_action_group(GTK_WIDGET(m_page), "account", G_ACTION_GROUP(m_actionGroup));
        //New Transaction Action
        GSimpleAction* actTransaction{ g_simple_action_new("newTransaction", nullptr) };
        g_signal_connect(actTransaction, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->newTransaction(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actTransaction));
        //New Group Action
        GSimpleAction* actGroup{ g_simple_action_new("newGroup", nullptr) };
        g_signal_connect(actGroup, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->newGroup(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actGroup));
        //Transfer Action
        GSimpleAction* actTransfer{ g_simple_action_new("transfer", nullptr) };
        g_signal_connect(actTransfer, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->transfer(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actTransfer));
        //Import From File Action
        GSimpleAction* actImport{ g_simple_action_new("import", nullptr) };
        g_signal_connect(actImport, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->importFromFile(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actImport));
        //Export To CSV Action
        GSimpleAction* actExportCSV{ g_simple_action_new("exportCSV", nullptr) };
        g_signal_connect(actExportCSV, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->exportToCSV(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actExportCSV));
        //Export To PDF Action
        GSimpleAction* actExportPDF{ g_simple_action_new("exportPDF", nullptr) };
        g_signal_connect(actExportPDF, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->exportToPDF(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actExportPDF));
        //Account Settings Action
        GSimpleAction* actSettings{ g_simple_action_new("settings", nullptr) };
        g_signal_connect(actSettings, "activate", G_CALLBACK(+[](GSimpleAction*, GVariant*, gpointer data){ reinterpret_cast<AccountPage*>(data)->accountSettings(); }), this);
        g_action_map_add_action(G_ACTION_MAP(m_actionGroup), G_ACTION(actSettings));
        //Load overview amounts
        adw_preferences_group_set_title(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "overviewGroup")), m_controller->getMetadata().getName().c_str());
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewTotalLabel")), m_controller->getTotalAmountString().c_str());
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewIncomeLabel")), m_controller->getIncomeAmountString().c_str());
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "overviewExpenseLabel")), m_controller->getExpenseAmountString().c_str());
        //Load overview reminders
        std::vector<TransactionReminder> reminders{ m_controller->getTransactionReminders() };
        if(reminders.empty())
        {
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), _("No Transaction Reminders"));
            adw_action_row_add_prefix(row, gtk_image_new_from_icon_name("bell-outline-symbolic"));
            adw_preferences_group_add(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "remindersGroup")), GTK_WIDGET(row));
        }
        for(const TransactionReminder& reminder : reminders)
        {
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), reminder.getDescription().c_str());
            adw_action_row_set_subtitle(row, reminder.getWhenString().c_str());
            adw_action_row_add_prefix(row, gtk_image_new_from_icon_name("bell-symbolic"));
            adw_action_row_add_suffix(row, gtk_label_new(reminder.getAmountString().c_str()));
        }
        //Load overview groups
        for(const std::pair<Group, std::string>& pair : m_controller->getGroups())
        {
            GdkPixbuf* colorBuf{ gdk_pixbuf_new(GDK_COLORSPACE_RGB, false, 8, 1, 1) };
            gdk_pixbuf_fill(colorBuf, pair.first.getColor().toHex(false));
            GdkTexture* colorTexture{ gdk_texture_new_for_pixbuf(colorBuf) };
            AdwAvatar* colorAvatar{ ADW_AVATAR(adw_avatar_new(8, nullptr, false)) };
            gtk_widget_set_valign(GTK_WIDGET(colorAvatar), GTK_ALIGN_CENTER);
            adw_avatar_set_custom_image(ADW_AVATAR(colorAvatar), GDK_PAINTABLE(colorTexture));
            AdwActionRow* row{ ADW_ACTION_ROW(adw_action_row_new()) };
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(row), pair.first.getName().c_str());
            if(!pair.first.getDescription().empty())
            {
                adw_action_row_set_subtitle(row, pair.first.getDescription().c_str());
            }
            adw_action_row_add_prefix(row, GTK_WIDGET(colorAvatar));
            adw_action_row_add_suffix(row, gtk_label_new(pair.second.c_str()));
            if(pair.first.getName() != _("Ungrouped"))
            {
                GtkButton* editButton{ GTK_BUTTON(gtk_button_new_from_icon_name("document-edit-symbolic")) };
                gtk_widget_set_tooltip_text(GTK_WIDGET(editButton), _("Edit Group"));
                gtk_widget_set_valign(GTK_WIDGET(editButton), GTK_ALIGN_CENTER);
                gtk_widget_add_css_class(GTK_WIDGET(editButton), "flat");
                adw_action_row_add_suffix(row, GTK_WIDGET(editButton));
                adw_action_row_set_activatable_widget(row, GTK_WIDGET(editButton));
            }
            adw_preferences_group_add(ADW_PREFERENCES_GROUP(gtk_builder_get_object(m_builder, "groupsGroup")), GTK_WIDGET(row));
            g_object_unref(colorTexture);
            g_object_unref(colorBuf);
        }
    }

    AccountPage::~AccountPage()
    {
        g_object_unref(m_actionGroup);
        g_object_unref(m_builder);
    }

    AdwViewStack* AccountPage::gobj()
    {
        return m_page;
    }

    const std::string& AccountPage::getTitle() const
    {
        return m_controller->getMetadata().getName();
    }

    void AccountPage::newTransaction()
    {

    }

    void AccountPage::newGroup()
    {

    }

    void AccountPage::transfer()
    {

    }

    void AccountPage::importFromFile()
    {

    }

    void AccountPage::exportToCSV()
    {

    }

    void AccountPage::exportToPDF()
    {

    }

    void AccountPage::accountSettings()
    {
        DialogPtr<AccountSettingsDialog> settingsDialog{ m_controller->createAccountSettingsDialogController(), m_parent };
        settingsDialog->closed() += [&](const EventArgs& args)
        {
            m_controller->accountNameChanged().invoke({ m_controller->getMetadata().getName() });
        };
        settingsDialog->present();
    }
}
