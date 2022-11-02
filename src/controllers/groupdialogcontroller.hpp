#pragma once

#include <string>
#include "../models/group.hpp"

namespace NickvisionMoney::Controllers
{
	/**
	 * Statuses for when a group is checked
	 */
	enum class GroupCheckStatus
	{
		Valid = 0,
		EmptyName,
		EmptyDescription
	};

	/**
	 * A controller for the GroupDialog
	 */
	class GroupDialogController
	{
	public:
		/**
		 * Constructs a GroupDialogController
		 *
		 * @param newId The Id of the new group
		 */
		GroupDialogController(unsigned int newId);
		/**
		 * Constructs a GroupDialogController
		 *
		 * @param group The group to update
		 */
		GroupDialogController(const NickvisionMoney::Models::Group& group);
		/**
		 * Gets the response of the dialog
		 *
		 * @returns The response of the dialog
		 */
		const std::string& getResponse() const;
		/**
		 * Sets the response of the dialog
		 *
		 * @param response The new response of the dialog
		 */
		void setResponse(const std::string& response);
		/**
		 * Gets the group managed by the dialog
		 */
		const NickvisionMoney::Models::Group& getGroup() const;
		/**
		 * Gets the name of the group
		 */
		const std::string& getName() const;
		/**
		 * Gets the description of the group
		 */
		const std::string& getDescription() const;
		/**
		 * Updates the transaction with the provided values
		 *
		 * @param name The name
		 * @param description The description
		 * @returns The GroupCheckStatus
		 */
		GroupCheckStatus updateGroup(const std::string& name, const std::string& description);

	private:
		std::string m_response;
		NickvisionMoney::Models::Group m_group;
	};
}