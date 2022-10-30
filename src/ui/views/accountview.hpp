#pragma once

#include <memory>
#include <vector>
#include <adwaita.h>
#include "../controls/transactionrow.hpp"
#include "../../controllers/accountviewcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
	/**
	 * The view for an opened account
	 */
	class AccountView
	{
	public:
		AccountView(GtkWindow* parentWindow, AdwTabView* parentTabView, const NickvisionMoney::Controllers::AccountViewController& controller);
		/**
		 * Gets the AdwTabPage* representing the AccountView
		 *
		 * @returns The AdwTabPage* representing the AccountView
		 */
		AdwTabPage* gobj();

	private:
		NickvisionMoney::Controllers::AccountViewController m_controller;
		GtkWindow* m_parentWindow{ nullptr };
		AdwTabPage* m_gobj{ nullptr };
		GtkWidget* m_scrollMain{ nullptr };
		GtkWidget* m_boxMain{ nullptr };
		GtkWidget* m_grpOverview{ nullptr };
		GtkWidget* m_rowTotal{ nullptr };
		GtkWidget* m_rowIncome{ nullptr };
		GtkWidget* m_lblIncome{ nullptr };
		GtkWidget* m_rowExpense{ nullptr };
		GtkWidget* m_lblExpense{ nullptr };
		GtkWidget* m_btnMenuAccountActions{ nullptr };
		GtkWidget* m_grpTransactions{ nullptr };
		GtkWidget* m_btnNewTransaction{ nullptr };
		GSimpleActionGroup* m_actionMap{ nullptr };
		GSimpleAction* m_actExportAsCSV{ nullptr };
		GSimpleAction* m_actImportFromCSV{ nullptr };
		GSimpleAction* m_actNewTransaction{ nullptr };
		GtkEventController* m_shortcutController{ nullptr };
		std::vector<std::shared_ptr<NickvisionMoney::UI::Controls::TransactionRow>> m_transactionRows;
		/**
		 * Refreshes the UI with the account information
		 */
		void onAccountInfoChanged();
		/**
		 * Occurs when the export as csv menu item is clicked
		 */
		void onExportAsCSV();
		/**
		 * Occurs when the import from csv menu item is clicked
		 */
		void onImportFromCSV();
		/**
		 * Occurs when the new transaction button is clicked
		 */
		void onNewTransaction();
		/**
		 * Occurs when the edit transaction button is clicked
		 *
		 * @param id The id of the transaction to edit
		 */
		void onEditTransaction(unsigned int id);
		/**
		 * Occurs when the delete transaction button is clicked
		 *
		 * @param id The id of the transaction to delete
		 */
		void onDeleteTransaction(unsigned int id);
	};
}