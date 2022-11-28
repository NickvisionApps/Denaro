#pragma once

#include <locale>
#include <map>
#include <string>
#include <vector>
#include "../models/group.hpp"
#include "../models/transaction.hpp"
#include "../models/configuration.hpp"

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
		 * @param groups The groups of the account
		 * @param configuration The Configuration for the application (Stored as a reference)
		 */
		TransactionDialogController(unsigned int newId, const std::map<unsigned int, NickvisionMoney::Models::Group>& groups, NickvisionMoney::Models::Configuration& configuration);
		/**
		 * Constructs a TransactionDialogController
		 *
		 * @param transaction The transaction to update
		 * @param groups The groups of the account
		 * @param configuration The Configuration for the application (Stored as a reference)
		 */
		TransactionDialogController(const NickvisionMoney::Models::Transaction& transaction, const std::map<unsigned int, NickvisionMoney::Models::Group>& groups, NickvisionMoney::Models::Configuration& configuration);
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
		 * Gets the type of the transaction
		 *
		 * @returns The type of the transaction
		 */
		NickvisionMoney::Models::TransactionType getType() const;
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
         	 * Gets the rgba color of the transaction
         	 *
         	 * @returns The rgba color of the transaction
         	 */
        	const std::string& getRGBA() const;
		/**
		 * Gets transaction default color from configuration
		 *
		 * @returns Transaction default color
		 */
		const std::string& getTransactionDefaultColor() const;
		/**
		 * Gets the amount of the transaction as a string
		 *
		 * @returns The amount of the transaction
		 */
		std::string getAmountAsString() const;
		/**
		 * Gets whether or not the user's locale separates the decimal by "." or ","
		 *
		 * @returns True if separated by ".", false for ","
		 */
		bool isLocaleDotDecimalSeperated() const;
		/**
		 * Updates the transaction with the provided values
		 *
		 * @param dateString The date string
		 * @param description The description
		 * @param type The TransactionType
		 * @param repeatInterval The repeat interval as an int
		 * @param groupIndex The index of the group
		 * @param rgba The rgba string
		 * @param amountString The amount string
		 * @returns The TransactionCheckStatus
		 */
		TransactionCheckStatus updateTransaction(const std::string& dateString, const std::string& description, NickvisionMoney::Models::TransactionType type, int repeatInterval, int groupIndex, const std::string& rgba, std::string amountString);

	private:
		std::string m_response;
		NickvisionMoney::Models::Transaction m_transaction;
		std::map<unsigned int, NickvisionMoney::Models::Group> m_groups;
		std::vector<std::string> m_groupNames;
		NickvisionMoney::Models::Configuration& m_configuration;
	};
}