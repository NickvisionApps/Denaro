#pragma once

#include <string>
#include <adwaita.h>
#include "../../controllers/groupdialogcontroller.hpp"
#include "../../utilities/translation.hpp"

namespace NickvisionMoney::UI::Views
{
	/**
     * A dialog for managing a group
     */
 	class GroupDialog
 	{
 	public:
 		/**
		 * Constructs a GroupDialog
		 *
		 * @param parent The parent window for the dialog
		 * @param controller The GroupDialogController
		 */
		GroupDialog(GtkWindow* parent, NickvisionMoney::Controllers::GroupDialogController& controller);
		/**
    	 * Gets the GtkWidget* representing the GroupDialog
    	 *
    	 * @returns The GtkWidget* representing the GroupDialog
    	 */
		GtkWidget* gobj();
		/**
    	 * Run the GroupDialog
    	 *
    	 * @returns True if dialog was accepted, else false
    	 */
		bool run();

 	private:
 		NickvisionMoney::Controllers::GroupDialogController& m_controller;
		GtkWidget* m_gobj;
		GtkWidget* m_preferencesGroup{ nullptr };
		GtkWidget* m_rowName{ nullptr };
		GtkWidget* m_rowDescription{ nullptr };
		/**
    	 * Sets the response
    	 *
    	 * @param The new response
    	 */
		void setResponse(const std::string& response);
 	};
}