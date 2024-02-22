#include "models/transaction.h"
#include <algorithm>

namespace Nickvision::Money::Shared::Models
{
    Transaction::Transaction(int id)
        : m_id{ id },
        m_date{ boost::gregorian::day_clock::local_day() },
        m_type{ TransactionType::Income },
        m_amount{ 0.0 },
        m_groupId{ -1 },
        m_useGroupColor{ true },
        m_repeatInterval{ TransactionRepeatInterval::Never },
        m_repeatFrom{ -1 }
    {

    }

    int Transaction::getId() const
    {
        return m_id;
    }

    const boost::gregorian::date& Transaction::getDate() const
    {
        return m_date;
    }

    void Transaction::setDate(const boost::gregorian::date& date)
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

    double Transaction::getAmount() const
    {
        return m_amount;
    }

    void Transaction::setAmount(double amount)
    {
        m_amount = amount;
    }

    int Transaction::getGroupId() const
    {
        return m_groupId;
    }

    void Transaction::setGroupId(int groupId)
    {
        if(groupId <= 0)
        {
            groupId = -1;
        }
        m_groupId = groupId;
    }

    const Color& Transaction::getColor() const
    {
        return m_color;
    }

    void Transaction::setColor(const Color& color)
    {
        m_color = color;
    }

    bool Transaction::getUseGroupColor() const
    {
        return m_useGroupColor;
    }

    void Transaction::setUseGroupColor(bool useGroupColor)
    {
        m_useGroupColor = useGroupColor;
    }

    const Receipt& Transaction::getReceipt() const
    {
        return m_receipt;
    }

    void Transaction::setReceipt(const Receipt& receipt)
    {
        m_receipt = receipt;
    }

    TransactionRepeatInterval Transaction::getRepeatInterval() const
    {
        return m_repeatInterval;
    }

    void Transaction::setRepeatInterval(TransactionRepeatInterval repeatInterval)
    {
        m_repeatInterval = repeatInterval;
    }

    int Transaction::getRepeatFrom() const
    {
        return m_repeatFrom;
    }

    void Transaction::setRepeatFrom(int repeatFrom)
    {
        m_repeatFrom = repeatFrom;
    }

    const boost::gregorian::date& Transaction::getRepeatEndDate() const
    {
        return m_repeatEndDate;
    }

    void Transaction::setRepeatEndDate(const boost::gregorian::date& repeatEndDate)
    {
        m_repeatEndDate = repeatEndDate;
    }

    const std::vector<std::string>& Transaction::getTags() const
    {
        return m_tags;
    }

    bool Transaction::addTag(const std::string& tag)
    {
        if(std::find(m_tags.begin(), m_tags.end(), tag) == m_tags.end())
        {
            m_tags.push_back(tag);
            return true;
        }
        return false;
    }

    bool Transaction::removeTag(const std::string& tag)
    {
        std::vector<std::string>::iterator find{ std::find(m_tags.begin(), m_tags.end(), tag) };
        if(find == m_tags.end())
        {
            return false;
        }
        m_tags.erase(find);
        return true;
    }

    void Transaction::setTags(const std::vector<std::string>& tags)
    {
        m_tags = tags;
    }

    const std::string& Transaction::getNotes() const
    {
        return m_notes;
    }

    void Transaction::setNotes(const std::string& notes)
    {
        m_notes = notes;
    }

    Transaction Transaction::repeat(int newId, const boost::gregorian::date& newDate) const
    {
        Transaction t{ newId };
        t.setDate(newDate);
        t.setDescription(m_description);
        t.setType(m_type);
        t.setAmount(m_amount);
        t.setGroupId(m_groupId);
        t.setColor(m_color);
        t.setUseGroupColor(m_useGroupColor);
        t.setReceipt(m_receipt);
        t.setRepeatInterval(m_repeatInterval);
        t.setRepeatFrom(m_id);
        t.setRepeatEndDate(m_repeatEndDate);
        for(const std::string& tag : m_tags)
        {
            t.addTag(tag);
        }
        t.setNotes(m_notes);
        return t;
    }

    bool Transaction::operator==(const Transaction& compare) const
    {
        return m_id == compare.m_id;
    }

    bool Transaction::operator!=(const Transaction& compare) const
    {
        return !operator==(compare);
    }

    bool Transaction::operator<(const Transaction& compare) const
    {
        return m_id < compare.m_id;
    }

    bool Transaction::operator>(const Transaction& compare) const
    {
        return m_id > compare.m_id;
    }
}