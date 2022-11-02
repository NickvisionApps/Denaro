#pragma once

#include <functional>
#include <string>
#include <adwaita.h>
#include "../../models/group.hpp"

namespace NickvisionMoney::UI::Controls
{
	/**
	 * A widget for managing a group
	 */
	class GroupRow
	{
	public:
		/**
		 * Constructs a GroupRow
		 *
		 * @param group The Group model to manage
		 */
		GroupRow(const NickvisionMoney::Models::Group& group, const std::string& currencySymbol, bool displayCurrencySymbolOnRight);
		/**
		 * Gets the GtkWidget* representing the GroupRow
		 *
		 * @returns The GtkWidget* representing the GroupRow
		 */
		GtkWidget* gobj();
		/**
		 * Registers a callback for editing the group
		 *
		 * @param callback A void(unsigned int) function
		 */
		void registerEditCallback(const std::function<void(unsigned int)>& callback);
		/**
		 * Registers a callback for deleting the group
		 *
		 * @param callback A void(unsigned int) function
		 */
		void registerDeleteCallback(const std::function<void(unsigned int)>& callback);

	private:
		NickvisionMoney::Models::Group m_group;
		std::function<void(unsigned int)> m_editCallback;
		std::function<void(unsigned int)> m_deleteCallback;
		GtkWidget* m_gobj;
		GtkWidget* m_box;
		GtkWidget* m_lblAmount;
		GtkWidget* m_btnEdit;
		GtkWidget* m_btnDelete;
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