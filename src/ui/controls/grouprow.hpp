#pragma once

#include <functional>
#include <locale>
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
		 * @param locale The user's locale
		 * @param filterActive Whether or not the filter should be active
		 */
		GroupRow(const NickvisionMoney::Models::Group& group, const std::locale& locale, bool filterActive = true);
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
		/**
		 * Registers a callback for updating the filter
		 *
		 * @param callback A void(int, bool) function
		 */
		void registerUpdateFilterCallback(const std::function<void(int, bool)>& callback);
		/**
		 * Resets the GroupRow's filter
		 */
		void resetFilter();

	private:
		NickvisionMoney::Models::Group m_group;
		std::function<void(unsigned int)> m_editCallback;
		std::function<void(unsigned int)> m_deleteCallback;
		std::function<void(int, bool)> m_updateFilterCallback;
		GtkWidget* m_gobj;
		GtkWidget* m_chkFilter;
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
		/**
		 * Occurs when the filter checkbox is changed
		 */
		void onUpdateFilter();
	};
}