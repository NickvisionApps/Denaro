#ifndef TRANSACTIONREMINDER_H
#define TRANSACTIONREMINDER_H

#include <string>
#include <boost/date_time/gregorian/gregorian.hpp>
#include "currency.h"
#include "transactiontype.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A reminder for a transaction.
     */
    class TransactionReminder
    {
    public:
        /**
         * @brief Constructs a TransactionReminder.
         * @param description The description of the transaction.
         * @param when The date of when the transaction is occurring.
         */
        TransactionReminder(const std::string& description, const boost::gregorian::date& when);
        /**
         * @brief Gets the description of the transaction.
         * @return The description of the transaction 
         */
        const std::string& getDescription() const;
        /**
         * @brief Sets the description of the transaction.
         * @param description The new description of the transaction
         */
        void setDescription(const std::string& description);
        /**
         * @brief Gets the amount of the transaction.
         * @return The amount of the transaction
         */
        double getAmount() const;
        /**
         * @brief Gets the amount of the transaction as a string.
         * @return The amount of the transaction as a string
         */
        const std::string& getAmountString() const;
        /**
         * @brief Sets the amount of the transaction.
         * @param amount The new amount of the transaction
         * @param type The type of the transaction
         * @param currency The currency of the account
         */
        void setAmount(double amount, TransactionType type, const Currency& currency);
        /**
         * @brief Gets when the transaction is occurring.
         * @return When the transaction is occurring
         */
        const boost::gregorian::date& getWhen() const;
        /**
         * @brief Gets when the transaction is occurring as a string.
         * @return When the transaction is occurring as a string
         */
        const std::string& getWhenString() const;
        /**
         * @brief Sets the when the transaction is occurring.
         * @param when The new date of when the transaction is occurring
         */
        void setWhen(const boost::gregorian::date& when);

    private:
        std::string m_description;
        double m_amount;
        std::string m_amountString;
        boost::gregorian::date m_when;
        std::string m_whenString;
    };
}

#endif //TRANSACTIONREMINDER_H