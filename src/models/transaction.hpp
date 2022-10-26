#pragma once

#include <string>
#include <boost/date_time/gregorian/gregorian.hpp>
#include <boost/multiprecision/cpp_dec_float.hpp>

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
        unsigned int getId() const;
        const boost::gregorian::date& getDate() const;
        void setDate(const boost::gregorian::date& date);
        const std::string& getDescription() const;
        void setDescription(const std::string& description);
        TransactionType getType() const;
        void setType(TransactionType type);
        RepeatInterval getRepeatInterval() const;
        void setRepeatInterval(RepeatInterval repeatInterval);
        boost::multiprecision::cpp_dec_float_50 getAmount() const;
        void setAmount(boost::multiprecision::cpp_dec_float_50 amount);
        bool operator<(const Transaction& toCompare) const;
        bool operator>(const Transaction& toCompare) const;
        bool operator==(const Transaction& toCompare) const;
        bool operator!=(const Transaction& toCompare) const;

    private:
        unsigned int m_id;
        boost::gregorian::date m_date;
        std::string m_description;
        TransactionType m_type;
        RepeatInterval m_repeatInterval;
        boost::multiprecision::cpp_dec_float_50 m_amount;
    };
}