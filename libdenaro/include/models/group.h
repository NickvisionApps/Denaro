#ifndef GROUP_H
#define GROUP_H

#include "color.h"
#include <string>

namespace Nickvision::Money::Shared::Models 
{
    /**
     * @brief A model of a group of transactions.
     */
    class Group {
    public:
        /**
         * @brief Constructs a Group.
         * @param id The id of the group
         */
        Group(unsigned int id);
        /*
         * @brief Gets the id of the group.
         * @return The id of the group.
         */
        unsigned int getId() const;
        /*
         * @brief Gets the name of the group.
         * @return The name of the group.
         */
        const std::string& getName() const;
        /*
         * @brief Sets the name of the group.
         * @param name The new name of the group.
         */
        void setName(const std::string& name);
        /*
         * @brief Gets the description of the group.
         * @return The description of the group.
         */
        const std::string& getDescription() const;
        /*
         * @brief Sets the description of the group.
         * @param description The new description of the group.
         */
        void setDescription(const std::string& description);
        /*
         * @brief Gets the income of the group.
         * @return The income of the group.
         */
        double getIncome() const;
        /*
         * @brief Sets the income of the group.
         * @param income The new income of the group.
         */
        void setIncome(double income);
        /*
         * @brief Gets the expense of the group.
         * @return The expense of the group.
         */
        double getExpense() const;
        /*
         * @brief Sets the expense of the group.
         * @param expense The new expense of the group.
         */
        void setExpense(double expense);
        /*
         * @brief Gets the balance of the group.
         * @return The balance of the group.
         */
        double getBalance() const;
        /*
         * @brief Gets the color of the group.
         * @return The color of the group.
         */
        const Color& getColor() const;
        /*
         * @brief Sets the color of the group.
         * @param color The new color of the group.
         */
        void setColor(const Color& color);
        /*
         * @brief Checks if this object is the same as another group object.
         */
        bool operator==(const Group& other) const;
        /*
         * @brief Checks if this object is not the same as another group object.
         */
        bool operator!=(const Group& other) const;
        /*
         * @brief Checks if this object is less than another group object.
         */
        bool operator<(const Group& other) const;
        /*
         * @brief Checks if this object is greater than another group object.
         */
        bool operator>(const Group& other) const;
        
    private:
        unsigned int m_id;
        std::string m_name;
        std::string m_description;
        double m_income;
        double m_expense;
        Color m_color;
    };
    
}


#endif //GROUP_H
