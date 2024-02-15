#ifndef TRANSACTION_H
#define TRANSACTION_H

#include <string>
#include <vector>
#include <boost/date_time/gregorian/gregorian.hpp>
#include "color.h"
#include "receipt.h"
#include "transactionrepeatinterval.h"
#include "transactiontype.h"

namespace Nickvision::Money::Shared::Models
{
    /**
     * @brief A model of a transaction.
     */
    class Transaction
    {
    public:
        /**
         * @brief Constructs a Transaction.
         * @param id The id of the transaction 
         */
        Transaction(unsigned int id);
        /**
         * @brief Gets the id of the transaction.
         * @return The transaction id 
         */
        unsigned int getId() const;
        /**
         * @brief Gets the date of the transaction.
         * @return The transaction date 
         */
        const boost::gregorian::date& getDate() const;
        /**
         * @brief Sets the date of the transaction.
         * @param date The new transaction date 
         */
        void setDate(const boost::gregorian::date& date);
        /**
         * @brief Gets the description of the transaction.
         * @return The transaction description 
         */
        const std::string& getDescription() const;
        /**
         * @brief Sets the description of the transaction.
         * @param description The new transaction description 
         */
        void setDescription(const std::string& description);
        /**
         * @brief Gets the type of the transaction.
         * @return The transaction type 
         */
        TransactionType getType() const;
        /**
         * @brief Sets the type of the transaction.
         * @param type The new transaction type 
         */
        void setType(TransactionType type);
        /**
         * @brief Gets the amount of the transaction.
         * @return The transaction amount 
         */
        double getAmount() const;
        /**
         * @brief Sets the amount of the transaction.
         * @param amount The new transaction amount 
         */
        void setAmount(double amount);
        /**
         * @brief Gets the group id of the transaction.
         * @return The transaction group id (-1 for no group)
         */
        int getGroupId() const;
        /**
         * @brief Sets the group id of the transaction.
         * @param groupId The new transaction group id (-1 for no group) 
         */
        void setGroupId(int groupId);
        /**
         * @brief Gets the color of the transaction.
         * @return The transaction color 
         */
        const Color& getColor() const;
        /**
         * @brief Sets the color of the transaction.
         * @param color The new transaction color 
         */
        void setColor(const Color& color);
        /**
         * @brief Gets whether or not to use the color of the associated group instead of this transaction's color.
         * @return True to use group color, else false 
         */
        bool getUseGroupColor() const;
        /**
         * @brief Sets whether or not to use the color of the associated group isntead of this transaction's color.
         * @param useGroupColor True to use group color, else false 
         */
        void setUseGroupColor(bool useGroupColor);
        /**
         * @brief Gets the receipt of the transaction.
         * @return The transaction receipt 
         */
        const Receipt& getReceipt() const;
        /**
         * @brief Sets the receipt of the transaction.
         * @param receipt The new transaction receipt 
         */
        void setReceipt(const Receipt& receipt);
        /**
         * @brief Gets the repeat interval of the transaction.
         * @return The transaction repeat interval 
         */
        TransactionRepeatInterval getRepeatInterval() const;
        /**
         * @brief Sets the repeat interval of the transaction.
         * @param repeatInterval The new transaction repeat interval 
         */
        void setRepeatInterval(TransactionRepeatInterval repeatInterval);
        /**
         * @brief Gets the source id of the repeat transaction.
         * @return The repeat transaction source id (-1 if not a repeat transaction, 0 if source transaction)
         */
        int getRepeatFrom() const;
        /**
         * @brief Sets the source id of the repeat transaction.
         * @param repeatFrom The new repeat transaction source id (-1 if not a repeat transaction, 0 if source transaction)
         */
        void setRepeatFrom(int repeatFrom);
        /**
         * @brief Gets the repeat end date of the transaction.
         * @return The transaction repeat end date 
         */
        const boost::gregorian::date& getRepeatEndDate() const;
        /**
         * @brief Sets the repeat end date of the transaction.
         * @param repeatEndDate The new transaction repeat end date
         */
        void setRepeatEndDate(const boost::gregorian::date& repeatEndDate);
        /**
         * @brief Gets the tags of the transaction.
         * @return The transaction tags
         */
        const std::vector<std::string>& getTags() const;
        /**
         * @brief Adds a tag to the transaction.
         * @param tag The tag to add
         * @return True if successful, else false if the tag already exists in the transaction
         */
        bool addTag(const std::string& tag);
        /**
         * @brief Removes a tag from the transaction.
         * @param tag The tag to remove
         * @return True if successful, else false if the tag does not exist in the transaction
         */
        bool removeTag(const std::string& tag);
        /**
         * @brief Gets the notes of the transaction.
         * @return The transaction notes  
         */
        const std::string& getNotes() const;
        /**
         * @brief Sets the notes of the transaction.
         * @param notes The new transaction notes 
         */
        void setNotes(const std::string& notes);
        /**
         * @brief Creates a repeat transaction using this transaction as the source.
         * @param newId The id to use for the repeat transaction
         * @param newDate The date to use for the repeat transaction
         * @return The new repeat transaction
         */
        Transaction repeat(unsigned int newId, const boost::gregorian::date& newDate) const;
        /**
         * @brief Gets whether or not this Transaction is equal to compare Transaction.
         * @param compare The Transaction to compare to
         * @return True if this Transaction == compare Transaction 
         */
        bool operator==(const Transaction& compare) const;
        /**
         * @brief Gets whether or not this Transaction is not equal to compare Transaction.
         * @param compare The Transaction to compare to
         * @return True if this Transaction != compare Transaction 
         */
        bool operator!=(const Transaction& compare) const;
        /**
         * @brief Gets whether or not this Transaction is less than to compare Transaction.
         * @param compare The Transaction to compare to
         * @return True if this Transaction < compare Transaction 
         */
        bool operator<(const Transaction& compare) const;
        /**
         * @brief Gets whether or not this Transaction is greater than to compare Transaction.
         * @param compare The Transaction to compare to
         * @return True if this Transaction > compare Transaction 
         */
        bool operator>(const Transaction& compare) const;

    private:
        unsigned int m_id;
        boost::gregorian::date m_date;
        std::string m_description;
        TransactionType m_type;
        double m_amount;
        int m_groupId;
        Color m_color;
        bool m_useGroupColor;
        Receipt m_receipt;
        TransactionRepeatInterval m_repeatInterval;
        int m_repeatFrom;
        boost::gregorian::date m_repeatEndDate;
        std::vector<std::string> m_tags;
        std::string m_notes;
    };
}

#endif //TRANSACTION_H