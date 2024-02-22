#include "models/currency.h"

namespace Nickvision::Money::Shared::Models
{
    Currency::Currency()
        : m_decimalSeparator{ '.' },
        m_groupSeparator{ ',' }, 
        m_decimalDigits{ 2 },
        m_amountStyle{ AmountStyle::SymbolNumber }
    {

    }

    Currency::Currency(const std::string& symbol, const std::string& code)
        : m_symbol{ symbol },
        m_code{ code }, 
        m_decimalSeparator{ '.' },
        m_groupSeparator{ ',' }, 
        m_decimalDigits{ 2 },
        m_amountStyle{ AmountStyle::SymbolNumber }
    {

    }

    bool Currency::empty() const
    {
        return m_symbol.empty() && m_code.empty();
    }

    const std::string& Currency::getSymbol() const
    {
        return m_symbol;
    }

    void Currency::setSymbol(const std::string& symbol)
    {
        m_symbol = symbol;
    }

    const std::string& Currency::getCode() const
    {
        return m_code;
    }

    void Currency::setCode(const std::string& code)
    {
        m_code = code;
    }

    char Currency::getDecimalSeparator() const
    {
        return m_decimalSeparator;
    }

    void Currency::setDecimalSeparator(char separator)
    {
        if(separator == m_groupSeparator)
        {
            return;
        }
        m_decimalSeparator = separator;
    }

    char Currency::getGroupSeparator() const
    {
        return m_groupSeparator;
    }

    void Currency::setGroupSeparator(char separator)
    {
        if(separator == m_decimalSeparator)
        {
            return;
        }
        m_groupSeparator = separator;
    }

    int Currency::getDecimalDigits() const
    {
        return m_decimalDigits;
    }

    void Currency::setDecimalDigits(int digits)
    {
        if(digits <= 2)
        {
            m_decimalDigits = 2;
        }
        else if(digits > 6)
        {
            m_decimalDigits = 6;
        }
        else
        {
            m_decimalDigits = digits;
        }
    }

    AmountStyle Currency::getAmountStyle() const
    {
        return m_amountStyle;
    }

    void Currency::setAmountStyle(AmountStyle style)
    {
        m_amountStyle = style;
    }

    Currency::operator bool() const
    {
        return !empty();
    }
}