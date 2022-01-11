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

    class Transaction
    {
    public:
        Transaction(int id = 0);
        int getID() const;
        const std::string& getDate() const;
        void setDate(const std::string& date);
        const std::string& getDescription() const;
        void setDescription(const std::string& description);
        TransactionType getType() const;
        std::string getTypeAsString() const;
        void setType(TransactionType type);
        double getAmount() const;
        void setAmount(double amount);
        bool operator<(const Transaction& toCompare) const;
        bool operator>(const Transaction& toCompare) const;
        bool operator==(const Transaction& toCompare) const;
        bool operator!=(const Transaction& toCompare) const;

    private:
        int m_id;
        std::string m_date;
        std::string m_description;
        TransactionType m_type;
        double m_amount;
    };
}

#endif // TRANSACTION_H
