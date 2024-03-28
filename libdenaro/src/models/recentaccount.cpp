#include "models/recentaccount.h"

namespace Nickvision::Money::Shared::Models
{
    RecentAccount::RecentAccount()
        : m_type{ AccountType::Checking }
    {

    }

    RecentAccount::RecentAccount(const std::filesystem::path& path)
        : m_path{ path },
        m_name{ path.stem().string() },
        m_type{ AccountType::Checking }
    {

    }

    const std::filesystem::path& RecentAccount::getPath() const
    {
        return m_path;
    }

    void RecentAccount::setPath(const std::filesystem::path& path)
    {
        m_path = path;
        if(m_name.empty())
        {
            m_name = m_path.stem().string();
        }
    }

    const std::string& RecentAccount::getName() const
    {
        return m_name;
    }

    void RecentAccount::setName(const std::string& name)
    {
        m_name = name;
    }

    AccountType RecentAccount::getType() const
    {
        return m_type;
    }

    void RecentAccount::setType(AccountType type)
    {
        m_type = type;
    }

    bool RecentAccount::empty() const
    {
        return m_path.empty() || !std::filesystem::exists(m_path);
    }

    bool RecentAccount::operator==(const RecentAccount& compare) const
    {
        return m_path == compare.m_path;
    }

    bool RecentAccount::operator!=(const RecentAccount& compare) const
    {
        return !operator==(compare);
    }

    RecentAccount::operator bool() const
    {
        return !empty();
    }
}