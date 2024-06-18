#include "controls/currencyconverterpage.h"
#include <libnick/localization/gettext.h>
#include "helpers/builder.h"
#include "helpers/currencyhelpers.h"
#include "models/currencyconversionservice.h"

using namespace Nickvision::Money::Shared;
using namespace Nickvision::Money::Shared::Models;

namespace Nickvision::Money::GNOME::Controls
{
    CurrencyConverterPage::CurrencyConverterPage(GtkWindow* parent)
        : m_builder{ BuilderHelpers::fromBlueprint("currency_converter_page") },
        m_page{ ADW_CLAMP(gtk_builder_get_object(m_builder, "root")) },
        m_currencyList{ gtk_string_list_new(nullptr) }
    {
        //Signals
        g_signal_connect(gtk_builder_get_object(m_builder, "switchButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<CurrencyConverterPage*>(data)->switchCurrencies(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "sourceCurrencyRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<CurrencyConverterPage*>(data)->onSourceCurrencyChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "resultCurrencyRow"), "notify::selected-item", G_CALLBACK(+[](GObject*, GParamSpec*, gpointer data){ reinterpret_cast<CurrencyConverterPage*>(data)->onResultCurrencyChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "sourceAmountRow"), "changed", G_CALLBACK(+[](GtkEditable*, gpointer data){ reinterpret_cast<CurrencyConverterPage*>(data)->onSourceAmountChanged(); }), this);
        g_signal_connect(gtk_builder_get_object(m_builder, "copyResultButton"), "clicked", G_CALLBACK(+[](GtkButton*, gpointer data){ reinterpret_cast<CurrencyConverterPage*>(data)->copyResult(); }), this);
        //Load
        const std::map<std::string, double>& conversionRates{ CurrencyConversionService::getConversionRates("USD") };
        if(conversionRates.empty())
        {
            AdwAlertDialog* alertDialog{ ADW_ALERT_DIALOG(adw_alert_dialog_new(_("Error"), _("Unable to load currency data. Please try again. If the error still persists, report a bug."))) };
            adw_alert_dialog_add_response(alertDialog, "close", _("Close"));
            adw_alert_dialog_set_default_response(alertDialog, "close");
            adw_alert_dialog_set_close_response(alertDialog, "close");
            g_signal_connect(alertDialog, "response", G_CALLBACK(+[](AdwAlertDialog* self, const char*, gpointer){ adw_dialog_force_close(ADW_DIALOG(self)); }), nullptr);
            adw_dialog_present(ADW_DIALOG(alertDialog), GTK_WIDGET(parent));
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
        }
    }

    CurrencyConverterPage::~CurrencyConverterPage()
    {
        g_object_unref(m_currencyList);
        g_object_unref(m_builder);
    }

    AdwClamp* CurrencyConverterPage::gobj()
    {
        return m_page;
    }

    void CurrencyConverterPage::switchCurrencies()
    {
        unsigned int sourceIndex{ adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow"))) };
        unsigned int resultIndex{ adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow"))) };
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")), sourceIndex);
        adw_combo_row_set_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")), resultIndex);
    }

    void CurrencyConverterPage::onSourceCurrencyChanged()
    {
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(gtk_builder_get_object(m_builder, "sourceAmountRow")), gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")))));
        onCurrencyChange();
    }

    void CurrencyConverterPage::onResultCurrencyChanged()
    {
        adw_preferences_row_set_title(ADW_PREFERENCES_ROW(gtk_builder_get_object(m_builder, "resultAmountRow")), gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")))));
        onCurrencyChange();
    }

    void CurrencyConverterPage::onSourceAmountChanged()
    {
        onCurrencyChange();
    }

    void CurrencyConverterPage::copyResult()
    {
        std::string resultText{ adw_action_row_get_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(m_builder, "resultAmountRow"))) };
        if(!resultText.empty())
        {
            GdkClipboard* clipboard{ gtk_widget_get_clipboard(GTK_WIDGET(gtk_builder_get_object(m_builder, "resultAmountRow"))) };
            gdk_clipboard_set_text(clipboard, resultText.c_str());
            adw_toast_overlay_add_toast(ADW_TOAST_OVERLAY(gtk_builder_get_object(m_builder, "toastOverlay")), adw_toast_new(_("Result was copied to clipboard.")));
        }
    }

    void CurrencyConverterPage::onCurrencyChange()
    {
        std::string sourceText{ gtk_editable_get_text(GTK_EDITABLE(gtk_builder_get_object(m_builder, "sourceAmountRow"))) };
        gtk_widget_remove_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "resultAmountRow")), "error");
        if(sourceText.empty())
        {
            adw_action_row_set_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(m_builder, "resultAmountRow")), "");
        }
        else
        {
            double sourceAmount{ CurrencyHelpers::toAmount(sourceText, CurrencyHelpers::getSystemCurrency()) };
            std::string sourceCurrency{ gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "sourceCurrencyRow")))) };
            std::string resultCurrency{ gtk_string_list_get_string(m_currencyList, adw_combo_row_get_selected(ADW_COMBO_ROW(gtk_builder_get_object(m_builder, "resultCurrencyRow")))) };
            std::optional<CurrencyConversion> conversion{ CurrencyConversionService::convert(sourceCurrency, sourceAmount, resultCurrency) };
            if (conversion.has_value())
            {
                std::string resultAmountString{ CurrencyHelpers::toAmountString(conversion->getResultAmount(), CurrencyHelpers::getSystemCurrency(), false, true) };
                adw_action_row_set_subtitle(ADW_ACTION_ROW(gtk_builder_get_object(m_builder, "resultAmountRow")), resultAmountString.c_str());
            }
            else
            {
                gtk_widget_add_css_class(GTK_WIDGET(gtk_builder_get_object(m_builder, "resultAmountRow")), "error");
            }
        }
    }
}