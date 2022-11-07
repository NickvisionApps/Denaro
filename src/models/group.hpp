#pragma once

#include <string>
#include <boost/multiprecision/cpp_dec_float.hpp>

namespace NickvisionMoney::Models
{
	/**
	 * A model of a group
	 */
	class Group
	{
	public:
		/**
    	 * Constructs a Group
    	 *
    	 * @param id The id of the group
    	 */
		Group(unsigned int id = 0);
		/**
         * Gets the id of the transaction
         *
         * @returns The id of the transaction
         */
        unsigned int getId() const;
        /**
         * Gets the name of the group
         *
         * @returns The name of the group
         */
        const std::string& getName() const;
        /**
         * Sets the name of the group
         *
		 * @param name The new name
         */
        void setName(const std::string& name);
        /**
         * Gets the description of the group
         *
         * @returns The description of the group
         */
        const std::string& getDescription() const;
        /**
         * Sets the description of the group
         *
		 * @param description The new description
         */
        void setDescription(const std::string& description);
        /**
         * Gets the balance of the group
         *
         * @returns The balance of the group
         */
        boost::multiprecision::cpp_dec_float_50 getBalance() const;
        /**
         * Sets the balance of the group
         *
         * @param monthlyAllowance The new balance
         */
        void setBalance(boost::multiprecision::cpp_dec_float_50 balance);
        /**
         * Compares two Groups via less-than
         *
         * @param toComapre The group to compare
         * @returns True if this group < toCompare, else false
         */
        bool operator<(const Group& toCompare) const;
         /**
         * Compares two Groups via greater-than
         *
         * @param toComapre The group to compare
         * @returns True if this group > toCompare, else false
         */
        bool operator>(const Group& toCompare) const;
         /**
         * Compares two Groups via equals
         *
         * @param toComapre The group to compare
         * @returns True if this group == toCompare, else false
         */
        bool operator==(const Group& toCompare) const;
         /**
         * Compares two Groups via not equals
         *
         * @param toComapre The group to compare
         * @returns True if this group != toCompare, else false
         */
        bool operator!=(const Group& toCompare) const;

	private:
		unsigned int m_id;
		std::string m_name;
		std::string m_description;
		boost::multiprecision::cpp_dec_float_50 m_balance;
	};
}