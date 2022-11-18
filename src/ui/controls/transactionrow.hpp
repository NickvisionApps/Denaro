#pragma once

#include <functional>
#include <locale>
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
		TransactionRow(const NickvisionMoney::Models::Transaction& transaction, const std::locale& locale);
		/**
		 * Gets the GtkWidget* representing the TransactionRow
		 *
		 * @returns The GtkWidget* representing the TransactionRow
		 */
		GtkWidget* gobj();
		/**
		 * Registers a callback for editing the transaction
		 *
		 * @param callback A void(unsigned int) function
		 */
		void registerEditCallback(const std::function<void(unsigned int)>& callback);
		/**
		 * Registers a callback for deleting the transaction
		 *
		 * @param callback A void(unsigned int) function
		 */
		void registerDeleteCallback(const std::function<void(unsigned int)>& callback);

	private:
		NickvisionMoney::Models::Transaction m_transaction;
		std::function<void(unsigned int)> m_editCallback;
		std::function<void(unsigned int)> m_deleteCallback;
		GtkWidget* m_gobj;
		GtkWidget* m_btnId;
		GtkWidget* m_box;
		GtkWidget* m_lblAmount;
		GtkWidget* m_btnEdit;
		GtkWidget* m_btnDelete;
		GtkGesture* m_gestureMouseClick;
		/**
		 * Occurs when the edit button is clicked
		 */
		void onEdit();
		/**
		 * Occurs when the delete button is clicked
		 */
		void onDelete();
	};
}