#include "transfer.hpp"

using namespace NickvisionMoney::Models;

Transfer::Transfer(const std::string& sourceAccountPath) : m_sourceAccountPath{ sourceAccountPath }, m_destAccountPath{ "" }, m_amount{ 0.00 }
{

}

const std::string& Transfer::getSourceAccountPath() const
{
    return m_sourceAccountPath;
}

const std::string& Transfer::getDestAccountPath() const
{
    return m_destAccountPath;
}

void Transfer::setDestAccountPath(const std::string& destAccountPath)
{
    m_destAccountPath = destAccountPath;
}

boost::multiprecision::cpp_dec_float_50 Transfer::getAmount() const
{
    return m_amount;
}

void Transfer::setAmount(boost::multiprecision::cpp_dec_float_50 amount)
{
    m_amount = amount;
}
