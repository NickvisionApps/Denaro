#pragma once

#include <string>
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
		TransactionDialogController(unsigned int newId, const std::string& currencySymbol);
		/**
		 * Constructs a TransactionDialogController
		 *
		 * @param transaction The transaction to update
		 * @param currencySymbol The currency symbol
		 */
		TransactionDialogController(const NickvisionMoney::Models::Transaction& transaction, const std::string& currencySymbol);
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
		 */
		const NickvisionMoney::Models::Transaction& getTransaction() const;
		/**
		 * Gets the id of the transaction as a string
		 */
		std::string getIdAsString() const;
		/**
		 * Gets the year of the date of the transaction
		 */
		int getYear() const;
		/**
		 * Gets the month of the date of the transaction
		 */
		int getMonth() const;
		/**
		 * Gets the day of the date of the transaction
		 */
		int getDay() const;
		/**
		 * Gets the description of the transaction
		 */
		const std::string& getDescription() const;
		/**
		 * Gets the type of the transaction as an int
		 */
		int getTypeAsInt() const;
		/**
		 * Gets the repeat interval of the transaction as an int
		 */
		int getRepeatIntervalAsInt() const;
		/**
		 * Gets the amount of the transaction as a string
		 */
		std::string getAmountAsString() const;
		/**
		 * Updates the transaction with the provided values
		 *
		 * @param dateString The date string
		 * @param description The description
		 * @param type The type as an int
		 * @param repeatInterval The repeat interval as an int
		 * @param amountString The amount string
		 * @returns The TransactionCheckStatus
		 */
		TransactionCheckStatus updateTransaction(const std::string& dateString, const std::string& description, int type, int repeatInterval, const std::string& amountString);

	private:
		std::string m_response;
		std::string m_currencySymbol;
		NickvisionMoney::Models::Transaction m_transaction;
	};
}