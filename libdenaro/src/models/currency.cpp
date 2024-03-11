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

    CurrencyCheckStatus Currency::validate() const
    {
        if(m_symbol.empty())
        {
            return CurrencyCheckStatus::EmptySymbol;
        }
        else if(m_code.empty())
        {
            return CurrencyCheckStatus::EmptyCode;
        }
        else if(m_decimalSeparator == '\0')
        {
            return CurrencyCheckStatus::EmptyDecimalSeparator;
        }
        else if(m_decimalSeparator == m_groupSeparator)
        {
            return CurrencyCheckStatus::SameSeparators;
        }
        else if(m_symbol.find(m_decimalSeparator) != std::string::npos)
        {
            return CurrencyCheckStatus::SameSymbolAndDecimalSeparator;
        }
        else if(m_symbol.find(m_groupSeparator) != std::string::npos)
        {
            return CurrencyCheckStatus::SameSymbolAndGroupSeparator;
        }
        return CurrencyCheckStatus::Valid;
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
        m_decimalSeparator = separator;
    }

    char Currency::getGroupSeparator() const
    {
        return m_groupSeparator;
    }

    void Currency::setGroupSeparator(char separator)
    {
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
        return validate() == CurrencyCheckStatus::Valid;
    }
}