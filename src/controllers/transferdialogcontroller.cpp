#include "transferdialogcontroller.hpp"
#include <filesystem>
#include "../helpers/moneyhelpers.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Helpers;

TransferDialogController::TransferDialogController(const std::string& sourceAccountPath, const std::locale& locale) : m_response{ "cancel" }, m_locale{ locale }, m_transfer{ sourceAccountPath }
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

TransferCheckStatus TransferDialogController::updateTransfer(const std::string& destAccountPath, std::string amountString)
{
    if(!std::filesystem::exists(destAccountPath))
    {
        return TransferCheckStatus::InvalidDestPath;
    }
    if(amountString.empty())
    {
        return TransferCheckStatus::InvalidAmount;
    }
    boost::multiprecision::cpp_dec_float_50 amount{ MoneyHelpers::localeStringToBoostMoney(amountString, m_locale) };
    if(amount == 0)
    {
        return TransferCheckStatus::InvalidAmount;
    }
    m_transfer.setDestAccountPath(destAccountPath);
    m_transfer.setAmount(amount);
    return TransferCheckStatus::Valid;
}
