#include "moneyhelpers.hpp"
#include <sstream>

using namespace NickvisionMoney::Helpers;

std::string MoneyHelpers::boostMoneyToLocaleString(boost::multiprecision::cpp_dec_float_50 amount, const std::locale& locale)
{
    std::stringstream builder;
    //Get Amount As String
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
    return 0.0;
}
