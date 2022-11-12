#include "moneyhelpers.hpp"
#include <sstream>

using namespace NickvisionMoney::Helpers;

std::string MoneyHelpers::boostMoneyToLocaleString(boost::multiprecision::cpp_dec_float_50 amount, const std::locale& locale)
{
    std::stringstream builder;
    //Zero
    if(amount == 0)
    {
        if(isLocaleCurrencySymbolOnLeft(locale))
        {
            return getLocaleCurrencySymbol(locale) + (isLocaleDotDecimalSeperated(locale) ? "0.00" : "0,00");
        }
        else
        {
            return (isLocaleDotDecimalSeperated(locale) ? "0.00 " : "0,00 ") + getLocaleCurrencySymbol(locale);
        }
    }
    //Dollar Amount
    amount *= 100.0;
    builder << amount;
    std::string amountAsString{ builder.str() };
    //Reset Builder
    builder.str("");
    builder.clear();
    //Get Amount as Locale String
    builder.imbue(locale);
    builder << std::showbase << std::put_money(amountAsString);
    std::string result{ builder.str() };
    if(amount > -100 && amount < 100)
    {
        result.insert(isLocaleCurrencySymbolOnLeft(locale) ? 1 : 0, "0");
    }
    return result;
}

boost::multiprecision::cpp_dec_float_50 MoneyHelpers::localeStringToBoostMoney(const std::string& localeString, const std::locale& locale)
{
    std::stringstream builder;
    builder.imbue(locale);
    builder << localeString;
    long double value{ 0.00 };
    builder >> std::get_money(value);
    value /= 100;
    return { value };
}

bool MoneyHelpers::isLocaleDotDecimalSeperated(const std::locale& locale)
{
    return std::use_facet<std::moneypunct<char>>(locale).decimal_point() == '.';
}

bool MoneyHelpers::isLocaleCurrencySymbolOnLeft(const std::locale& locale)
{
    std::stringstream builder;
    builder.imbue(locale);
    builder << std::showbase << std::put_money("1");
    std::string monetaryValue{ builder.str() };
    return monetaryValue.substr(0, 1) == std::use_facet<std::moneypunct<char>>(locale).curr_symbol();
}

std::string MoneyHelpers::getLocaleCurrencySymbol(const std::locale& locale)
{
    return std::use_facet<std::moneypunct<char>>(locale).curr_symbol();
}

std::string MoneyHelpers::fixLocaleStringFormat(const std::string& s, const std::locale& locale)
{
    //Generate Number String
    std::string sNew{ "0" };
    for(char c : s)
    {
        if(std::isdigit(c) || c == ',' || c == '.')
        {
            sNew += c;
        }
    }
    //Check Decimal Places
    if(isLocaleDotDecimalSeperated(locale))
    {
        if(sNew.find(".") == std::string::npos)
        {
            sNew += ".00";
        }
        else if(sNew.substr(sNew.find(".")).length() == 2)
        {
            sNew += "0";
        }
    }
    else
    {
        if(sNew.find(",") == std::string::npos)
        {
            sNew += ",00";
        }
        else if(sNew.substr(sNew.find(",")).length() == 2)
        {
            sNew += "0";
        }
    }
    //Add Currency Symbol
    if(isLocaleCurrencySymbolOnLeft(locale))
    {
        sNew.insert(0, getLocaleCurrencySymbol(locale));
    }
    else
    {
        sNew += " ";
        sNew += getLocaleCurrencySymbol(locale);
    }
    return sNew;
}
