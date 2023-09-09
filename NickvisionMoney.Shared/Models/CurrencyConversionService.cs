using Nickvision.Aura;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Models;

/// <summary>
/// A model of a result of a currency conversion
/// </summary>
public class CurrencyConversion
{
    /// <summary>
    /// The currency code of the source amount
    /// </summary>
    public string SourceCurrnecy { get; init; }
    /// <summary>
    /// The source amount to convert
    /// </summary>
    public decimal SourceAmount { get; init; }
    /// <summary>
    /// The currency code for the result amount
    /// </summary>
    public string ResultCurrency { get; init; }
    /// <summary>
    /// The rate of conversion from the source currency to the result currency
    /// </summary>
    public decimal ConversionRate { get; init; }

    /// <summary>
    /// The result amount
    /// </summary>
    public decimal ResultAmount => SourceAmount / ConversionRate;
    
    /// <summary>
    /// Constructs a CurrencyConversion
    /// </summary>
    /// <param name="sourceCurrency">The currency code of the source amount</param>
    /// <param name="sourceAmount">The source amount to convert</param>
    /// <param name="resultCurrency">The currency code for the result amount</param>
    /// <param name="conversionRate">The rate of conversion from the source currency to the result currency</param>
    public CurrencyConversion(string sourceCurrency, decimal sourceAmount, string resultCurrency, decimal conversionRate)
    {
        SourceCurrnecy = sourceCurrency;
        SourceAmount = sourceAmount;
        ResultCurrency = resultCurrency;
        ConversionRate = conversionRate;
    }
}

/// <summary>
/// A service for getting currency conversions
/// </summary>
public class CurrencyConversionService
{
    private static readonly HttpClient _http;
    
    /// <summary>
    /// Constructs a static CurrencyConversionService
    /// </summary>
    static CurrencyConversionService()
    {
        _http = new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/116.0");
    }

    /// <summary>
    /// Converts a sourceAmount from the sourceCurrency to the resultCurrency
    /// </summary>
    /// <param name="sourceCurrency">The currency code of the source amount</param>
    /// <param name="sourceAmount">The source amount to convert</param>
    /// <param name="resultCurrency">The currency code for the result amount</param>
    /// <returns>CurrencyConversion if successful, else null</returns>
    public static async Task<CurrencyConversion?> ConvertAsync(string sourceCurrency, decimal sourceAmount, string resultCurrency)
    {
        if (sourceCurrency == resultCurrency)
        {
            return new CurrencyConversion(sourceCurrency, sourceAmount, resultCurrency, 1);
        }
        var rates = await GetConversionRatesAsync(sourceCurrency);
        if (rates == null || !rates.ContainsKey(resultCurrency))
        {
            return null;
        }
        return new CurrencyConversion(sourceCurrency, sourceAmount, resultCurrency, rates[resultCurrency]);
    }

    /// <summary>
    /// Gets a dictionary of conversion rates for the source currency
    /// </summary>
    /// <param name="sourceCurrency">The currency code to get converting rates for</param>
    /// <returns>Dictionary&lt;string, decimal&gt; is successful, else null</returns>
    /// <remarks>This method will cache the data for the sourceCurrency on disk</remarks>
    public static async Task<Dictionary<string, decimal>?> GetConversionRatesAsync(string sourceCurrency)
    {
        var cache = $"{UserDirectories.ApplicationCache}{Path.DirectorySeparatorChar}currency_{sourceCurrency}.json";
        var rates = await GetCachedConversionRatesAsync(cache);
        if (rates != null)
        {
            return rates;
        }
        string jsonData;
        (jsonData, rates) = await GetConversionRatesFromServiceAsync(sourceCurrency) ?? default;
        if (rates != null)
        {
            CacheRates(cache, jsonData);
            return rates;
        }
        return null;
    }

    /// <summary>
    /// Gets a dictionary of conversion rates from file cache
    /// </summary>
    /// <param name="cache">Path to cache file with conversion rates</param>
    /// <returns>Dictionary&lt;string, decimal&gt; is successful, else null</returns>
    private static async Task<Dictionary<string, decimal>?> GetCachedConversionRatesAsync(string cache)
    {
        if (File.Exists(cache))
        {
            try
            {
                using var json = JsonDocument.Parse(await File.ReadAllTextAsync(cache));
                var seconds = json.RootElement.GetProperty("time_next_update_unix").GetInt64();
                var nextUpdate = DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime();
                if (nextUpdate > DateTime.Now)
                {
                    return GetConversionRates(json);
                }
            }
            catch (Exception e)
            {
                // Couldn't get the cached rates
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        return null;
    }

    /// <summary>
    /// Gets a dictionary of conversion rates for the source currency from
    /// web service
    /// </summary>
    /// <param name="sourceCurrency">The currency code to get converting rates for</param>
    /// <returns>(string, Dictionary&lt;string, decimal&gt); is successful, else null</returns>
    private static async Task<(string, Dictionary<string, decimal>)?> GetConversionRatesFromServiceAsync(string sourceCurrency)
    {
        var apiUrl = $"https://open.er-api.com/v6/latest/{sourceCurrency}";
        try
        {
            var response = await _http.GetStringAsync(apiUrl);
            using var json = JsonDocument.Parse(response);
            if (json.RootElement.GetProperty("result").GetString() != "success")
            {
                return null;
            }
            var rates = GetConversionRates(json);
            if (rates != null)
            {
                return (response, rates);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            return null;
        }
        return null;
    }

    /// <summary>
    /// Cache conversion rates in file
    /// </summary>
    /// <param name="cache">Path to cache file</param>
    /// <param name="json">JSON string with rate data</param>
    private static async Task CacheRates(string cache, string json)
    {
        try
        {
            await File.WriteAllTextAsync(cache, json);
        }
        catch (Exception e)
        {
            // Couldn't cache the rates
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }

    /// <summary>
    /// Gets a dictionary of conversion rates from JsonDocument
    /// </summary>
    /// <param name="json">The JsonDocument with converting rates</param>
    /// <returns>Dictionary&lt;string, decimal&gt; is successful, else null</returns>
    private static Dictionary<string, decimal>? GetConversionRates(JsonDocument json)
    {
        if (json != null)
        {
            try
            {
                var rates = json.RootElement.GetProperty("rates").ToString() ?? "";
                return JsonSerializer.Deserialize<Dictionary<string, decimal>>(rates);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }
        return null;
    }
}
