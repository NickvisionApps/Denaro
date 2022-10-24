#pragma once

#include <string>

namespace NickvisionMoney::Models
{
	enum class TransactionType
    {
        Income,
        Expense
    };

    enum class RepeatInterval
    {
        Never,
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        Biyearly
    };

    class Transaction
    {
    public:
        Transaction(unsigned int id = 0);
        unsigned int getID() const;
        const std::string& getDate() const;
        void setDate(const std::string& date);
        const std::string& getDescription() const;
        void setDescription(const std::string& description);
        TransactionType getType() const;
        void setType(TransactionType type);
        RepeatInterval getRepeatInterval() const;
        void setRepeatInterval(RepeatInterval repeatInterval);
        double getAmount() const;
        void setAmount(double amount);
        bool operator<(const Transaction& toCompare) const;
        bool operator>(const Transaction& toCompare) const;
        bool operator==(const Transaction& toCompare) const;
        bool operator!=(const Transaction& toCompare) const;

    private:
        unsigned int m_id;
        std::string m_date;
        std::string m_description;
        TransactionType m_type;
        RepeatInterval m_repeatInterval;
        double m_amount;
    };
}