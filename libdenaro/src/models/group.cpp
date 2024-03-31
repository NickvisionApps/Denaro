#include "models/group.h"
#include <utility>

namespace Nickvision::Money::Shared::Models 
{
    Group::Group(int id)
        : m_id{ id },
        m_name{ "" },
        m_description{ "" },
        m_balance{ 0.0 }
    {

    }

    int Group::getId() const
    {
        return m_id;
    }

    const std::string& Group::getName() const
    {
        return m_name;
    }

    void Group::setName(const std::string& name)
    {
        m_name = name;
    }

    const std::string& Group::getDescription() const
    {
        return m_description;
    }

    void Group::setDescription(const std::string& description)
    {
        m_description = description;
    }
    
    const Color& Group::getColor() const
    {
        return m_color;
    }
    
    void Group::setColor(const Color& color)
    {
        m_color = color;
    }

    double Group::getBalance(const std::vector<int>& transactionIds) const
    {
        if(transactionIds.empty())
        {
            return m_balance;
        }
        double balance{ 0.0 };
        for(const std::pair<const int, double>& pair : m_amounts)
        {
            if(std::find(transactionIds.begin(), transactionIds.end(), pair.first) != transactionIds.end())
            {
                balance += pair.second;
            }
        }
        return balance;
    }

    double Group::updateBalance(const Transaction& transaction, bool remove)
    {
        if(remove)
        {
            if(m_amounts.contains(transaction.getId()))
            {
                m_balance -= m_amounts[transaction.getId()];
                m_amounts.erase(transaction.getId());
            }
        }
        else
        {
            if(m_amounts.contains(transaction.getId()))
            {
                m_balance -= m_amounts[transaction.getId()];
            }
            m_amounts[transaction.getId()] = transaction.getType() == TransactionType::Income ? transaction.getAmount() : transaction.getAmount() * -1.0;
            m_balance += m_amounts[transaction.getId()];
        }
        return m_balance;
    }

    double Group::getIncome(const std::vector<int>& transactionIds) const
    {
        double income{ 0.0 };
        for(const std::pair<const int, double>& pair : m_amounts)
        {
            if(!transactionIds.empty() && std::find(transactionIds.begin(), transactionIds.end(), pair.first) == transactionIds.end())
            {
                continue;
            }
            if(pair.second >= 0.0)
            {
                income += pair.second;
            }
        }
        return income;
    }

    double Group::getExpense(const std::vector<int>& transactionIds) const
    {
        double expense{ 0.0 };
        for(const std::pair<const int, double>& pair : m_amounts)
        {
            if(!transactionIds.empty() && std::find(transactionIds.begin(), transactionIds.end(), pair.first) == transactionIds.end())
            {
                continue;
            }
            if(pair.second < 0.0)
            {
                expense += pair.second;
            }
        }
        return expense;
    }
    
    bool Group::operator==(const Group& compare) const
    {
        return m_id == compare.m_id;
    }
    
    bool Group::operator!=(const Group& compare) const
    {
        return !operator==(compare);
    }
    
    bool Group::operator<(const Group& compare) const
    {
        return m_name < compare.m_name;
    }
    
    bool Group::operator>(const Group& compare) const
    {
        return m_name > compare.m_name;
    }
}
