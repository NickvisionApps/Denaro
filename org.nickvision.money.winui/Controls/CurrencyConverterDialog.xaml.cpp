#include "Controls/CurrencyConverterDialog.xaml.h"
#if __has_include("Controls/CurrencyConverterDialog.g.cpp")
#include "Controls/CurrencyConverterDialog.g.cpp"
#endif
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"
#include "models/currencyconversionservice.h"

using namespace ::Nickvision::Money::Shared;
using namespace ::Nickvision::Money::Shared::Models;
using namespace winrt::Microsoft::UI::Xaml;
using namespace winrt::Microsoft::UI::Xaml::Controls;
using namespace winrt::Windows::ApplicationModel::DataTransfer;

namespace winrt::Nickvision::Money::WinUI::Controls::implementation 
{
    CurrencyConverterDialog::CurrencyConverterDialog()
    {
        InitializeComponent();
        Title(winrt::box_value(winrt::to_hstring(_("Currency Converter"))));
        CloseButtonText(winrt::to_hstring(_("Close")));
        LblCurrency().Text(winrt::to_hstring(_("Currency")));
        RowSourceCurrency().Title(winrt::to_hstring(_("Source")));
        RowResultCurrency().Title(winrt::to_hstring(_("Result")));
        ToolTipService::SetToolTip(BtnSwitch(), winrt::box_value(winrt::to_hstring(_("Switch Currencies"))));
        LblAmount().Text(winrt::to_hstring(_("Amount")));
        TxtSourceAmount().PlaceholderText(winrt::to_hstring(_("Enter source amount")));
        TxtResultAmount().PlaceholderText(L"0");
        ToolTipService::SetToolTip(BtnCopy(), winrt::box_value(winrt::to_hstring(_("Copy Result Amount"))));
    }

    Windows::Foundation::IAsyncAction CurrencyConverterDialog::OnOpened(const ContentDialog& sender, const ContentDialogOpenedEventArgs& args)
    {
        const std::map<std::string, double>& conversionRates{ CurrencyConversionService::getConversionRates("USD") };
        if(conversionRates.empty())
        {
            Hide();
            ContentDialog noRatesDialog;
            noRatesDialog.Title(winrt::box_value(winrt::to_hstring(_("Error"))));
            noRatesDialog.Content(winrt::box_value(winrt::to_hstring(_("Unable to load currency data. Please try again. If the error still persists, report a bug."))));
            noRatesDialog.CloseButtonText(winrt::to_hstring(_("OK")));
            noRatesDialog.DefaultButton(ContentDialogButton::Close);
            noRatesDialog.XamlRoot(XamlRoot());
            co_await noRatesDialog.ShowAsync();
        }
        else
        {
            int i{ 0 };
            for(const std::pair<const std::string, double>& pair : conversionRates)
            {
                CmbSourceCurrency().Items().Append(winrt::box_value(winrt::to_hstring(pair.first)));
                CmbResultCurrency().Items().Append(winrt::box_value(winrt::to_hstring(pair.first)));
                if(pair.first == "USD")
                {
                    CmbSourceCurrency().SelectedIndex(i);
                }
                else if(pair.first == "EUR")
                {
                    CmbResultCurrency().SelectedIndex(i);
                }
                i++;
            }
        }
    }

    void CurrencyConverterDialog::OnCmbSourceCurrencyChanged(const IInspectable& sender, const SelectionChangedEventArgs& args)
    {
        RowSourceAmount().Title(CmbSourceCurrency().SelectedItem().as<winrt::hstring>());
        onCurrencyChange();
    }

    void CurrencyConverterDialog::OnCmbResultCurrencyChanged(const IInspectable& sender, const SelectionChangedEventArgs& args)
    {
        RowResultAmount().Title(CmbResultCurrency().SelectedItem().as<winrt::hstring>());
        onCurrencyChange();
    }

    void CurrencyConverterDialog::SwitchCurrencies(const IInspectable& sender, const RoutedEventArgs& args)
    {
        int sourceIndex{ CmbSourceCurrency().SelectedIndex() };
        int resultIndex{ CmbResultCurrency().SelectedIndex() };
        CmbResultCurrency().SelectedIndex(sourceIndex);
        CmbSourceCurrency().SelectedIndex(resultIndex);
    }

    void CurrencyConverterDialog::OnTxtSourceAmountChanged(const IInspectable& sender, const TextChangedEventArgs& args)
    {
        onCurrencyChange();
    }

    void CurrencyConverterDialog::CopyResult(const IInspectable& sender, const RoutedEventArgs& args)
    {
        DataPackage dataPackage;
        dataPackage.SetText(TxtResultAmount().Text());
        Clipboard::SetContent(dataPackage);
    }

    void CurrencyConverterDialog::onCurrencyChange()
    {
        RowResultCurrency().Title(winrt::to_hstring(_("Result")));
        if(TxtSourceAmount().Text().empty())
        {
            TxtResultAmount().Text(L"");
        }
        else
        {
            double sourceAmount{ CurrencyHelpers::toAmount(winrt::to_string(TxtSourceAmount().Text()), CurrencyHelpers::getSystemCurrency())};
            std::string sourceCurrency{ winrt::to_string(CmbSourceCurrency().SelectedItem().as<winrt::hstring>()) };
            std::string resultCurrency{ winrt::to_string(CmbResultCurrency().SelectedItem().as<winrt::hstring>()) };
            std::optional<CurrencyConversion> conversion{ CurrencyConversionService::convert(sourceCurrency, sourceAmount, resultCurrency) };
            if(conversion.has_value())
            {
                TxtResultAmount().Text(winrt::to_hstring(CurrencyHelpers::toAmountString(conversion->getResultAmount(), CurrencyHelpers::getSystemCurrency(), false, true)));
            }
            else
            {
                RowResultCurrency().Title(winrt::to_hstring(_("Result (Error)")));
            }
        }
    }
}