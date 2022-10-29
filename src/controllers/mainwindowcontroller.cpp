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

void MainWindowController::registerAccountAddedCallback(const std::function<void()>& callback)
{
    m_accountAddedCallback = callback;
}

int MainWindowController::getNumberOfOpenAccounts() const
{
    return m_openAccounts.size();
}

std::string MainWindowController::getFirstOpenAccountPath() const
{
    return m_openAccounts[0];
}

AccountViewController MainWindowController::createAccountViewControllerForLatestAccount() const
{
    return { m_openAccounts[m_openAccounts.size() - 1], m_configuration.getCurrencySymbol(), m_sendToastCallback };
}

void MainWindowController::addAccount(std::string& path)
{
    if(std::filesystem::path(path).extension().empty() || std::filesystem::path(path).extension() != ".nmoney")
    {
        path += ".nmoney";
    }
    if(std::find(m_openAccounts.begin(), m_openAccounts.end(), path) == m_openAccounts.end())
    {
        m_openAccounts.push_back(path);
        m_accountAddedCallback();
    }
}

void MainWindowController::closeAccount(int index)
{
    m_openAccounts.erase(m_openAccounts.begin() + index);
}
