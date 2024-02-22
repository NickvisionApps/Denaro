#include "models/group.h"

namespace Nickvision::Money::Shared::Models 
{
    Group::Group(int id)
        : m_id{ id },
        m_name{ "" },
        m_description{ "" }
    {

    }

    int Group::getId() const
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
    
    const Color& Group::getColor() const
    {
        return m_color;
    }
    
    void Group::setColor(const Color& color)
    {
        m_color = color;
    }
    
    bool Group::operator==(const Group& compare) const
    {
        return m_id == compare.m_id;
    }
    
    bool Group::operator!=(const Group& compare) const
    {
        return !operator==(compare);
    }
    
    bool Group::operator<(const Group& compare) const
    {
        return m_name < compare.m_name;
    }
    
    bool Group::operator>(const Group& compare) const
    {
        return m_name > compare.m_name;
    }
}
