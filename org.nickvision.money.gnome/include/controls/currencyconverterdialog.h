#ifndef CURRENCYCONVERTERDIALOG_H
#define CURRENCYCONVERTERDIALOG_H

#include <string>
#include <adwaita.h>

namespace Nickvision::Money::GNOME::Controls
{
    /**
     * @brief A dialog for converting currencies.
     */
    class CurrencyConverterDialog
    {
    public:
        /**
         * @brief Constructs a CurrencyConverterDialog.
         * @param parent The GtkWindow object of the parent window
         * @param iconName The name of the icon to use for the window 
         */
        CurrencyConverterDialog(GtkWindow* parent, const std::string& iconName);
        /**
         * @brief Destructs a CurrencyConverterDialog. 
         */
        ~CurrencyConverterDialog();
        /**
         * @brief Shows the CurrencyConverterDialog and waits for it to close.
         */
        void run();

    private:
        void switchCurrencies();
        void onSourceCurrencyChanged();
        void onResultCurrencyChanged();
        void onSourceAmountChanged();
        void copyResult();
        void onCurrencyChange();
        GtkBuilder* m_builder;
        GtkWindow* m_parent;
        AdwWindow* m_window;
        GtkStringList* m_currencyList;
    };
}

#endif //CURRENCYCONVERTERDIALOG_H