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

TEST(CurrencyHelpers, ToUSAmountString1)
{
    ASSERT_EQ(CurrencyHelpers::toAmountString(12347.89, CurrencyHelpers::getSystemCurrency()), "$12,347.89");
}

TEST(CurrencyHelpers, ToUSAmountString2)
{
    ASSERT_EQ(CurrencyHelpers::toAmountString(100, CurrencyHelpers::getSystemCurrency()), "$100.00");
}

TEST(CurrencyHelpers, ToUSAmountString3)
{
    ASSERT_EQ(CurrencyHelpers::toAmountString(5.50, CurrencyHelpers::getSystemCurrency(), false), "5.50");
}

TEST(CurrencyHelpers, FromUSAmountString1)
{
    ASSERT_EQ(CurrencyHelpers::toAmount("$12,347.89", CurrencyHelpers::getSystemCurrency()), 12347.89);
}

TEST(CurrencyHelpers, ToCustomCurrency1)
{
    Currency currency{ "#", "HAS" };
    currency.setDecimalSeparator(',');
    currency.setGroupSeparator('.');
    currency.setAmountStyle(Models::AmountStyle::NumberSpaceSymbol);
    ASSERT_TRUE(currency.validate() == CurrencyCheckStatus::Valid);
    ASSERT_EQ(CurrencyHelpers::toAmountString(12347.89, currency), "12.347,89 #");
    ASSERT_EQ(CurrencyHelpers::toAmountString(12347.89, currency, false), "12.347,89");
}

TEST(CurrencyHelpers, FromCustomCurrency1)
{
    Currency currency{ "#", "HAS" };
    currency.setDecimalSeparator(',');
    currency.setGroupSeparator('.');
    currency.setAmountStyle(Models::AmountStyle::NumberSpaceSymbol);
    ASSERT_EQ(CurrencyHelpers::toAmount("1.567,21 #", currency), 1567.21);
}

TEST(CurrencyHelpers, ToCustomCurrency2)
{
    Currency currency{ "@", "HAS" };
    currency.setDecimalSeparator('-');
    currency.setGroupSeparator('*');
    currency.setAmountStyle(Models::AmountStyle::SymbolSpaceNumber);
    ASSERT_TRUE(currency.validate() == CurrencyCheckStatus::Valid);
    ASSERT_EQ(CurrencyHelpers::toAmountString(2765.9, currency), "@ 2*765-90");
}

TEST(CurrencyHelpers, ToCustomCurrency3)
{
    Currency currency{ "@", "HAS" };
    currency.setDecimalSeparator('-');
    currency.setGroupSeparator('-');
    ASSERT_TRUE(currency.validate() == CurrencyCheckStatus::SameSeparators);
}