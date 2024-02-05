#ifndef CUSTOMCURRENCY_H
#define CUSTOMCURRENCY_H

#include <string>
#include "amountstyle.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a custom currency. 
     */
    class CustomCurrency
    {
    public:
        /**
         * @brief Constructs a CustomCurrency. 
         */
        CustomCurrency();
        /**
         * @brief Constructs a CustomCurrency.
         * @param symbol The symbol of the custom currency
         * @param code The code of the custom currency
         */
        CustomCurrency(const std::string& symbol, const std::string& code);
        /**
         * @brief Gets whether or not the CustomCurrency object represents an empty value.
         * @return True if empty, else false 
         */
        bool empty() const;
        /**
         * @brief Gets the symbol of the custom currency.
         * @return The symbol of the custom currency
         */
        const std::string& getSymbol() const;
        /**
         * @brief Sets the symbol of the custom currency.
         * @brief If the new symbol is empty, the new symbol will not be set.
         * @param symbol The new symbol of the custom currency 
         */
        void setSymbol(const std::string& symbol);
        /**
         * @brief Gets the code of the custom currency.
         * @return The code of the custom currency
         */
        const std::string& getCode() const;
        /**
         * @brief Sets the code of the custom currency.
         * @brief If the new code is empty, the new code will not be set.
         * @param code The new code of the custom currency 
         */
        void setCode(const std::string& code);
        /**
         * @brief Gets the decimal separator of the custom currency.
         * @return The decimal separator of the custom currency
         */
        const std::string& getDecimalSeparator() const;
        /**
         * @brief Sets the decimal separator of the custom currency.
         * @brief If the new decimal separator is the same as the current group separator, the new decimal separator will not be set.
         * @brief If the new decimal separator is empty, the new decimal separator will not be set.
         * @param separator The new decimal separator of the custom currency 
         */
        void setDecimalSeparator(const std::string& separator);
        /**
         * @brief Gets the group separator of the custom currency.
         * @return The group separator of the custom currency
         */
        const std::string& getGroupSeparator() const;
        /**
         * @brief Sets the group separator of the custom currency.
         * @brief If the new group separator is the same as the current decimal separator, the new group separator will not be set.
         * @param separator The new group separator of the custom currency 
         */
        void setGroupSeparator(const std::string& separator);
        /**
         * @brief Gets the number of decimal digits of the custom currency.
         * @return The number of decimal digits of the custom currency
         */
        int getDecimalDigits() const;
        /**
         * @brief Sets the number of decimal digits of the custom currency.
         * @param digits The new number of decimal digits of the custom currency 
         */
        void setDecimalDigits(int digits);
        /**
         * @brief Gets the style to use for displaying amounts of the custom currency.
         * @return The style to use for displaying amounts of the custom currency
         */
        AmountStyle getAmountStyle() const;
        /**
         * @brief Sets the style to use for displaying amounts of the custom currency.
         * @param style The new style to use for displaying amounts of the custom currency 
         */
        void setAmountStyle(AmountStyle style);
        /**
         * @brief Gets whether or not the object is valid or not.
         * @return True if valid (!empty), else false 
         */
        operator bool() const;

    private:
        std::string m_symbol;
        std::string m_code;
        std::string m_decimalSeparator;
        std::string m_groupSeparator;
        int m_decimalDigits;
        AmountStyle m_amountStyle;
    };
}

#endif //CUSTOMCURRENCY_H