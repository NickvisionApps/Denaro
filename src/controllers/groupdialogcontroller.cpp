#include "groupdialogcontroller.hpp"
#include <algorithm>

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;

GroupDialogController::GroupDialogController(unsigned int newId, const std::vector<std::string>& existingNames) : m_response{ "cancel" }, m_existingNames{ existingNames }, m_group{ newId }
{

}

GroupDialogController::GroupDialogController(const Group& group, const std::vector<std::string>& existingNames) : m_response{ "cancel" }, m_existingNames{ existingNames }, m_group{ group }
{

}

const std::string& GroupDialogController::getResponse() const
{
    return m_response;
}

void GroupDialogController::setResponse(const std::string& response)
{
    m_response = response;
}

const Group& GroupDialogController::getGroup() const
{
    return m_group;
}

const std::string& GroupDialogController::getName() const
{
    return m_group.getName();
}

const std::string& GroupDialogController::getDescription() const
{
    return m_group.getDescription();
}

GroupCheckStatus GroupDialogController::updateGroup(const std::string& name, const std::string& description)
{
    if(name.empty())
    {
        return GroupCheckStatus::EmptyName;
    }
    if(description.empty())
    {
        return GroupCheckStatus::EmptyDescription;
    }
    if(name != m_group.getName() && std::find(m_existingNames.begin(), m_existingNames.end(), name) != m_existingNames.end())
    {
        return GroupCheckStatus::NameExists;
    }
    m_group.setName(name);
    m_group.setDescription(description);
    return GroupCheckStatus::Valid;
}
