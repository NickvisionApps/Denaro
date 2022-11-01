#include "group.hpp"

using namespace NickvisionMoney::Models;

Group::Group(unsigned int id) : m_id{ 0 }, m_name{ "" }, m_description{ "" }, m_monthlyAllowance{ -1.0 }
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

boost::multiprecision::cpp_dec_float_50 Group::getMonthlyAllowance() const
{
    return m_monthlyAllowance;
}

void Group::setMonthlyAllowance(boost::multiprecision::cpp_dec_float_50 monthlyAllowance)
{
    m_monthlyAllowance = monthlyAllowance;
}
