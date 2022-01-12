#ifndef TRANSACTION_H
#define TRANSACTION_H

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
        std::string getTypeAsString() const;
        void setType(TransactionType type);
        RepeatInterval getRepeatInterval() const;
        std::string getRepeatIntervalAsString() const;
        void setRepeatInterval(RepeatInterval repeatInterval);
        double getAmount() const;
        std::string getAmountAsString() const;
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

#endif // TRANSACTION_H
