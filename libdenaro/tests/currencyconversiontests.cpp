#include <gtest/gtest.h>
#include <filesystem>
#include <libnick/app/aura.h>
#include <libnick/filesystem/userdirectories.h>
#include "models/currencyconversion.h"
#include "models/currencyconversionservice.h"

using namespace Nickvision::App;
using namespace Nickvision::Filesystem;
using namespace Nickvision::Money::Shared::Models;

class CurrencyConversionTest : public testing::Test
{
public:
    static void SetUpTestSuite()
    {
        Aura::getActive().init("org.nickvision.money", "Nickvision Denaro", "Denaro");
    }
};

TEST_F(CurrencyConversionTest, ServiceGetRates)
{
    std::map<std::string, double> rates{ CurrencyConversionService::getConversionRates("USD") };
    ASSERT_TRUE(rates.size() > 0);
    ASSERT_TRUE(std::filesystem::exists(UserDirectories::getApplicationCache() / ("currency_USD.json")));
}

TEST_F(CurrencyConversionTest, ServiceConvert)
{
    std::optional<CurrencyConversion> conversion{ CurrencyConversionService::convert("USD", 2.0, "EUR") };
    ASSERT_TRUE(conversion.has_value());
    ASSERT_EQ(conversion->getSourceCurrency(), "USD");
    ASSERT_EQ(conversion->getResultCurrency(), "EUR");
    ASSERT_EQ(conversion->getSourceAmount(), 2.0);
    ASSERT_TRUE(conversion->getResultAmount() > 0);
}