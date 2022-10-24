#include "transaction.hpp"

using namespace NickvisionMoney::Models;

Transaction::Transaction(unsigned int id) : m_id{ id }, m_date{ "1/1/2022" }, m_description{ "" }, m_type{ TransactionType::Income }, m_repeatInterval{ RepeatInterval::Never }, m_amount{ 0.00 }
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

void Transaction::setType(TransactionType type)
{
    m_type = type;
}

RepeatInterval Transaction::getRepeatInterval() const
{
    return m_repeatInterval;
}

void Transaction::setRepeatInterval(RepeatInterval repeatInterval)
{
    m_repeatInterval = repeatInterval;
}

double Transaction::getAmount() const
{
    return m_amount;
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

