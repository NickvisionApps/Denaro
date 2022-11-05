#include "groupdialog.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Views;

GroupDialog::GroupDialog(GtkWindow* parent, NickvisionMoney::Controllers::GroupDialogController& controller) : m_controller{ controller }, m_gobj{ adw_message_dialog_new(parent, "Group", nullptr) }
{
    //Dialog Settings
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(m_gobj), "cancel", _("Cancel"), "ok", _("OK"), nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(m_gobj), "ok", ADW_RESPONSE_SUGGESTED);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(m_gobj), "ok");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    g_signal_connect(m_gobj, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog*, gchar* response, gpointer data) { reinterpret_cast<GroupDialog*>(data)->setResponse({ response }); })), this);
    //Preferences Group
    m_preferencesGroup = adw_preferences_group_new();
    //Name
    m_rowName = adw_entry_row_new();
    gtk_widget_set_size_request(m_rowName, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowName), _("Name"));
    adw_entry_row_set_activates_default(ADW_ENTRY_ROW(m_rowName), true);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowName);
    //Description
    m_rowDescription = adw_entry_row_new();
    gtk_widget_set_size_request(m_rowDescription, 420, -1);
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDescription), _("Description"));
    adw_entry_row_set_activates_default(ADW_ENTRY_ROW(m_rowDescription), true);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowDescription);
    //Layout
    adw_message_dialog_set_extra_child(ADW_MESSAGE_DIALOG(m_gobj), m_preferencesGroup);
    //Load Transaction
    gtk_editable_set_text(GTK_EDITABLE(m_rowName), m_controller.getName().c_str());
    gtk_editable_set_text(GTK_EDITABLE(m_rowDescription), m_controller.getDescription().c_str());
}

GtkWidget* GroupDialog::gobj()
{
    return m_gobj;
}

bool GroupDialog::run()
{
    gtk_widget_show(m_gobj);
    gtk_widget_grab_focus(m_rowName);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    if(m_controller.getResponse() == "ok")
    {
        gtk_widget_hide(m_gobj);
        GroupCheckStatus status{ m_controller.updateGroup(gtk_editable_get_text(GTK_EDITABLE(m_rowName)), gtk_editable_get_text(GTK_EDITABLE(m_rowDescription))) };
        //Invalid Group
        if(status != GroupCheckStatus::Valid)
        {
            //Reset UI
            gtk_style_context_remove_class(gtk_widget_get_style_context(m_rowName), "error");
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowName), _("Name"));
            gtk_style_context_remove_class(gtk_widget_get_style_context(m_rowDescription), "error");
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDescription), _("Description"));
            //Mark Error
            if(status == GroupCheckStatus::EmptyName)
            {
                gtk_style_context_add_class(gtk_widget_get_style_context(m_rowName), "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowName), _("Name (Empty)"));
            }
            else if(status == GroupCheckStatus::EmptyDescription)
            {
                gtk_style_context_add_class(gtk_widget_get_style_context(m_rowDescription), "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowDescription), _("Description (Empty)"));
            }
            return run();
        }
    }
    gtk_window_destroy(GTK_WINDOW(m_gobj));
    return m_controller.getResponse() == "ok";
}

void GroupDialog::setResponse(const std::string& response)
{
    m_controller.setResponse(response);
}
