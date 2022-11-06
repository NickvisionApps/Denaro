#pragma once

#include <map>
#include <string>
#include <vector>
#include "../models/group.hpp"
#include "../models/transaction.hpp"

namespace NickvisionMoney::Controllers
{
	/**
	 * Statuses for when a transaction is checked
	 */
	enum class TransactionCheckStatus
	{
		Valid = 0,
		EmptyDescription,
		EmptyAmount,
		InvalidAmount
	};

	/**
	 * A controller for the TransactionDialog
	 */
	class TransactionDialogController
	{
	public:
		/**
		 * Constructs a TransactionDialogController
		 *
		 * @param newId The Id of the new transaction
		 * @param currencySymbol The currency symbol
		 */
		TransactionDialogController(unsigned int newId, const std::string& currencySymbol, const std::map<unsigned int, NickvisionMoney::Models::Group>& groups);
		/**
		 * Constructs a TransactionDialogController
		 *
		 * @param transaction The transaction to update
		 * @param currencySymbol The currency symbol
		 */
		TransactionDialogController(const NickvisionMoney::Models::Transaction& transaction, const std::string& currencySymbol, const std::map<unsigned int, NickvisionMoney::Models::Group>& groups);
		/**
		 * Gets the response of the dialog
		 *
		 * @returns The response of the dialog
		 */
		const std::string& getResponse() const;
		/**
		 * Sets the response of the dialog
		 *
		 * @param response The new response of the dialog
		 */
		void setResponse(const std::string& response);
		/**
		 * Gets the currency symbol
		 *
		 * @returns The currency symbol
		 */
		const std::string& getCurrencySymbol() const;
		/**
		 * Gets the transaction managed by the dialog
		 *
		 * @returns The transaction
		 */
		const NickvisionMoney::Models::Transaction& getTransaction() const;
		/**
		 * Gets the id of the transaction as a string
		 *
		 * @returns The id of the transaction
		 */
		std::string getIdAsString() const;
		/**
		 * Gets the year of the date of the transaction
		 *
		 * @returns The year of the date of the transaction
		 */
		int getYear() const;
		/**
		 * Gets the month of the date of the transaction
		 *
		 * @returns The month of the date of the transaction
		 */
		int getMonth() const;
		/**
		 * Gets the day of the date of the transaction
		 *
		 * @returns The day of the date of the transaction
		 */
		int getDay() const;
		/**
		 * Gets the description of the transaction
		 *
		 * @returns The description of the transaction
		 */
		const std::string& getDescription() const;
		/**
		 * Gets the type of the transaction as an int
		 *
		 * @returns The type of the transaction
		 */
		int getTypeAsInt() const;
		/**
		 * Gets the repeat interval of the transaction as an int
		 *
		 * @returns The repeat interval of the transaction
		 */
		int getRepeatIntervalAsInt() const;
		/**
		 * Get a list of the group names
		 *
		 * @returns The list of group names
		 */
		const std::vector<std::string>& getGroupNames() const;
		/**
		 * Gets the group of the transaction as an index
		 *
		 * @returns The index of the group of the transaction
		 */
		int getGroupAsIndex() const;
		/**
		 * Gets the amount of the transaction as a string
		 *
		 * @returns The amount of the transaction
		 */
		std::string getAmountAsString() const;
		/**
		 * Updates the transaction with the provided values
		 *
		 * @param dateString The date string
		 * @param description The description
		 * @param type The type as an int
		 * @param repeatInterval The repeat interval as an int
		 * @param groupIndex The index of the group
		 * @param amountString The amount string
		 * @returns The TransactionCheckStatus
		 */
		TransactionCheckStatus updateTransaction(const std::string& dateString, const std::string& description, int type, int repeatInterval, int groupIndex, const std::string& amountString);

	private:
		std::string m_response;
		std::string m_currencySymbol;
		NickvisionMoney::Models::Transaction m_transaction;
		std::map<unsigned int, NickvisionMoney::Models::Group> m_groups;
		std::vector<std::string> m_groupNames;
	};
}