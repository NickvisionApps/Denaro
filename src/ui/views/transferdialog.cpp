#include "transferdialog.hpp"
#include "../../helpers/translation.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::UI::Views;

TransferDialog::TransferDialog(GtkWindow* parent, TransferDialogController& controller) : m_controller{ controller }, m_gobj{ adw_message_dialog_new(parent, _("Transfer"), nullptr) }
{
    //Dialog Settings
    gtk_window_set_hide_on_close(GTK_WINDOW(m_gobj), true);
    adw_message_dialog_set_body(ADW_MESSAGE_DIALOG(m_gobj), _("Transferring money will create an expense transaction with the given amount in this account and an income transaction with the given amount in the account to transfer to."));
    adw_message_dialog_add_responses(ADW_MESSAGE_DIALOG(m_gobj), "cancel", _("Cancel"), "ok", _("OK"), nullptr);
    adw_message_dialog_set_response_appearance(ADW_MESSAGE_DIALOG(m_gobj), "ok", ADW_RESPONSE_SUGGESTED);
    adw_message_dialog_set_default_response(ADW_MESSAGE_DIALOG(m_gobj), "ok");
    adw_message_dialog_set_close_response(ADW_MESSAGE_DIALOG(m_gobj), "cancel");
    g_signal_connect(m_gobj, "response", G_CALLBACK((void (*)(AdwMessageDialog*, gchar*, gpointer))([](AdwMessageDialog*, gchar* response, gpointer data) { reinterpret_cast<TransferDialog*>(data)->setResponse({ response }); })), this);
    //Preferences Group
    m_preferencesGroup = adw_preferences_group_new();
    gtk_widget_set_margin_top(m_preferencesGroup, 10);
    //Transfer Account Label
    m_lblTransferAccount = gtk_label_new(_("No Account Selected"));
    gtk_widget_set_valign(m_lblTransferAccount, GTK_ALIGN_CENTER);
    gtk_widget_set_margin_start(m_lblTransferAccount, 20);
    //Select Account Button
    m_btnSelectAccount = gtk_button_new();
    gtk_widget_add_css_class(m_btnSelectAccount, "flat");
    gtk_widget_set_valign(m_btnSelectAccount, GTK_ALIGN_CENTER);
    gtk_button_set_icon_name(GTK_BUTTON(m_btnSelectAccount), "document-open-symbolic");
    gtk_widget_set_tooltip_text(m_btnSelectAccount, _("Select Account"));
    g_signal_connect(m_btnSelectAccount, "clicked", G_CALLBACK((void (*)(GtkButton*, gpointer))[](GtkButton*, gpointer data) { reinterpret_cast<TransferDialog*>(data)->onSelectAccount(); }), this);
    //Transfer Account
    m_rowTransferAccount = adw_action_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTransferAccount), _("Transfer Account"));
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowTransferAccount), m_lblTransferAccount);
    adw_action_row_add_suffix(ADW_ACTION_ROW(m_rowTransferAccount), m_btnSelectAccount);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowTransferAccount);
    //Amount
    m_rowAmount = adw_entry_row_new();
    adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), _("Amount"));
    adw_entry_row_set_activates_default(ADW_ENTRY_ROW(m_rowAmount), true);
    adw_preferences_group_add(ADW_PREFERENCES_GROUP(m_preferencesGroup), m_rowAmount);
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
    gtk_widget_grab_focus(m_rowAmount);
    while(gtk_widget_is_visible(m_gobj))
    {
        g_main_context_iteration(g_main_context_default(), false);
    }
    if(m_controller.getResponse() == "ok")
    {
        gtk_widget_hide(m_gobj);
        gtk_window_set_modal(GTK_WINDOW(m_gobj), false);
        TransferCheckStatus status{ m_controller.updateTransfer(gtk_label_get_text(GTK_LABEL(m_lblTransferAccount)), gtk_editable_get_text(GTK_EDITABLE(m_rowAmount))) };
        //Invalid Transfer
        if(status != TransferCheckStatus::Valid)
        {
            //Reset UI
            gtk_widget_remove_css_class(m_rowTransferAccount, "error");
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTransferAccount), _("Transfer Account"));
            gtk_widget_remove_css_class(m_rowAmount, "error");
            adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), _("Amount"));
            //Mark Error
            if(status == TransferCheckStatus::InvalidDestPath)
            {
                gtk_widget_add_css_class(m_rowTransferAccount, "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowTransferAccount), _("Transfer Account (Invalid)"));
            }
            else if(status == TransferCheckStatus::InvalidAmount)
            {
                gtk_widget_add_css_class(m_rowAmount, "error");
                adw_preferences_row_set_title(ADW_PREFERENCES_ROW(m_rowAmount), _("Amount (Invalid)"));
            }
            return run();
        }
    }
    gtk_window_destroy(GTK_WINDOW(m_gobj));
    return m_controller.getResponse() == "ok";
}

void TransferDialog::setResponse(const std::string& response)
{
    m_controller.setResponse(response);
}

void TransferDialog::onSelectAccount()
{
    GtkFileChooserNative* openFileDialog{ gtk_file_chooser_native_new(_("Select Account"), GTK_WINDOW(m_gobj), GTK_FILE_CHOOSER_ACTION_OPEN, _("_Open"), _("_Cancel")) };
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
            TransferDialog* transferDialog{ reinterpret_cast<TransferDialog*>(data) };
            GFile* file{ gtk_file_chooser_get_file(GTK_FILE_CHOOSER(dialog)) };
            std::string path{ g_file_get_path(file) };
            if(path != transferDialog->m_controller.getSourceAccountPath())
            {
                gtk_label_set_text(GTK_LABEL(transferDialog->m_lblTransferAccount), path.c_str());
            }
            g_object_unref(file);
        }
        g_object_unref(dialog);
    })), this);
    gtk_native_dialog_show(GTK_NATIVE_DIALOG(openFileDialog));
}
