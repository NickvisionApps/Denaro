#include "models/transfer.h"

namespace Nickvision::Money::Shared::Models
{
    Transfer::Transfer(const std::filesystem::path& sourceAccountPath, const std::string& sourceAccountName)
        : m_sourceAccountPath{ sourceAccountPath },
        m_sourceAccountName{ sourceAccountName.empty() ? sourceAccountPath.stem().string() : sourceAccountName },
        m_sourceAmount{ 0.0 },
        m_destinationAccountPath{ std::filesystem::path() },
        m_destinationAccountName{ "" },
        m_destinationAccountPassword{ "" },
        m_conversionRate{ 1.0 }
    {

    }

    const std::filesystem::path& Transfer::getSourceAccountPath() const
    {
        return m_sourceAccountPath;
    }

    const std::string& Transfer::getSourceAccountName() const
    {
        return m_sourceAccountName;
    }

    void Transfer::setSourceAccountName(const std::string& sourceAccountName)
    {
        m_sourceAccountName = sourceAccountName;
    }

    double Transfer::getSourceAmount() const
    {
        return m_sourceAmount;
    }

    void Transfer::setSourceAmount(double sourceAmount)
    {
        m_sourceAmount = sourceAmount;
    }

    const std::filesystem::path& Transfer::getDestinationAccountPath() const
    {
        return m_destinationAccountPath;
    }

    void Transfer::setDestinationAccountPath(const std::filesystem::path& destinationAccountPath)
    {
        m_destinationAccountPath = destinationAccountPath;
    }

    const std::string& Transfer::getDestinationAccountName() const
    {
        return m_destinationAccountName;
    }

    void Transfer::setDestinationAccountName(const std::string& destinationAccountName)
    {
        m_destinationAccountName = destinationAccountName;
    }

    const std::string& Transfer::getDestinationAccountPassword() const
    {
        return m_destinationAccountPassword;
    }

    void Transfer::setDestinationAccountPassword(const std::string& destinationAccountPassword)
    {
        m_destinationAccountPassword = destinationAccountPassword;
    }

    double Transfer::getConversionRate() const
    {
        return m_conversionRate;
    }

    void Transfer::setConversionRate(double conversionRate)
    {
        m_conversionRate = conversionRate;
    }

    double Transfer::getDestinationAmount() const
    {
        return m_sourceAmount / m_conversionRate;
    }
}