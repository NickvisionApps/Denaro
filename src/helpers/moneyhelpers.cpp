#include "moneyhelpers.hpp"
#include <sstream>
#include <iostream>

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
    std::stringstream builder;
    builder.imbue(locale);
    builder << std::showbase << std::put_money("1");
    std::string monetaryValue{ builder.str() };
    std::cout << monetaryValue << std::endl;
    return monetaryValue.find(".") != std::string::npos;
}
