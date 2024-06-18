#include "views/accountsettingsdialog.h"
#include <libnick/localization/gettext.h>
#include "models/currencycheckstatus.h"

using namespace Nickvision::Money::GNOME::Helpers;
using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Views
{
    AccountSettingsDialog::AccountSettingsDialog(const std::shared_ptr<AccountSettingsDialogController>& controller, GtkWindow* parent)
        : DialogBase{ parent, "account_settings_dialog" },
        m_controller{ controller }
    {
        //Load
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "accountNameRow")), m_controller->getMetadata().getName().c_str());
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "accountTypeRow")), static_cast<unsigned int>(m_controller->getMetadata().getType()));
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionTypeRow")), static_cast<unsigned int>(m_controller->getMetadata().getDefaultTransactionType()));
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionRemindersRow")), static_cast<unsigned int>(m_controller->getMetadata().getTransactionRemindersThreshold()));
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(gtk_builder_get_object(m_builder, "systemCurrencyRow")), m_controller->getReportedCurrencyString().c_str());
        adw_expander_row_set_enable_expansion(ADW_EXPANDER_ROW(gtk_builder_get_object(m_builder, "customCurrencyRow")), m_controller->getMetadata().getUseCustomCurrency());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customSymbolRow")), m_controller->getMetadata().getCustomCurrency().getSymbol().c_str());
        gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customCodeRow")), m_controller->getMetadata().getCustomCurrency().getCode().c_str());
        switch(m_controller->getMetadata().getCustomCurrency().getDecimalSeparator())
        {
        case '.':
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), 0);
            break;
        case ',':
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), 1);
            break;
        default:
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), 2);
            gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry")), true);
            std::string separator{ std::to_string(m_controller->getMetadata().getCustomCurrency().getDecimalSeparator()) };
            gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry")), separator.c_str());
            break;
        }
        switch(m_controller->getMetadata().getCustomCurrency().getGroupSeparator())
        {
        case '.':
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), 0);
            break;
        case ',':
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), 1);
            break;
        case '\'':
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), 2);
            break;
        case '\0':
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), 3);
            break;
        default:
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), 4);
            gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), true);
            std::string separator{ std::to_string(m_controller->getMetadata().getCustomCurrency().getGroupSeparator()) };
            gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), separator.c_str());
            break;
        }
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customDecimalDigitsRow")), static_cast<unsigned int>(m_controller->getMetadata().getCustomCurrency().getDecimalDigits() - 2));
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customAmountStyleRow")), static_cast<unsigned int>(m_controller->getMetadata().getCustomCurrency().getAmountStyle()));
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "removePasswordRow")), m_controller->isEncrypted());
        //Signals
        g_signal_connect(gtk_builder_get_object(m_builder, "accountNameRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onAccountInfoChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountTypeRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onAccountInfoChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "transactionTypeRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onAccountInfoChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "transactionRemindersRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onAccountInfoChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customCurrencyRow"), "notify::enable-expansion", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customSymbolRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customCodeRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customGroupSeparatorRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customDecimalDigitsRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customAmountStyleRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "newPasswordRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onNewPasswordChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "newPasswordConfirmRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->onNewPasswordChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "changePasswordRow"), "activated", G_CALLBACK(+[](AdwActionRow*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->changePassword(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "removePasswordRow"), "activated", G_CALLBACK(+[](AdwActionRow*, gpointer data){ reinterpret_cast<AccountSettingsDialog*>(data)->removePassword(); }), this);
    }

    void AccountSettingsDialog::onAccountInfoChanged()
    {
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "accountNameRow")), "error");
        if(!m_controller->setName(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "accountNameRow")))))
        {
            gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "accountNameRow")), "error");
        }
        m_controller->setAccountType(static_cast<AccountType>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "accountTypeRow")))));
        m_controller->setDefaultTransactionType(static_cast<TransactionType>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionTypeRow")))));
        m_controller->setTransactionRemindersThreshold(static_cast<RemindersThreshold>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionRemindersRow")))));
    }

    void AccountSettingsDialog::onCurrencyChange()
    {
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customSymbolRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customCodeRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), "error");
        if(!adw_expander_row_get_enable_expansion(ADW_EXPANDER_ROW(gtk_builder_get_object(m_builder, "customCurrencyRow"))))
        {
            m_controller->setCustomCurrencyOff();
        }
        else
        {
            std::string symbol{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customSymbolRow"))) };
            std::string code{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customCodeRow"))) };
            char decimalSeparator;
            switch(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow"))))
            {
            case 0:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry")), false);
                decimalSeparator = '.';
                break;
            case 1:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry")), false);
                decimalSeparator = ',';
                break;
            default:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry")), true);
                decimalSeparator = gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry")))[0];
                break;
            }
            char groupSeparator;
            switch(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customGroupSeparatorRow"))))
            {
            case 0:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), false);
                groupSeparator = '.';
                break;
            case 1:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), false);
                groupSeparator = ',';
                break;
            case 2:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), false);
                groupSeparator = '\'';
                break;
            case 3:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), false);
                groupSeparator = '\0';
                break;
            default:
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")), true);
                groupSeparator = gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry")))[0];
                break;
            }
            int decimalDigits{ static_cast<int>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customDecimalDigitsRow")))) + 2 };
            AmountStyle amountStyle{ static_cast<AmountStyle>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "customAmountStyleRow")))) };
            CurrencyCheckStatus status{ m_controller->setCustomCurrency(symbol, code, decimalSeparator, groupSeparator, decimalDigits, amountStyle) };
            if(status == CurrencyCheckStatus::EmptySymbol)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customSymbolRow")), "error");
            }
            else if(status == CurrencyCheckStatus::EmptyCode)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customCodeRow")), "error");
            }
            else if(status == CurrencyCheckStatus::EmptyDecimalSeparator)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), "error");
            }
            else if(status == CurrencyCheckStatus::SameSeparators)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), "error");
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), "error");
            }
            else if(status == CurrencyCheckStatus::SameSymbolAndDecimalSeparator)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customSymbolRow")), "error");
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), "error");
            }
            else if(status == CurrencyCheckStatus::SameSymbolAndGroupSeparator)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customSymbolRow")), "error");
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), "error");
            }
        }
    }

    void AccountSettingsDialog::onNewPasswordChange()
    {
        std::string newPassword{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "newPasswordRow"))) };
        std::string newPasswordConfirm{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "newPasswordConfirmRow"))) };
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "newPasswordRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "newPasswordConfirmRow")), "error");
        if(newPassword.empty())
        {
            gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "newPasswordRow")), "error");
            gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "changePasswordRow")), false);
        }
        else if(newPassword == newPasswordConfirm)
        {
            gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "changePasswordRow")), true);
        }
        else
        {
            gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "newPasswordConfirmRow")), "error");
            gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "changePasswordRow")), false);
        }
    }

    void AccountSettingsDialog::changePassword()
    {
        if(m_controller->setPassword(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "newPasswordRow")))))
        {
            gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "removePasswordRow")), true);
            adw_preferences_dialog_add_toast(ADW_PREFERENCES_DIALOG(m_dialog), adw_toast_new(_("Password changed")));
            gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "newPasswordRow")), "");
            gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "newPasswordConfirmRow")), "");
        }
        else
        {
            adw_preferences_dialog_add_toast(ADW_PREFERENCES_DIALOG(m_dialog), adw_toast_new(_("Unable to change password")));
        }
    }

    void AccountSettingsDialog::removePassword()
    {
        if(m_controller->setPassword(""))
        {
            gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "removePasswordRow")), false);
            adw_preferences_dialog_add_toast(ADW_PREFERENCES_DIALOG(m_dialog), adw_toast_new(_("Password removed")));
        }
        else
        {
            adw_preferences_dialog_add_toast(ADW_PREFERENCES_DIALOG(m_dialog), adw_toast_new(_("Unable to change password")));
        }
    }
}
