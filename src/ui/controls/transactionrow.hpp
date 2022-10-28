#pragma once

#include <string>
#include <adwaita.h>
#include "../../models/transaction.hpp"

namespace NickvisionMoney::UI::Controls
{
	/**
	 * A widget for managing a transaction
	 */
	class TransactionRow
	{
	public:
		/**
		 * Constructs a TransactionRow
		 *
		 * @param transaction The Transaction model to manage
		 */
		TransactionRow(const NickvisionMoney::Models::Transaction& transaction, const std::string& currencySymbol);
		/**
		 * Gets the GtkWidget* representing the TransactionRow
		 *
		 * @returns The GtkWidget* representing the TransactionRow
		 */
		GtkWidget* gobj();

	private:
		NickvisionMoney::Models::Transaction m_transaction;
		GtkWidget* m_gobj;
		GtkWidget* m_boxButtons;
		GtkWidget* m_btnEdit;
		GtkWidget* m_btnDelete;
	};
}