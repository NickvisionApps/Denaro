#include "models/group.h"

namespace Nickvision::Money::Shared::Models {
    
        Group::Group(unsigned int id)
            : m_id{ id },
            m_name{ "" },
            m_description{ "" },
            m_income{ 0.0 },
            m_expense{ 0.0 }
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
    
        double Group::getIncome() const
        {
            return m_income;
        }
    
        void Group::setIncome(double income)
        {
            m_income = income;
        }
    
        double Group::getExpense() const
        {
            return m_expense;
        }
    
        void Group::setExpense(double expense)
        {
            m_expense = expense;
        }
    
        double Group::getBalance() const
        {
            return m_income - m_expense;
        }
        
        const Color& Group::getColor() const
        {
            return m_color;
        }
        
        void Group::setColor(const Color& color)
        {
            m_color = color;
        }
        
        bool Group::operator==(const Group& other) const
        {
            return m_id == other.m_id;
        }
        
        bool Group::operator!=(const Group& other) const
        {
            return m_id != other.m_id;
        }
        
        bool Group::operator<(const Group& other) const
        {
            return m_name < other.m_name;
        }
        
        bool Group::operator>(const Group& other) const
        {
            return m_name > other.m_name;
        }
}
