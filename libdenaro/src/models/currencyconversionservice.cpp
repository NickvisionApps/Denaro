#include "models/currencyconversionservice.h"

namespace Nickvision::Money::Shared::Models
{
    std::optional<CurrencyConversion> CurrencyConversionService::convert(const std::string& sourceCurrency, double sourceAmount, const std::string& resultCurrency)
    {
        if(sourceCurrency == resultCurrency)
        {
            return CurrencyConversion{ sourceCurrency, sourceAmount, resultCurrency, 1 };
        }
        std::unordered_map<std::string, double> rates{ getConversionRates(sourceCurrency) };
        if(rates.empty() || !rates.contains(resultCurrency))
        {
            return std::nullopt;
        }
        return CurrencyConversion{ sourceCurrency, sourceAmount, resultCurrency, rates[resultCurrency] };
    }

    std::unordered_map<std::string, double> CurrencyConversionService::getConversionRates(const std::string& sourceCurrency)
    {
        return {};
    }
}