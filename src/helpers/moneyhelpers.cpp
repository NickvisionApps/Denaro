#include "moneyhelpers.hpp"
#include <sstream>

using namespace NickvisionMoney::Helpers;

std::string MoneyHelpers::boostMoneyToLocaleString(boost::multiprecision::cpp_dec_float_50 amount, const std::locale& locale)
{
    std::stringstream builder;
    //Get Amount As String
    amount *= 100.0;
    builder << amount;
    std::string amountAsString{ builder.str() };
    //Reset Builder
    builder.str("");
    builder.clear();
    //Get Amount as Locale String
    builder.imbue(locale);
    builder << std::showbase << std::put_money(amountAsString);
    return builder.str();
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

void MoneyHelpers::fixLocaleStringFormat(std::string& s, const std::locale& locale)
{
    if(s.find(getLocaleCurrencySymbol(locale)) != std::string::npos)
    {
        s.erase(s.find(getLocaleCurrencySymbol(locale)), 1);
    }
    if(s.find(" ") != std::string::npos)
    {
        s.erase(s.find(" "), 1);
    }
    if(isLocaleDotDecimalSeperated(locale) && s.find(".") == std::string::npos)
    {
        s += ".00";
    }
    if(!isLocaleDotDecimalSeperated(locale) && s.find(",") == std::string::npos)
    {
        s += ",00";
    }
    if(isLocaleCurrencySymbolOnLeft(locale))
    {
        s.insert(0, getLocaleCurrencySymbol(locale));
    }
    else
    {
        s += " ";
        s += getLocaleCurrencySymbol(locale);
    }
}
