#include "group.hpp"

using namespace NickvisionMoney::Models;

Group::Group(unsigned int id) : m_id{ id }, m_name{ "" }, m_description{ "" }, m_balance{ 0.0 }
{

}

unsigned int Group::getId() const
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

boost::multiprecision::cpp_dec_float_50 Group::getBalance() const
{
    return m_balance;
}

void Group::setBalance(boost::multiprecision::cpp_dec_float_50 balance)
{
    m_balance = balance;
}
