#pragma once

#include <string>
#include <adwaita.h>
#include "../../controllers/transactiondialogcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
	/**
     * A dialog for managing a transaction
     */
 	class TransactionDialog
 	{
 	public:
 		/**
		 * Constructs a TransactionDialog
		 *
		 * @param parent The parent window for the dialog
		 * @param controller The TransactionDialogController
		 */
		TransactionDialog(GtkWindow* parent, NickvisionMoney::Controllers::TransactionDialogController& controller);
		/**
    	 * Gets the GtkWidget* representing the TransactionDialog
    	 *
    	 * @returns The GtkWidget* representing the TransactionDialog
    	 */
		GtkWidget* gobj();
		/**
    	 * Run the TransactionDialog
    	 *
    	 * @returns True if dialog was accepted, else false
    	 */
		bool run();

 	private:
 		NickvisionMoney::Controllers::TransactionDialogController& m_controller;
		GtkWidget* m_gobj;
		GtkWidget* m_preferencesGroup{ nullptr };
		GtkWidget* m_rowId{ nullptr };
		GtkWidget* m_rowDate{ nullptr };
		GtkWidget* m_btnDate{ nullptr };
		GtkWidget* m_popoverDate{ nullptr };
		GtkWidget* m_calendarDate{ nullptr };
		GtkWidget* m_rowDescription{ nullptr };
		GtkWidget* m_rowType{ nullptr };
		GtkWidget* m_rowRepeatInterval{ nullptr };
		GtkWidget* m_rowAmount{ nullptr };
		/**
    	 * Sets the response
    	 *
    	 * @param The new response
    	 */
		void setResponse(const std::string& response);
		/**
		 * Occurs when the date selected is changed
		 */
		void onDateChanged();
 	};
}