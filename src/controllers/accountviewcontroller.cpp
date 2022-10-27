#include "accountviewcontroller.hpp"
#include <sstream>

using namespace NickvisionMoney::Controllers;

AccountViewController::AccountViewController(const std::string& path) : m_account{ path }
{

}

const std::string& AccountViewController::getAccountPath() const
{
    return m_account.getPath();
}

std::string AccountViewController::getAccountTotalString() const
{
    std::stringstream builder;
    builder << m_account.getTotal();
    return builder.str();
}

std::string AccountViewController::getAccountIncomeString() const
{
    std::stringstream builder;
    builder << m_account.getIncome();
    return builder.str();
}

std::string AccountViewController::getAccountExpenseString() const
{
    std::stringstream builder;
    builder << m_account.getExpense();
    return builder.str();
}
