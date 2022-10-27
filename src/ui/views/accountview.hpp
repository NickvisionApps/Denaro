#pragma once

#include <adwaita.h>
#include "../../controllers/accountviewcontroller.hpp"

namespace NickvisionMoney::UI::Views
{
	class AccountView
	{
	public:
		AccountView(AdwTabView* parent, const NickvisionMoney::Controllers::AccountViewController& controller);
		/**
		 * Gets the AdwTabPage* representing the AccountView
		 *
		 * @returns The AdwTabPage* representing the AccountView
		 */
		AdwTabPage* gobj();

	private:
		NickvisionMoney::Controllers::AccountViewController m_controller;
		AdwTabPage* m_gobj{ nullptr };
		GtkWidget* m_scrollMain{ nullptr };
		GtkWidget* m_boxMain{ nullptr };
		GtkWidget* m_grpOverview{ nullptr };
		GtkWidget* m_rowTotal{ nullptr };
		GtkWidget* m_rowIncome{ nullptr };
		GtkWidget* m_lblIncome{ nullptr };
		GtkWidget* m_rowExpense{ nullptr };
		GtkWidget* m_lblExpense{ nullptr };
		/*
		 * Refreshes the UI with the account information
		 */
		void refreshInformation();
	};
}