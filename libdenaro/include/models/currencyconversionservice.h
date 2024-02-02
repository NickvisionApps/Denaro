#ifndef CURRENCYCONVERSIONSERVICE_H
#define CURRENCYCONVERSIONSERVICE_H

#include <string>
#include <optional>
#include <unordered_map>
#include "currencyconversion.h"

namespace Nickvision::Money::Shared::Models::CurrencyConversionService
{
    /**
     * @brief Converts a sourceAmount from the sourceCurrency to the resultCurrency.
     * @param sourceCurrency The currency code of the source amount
     * @param sourceAmount The source amount to convert
     * @param resultCurrency The currency code of the result amount
     * @return The CurrencyConversion object if successful, else std::nullopt 
     */
    std::optional<CurrencyConversion> convert(const std::string& sourceCurrency, double sourceAmount, const std::string& resultCurrency);
    /**
     * @brief Gets a map of conversion rates from the sourceCurrency to other currencies.
     * @brief This method will cache the data for the sourceCurrency on disk.
     * @param sourceCurrency The currency code to get converting rates for 
     * @return The map of conversion rates
     */
    std::unordered_map<std::string, double> getConversionRates(const std::string& sourceCurrency);
}

#endif //CURRENCYCONVERSIONSERVICE_H