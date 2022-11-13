#include "moneyhelpers.hpp"
#include <cmath>
#include <sstream>
#include <boost/locale.hpp>

using namespace NickvisionMoney::Helpers;

std::string MoneyHelpers::boostMoneyToLocaleString(boost::multiprecision::cpp_dec_float_50 amount, const std::locale& locale)
{
    std::stringstream builder;
    long double value{ static_cast<long double>(amount) };
    builder.imbue(locale);
    if(isLocaleCurrencySymbolOnLeft(locale))
    {
        builder << getLocaleCurrencySymbol(locale);
    }
    builder << boost::locale::as::currency << value;
    int decimal{ (int)(std::fmod(value, 1.0) * 100) };
    if(decimal == 0)
    {
        builder << (isLocaleDotDecimalSeperated(locale) ? ".00" : ",00");
    }
    else if(decimal % 10 == 0)
    {
        builder << "0";
    }
    if(!isLocaleCurrencySymbolOnLeft(locale))
    {
        builder << " " << getLocaleCurrencySymbol(locale);
    }
    return builder.str();
}

boost::multiprecision::cpp_dec_float_50 MoneyHelpers::localeStringToBoostMoney(std::string localeString, const std::locale& locale)
{
    //Prepare String
    if(localeString.find(getLocaleCurrencySymbol(locale)) != std::string::npos)
    {
        localeString.erase(localeString.find(getLocaleCurrencySymbol(locale)), 1);
    }
    while(localeString.find(" ") != std::string::npos)
    {
        localeString.erase(localeString.find(" "), 1);
    }
    std::stringstream builder;
    builder.imbue(locale);
    builder << localeString;
    long double value{ 0.00 };
    builder >> boost::locale::as::currency >> value;
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
