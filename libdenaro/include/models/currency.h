#ifndef CUSTOMCURRENCY_H
#define CUSTOMCURRENCY_H

#include <string>
#include "amountstyle.h"
#include "currencycheckstatus.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a currency. 
     */
    class Currency
    {
    public:
        /**
         * @brief Constructs a Currency. 
         */
        Currency();
        /**
         * @brief Constructs a Currency.
         * @param symbol The symbol of the currency
         * @param code The code of the currency
         */
        Currency(const std::string& symbol, const std::string& code);
        /**
         * @brief Checks the validity of the Currency object.
         * @return CurrencyCheckStatus
         */
        CurrencyCheckStatus validate() const;
        /**
         * @brief Gets the symbol of the currency.
         * @return The symbol of the currency
         */
        const std::string& getSymbol() const;
        /**
         * @brief Sets the symbol of the currency.
         * @brief If the new symbol is empty, the new symbol will not be set.
         * @param symbol The new symbol of the currency 
         */
        void setSymbol(const std::string& symbol);
        /**
         * @brief Gets the code of the currency.
         * @return The code of the currency
         */
        const std::string& getCode() const;
        /**
         * @brief Sets the code of the currency.
         * @brief If the new code is empty, the new code will not be set.
         * @param code The new code of the currency 
         */
        void setCode(const std::string& code);
        /**
         * @brief Gets the decimal separator of the currency.
         * @return The decimal separator of the currency
         */
        char getDecimalSeparator() const;
        /**
         * @brief Sets the decimal separator of the currency.
         * @param separator The new decimal separator of the currency 
         */
        void setDecimalSeparator(char separator);
        /**
         * @brief Gets the group separator of the currency.
         * @return The group separator of the currency
         */
        char getGroupSeparator() const;
        /**
         * @brief Sets the group separator of the currency.
         * @param separator The new group separator of the currency 
         */
        void setGroupSeparator(char separator);
        /**
         * @brief Gets the number of decimal digits of the currency.
         * @return The number of decimal digits of the currency
         */
        int getDecimalDigits() const;
        /**
         * @brief Sets the number of decimal digits of the currency.
         * @param digits The new number of decimal digits of the currency 
         */
        void setDecimalDigits(int digits);
        /**
         * @brief Gets the style to use for displaying amounts of the currency.
         * @return The style to use for displaying amounts of the currency
         */
        AmountStyle getAmountStyle() const;
        /**
         * @brief Sets the style to use for displaying amounts of the currency.
         * @param style The new style to use for displaying amounts of the currency 
         */
        void setAmountStyle(AmountStyle style);
        /**
         * @brief Gets the string representation of the Currency. "Symbol (Code)"
         * @return The string representation of the Currency 
         */
        std::string toString() const;
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (check() == CurrencyCheckStatus::Valid), else false 
         */
        operator bool() const;

    private:
        std::string m_symbol;
        std::string m_code;
        char m_decimalSeparator;
        char m_groupSeparator;
        int m_decimalDigits;
        AmountStyle m_amountStyle;
    };
}

#endif //CUSTOMCURRENCY_H