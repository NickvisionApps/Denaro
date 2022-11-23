#pragma once

#include <string>
#include <adwaita.h>
#include "../../controllers/transferdialogcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
	/**
     * A dialog for managing a transfer
     */
 	class TransferDialog
 	{
 	public:
 		/**
		 * Constructs a TransferDialog
		 *
		 * @param parent The parent window for the dialog
		 * @param controller The TransferDialogController
		 */
		TransferDialog(GtkWindow* parent, NickvisionMoney::Controllers::TransferDialogController& controller);
		/**
    	 * Gets the GtkWidget* representing the TransferDialog
    	 *
    	 * @returns The GtkWidget* representing the TransferDialog
    	 */
		GtkWidget* gobj();
		/**
    	 * Run the TransferDialog
    	 *
    	 * @returns True if dialog was accepted, else false
    	 */
		bool run();

 	private:
 		NickvisionMoney::Controllers::TransferDialogController& m_controller;
		GtkWidget* m_gobj;
		GtkWidget* m_preferencesGroup{ nullptr };
		GtkWidget* m_rowTransferAccount{ nullptr };
		GtkWidget* m_lblTransferAccount{ nullptr };
		GtkWidget* m_btnSelectAccount{ nullptr };
		GtkWidget* m_rowAmount{ nullptr };
		/**
    	 * Sets the response
    	 *
    	 * @param The new response
    	 */
		void setResponse(const std::string& response);
		/**
		 * Occurs when the select account button is clicked
		 */
		void onSelectAccount();
 	};
}