#include "models/currencyconversion.h"

namespace Nickvision::Money::Shared::Models
{
    CurrencyConversion::CurrencyConversion(const std::string& sourceCurrency, double sourceAmount, const std::string& resultCurrency, double conversionRate)
        : m_sourceCurrency{ sourceCurrency },
        m_sourceAmount{ sourceAmount },
        m_resultCurrency{ resultCurrency },
        m_conversionRate{ m_conversionRate }
    {

    }

    const std::string& CurrencyConversion::getSourceCurrency() const
    {
        return m_sourceCurrency;
    }

    double CurrencyConversion::getSourceAmount() const
    {
        return m_sourceAmount;
    }

    const std::string& CurrencyConversion::getResultCurrency() const
    {
        return m_resultCurrency;
    }

    double CurrencyConversion::getConversionRate() const
    {
        return m_conversionRate;
    }

    double CurrencyConversion::getResultAmount() const
    {
        return m_sourceAmount * m_conversionRate;
    }
}