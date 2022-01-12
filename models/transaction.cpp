#include "transaction.h"
#include <sstream>
#include <iomanip>
#include <cmath>

namespace NickvisionMoney::Models
{
    Transaction::Transaction(unsigned int id) : m_id(id), m_date("1-1-2000"), m_description(""), m_type(TransactionType::Income), m_repeatInterval(RepeatInterval::Never), m_amount(0.00)
    {

    }

    unsigned int Transaction::getID() const
    {
        return m_id;
    }

    const std::string& Transaction::getDate() const
    {
        return m_date;
    }

    void Transaction::setDate(const std::string& date)
    {
        m_date = date;
    }

    const std::string& Transaction::getDescription() const
    {
        return m_description;
    }

    void Transaction::setDescription(const std::string& description)
    {
        m_description = description;
    }

    TransactionType Transaction::getType() const
    {
        return m_type;
    }

    std::string Transaction::getTypeAsString() const
    {
        if(m_type == TransactionType::Income)
        {
            return "Income";
        }
        else
        {
            return "Expense";
        }
    }

    void Transaction::setType(TransactionType type)
    {
        m_type = type;
    }

    RepeatInterval Transaction::getRepeatInterval() const
    {
        return m_repeatInterval;
    }

    std::string Transaction::getRepeatIntervalAsString() const
    {
        if(m_repeatInterval == RepeatInterval::Never)
        {
            return "Never";
        }
        else if(m_repeatInterval == RepeatInterval::Daily)
        {
            return "Daily";
        }
        else if(m_repeatInterval == RepeatInterval::Weekly)
        {
            return "Weekly";
        }
        else if(m_repeatInterval == RepeatInterval::Monthly)
        {
            return "Monthly";
        }
        else if(m_repeatInterval == RepeatInterval::Quarterly)
        {
            return "Quarterly";
        }
        else if(m_repeatInterval == RepeatInterval::Yearly)
        {
            return "Yearly";
        }
        else
        {
            return "Biyearly";
        }
    }

    void Transaction::setRepeatInterval(RepeatInterval repeatInterval)
    {
        m_repeatInterval = repeatInterval;
    }

    double Transaction::getAmount() const
    {
        return m_amount;
    }

    std::string Transaction::getAmountAsString() const
    {
        std::stringstream builder;
        builder << std::fixed << std::setprecision(2) << std::ceil(m_amount * 100.0) / 100.0;
        return builder.str();
    }

    void Transaction::setAmount(double amount)
    {
        m_amount = amount;
    }

    bool Transaction::operator<(const Transaction& toCompare) const
    {
        return m_id < toCompare.m_id;
    }

    bool Transaction::operator>(const Transaction& toCompare) const
    {
        return m_id > toCompare.m_id;
    }

    bool Transaction::operator==(const Transaction& toCompare) const
    {
        return m_id == toCompare.m_id;
    }

    bool Transaction::operator!=(const Transaction& toCompare) const
    {
        return m_id != toCompare.m_id;
    }
}
