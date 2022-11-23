#include "transferdialog.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Views;

TransferDialog::TransferDialog(GtkWindow* parent, TransferDialogController& controller) : m_controller{ controller }, m_gobj{ adw_message_dialog_new(parent, _("Transfer"), nullptr) }
{
    //Dialog Settings
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(m_gobj), "cancel", _("Cancel"), "ok", _("OK"), nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(m_gobj), "ok", ADW_RESPONSE_SUGGESTED);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(m_gobj), "ok");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    g_signal_connect(m_gobj, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog*, gchar* response, gpointer data) { reinterpret_cast<TransferDialog*>(data)->setResponse({ response }); })), this);
    //Preferences Group
    m_preferencesGroup = adw_preferences_group_new();
    //Layout
    adw_message_dialog_set_extra_child(ADW_MESSAGE_DIALOG(m_gobj), m_preferencesGroup);
}

GtkWidget* TransferDialog::gobj()
{
    return m_gobj;
}

bool TransferDialog::run()
{
    gtk_widget_show(m_gobj);
    gtk_window_set_modal(GTK_WINDOW(m_gobj), true);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    gtk_window_destroy(GTK_WINDOW(m_gobj));
    return m_controller.getResponse() == "ok";
}

void TransferDialog::setResponse(const std::string& response)
{
    m_controller.setResponse(response);
}
