#pragma once

#include <locale>
#include <string>
#include "../models/transfer.hpp"

namespace NickvisionMoney::Controllers
{
	/**
	 * Statuses for when a transfer is checked
	 */
	enum class TransferCheckStatus
	{
		Valid = 0,
		InvalidDestPath,
		InvalidAmount
	};

	/**
	 * A controller for the TransferDialog
	 */
	class TransferDialogController
	{
	public:
		/**
		 * Constructs a TransferDialogController
		 *
		 * @param sourceAccountPath The path to the source account
		 * @param locale The user's locale
		 */
		TransferDialogController(const std::string& sourceAccountPath, const std::locale& locale);
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
		 * Gets the path of the source account
		 *
		 * @returns The path of the source account
		 */
		const std::string& getSourceAccountPath() const;
		/**
		 * Updates the transfer with the provided values
		 *
		 * @param destAccountPath The path of the destination account
		 * @param amountString The amount string
		 * @returns The TransferCheckStatus
		 */
		TransferCheckStatus updateTransfer(const std::string& destAccountPath, std::string amountString);

	private:
		std::string m_response;
		const std::locale& m_locale;
		NickvisionMoney::Models::Transfer m_transfer;
	};
}