#ifndef CURRENCYHELPERS_H
#define CURRENCYHELPERS_H

#include <string>
#include "models/currency.h"

namespace Nickvision::Money::Shared::CurrencyHelpers
{
    /**
     * @brief Gets the currency of the system's locale.
     * @return The system currency
     */
    const Models::Currency& getSystemCurrency();
    /**
     * @brief Converts an amount to a currency string.
     * @param amount The amount to convert
     * @param currency The currency to use to format the string
     * @param showCurrencySymbol Whether or not to add the currency symbol to the string
     * @param overwriteDecimal Whether or not to keep more digits in the decimal part of the string to increase precision
     * @return The currency string for the amount
     */
    std::string toAmountString(double amount, const Models::Currency& currency, bool showCurrencySymbol = true, bool overwriteDecimal = false);
}

#endif //CURRENCYHELPERS_H