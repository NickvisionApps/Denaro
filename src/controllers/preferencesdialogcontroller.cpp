#include "preferencesdialogcontroller.hpp"

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;

PreferencesDialogController::PreferencesDialogController(Configuration& configuration) : m_configuration{ configuration }
{

}

int PreferencesDialogController::getThemeAsInt() const
{
    return static_cast<int>(m_configuration.getTheme());
}

void PreferencesDialogController::setTheme(int theme)
{
    m_configuration.setTheme(static_cast<Theme>(theme));
}

const std::string& PreferencesDialogController::getTransactionDefaultColor() const
{
    return m_configuration.getTransactionDefaultColor();
}

void PreferencesDialogController::setTransactionDefaultColor(std::string color)
{
    m_configuration.setTransactionDefaultColor(color);
}

const std::string& PreferencesDialogController::getTransferDefaultColor() const
{
    return m_configuration.getTransferDefaultColor();
}

void PreferencesDialogController::setTransferDefaultColor(std::string color)
{
    m_configuration.setTransferDefaultColor(color);
}

void PreferencesDialogController::saveConfiguration() const
{
    m_configuration.save();
}
