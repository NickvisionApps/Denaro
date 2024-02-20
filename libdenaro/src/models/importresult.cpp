#include "models/importresult.h"
#include <algorithm>

namespace Nickvision::Money::Shared::Models
{
    bool ImportResult::empty() const
    {
        return m_newTransactions.empty() && m_newGroups.empty() && m_newTags.empty();
    }

    const std::vector<int>& ImportResult::getNewTransactionIds() const
    {
        return m_newTransactions;
    }

    const std::vector<int>& ImportResult::getNewGroupIds() const
    {
        return m_newGroups;
    }

    const std::vector<std::string>& ImportResult::getNewTags() const
    {
        return m_newTags;
    }

    void ImportResult::addTransaction(int id)
    {
        if(std::find(m_newTransactions.begin(), m_newTransactions.end(), id) == m_newTransactions.end())
        {
            m_newTransactions.push_back(id);
        }
    }

    void ImportResult::addGroup(int id)
    {
        if(std::find(m_newGroups.begin(), m_newGroups.end(), id) == m_newGroups.end())
        {
            m_newGroups.push_back(id);
        }
    }

    void ImportResult::addTags(const std::vector<std::string>& tags)
    {
        for(const std::string& tag : tags)
        {
            if(std::find(m_newTags.begin(), m_newTags.end(), tag) == m_newTags.end())
            {
                m_newTags.push_back(tag);
            }
        }
    }

    ImportResult::operator bool() const
    {
        return !empty();
    }
}