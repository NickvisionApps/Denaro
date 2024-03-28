#ifndef CURRENCYCONVERTERDIALOG_H
#define CURRENCYCONVERTERDIALOG_H

#include <string>
#include <adwaita.h>

namespace Nickvision::Money::GNOME::Controls
{
    /**
     * @brief A page for converting currencies.
     */
    class CurrencyConverterPage
    {
    public:
        /**
         * @brief Constructs a CurrencyConverterPage.
         * @param parent The parent window
         */
        CurrencyConverterPage(GtkWindow* parent);
        /**
         * @brief Destructs a CurrencyConverterPage. 
         */
        ~CurrencyConverterPage();
        /**
         * @brief Gets the gobj of the control
         * @return AdwClamp*
         */
        AdwClamp* gobj();

    private:
        /**
         * @brief Switches the source and result currencies.
         */
        void switchCurrencies();
        /**
         * @brief Handles when the source currency is changed.
         */
        void onSourceCurrencyChanged();
        /**
         * @brief Handles when the result currency is changed.
         */
        void onResultCurrencyChanged();
        /**
         * @brief Handles when the source amount is changed.
         */
        void onSourceAmountChanged();
        /**
         * @brief Copies the result to the clipboard.
         */
        void copyResult();
        /**
         * @brief Handles when the currency is changed.
         */
        void onCurrencyChange();
        GtkBuilder* m_builder;
        AdwClamp* m_page;
        GtkStringList* m_currencyList;
    };
}

#endif //CURRENCYCONVERTERDIALOG_H
