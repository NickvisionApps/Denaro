#pragma once

#include <string>
#include "../models/transfer.hpp"

namespace NickvisionMoney::Controllers
{
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
		 */
		TransferDialogController(const std::string& sourceAccountPath);
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

	private:
		std::string m_response;
		NickvisionMoney::Models::Transfer m_transfer;
	};
}