#include "views/newaccountdialog.h"
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"

using namespace Nickvision::Events;
using namespace Nickvision::Money::GNOME::Helpers;
using namespace Nickvision::Money::Shared;
using namespace Nickvision::Money::Shared::Controllers;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Views
{
    NewAccountDialog::NewAccountDialog(const std::shared_ptr<NewAccountDialogController>& controller, GtkWindow* parent)
        : DialogBase{ parent, "new_account_dialog" },
        m_controller{ controller },
        m_currentPageNumber{ 0 }
    {
        //Load
        adw_action_row_set_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(m_builder, "accountFolderRow")), m_controller->getFolder().filename().string().c_str());
        adw_switch_row_set_active(ADW_SWITCH_ROW(gtk_builder_get_object(m_builder, "accountOverwriteRow")), m_controller->getOverwriteExisting());
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "accountTypeRow")), static_cast<unsigned int>(m_controller->getMetadata().getType()));
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionTypeRow")), static_cast<unsigned int>(m_controller->getMetadata().getDefaultTransactionType()));
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionRemindersRow")), static_cast<unsigned int>(m_controller->getMetadata().getTransactionRemindersThreshold()));
        std::string reportedCurrency{ _("Your system reported that your currency is") };
        reportedCurrency += "\n<b>" + CurrencyHelpers::getSystemCurrency().toString() + "</b>";
        gtk_label_set_label(GTK_LABEL(gtk_builder_get_object(m_builder, "reportedCurrencyLabel")), reportedCurrency.c_str());
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
        //Signals
        g_signal_connect(gtk_builder_get_object(m_builder, "backButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->goBack(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "startButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->goForward(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountNameRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onAccountNameChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "selectAccountFolderButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->selectAccountFolder(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountOverwriteRow"), "activated", G_CALLBACK(+[](AdwActionRow*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onAccountOverwriteChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "storagePageNextButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->goForward(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "accountOptionsPageNextButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->goForward(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customCurrencyRow"), "notify::enable-expansion", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customSymbolRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customCodeRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customDecimalSeparatorEntry"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customGroupSeparatorRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customGroupSeparatorEntry"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customDecimalDigitsRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customAmountStyleRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->onCurrencyChange(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "customCurrencyPageNextButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->goForward(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "selectImportFileButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->selectImportFile(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "clearImportFileButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->clearImportFile(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "importPageCreateButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<NewAccountDialog*>(data)->finish(); }), this);
    }

    Event<ParamEventArgs<std::shared_ptr<NewAccountDialogController>>>& NewAccountDialog::finished()
    {
        return m_finished;
    }

    void NewAccountDialog::goBack()
    {
        if(m_currentPageNumber > 0)
        {
            m_currentPageNumber--;
        }
        AdwCarousel* carousel{ ADW_CAROUSEL(gtk_builder_get_object(m_builder, "carousel")) };
        adw_carousel_scroll_to(carousel, adw_carousel_get_nth_page(carousel, m_currentPageNumber), true);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "backButton")), m_currentPageNumber > 0);
    }

    void NewAccountDialog::goForward()
    {
        if(m_currentPageNumber < 4)
        {
            m_currentPageNumber++;
        }
        AdwCarousel* carousel{ ADW_CAROUSEL(gtk_builder_get_object(m_builder, "carousel")) };
        adw_carousel_scroll_to(carousel, adw_carousel_get_nth_page(carousel, m_currentPageNumber), true);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "backButton")), m_currentPageNumber > 0);
    }

    void NewAccountDialog::onAccountNameChanged()
    {
        gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "storagePageNextButton")), m_controller->setName(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "accountNameRow")))));
    }

    void NewAccountDialog::selectAccountFolder()
    {
        GtkFileDialog* folderDialog{ gtk_file_dialog_new() };
        gtk_file_dialog_set_title(folderDialog, _("Select Folder"));
        gtk_file_dialog_select_folder(folderDialog, m_parent, nullptr, GAsyncReadyCallback(+[](GObject* self, GAsyncResult* res, gpointer data)
        {
            GFile* folder{ gtk_file_dialog_select_folder_finish(GTK_FILE_DIALOG(self), res, nullptr) };
            if(folder)
            {
                NewAccountDialog* dialog{ reinterpret_cast<NewAccountDialog*>(data) };
                std::filesystem::path folderPath{ g_file_get_path(folder) };
                bool result{ dialog->m_controller->setFolder(folderPath) };
                if(result)
                {
                    adw_action_row_set_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(dialog->m_builder, "accountFolderRow")), folderPath.filename().string().c_str());
                }
                gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(dialog->m_builder, "storagePageNextButton")), result && !dialog->m_controller->getMetadata().getName().empty());
            }
        }), this);
    }

    void NewAccountDialog::onAccountOverwriteChanged()
    {
        m_controller->setOverwriteExisting(adw_switch_row_get_active(ADW_SWITCH_ROW(gtk_builder_get_object(m_builder, "accountOverwriteRow"))));
        GtkWidget* nextButton{ GTK_WIDGET(gtk_builder_get_object(m_builder, "storagePageNextButton")) };
        gtk_widget_set_sensitive(nextButton, gtk_widget_get_sensitive(nextButton) && (m_controller->getOverwriteExisting() ? true : !std::filesystem::exists(m_controller->getFilePath())));
    }

    void NewAccountDialog::onCurrencyChange()
    {
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customSymbolRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customCodeRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customDecimalSeparatorRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "customGroupSeparatorRow")), "error");
        if(!adw_expander_row_get_enable_expansion(ADW_EXPANDER_ROW(gtk_builder_get_object(m_builder, "customCurrencyRow"))))
        {
            m_controller->setCustomCurrencyOff();
            gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "customCurrencyPageNextButton")), true);
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
            gtk_widget_set_sensitive(GTK_WIDGET(gtk_builder_get_object(m_builder, "customCurrencyPageNextButton")), status == CurrencyCheckStatus::Valid);
        }
    }

    void NewAccountDialog::selectImportFile()
    {
        GtkFileDialog* fileDialog{ gtk_file_dialog_new() };
        gtk_file_dialog_set_title(fileDialog, _("Select Import File"));
        GtkFileFilter* filterAll{ gtk_file_filter_new() };
        gtk_file_filter_set_name(filterAll, _("All Files (*.csv, *.ofx, *.qif)"));
        gtk_file_filter_add_pattern(filterAll, "*.csv");
        gtk_file_filter_add_pattern(filterAll, "*.CSV");
        gtk_file_filter_add_pattern(filterAll, "*.ofx");
        gtk_file_filter_add_pattern(filterAll, "*.OFX");
        gtk_file_filter_add_pattern(filterAll, "*.qif");
        gtk_file_filter_add_pattern(filterAll, "*.QIF");
        GtkFileFilter* filterCSV{ gtk_file_filter_new() };
        gtk_file_filter_set_name(filterCSV, _("CSV (*.csv)"));
        gtk_file_filter_add_pattern(filterCSV, "*.csv");
        gtk_file_filter_add_pattern(filterCSV, "*.CSV");
        GtkFileFilter* filterOFX{ gtk_file_filter_new() };
        gtk_file_filter_set_name(filterOFX, _("Open Financial Exchange (*.ofx)"));
        gtk_file_filter_add_pattern(filterOFX, "*.ofx");
        gtk_file_filter_add_pattern(filterOFX, "*.OFX");
        GtkFileFilter* filterQIF{ gtk_file_filter_new() };
        gtk_file_filter_set_name(filterQIF, _("Quicken Interchange Format (*.qif)"));
        gtk_file_filter_add_pattern(filterQIF, "*.qif");
        gtk_file_filter_add_pattern(filterQIF, "*.QIF");
        GListStore* filters{ g_list_store_new(gtk_file_filter_get_type()) };
        g_list_store_append(filters, G_OBJECT(filterAll));
        g_list_store_append(filters, G_OBJECT(filterCSV));
        g_list_store_append(filters, G_OBJECT(filterOFX));
        g_list_store_append(filters, G_OBJECT(filterQIF));
        gtk_file_dialog_set_filters(fileDialog, G_LIST_MODEL(filters));
        gtk_file_dialog_open(fileDialog, m_parent, nullptr, GAsyncReadyCallback(+[](GObject* self, GAsyncResult* res, gpointer data) 
        {
            GFile* file{ gtk_file_dialog_open_finish(GTK_FILE_DIALOG(self), res, nullptr) };
            if(file)
            {
                std::filesystem::path filePath{ g_file_get_path(file) };
                reinterpret_cast<NewAccountDialog*>(data)->m_controller->setImportFile(filePath);
                adw_action_row_set_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(reinterpret_cast<NewAccountDialog*>(data)->m_builder, "importFileRow")), filePath.filename().string().c_str());
                gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(reinterpret_cast<NewAccountDialog*>(data)->m_builder, "clearImportFileButton")), true);
            }
        }), this);

    }

    void NewAccountDialog::clearImportFile()
    {
        m_controller->setImportFile({});
        adw_action_row_set_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(m_builder, "importFileRow")), nullptr);
        gtk_widget_set_visible(GTK_WIDGET(gtk_builder_get_object(m_builder, "clearImportFileButton")), false);
    }

    void NewAccountDialog::finish()
    {
        m_controller->setPassword(gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "accountPasswordRow"))));
        m_controller->setAccountType(static_cast<AccountType>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "accountTypeRow")))));
        m_controller->setDefaultTransactionType(static_cast<TransactionType>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionTypeRow")))));
        m_controller->setTransactionRemindersThreshold(static_cast<RemindersThreshold>(adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "transactionRemindersRow")))));
        m_finished.invoke(m_controller);
        adw_dialog_close(m_dialog);
    }
}
