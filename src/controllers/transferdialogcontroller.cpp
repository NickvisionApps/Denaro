#include "transferdialogcontroller.hpp"

using namespace NickvisionMoney::Controllers;

TransferDialogController::TransferDialogController() : m_response{ "cancel" }
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
