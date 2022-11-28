#pragma once

#include <adwaita.h>
#include "../../controllers/preferencesdialogcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
	/**
	 * A dialog for managing appplication preferences
	 */
	class PreferencesDialog
	{
	public:
		/**
		 * Constructs a PreferencesDialog
		 *
		 * @param parent The parent window for the dialog
		 * @param controller PreferencesDialogController
		 */
		PreferencesDialog(GtkWindow* parent, const NickvisionMoney::Controllers::PreferencesDialogController& controller);
		/**
		 * Gets the GtkWidget* representing the PreferencesDialog
		 *
		 * @returns The GtkWidget* representing the PreferencesDialog
		 */
		GtkWidget* gobj();
		/**
		 * Runs the PreferencesDialog
		 */
		void run();

	private:
		NickvisionMoney::Controllers::PreferencesDialogController m_controller;
		GtkWidget* m_gobj{ nullptr };
		GtkWidget* m_mainBox{ nullptr };
		GtkWidget* m_headerBar{ nullptr };
		GtkWidget* m_page{ nullptr };
		GtkWidget* m_grpUserInterface{ nullptr };
		GtkWidget* m_rowTheme{ nullptr };
		GtkWidget* m_btnTransactionColor{ nullptr };
		GtkWidget* m_rowTransactionColor{ nullptr };
		GtkWidget* m_btnTransferColor{ nullptr };
		GtkWidget* m_rowTransferColor{ nullptr };
		/**
		 * Ocurrs when the theme row is changed
		 */
		void onThemeChanged();
		/**
		 * Occurs when the transaction color chooser's value is set
		 */
		void onTransactionColorSet();
		/**
		 * Occurs when the transfer color chooser's value is set
		 */
		void onTransferColorSet();
	};
}