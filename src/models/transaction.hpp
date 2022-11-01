#pragma once

#include <string>
#include <boost/date_time/gregorian/gregorian.hpp>
#include <boost/multiprecision/cpp_dec_float.hpp>

namespace NickvisionMoney::Models
{
	/**
	 * Types of a transaction
	 */
	enum class TransactionType
    {
        Income = 0,
        Expense
    };

	/**
	 * Repeat intervals of a transaction
	 */
    enum class RepeatInterval
    {
        Never = 0,
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        Biyearly
    };

	/**
	 * A model of a transaction
	 */
    class Transaction
    {
    public:
    	/**
    	 * Constructs a Transaction
    	 *
    	 * @param id The id of the transaction
    	 */
        Transaction(unsigned int id = 0);
        /**
         * Gets the id of the transaction
         *
         * @returns The id of the transaction
         */
        unsigned int getId() const;
        /**
         * Gets the date of the transaction
         *
         * @returns The date of the transaction
         */
        const boost::gregorian::date& getDate() const;
        /**
         * Sets the date of the transaction
         *
         * @param date The new date
         */
        void setDate(const boost::gregorian::date& date);
        /**
         * Gets the description of the transaction
         *
         * @returns The description of the transaction
         */
        const std::string& getDescription() const;
        /**
         * Sets the description of the transaction
         *
         * @param description The new description
         */
        void setDescription(const std::string& description);
        /**
         * Gets the type of the transaction
         *
         * @returns The type of the transaction
         */
        TransactionType getType() const;
         /**
         * Sets the type of the transaction
         *
         * @param type The new type
         */
        void setType(TransactionType type);
        /**
         * Gets the repeat interval of the transaction
         *
         * @returns The repeat interval of the transaction
         */
        RepeatInterval getRepeatInterval() const;
        /**
         * Gets the repeat interval of the transaction as a string
         *
         * @returns The repeat interval of the transaction as a string
         */
        std::string getRepeatIntervalAsString() const;
         /**
         * Sets the repeat interval of the transaction
         *
         * @param repeatInterval The new repeat interval
         */
        void setRepeatInterval(RepeatInterval repeatInterval);
        /**
         * Gets the amount of the transaction
         *
         * @returns The amount of the transaction
         */
        boost::multiprecision::cpp_dec_float_50 getAmount() const;
        /**
         * Sets the amount of the transaction
         *
         * @param amount The new amount
         */
        void setAmount(boost::multiprecision::cpp_dec_float_50 amount);
        /**
         * Gets the group id of the transaction
         *
         * @returns The group id of the transaction. -1 if no group associated
         */
        int getGroupId() const;
        /**
         * Sets the group id of the transaction
         *
         * @param groupId The new group id
         */
        void setGroupId(int groupId);
        /**
         * Compares two Transactions via less-than
         *
         * @param toComapre The transaction to compare
         * @returns True if this transaction < toCompare, else false
         */
        bool operator<(const Transaction& toCompare) const;
         /**
         * Compares two Transactions via greater-than
         *
         * @param toComapre The transaction to compare
         * @returns True if this transaction > toCompare, else false
         */
        bool operator>(const Transaction& toCompare) const;
         /**
         * Compares two Transactions via equals
         *
         * @param toComapre The transaction to compare
         * @returns True if this transaction == toCompare, else false
         */
        bool operator==(const Transaction& toCompare) const;
         /**
         * Compares two Transactions via not equals
         *
         * @param toComapre The transaction to compare
         * @returns True if this transaction != toCompare, else false
         */
        bool operator!=(const Transaction& toCompare) const;

    private:
        unsigned int m_id;
        boost::gregorian::date m_date;
        std::string m_description;
        TransactionType m_type;
        RepeatInterval m_repeatInterval;
        boost::multiprecision::cpp_dec_float_50 m_amount;
        int m_groupId;
    };
}