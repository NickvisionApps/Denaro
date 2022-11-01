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
         * Gets the monthly allowance of the group
         *
         * @returns The monthly allowance of the group
         */
        boost::multiprecision::cpp_dec_float_50 getMonthlyAllowance() const;
        /**
         * Sets the monthly allowance of the group
         *
         * @param monthlyAllowance The new monthly allowance
         */
        void setMonthlyAllowance(boost::multiprecision::cpp_dec_float_50 monthlyAllowance);

	private:
		unsigned int m_id;
		std::string m_name;
		std::string m_description;
		boost::multiprecision::cpp_dec_float_50 m_monthlyAllowance;
	};
}