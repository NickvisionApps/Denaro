#pragma once

#include <string>
#include <adwaita.h>

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
		TransactionDialog(GtkWindow* parent);
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
		GtkWidget* m_gobj;
		GtkWidget* m_preferencesGroup{ nullptr };
		/**
    	 * Sets the response
    	 *
    	 * @param The new response
    	 */
		void setResponse(const std::string& response);
 	};
}