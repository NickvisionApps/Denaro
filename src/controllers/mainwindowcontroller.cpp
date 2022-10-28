#include "mainwindowcontroller.hpp"
#include <algorithm>
#include <filesystem>

using namespace NickvisionMoney::Controllers;
using namespace NickvisionMoney::Models;

MainWindowController::MainWindowController(AppInfo& appInfo, Configuration& configuration) : m_appInfo{ appInfo }, m_configuration{ configuration }, m_isOpened{ false }, m_isDevVersion{ m_appInfo.getVersion().find("-") != std::string::npos }
{

}

const AppInfo& MainWindowController::getAppInfo() const
{
    return m_appInfo;
}

bool MainWindowController::getIsDevVersion() const
{
    return m_isDevVersion;
}

PreferencesDialogController MainWindowController::createPreferencesDialogController() const
{
    return { m_configuration };
}

void MainWindowController::registerSendToastCallback(const std::function<void(const std::string& message)>& callback)
{
    m_sendToastCallback = callback;
}

void MainWindowController::startup()
{
    if(!m_isOpened)
    {
        m_isOpened = true;
    }
}

void MainWindowController::onConfigurationChanged()
{

}

const std::string& MainWindowController::getCurrencySymbol() const
{
    return m_configuration.getCurrencySymbol();
}

void MainWindowController::registerAccountAddedCallback(const std::function<void(const std::string& path)>& callback)
{
    m_accountAddedCallback = callback;
}

void MainWindowController::addAccount(std::string& path)
{
    if(std::filesystem::path(path).extension().empty())
    {
        path += ".nmoney";
    }
    if(std::find(m_openedAccounts.begin(), m_openedAccounts.end(), path) == m_openedAccounts.end())
    {
        m_openedAccounts.push_back(path);
        m_accountAddedCallback(path);
    }
}

void MainWindowController::closeAccount(int index)
{
    m_openedAccounts.erase(m_openedAccounts.begin() + index);
}
