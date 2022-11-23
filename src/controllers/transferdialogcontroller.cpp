#include "transferdialogcontroller.hpp"

using namespace NickvisionMoney::Controllers;

TransferDialogController::TransferDialogController(const std::string& sourceAccountPath) : m_response{ "cancel" }, m_transfer{ sourceAccountPath }
{

}

const std::string& TransferDialogController::getResponse() const
{
    return m_response;
}

void TransferDialogController::setResponse(const std::string& response)
{
    m_response = response;
}

const std::string& TransferDialogController::getSourceAccountPath() const
{
    return m_transfer.getSourceAccountPath();
}

const std::string& TransferDialogController::getDestAccountPath() const
{
    return m_transfer.getDestAccountPath();
}

void TransferDialogController::setDestAccountPath(const std::string& destAccountPath)
{
    m_transfer.setDestAccountPath(destAccountPath);
}
