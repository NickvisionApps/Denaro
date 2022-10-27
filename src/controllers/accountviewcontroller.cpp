#include "accountviewcontroller.hpp"

using namespace NickvisionMoney::Controllers;

AccountViewController::AccountViewController(const std::string& path) : m_account{ path }
{

}

const std::string& AccountViewController::getAccountPath() const
{
    return m_account.getPath();
}
