#pragma once

#include <memory>
#include <vector>
#include <adwaita.h>
#include "../controls/grouprow.hpp"
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
		AccountView(GtkWindow* parentWindow, AdwTabView* parentTabView, GtkWidget* btnFlapToggle, const NickvisionMoney::Controllers::AccountViewController& controller);
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
		GtkWidget* m_flap{ nullptr };
		GtkWidget* m_scrollPane{ nullptr };
		GtkWidget* m_scrollTransactions{ nullptr };
		GtkWidget* m_boxMain{ nullptr };
		GtkWidget* m_paneBox{ nullptr };
		GtkWidget* m_overlayMain{ nullptr };
		GtkWidget* m_grpOverview{ nullptr };
		GtkWidget* m_rowTotal{ nullptr };
		GtkWidget* m_lblTotal{ nullptr };
		GtkWidget* m_rowIncome{ nullptr };
		GtkWidget* m_lblIncome{ nullptr };
		GtkWidget* m_chkIncome{ nullptr };
		GtkWidget* m_rowExpense{ nullptr };
		GtkWidget* m_lblExpense{ nullptr };
		GtkWidget* m_chkExpense{ nullptr };
		GtkWidget* m_boxButtonsOverview{ nullptr };
		GtkWidget* m_btnMenuAccountActions{ nullptr };
		GtkWidget* m_btnResetOverviewFilter{ nullptr };
		GtkWidget* m_grpGroups{ nullptr };
		GtkWidget* m_boxButtonsGroups{ nullptr };
		GtkWidget* m_btnNewGroup{ nullptr };
		GtkWidget* m_btnResetGroupsFilter{ nullptr };
		GtkWidget* m_calendar{ nullptr };
		GtkWidget* m_btnResetCalendarFilter{ nullptr };
		GtkWidget* m_ddStartYear{ nullptr };
		GtkWidget* m_ddStartMonth{ nullptr };
		GtkWidget* m_ddStartDay{ nullptr };
		GtkWidget* m_ddEndYear{ nullptr };
		GtkWidget* m_ddEndMonth{ nullptr };
		GtkWidget* m_ddEndDay{ nullptr };
		GtkWidget* m_boxStartRange{ nullptr };
		GtkWidget* m_boxEndRange{ nullptr };
		GtkWidget* m_rowStartRange{ nullptr };
		GtkWidget* m_rowEndRange{ nullptr };
		GtkWidget* m_grpRange{ nullptr };
		GtkWidget* m_expRange{ nullptr };
		GtkWidget* m_grpCalendar{ nullptr };
		GtkWidget* m_boxSort{ nullptr };
		GtkWidget* m_btnSortTopBottom{ nullptr };
		GtkWidget* m_btnSortBottomTop{ nullptr };
		GtkWidget* m_grpTransactions{ nullptr };
		GtkWidget* m_btnNewTransaction{ nullptr };
		GtkWidget* m_flowBox{ nullptr };
		GtkWidget* m_pageStatusNoTransactions{ nullptr };
		GSimpleActionGroup* m_actionMap{ nullptr };
		GSimpleAction* m_actExportAsCSV{ nullptr };
		GSimpleAction* m_actImportFromCSV{ nullptr };
		GSimpleAction* m_actNewGroup{ nullptr };
		GSimpleAction* m_actNewTransaction{ nullptr };
		GtkEventController* m_shortcutController{ nullptr };
		std::vector<std::shared_ptr<NickvisionMoney::UI::Controls::GroupRow>> m_groupRows;
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
		 * Occurs when the reset overview filter is clicked
		 */
		void onResetOverviewFilter();
		/**
		 * Occurs when the new group button is clicked
		 */
		void onNewGroup();
		/**
		 * Occurs when the edit group button is clicked
		 *
		 * @param id The id of the group to edit
		 */
		void onEditGroup(unsigned int id);
		/**
		 * Occurs when the delete group button is clicked
		 *
		 * @param id The id of the group to edit
		 */
		void onDeleteGroup(unsigned int id);
		/**
		 * Occurs when the reset groups filter is clicked
		 */
		void onResetGroupsFilter();
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
		/**
		 * Occurs when the reset calendar filter button is clicked
		 */
		void onResetCalendarFilter();
		/**
		 * Occurs when the selected date of the calendar is changed
		 */
		void onCalendarDateChanged();
	};
}