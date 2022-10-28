#pragma once

#include <adwaita.h>
#include "../../controllers/accountviewcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
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
		GtkWidget* m_grpTransactions{ nullptr };
		GtkWidget* m_btnNewTransaction{ nullptr };
		/**
		 * Refreshes the UI with the account information
		 */
		void refreshInformation();
		/**
		 * Occurs when the new transaction button is clicked
		 */
		void onNewTransaction();
	};
}