#include <gtest/gtest.h>
#include "helpers/currencyhelpers.h"

using namespace Nickvision::Money::Shared;
using namespace Nickvision::Money::Shared::Models;

TEST(CurrencyHelpers, GetUSCurrency)
{
    Currency currency{ CurrencyHelpers::getSystemCurrency() };
    ASSERT_EQ(currency.getSymbol(), "$");
    ASSERT_EQ(currency.getCode(), "USD");
    ASSERT_EQ(currency.getDecimalSeparator(), '.');
    ASSERT_EQ(currency.getGroupSeparator(), ',');
    ASSERT_EQ(currency.getAmountStyle(), Models::AmountStyle::SymbolNumber);
}

TEST(CurrencyHelpers, USAmountString1)
{
    ASSERT_EQ(CurrencyHelpers::toAmountString(12347.89, CurrencyHelpers::getSystemCurrency(), false), "$12,347.89");
}

TEST(CurrencyHelpers, USAmountString2)
{
    ASSERT_EQ(CurrencyHelpers::toAmountString(100, CurrencyHelpers::getSystemCurrency(), false), "$100.00");
}

TEST(CurrencyHelpers, USAmountString3)
{
    ASSERT_EQ(CurrencyHelpers::toAmountString(5.50, CurrencyHelpers::getSystemCurrency(), false), "$5.50");
}

TEST(CurrencyHelpers, CustomCurrency1)
{
    Currency currency{ "#", "HAS" };
    currency.setDecimalSeparator(',');
    currency.setGroupSeparator('.');
    currency.setAmountStyle(Models::AmountStyle::NumberSpaceSymbol);
    ASSERT_EQ(CurrencyHelpers::toAmountString(12347.89, currency, false), "12.347,89 #");
    ASSERT_EQ(CurrencyHelpers::toAmountString(12347.89, currency, false, false), "12.347,89");
}

TEST(CurrencyHelpers, CustomCurrency2)
{
    Currency currency{ "@", "HAS" };
    currency.setDecimalSeparator('-');
    currency.setGroupSeparator('*');
    currency.setAmountStyle(Models::AmountStyle::SymbolSpaceNumber);
    ASSERT_EQ(CurrencyHelpers::toAmountString(2765.9, currency, false), "@ 2*765-90");
}