#include "controls/currencyconverterdialog.h"
#include <libnick/localization/gettext.h>
#include "helpers/builder.h"
#include "helpers/currencyhelpers.h"
#include "models/currencyconversionservice.h"

using namespace Nickvision::Money::Shared;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Controls
{
    CurrencyConverterDialog::CurrencyConverterDialog(GtkWindow* parent, const std::string& iconName)
        : m_builder{ BuilderHelpers::fromBlueprint("currency_converter_dialog") },
        m_parent{ parent },
        m_window{ ADW_WINDOW(gtk_builder_get_object(m_builder, "root")) },
        m_currencyList{ gtk_string_list_new(nullptr) }
    {
        gtk_window_set_transient_for(GTK_WINDOW(m_window), m_parent);
        gtk_window_set_icon_name(GTK_WINDOW(m_window), iconName.c_str());
        //Signals
        g_signal_connect(gtk_builder_get_object(m_builder, "switchButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<CurrencyConverterDialog*>(data)->switchCurrencies(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "sourceCurrencyRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<CurrencyConverterDialog*>(data)->onSourceCurrencyChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "resultCurrencyRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<CurrencyConverterDialog*>(data)->onResultCurrencyChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "sourceAmountRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<CurrencyConverterDialog*>(data)->onSourceAmountChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "copyResultButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<CurrencyConverterDialog*>(data)->copyResult(); }), this);
    }

    CurrencyConverterDialog::~CurrencyConverterDialog()
    {
        g_object_unref(m_currencyList);
        gtk_window_destroy(GTK_WINDOW(m_window));
        g_object_unref(m_builder);
    }

    void CurrencyConverterDialog::run()
    {
        const std::map<std::string, double>& conversionRates{ CurrencyConversionService::getConversionRates("USD") };
        if(conversionRates.empty())
        {
            AdwMessageDialog* messageDialog{ ADW_MESSAGE_DIALOG(adw_message_dialog_new(m_parent, _("Error"), _("Unable to load currency data. Please try again. If the error still persists, report a bug."))) };
            adw_message_dialog_add_response(messageDialog, "close", _("Close"));
            adw_message_dialog_set_default_response(messageDialog, "close");
            adw_message_dialog_set_close_response(messageDialog, "close");
            g_signal_connect(messageDialog, "response", G_CALLBACK(+[](AdwMessageDialog* self, const char*, gpointer){ gtk_window_destroy(GTK_WINDOW(self)); }), nullptr);
            gtk_window_present(GTK_WINDOW(messageDialog));
        }
        else
        {
            unsigned int usdIndex{ 0 };
            unsigned int eurIndex{ 0 };
            unsigned int i{ 0 };
            for(const std::pair<const std::string, double>& pair : conversionRates)
            {
                gtk_string_list_append(m_currencyList, pair.first.c_str());
                if(pair.first == "USD")
                {
                    usdIndex = i;
                }
                else if(pair.first == "EUR")
                {
                    eurIndex = i;
                }
                i++;
            }
            adw_combo_row_set_model(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")), G_LIST_MODEL(m_currencyList));
            adw_combo_row_set_model(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")), G_LIST_MODEL(m_currencyList));
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")), usdIndex);
            adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")), eurIndex);
            gtk_window_present(GTK_WINDOW(m_window));
            while(gtk_widget_is_visible(GTK_WIDGET(m_window)))
            {
                g_main_context_iteration(g_main_context_default(), false);
            }
        }
    }

    void CurrencyConverterDialog::switchCurrencies()
    {
        unsigned int sourceIndex{ adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow"))) };
        unsigned int resultIndex{ adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow"))) };
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")), sourceIndex);
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")), resultIndex);
    }

    void CurrencyConverterDialog::onSourceCurrencyChanged()
    {
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(gtk_builder_get_object(m_builder, "sourceAmountRow")), gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")))));
        onCurrencyChange();
    }

    void CurrencyConverterDialog::onResultCurrencyChanged()
    {
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(gtk_builder_get_object(m_builder, "resultAmountRow")), gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")))));
        onCurrencyChange();
    }

    void CurrencyConverterDialog::onSourceAmountChanged()
    {
        onCurrencyChange();
    }

    void CurrencyConverterDialog::copyResult()
    {
        std::string resultText{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "resultAmountRow"))) };
        if(!resultText.empty())
        {
            GdkClipboard* clipboard{ gtk_widget_get_clipboard(GTK_WIDGET(gtk_builder_get_object(m_builder, "resultAmountRow"))) };
            gdk_clipboard_set_text(clipboard, resultText.c_str());
            adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(gtk_builder_get_object(m_builder, "toastOverlay")), adw_toast_new(_("Result was copied to clipboard.")));
        }
    }

    void CurrencyConverterDialog::onCurrencyChange()
    {
        std::string sourceText{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "sourceAmountRow"))) };
        std::string resultText{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "resultAmountRow"))) };
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "sourceAmountRow")), "error");
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "resultAmountRow")), "error");
        if(sourceText.empty())
        {
            gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "resultAmountRow")), "");
        }
        else
        {
            double sourceAmount{ 0 };
            try
            {
                sourceAmount = std::stod(sourceText);
            }
            catch(const std::exception&)
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "sourceAmountRow")), "error");
                gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "resultAmountRow")), "");
                return;
            }
            std::string sourceCurrency{ gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")))) };
            std::string resultCurrency{ gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")))) };
            std::optional<CurrencyConversion> conversion{ CurrencyConversionService::convert(sourceCurrency, sourceAmount, resultCurrency) };
            if (conversion.has_value())
            {
                std::string resultAmountString{ CurrencyHelpers::toAmountString(conversion->getResultAmount(), CurrencyHelpers::getSystemCurrency(), false, true) };
                gtk_editable_set_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "resultAmountRow")), resultAmountString.c_str());
            }
            else
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "resultAmountRow")), "error");
            }
        }
    }
}