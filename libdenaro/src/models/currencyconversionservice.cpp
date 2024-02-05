#include "models/currencyconversionservice.h"
#include <cstdint>
#include <chrono>
#include <filesystem>
#include <fstream>
#include <json/json.h>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/helpers/webhelpers.h>

using namespace Nickvision::Filesystem;

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
        std::filesystem::path path{ UserDirectories::getApplicationCache() / ("currency_" + sourceCurrency + ".json") };
        bool needsUpdate{ !std::filesystem::exists(path) };
        Json::Value json;
        if(!needsUpdate) //std::filesystem::exists(path)
        {
            std::ifstream in{ path };
            in >> json;
            std::int64_t seconds{ json.get("time_next_update_unix", 0).asInt64() };
            std::int64_t secondsNow{ std::chrono::duration_cast<std::chrono::seconds>(std::chrono::system_clock::now().time_since_epoch()).count() };
            if(seconds <= secondsNow)
            {
                needsUpdate = true;
                json.clear();
            }
        }
        if(needsUpdate)
        {
            std::string apiUrl{ "https://open.er-api.com/v6/latest/" + sourceCurrency };
            std::string response{ WebHelpers::fetchJsonString(apiUrl) };
            if(!response.empty())
            {
                json = { response };
                if(json.get("result", "").asString() != "success")
                {
                    return {};
                }
            }
        }
        if(!json.empty())
        {
            Json::Value ratesJson{ json.get("rates", {}) };
            if(!ratesJson.empty())
            {
                std::unordered_map<std::string, double> rates;
                double sourceRate{ ratesJson.get(sourceCurrency, 0.0).asDouble() };
                for(const std::string& rate : ratesJson.getMemberNames())
                {
                    rates[rate] = sourceRate / ratesJson.get(rate, 0.0).asDouble();
                }
                if(needsUpdate)
                {
                    std::ofstream out{ path };
                    out << json;
                }
                return rates;
            }
        }
        return {};
    }
}