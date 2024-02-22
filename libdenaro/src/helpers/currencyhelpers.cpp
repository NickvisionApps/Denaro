#include "helpers/currencyhelpers.h"
#include <iomanip>
#include <locale>
#include <memory>
#include <sstream>

using namespace Nickvision::Money::Shared::Models;

class MoneyFormat : public std::moneypunct<char>
{
public:
    MoneyFormat(const Currency& currency, bool showCurrencySymbol)
        : m_currency{ currency },
        m_showCurrencySymbol{ showCurrencySymbol }
    {
        
    }

    char do_decimal_point() const override
    {
        return m_currency.getDecimalSeparator();
    }

    char do_thousands_sep() const override
    {
        return m_currency.getGroupSeparator();
    }

    std::string do_grouping() const override
    {
        return "\3";
    }

    std::string do_curr_symbol() const override
    {
        return m_showCurrencySymbol ? m_currency.getSymbol() : "";
    }

    std::money_base::pattern do_pos_format() const override
    {
        switch(m_currency.getAmountStyle())
        {
        case AmountStyle::SymbolNumber:
            return { sign, symbol, none, value };
        case AmountStyle::NumberSymbol:
            return { value, none, symbol, sign };
        case AmountStyle::SymbolSpaceNumber:
            return { sign, symbol, space, value };
        case AmountStyle::NumberSpaceSymbol:
            return { value, space, symbol, sign };
        default:
            return { sign, symbol, none, value };
        }
    }

    std::money_base::pattern do_neg_format() const override
    {
        return do_pos_format();
    }

private:
    Currency m_currency;
    bool m_showCurrencySymbol;
};

namespace Nickvision::Money::Shared
{
    const Currency& CurrencyHelpers::getSystemCurrency()
    {
        static std::unique_ptr<Currency> systemCurrency;
        if(!systemCurrency)
        {
            systemCurrency = std::make_unique<Currency>();
            const std::moneypunct<char>& money{ std::use_facet<std::moneypunct<char>>(std::locale("")) };
            systemCurrency->setSymbol(money.curr_symbol());
            systemCurrency->setCode(std::use_facet<std::moneypunct<char, true>>(std::locale("")).curr_symbol());
            systemCurrency->setDecimalSeparator(money.decimal_point());
            systemCurrency->setGroupSeparator(money.thousands_sep());
            int numberIndex{ -1 };
            int signIndex{ -1 };
            int spaceIndex{ -1 };
            for(int i = 0; i < 4; i++)
            {
                if(money.pos_format().field[i] == std::moneypunct<char>::value)
                {
                    numberIndex = i;
                }
                if(money.pos_format().field[i] == std::moneypunct<char>::sign)
                {
                    signIndex = i;
                }
                if(money.pos_format().field[i] == std::moneypunct<char>::space)
                {
                    spaceIndex = i;
                }
            }
            if(signIndex < numberIndex)
            {
                systemCurrency->setAmountStyle(spaceIndex == -1 ? AmountStyle::SymbolNumber : AmountStyle::SymbolSpaceNumber);
            }
            else
            {
                systemCurrency->setAmountStyle(spaceIndex == -1 ? AmountStyle::NumberSymbol : AmountStyle::NumberSpaceSymbol);
            }
        }
        return *systemCurrency;
    }

    std::string CurrencyHelpers::toAmountString(double amount, Currency currency, bool useNativeDigits, bool showCurrencySymbol, bool overwriteDecimal)
    {
        MoneyFormat format{ currency, showCurrencySymbol };
        std::stringstream builder;
        builder.imbue(std::locale(builder.getloc(), &format));
        builder << std::fixed;
        if(!overwriteDecimal)
        {
            builder << std::setprecision(currency.getDecimalDigits());
        }
        builder << std::put_money(amount);
        return useNativeDigits ? replaceNativeDigits(builder.str()) : builder.str();
    }

    std::string CurrencyHelpers::replaceNativeDigits(const std::string& s)
    {
        //TODO
        return "";
    }
}