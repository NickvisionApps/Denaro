#ifndef GROUP_H
#define GROUP_H

#include <string>
#include "color.h"

namespace Nickvision::Money::Shared::Models 
{
    /**
     * @brief A model of a group of transactions.
     */
    class Group 
    {
    public:
        /**
         * @brief Constructs a Group.
         * @brief This default constructor is required for the use of std::unordered_map. It should not be used by clients.
         */
        Group();
        /**
         * @brief Constructs a Group.
         * @param id The id of the group
         */
        Group(int id);
        /**
         * @brief Gets the id of the group.
         * @return The id of the group.
         */
        int getId() const;
        /**
         * @brief Gets the name of the group.
         * @return The name of the group.
         */
        const std::string& getName() const;
        /**
         * @brief Sets the name of the group.
         * @param name The new name of the group.
         */
        void setName(const std::string& name);
        /**
         * @brief Gets the description of the group.
         * @return The description of the group.
         */
        const std::string& getDescription() const;
        /**
         * @brief Sets the description of the group.
         * @param description The new description of the group.
         */
        void setDescription(const std::string& description);
        /**
         * @brief Gets the color of the group.
         * @return The color of the group.
         */
        const Color& getColor() const;
        /**
         * @brief Sets the color of the group.
         * @param color The new color of the group.
         */
        void setColor(const Color& color);
        /**
         * @brief Gets whether or not this Group is equal to compare Group.
         * @param compare The Group to compare to
         * @return True if this Group == compare Group 
         */
        bool operator==(const Group& compare) const;
        /**
         * @brief Gets whether or not this Group is not equal to compare Group.
         * @param compare The Group to compare to
         * @return True if this Group != compare Group 
         */
        bool operator!=(const Group& compare) const;
        /**
         * @brief Gets whether or not this Group is less than to compare Group.
         * @param compare The Group to compare to
         * @return True if this Group < compare Group 
         */
        bool operator<(const Group& compare) const;
        /**
         * @brief Gets whether or not this Group is greater than to compare Group.
         * @param compare The Group to compare to
         * @return True if this Group > compare Group 
         */
        bool operator>(const Group& compare) const;
        
    private:
        int m_id;
        std::string m_name;
        std::string m_description;
        Color m_color;
    };
}
#endif //GROUP_H