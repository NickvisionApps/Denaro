#pragma once

#include <locale>
#include <string>
#include <boost/multiprecision/cpp_dec_float.hpp>

namespace NickvisionMoney::Helpers::MoneyHelpers
{
	/**
	 * Converts a money value to a string in the user's locale
	 *
	 * @param amount The money amount
	 * @param locale The user's locale
	 * @returns A string of the money value in the user's locale
	 */
	std::string boostMoneyToLocaleString(boost::multiprecision::cpp_dec_float_50 amount, const std::locale& locale);
	/**
	 * Converts a locale string to a money value
	 *
	 * @param localeString The money string
	 * @param locale The user's locale
	 * @returns The money value
	 */
	boost::multiprecision::cpp_dec_float_50 localeStringToBoostMoney(const std::string& localeString, const std::locale& locale);
	/*
	 * Gets whether or not the user's locale separates the decimal by "." or ","
	 *
	 * @param locale The user's locale
	 * @returns True if separated by ".", false for ","
	 */
	bool isLocaleDotDecimalSeperated(const std::locale& locale);
}