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
/// A service for getting currnecy conversions
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
            return new CurrencyConversion(sourceCurrency, sourceAmount, resultCurrency, sourceAmount);
        }
        var rates = await GetConversionRatesAsync(sourceCurrency);
        if (rates == null || !rates.ContainsKey(resultCurrency))
        {
            return null;
        }
        return new CurrencyConversion(sourceCurrency, sourceAmount, resultCurrency, rates[resultCurrency]);
    }

    /// <summary>
    /// Gets a dictionary of conversion rates from the source currency
    /// </summary>
    /// <param name="sourceCurrency">The currency code to get converting rates for</param>
    /// <returns>Dictionary&lt;string, decimal&gt; is successful, else false</returns>
    /// <remarks>This method will cache the data for the sourceCurrency on disk</remarks>
    public static async Task<Dictionary<string, decimal>?> GetConversionRatesAsync(string sourceCurrency)
    {
        var path = $"{ConfigurationLoader.ConfigDir}{Path.DirectorySeparatorChar}currency_{sourceCurrency}.json";
        var needsUpdate = false;
        JsonDocument? json = null;
        if (File.Exists(path))
        {
            try
            {
                json = JsonDocument.Parse(await File.ReadAllTextAsync(path));
                if (json.RootElement.GetProperty("time_next_update_utc").GetDateTime() <= DateTime.Today)
                {
                    needsUpdate = true;
                    json.Dispose();
                }
            }
            catch
            {
                needsUpdate = true;
                json?.Dispose();
            }
        }
        if (needsUpdate)
        {
            var apiUrl = $"https://open.er-api.com/v6/latest/{sourceCurrency}";
            try
            {
                var response = await _http.GetStringAsync(apiUrl);
                json = JsonDocument.Parse(response);
                if (json.RootElement.GetProperty("result").GetString() != "success")
                {
                    json.Dispose();
                    return null;
                }
                await File.WriteAllTextAsync(path, response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                json?.Dispose();
                return null;
            }
        }
        if (json != null)
        {
            try
            {
                var rates = json.RootElement.GetProperty("rates").GetString() ?? "";
                return JsonSerializer.Deserialize<Dictionary<string, decimal>>(rates);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
            finally
            {
                json.Dispose();
            }
        }
        return null;
    }
}