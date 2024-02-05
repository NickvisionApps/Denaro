#include "models/customcurrency.h"

namespace Nickvision::Money::Shared::Models
{
    CustomCurrency::CustomCurrency()
        : m_decimalSeparator{ "." },
        m_groupSeparator{ "," }, 
        m_decimalDigits{ 2 },
        m_amountStyle{ AmountStyle::SignNumber }
    {

    }

    CustomCurrency::CustomCurrency(const std::string& symbol, const std::string& code)
        : m_symbol{ symbol },
        m_code{ code }, 
        m_decimalSeparator{ "." },
        m_groupSeparator{ "," }, 
        m_decimalDigits{ 2 },
        m_amountStyle{ AmountStyle::SignNumber }
    {

    }

    bool CustomCurrency::empty() const
    {
        return m_symbol.empty() && m_code.empty();
    }

    const std::string& CustomCurrency::getSymbol() const
    {
        return m_symbol;
    }

    void CustomCurrency::setSymbol(const std::string& symbol)
    {
        m_symbol = symbol;
    }

    const std::string& CustomCurrency::getCode() const
    {
        return m_code;
    }

    void CustomCurrency::setCode(const std::string& code)
    {
        m_code = code;
    }

    const std::string& CustomCurrency::getDecimalSeparator() const
    {
        return m_decimalSeparator;
    }

    void CustomCurrency::setDecimalSeparator(const std::string& separator)
    {
        if(separator.empty() || separator == m_groupSeparator)
        {
            return;
        }
        m_decimalSeparator = separator;
    }

    const std::string& CustomCurrency::getGroupSeparator() const
    {
        return m_groupSeparator;
    }

    void CustomCurrency::setGroupSeparator(const std::string& separator)
    {
        if(separator == m_decimalSeparator)
        {
            return;
        }
        m_groupSeparator = separator;
    }

    int CustomCurrency::getDecimalDigits() const
    {
        return m_decimalDigits;
    }

    void CustomCurrency::setDecimalDigits(int digits)
    {
        m_decimalDigits = digits;
    }

    AmountStyle CustomCurrency::getAmountStyle() const
    {
        return m_amountStyle;
    }

    void CustomCurrency::setAmountStyle(AmountStyle style)
    {
        m_amountStyle = style;
    }

    CustomCurrency::operator bool() const
    {
        return !empty();
    }
}