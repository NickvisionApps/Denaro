#include "models/currencyconversionservice.h"
#include <cstdint>
#include <chrono>
#include <filesystem>
#include <fstream>
#include <unordered_map>
#include <json/json.h>
#include <libnick/app/aura.h>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/helpers/webhelpers.h>

using namespace Nickvision::App;
using namespace Nickvision::Filesystem;

namespace Nickvision::Money::Shared::Models
{
    std::optional<CurrencyConversion> CurrencyConversionService::convert(const std::string& sourceCurrency, double sourceAmount, const std::string& resultCurrency)
    {
        if(sourceCurrency == resultCurrency)
        {
            return CurrencyConversion{ sourceCurrency, sourceAmount, resultCurrency, 1 };
        }
        const std::map<std::string, double>& rates{ getConversionRates(sourceCurrency) };
        if(rates.empty() || !rates.contains(resultCurrency))
        {
            return std::nullopt;
        }
        return CurrencyConversion{ sourceCurrency, sourceAmount, resultCurrency, rates.at(resultCurrency) };
    }

    const std::map<std::string, double>& CurrencyConversionService::getConversionRates(const std::string& sourceCurrency)
    {
        static std::unordered_map<std::string, std::map<std::string, double>> cache;
        std::filesystem::path path{ UserDirectories::getApplicationCache() / ("currency_" + sourceCurrency + ".json") };
        bool needsUpdate{ !std::filesystem::exists(path) };
        Json::Value json;
        if(std::filesystem::exists(path))
        {
            std::ifstream in{ path };
            in >> json;
            std::int64_t seconds{ json.get("time_next_update_unix", 0).asInt64() };
            std::int64_t secondsNow{ std::chrono::duration_cast<std::chrono::seconds>(std::chrono::system_clock::now().time_since_epoch()).count() };
            if(seconds <= secondsNow)
            {
                needsUpdate = true;
                json.clear();
                cache[sourceCurrency].clear();
                Aura::getActive().getLogger().log(Logging::LogLevel::Debug, sourceCurrency + " rates on disk out-of-date.");
            }
            else if(!cache[sourceCurrency].empty())
            {
                Aura::getActive().getLogger().log(Logging::LogLevel::Debug, sourceCurrency + " rates fetched from cache.");
                return cache[sourceCurrency];
            }
        }
        if(needsUpdate)
        {
            std::string apiUrl{ "https://open.er-api.com/v6/latest/" + sourceCurrency };
            std::string response{ WebHelpers::fetchJsonString(apiUrl) };
            if(!response.empty())
            {
                Json::Reader reader;
                reader.parse(response, json, false);
                Aura::getActive().getLogger().log(Logging::LogLevel::Debug, sourceCurrency + " rates fetched from internet.");
            }
        }
        if(!json.empty())
        {
            Json::Value ratesJson{ json.get("rates", {}) };
            if(!ratesJson.empty())
            {
                std::map<std::string, double>& rates{ cache[sourceCurrency] };
                double sourceRate{ ratesJson.get(sourceCurrency, 0.0).asDouble() };
                for(const std::string& rate : ratesJson.getMemberNames())
                {
                    rates[rate] = sourceRate / ratesJson.get(rate, 0.0).asDouble();
                }
                Aura::getActive().getLogger().log(Logging::LogLevel::Debug, sourceCurrency + " rates cached.");
                if(needsUpdate)
                {
                    std::ofstream out{ path };
                    out << json;
                    Aura::getActive().getLogger().log(Logging::LogLevel::Debug, sourceCurrency + " rates saved to disk.");
                }
            }
        }
        return cache[sourceCurrency];
    }
}
