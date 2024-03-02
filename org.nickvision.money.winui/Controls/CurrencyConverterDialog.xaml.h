#ifndef CURRENCYCONVERTERDIALOG_H
#define CURRENCYCONVERTERDIALOG_H

#include "includes.h"
#include "Controls/SettingsRow.g.h"
#include "Controls/CurrencyConverterDialog.g.h"

namespace winrt::Nickvision::Money::WinUI::Controls::implementation 
{
    /**
     * @brief A dialog for converting currencies.
     */
    class CurrencyConverterDialog : public CurrencyConverterDialogT<CurrencyConverterDialog>
    {
    public:
        /**
         * @brief Constructs a CurrencyConverterDialog. 
         */
        CurrencyConverterDialog();
        /**
         * @brief Handles when the dialog is opened.
         * @param sender ContentDialog
         * @param args ContentDialogOpenedEventArgs
         */
        Windows::Foundation::IAsyncAction OnOpened(const Microsoft::UI::Xaml::Controls::ContentDialog& sender, const Microsoft::UI::Xaml::Controls::ContentDialogOpenedEventArgs& args);
        /**
         * @brief Handles when the source currency is changed. 
         * @param sender IInspectable
         * @param args SelectionChangedEventArgs
         */
        void OnCmbSourceCurrencyChanged(const IInspectable& sender, const Microsoft::UI::Xaml::Controls::SelectionChangedEventArgs& args);
        /**
         * @brief Handles when the result currency is changed. 
         * @param sender IInspectable
         * @param args SelectionChangedEventArgs
         */
        void OnCmbResultCurrencyChanged(const IInspectable& sender, const Microsoft::UI::Xaml::Controls::SelectionChangedEventArgs& args);
        /**
         * @brief Switches the result currency with the source currency.
         * @param sender IInspectable
         * @param args RoutedEventArgs
         */
        void SwitchCurrencies(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);
        /**
         * @brief Handles when the source amount text is changed.
         * @param sender IInspectable
         * @param args TextChangedEventArgs
         */
        void OnTxtSourceAmountChanged(const IInspectable& sender, const Microsoft::UI::Xaml::Controls::TextChangedEventArgs& args);
        /**
         * @brief Copies the result amount to the clipboard.
         * @param sender IInspectable
         * @param args RoutedEventArgs
         */
        void CopyResult(const IInspectable& sender, const Microsoft::UI::Xaml::RoutedEventArgs& args);

    private:
        /**
         * @brief Handles when the currency (amount or currency) is changed.
         */
        void onCurrencyChange();
    };
}

namespace winrt::Nickvision::Money::WinUI::Controls::factory_implementation 
{
    class CurrencyConverterDialog : public CurrencyConverterDialogT<CurrencyConverterDialog, implementation::CurrencyConverterDialog>
    {

    };
}

#endif //CURRENCYCONVERTERDIALOG_H