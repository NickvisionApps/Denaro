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
    Models::Currency getSystemCurrency();
    /**
     * @brief Converts an amount to a currency string.
     * @param amount The amount to convert
     * @param currency The currency to use to format the string
     * @param useNativeDigits Whether or not to convert Latin digits to native digits
     * @param showCurrencySymbol Whether or not to add the currency symbol to the string
     * @param overwriteDecimal Whether or not to keep more digits in the decimal part of the string to increase precision
     * @return The currency string for the amount
     */
    std::string toAmountString(double amount, const Models::Currency& currency, bool useNativeDigits, bool showCurrencySymbol = true, bool overwriteDecimal = false);
    /**
     * @brief Replaces native digits in a string with Latin digits.
     * @param s The amount string
     * @return The amount string with native digits replaced by Latin digits 
     */
    std::string replaceNativeDigits(const std::string& s);
}

#endif //CURRENCYHELPERS_H