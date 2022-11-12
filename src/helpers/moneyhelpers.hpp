#pragma once

#include <locale>
#include <string>
#include <boost/multiprecision/cpp_dec_float.hpp>

namespace NickvisionMoney::Helpers::MoneyHelpers
{
	std::string boostMoneyToLocaleString(boost::multiprecision::cpp_dec_float_50 amount, const std::locale& locale);
	boost::multiprecision::cpp_dec_float_50 localeStringToBoostMoney(const std::string& localeString, const std::locale& locale);
	bool isLocaleDotDecimalSeperated(const std::locale& locale);
}