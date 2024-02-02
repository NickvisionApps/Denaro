#ifndef CURRENCYCONVERSION_H
#define CURRENCYCONVERSION_H

#include <string>

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a currency conversion. 
     */
    class CurrencyConversion
    {
    public:
        /**
         * @brief Constructs a CurrencyConversion.
         * @param sourceCurrency The currency code of the source amount
         * @param sourceAmount The source amount to convert
         * @param resultCurrency The currency code of the result amount
         * @param conversionRate The rate of conversion from the source currency to the result currency 
         */
        CurrencyConversion(const std::string& sourceCurrency, double sourceAmount, const std::string& resultCurrency, double conversionRate);
        /**
         * @brief Gets the currency code of the source amount.
         * @return The source currency code 
         */
        const std::string& getSourceCurrency() const;
        /**
         * @brief Gets the source amount to convert.
         * @return The source amount 
         */
        double getSourceAmount() const;
        /**
         * @brief Gets the currency code of the result amount.
         * @return The result currency code 
         */
        const std::string& getResultCurrency() const;
        /**
         * @brief Gets the rate of conversion from the source currency to the result currency.
         * @return The conversion rate 
         */
        double getConversionRate() const;
        /**
         * @brief Gets the result amount using the conversion rate.
         * @return The result amount 
         */
        double getResultAmount() const;

    private:
        std::string m_sourceCurrency;
        double m_sourceAmount;
        std::string m_resultCurrency;
        double m_conversionRate;
    };
}

#endif //CURRENTCONVERSION_H