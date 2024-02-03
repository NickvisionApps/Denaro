#include "models/importresult.h"
#include <algorithm>

namespace Nickvision::Money::Shared::Models
{
    bool ImportResult::empty() const
    {
        return m_newTransactionIds.empty() && m_newGroupIds.empty() && m_newTags.empty();
    }

    const std::vector<unsigned int>& ImportResult::getNewTransactionIds() const
    {
        return m_newTransactionIds;
    }

    const std::vector<unsigned int>& ImportResult::getNewGroupIds() const
    {
        return m_newGroupIds;
    }

    const std::vector<std::string>& ImportResult::getNewTags() const
    {
        return m_newTags;
    }

    void ImportResult::addTransactionId(unsigned int id)
    {
        if(std::find(m_newTransactionIds.begin(), m_newTransactionIds.end(), id) == m_newTransactionIds.end())
        {
            m_newTransactionIds.push_back(id);
        }
    }

    void ImportResult::addGroupId(unsigned int id)
    {
        if(std::find(m_newGroupIds.begin(), m_newGroupIds.end(), id) == m_newGroupIds.end())
        {
            m_newGroupIds.push_back(id);
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