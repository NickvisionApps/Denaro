#pragma once

#include <string>
#include <boost/multiprecision/cpp_dec_float.hpp>

namespace NickvisionMoney::Models
{
	/**
	 * A model of a transfer
	 */
	class Transfer
	{
	public:
		/**
		 * Constructs a Transfer
		 *
		 * @param sourceAccountPath The path of the source account
		 */
		Transfer(const std::string& sourceAccountPath);
		/**
		 * Gets the path of the source account
		 *
		 * @returns The path of the source account
		 */
		const std::string& getSourceAccountPath() const;
		/**
		 * Gets the path of the destination account
		 *
		 * @returns The path of the destination account
		 */
		const std::string& getDestAccountPath() const;
		/**
		 * Sets the path of the destination account
		 *
		 * @param destAccountPath The new path
		 */
		void setDestAccountPath(const std::string& destAccountPath);
		/**
		 * Gets the amount of the transfer
		 *
		 * @returns The amount of the transfer
		 */
		boost::multiprecision::cpp_dec_float_50 getAmount() const;
		/**
		 * Sets the amount of the transfer
		 *
		 * @param amount The new amount
		 */
		void setAmount(boost::multiprecision::cpp_dec_float_50 amount);

	private:
		std::string m_sourceAccountPath;
		std::string m_destAccountPath;
		boost::multiprecision::cpp_dec_float_50 m_amount;
	};
}