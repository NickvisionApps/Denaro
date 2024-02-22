#include "helpers/currencyhelpers.h"
#include <iomanip>
#include <locale>
#include <memory>
#include <sstream>

using namespace Nickvision::Money::Shared::Models;

class NumberFormat : public std::numpunct<char>
{
public:
    NumberFormat(const Currency& currency)
        : m_currency{ currency }
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
        return "\003";
    }

private:
    Currency m_currency;
};

namespace Nickvision::Money::Shared
{
    Currency CurrencyHelpers::getSystemCurrency()
    {
        Currency curr;
        curr.setSymbol(std::use_facet<std::moneypunct<char>>(std::locale("")).curr_symbol());
        curr.setCode(std::use_facet<std::moneypunct<char, true>>(std::locale("")).curr_symbol());
        curr.setDecimalSeparator(std::use_facet<std::moneypunct<char>>(std::locale("")).decimal_point());
        curr.setGroupSeparator(std::use_facet<std::moneypunct<char>>(std::locale("")).thousands_sep());
        int numberIndex{ -1 };
        int signIndex{ -1 };
        int spaceIndex{ -1 };
        for(int i = 0; i < 4; i++)
        {
            if(std::use_facet<std::moneypunct<char>>(std::locale("")).pos_format().field[i] == std::moneypunct<char>::value)
            {
                numberIndex = i;
            }
            if(std::use_facet<std::moneypunct<char>>(std::locale("")).pos_format().field[i] == std::moneypunct<char>::sign)
            {
                signIndex = i;
            }
            if(std::use_facet<std::moneypunct<char>>(std::locale("")).pos_format().field[i] == std::moneypunct<char>::space)
            {
                spaceIndex = i;
            }
        }     
        if(signIndex < numberIndex)
        {
            curr.setAmountStyle(spaceIndex == -1 ? AmountStyle::SymbolNumber : AmountStyle::SymbolSpaceNumber);
        }
        else
        {
            curr.setAmountStyle(spaceIndex == -1 ? AmountStyle::NumberSymbol : AmountStyle::NumberSpaceSymbol);
        }
        return curr;
    }

    std::string CurrencyHelpers::toAmountString(double amount, const Currency& currency, bool useNativeDigits, bool showCurrencySymbol, bool overwriteDecimal)
    {
        std::stringstream builder;
        builder.imbue({ builder.getloc(), new NumberFormat(currency) });
        builder << std::fixed;
        if(!overwriteDecimal)
        {
            builder << std::setprecision(currency.getDecimalDigits());
        }
        if(showCurrencySymbol)
        {
            if(currency.getAmountStyle() == AmountStyle::SymbolNumber)
            {
                builder << (amount < 0 ? "−" : "") << currency.getSymbol();
            }
            else if(currency.getAmountStyle() == AmountStyle::SymbolSpaceNumber)
            {
                builder << (amount < 0 ? "−" : "") << currency.getSymbol() << " ";
            }
        }
        builder << (amount < 0 ? amount * -1 : amount);
        if(showCurrencySymbol)
        {
            if(currency.getAmountStyle() == AmountStyle::NumberSymbol)
            {
                builder << currency.getSymbol() << (amount < 0 ? "−" : "");
            }
            else if(currency.getAmountStyle() == AmountStyle::NumberSpaceSymbol)
            {
                builder << " " << currency.getSymbol() << (amount < 0 ? "−" : "");
            }
        }
        return useNativeDigits ? replaceNativeDigits(builder.str()) : builder.str();
    }

    std::string CurrencyHelpers::replaceNativeDigits(const std::string& s)
    {
        //TODO
        return "";
    }
}