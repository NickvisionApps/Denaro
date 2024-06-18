#ifndef TRANSFER_H
#define TRANSFER_H

#include <filesystem>
#include <string>

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of an amount transfer from a source account to a destination account.
     */
    class Transfer
    {
    public:
        /**
         * @brief Constructs a Transfer.
         * @param sourceAccountPath The path of the source account
         * @param sourceAccountName The name of the source account, if available 
         */
        Transfer(const std::filesystem::path& sourceAccountPath, const std::string& sourceAccountName = "");
        /**
         * @brief Gets the path of the source account.
         * @return The source account path  
         */
        const std::filesystem::path& getSourceAccountPath() const;
        /**
         * @brief Gets the name of the source account.
         * @return The source account name  
         */
        const std::string& getSourceAccountName() const;
        /**
         * @brief Sets the name of the source account.
         * @param sourceAccountName The new source account name
         */
        void setSourceAccountName(const std::string& sourceAccountName);
        /**
         * @brief Gets the amount of the transfer from the source account.
         * @return The source transfer amount 
         */
        double getSourceAmount() const;
        /**
         * @brief Sets the amount of the transfer from the source account.
         * @param sourceAmount The new source transfer amount
         */
        void setSourceAmount(double sourceAmount);
        /**
         * @brief Gets the path of the destination account.
         * @return The destination account path  
         */
        const std::filesystem::path& getDestinationAccountPath() const;
        /**
         * @brief Sets the path of the destination account.
         * @param destinationAccountPath The new destination account path 
         */
        void setDestinationAccountPath(const std::filesystem::path& destinationAccountPath);
        /**
         * @brief Gets the password of the destination account.
         * @return The destination account password  
         */
        const std::string& getDestinationAccountPassword() const;
        /**
         * @brief Sets the password of the destination account.
         * @param destinationAccountPassword The new destination account password 
         */
        void setDestinationAccountPassword(const std::string& destinationAccountPassword);
        /**
         * @brief Gets the rate of conversion from the source currency to the destination currency.
         * @brief The conversion rate will be 1 if the source and destination currencies are the same.
         * @return The conversion rate
         */
        double getConversionRate() const;
        /**
         * @brief Sets the rate of conversion from the source currency to the destination currency.
         * @brief Should be set to 1 if the source and destination currencies are the same.
         * @param conversionRate The new conversion rate 
         */
        void setConversionRate(double conversionRate);
        /**
         * @brief Gets the amount of the transfer to the destination account.
         * @return The destination transfer amount
         */
        double getDestinationAmount() const;

    private:
        std::filesystem::path m_sourceAccountPath;
        std::string m_sourceAccountName;
        double m_sourceAmount;
        std::filesystem::path m_destinationAccountPath;
        std::string m_destinationAccountPassword;
        double m_conversionRate;
    };
}

#endif //TRANSFER_H