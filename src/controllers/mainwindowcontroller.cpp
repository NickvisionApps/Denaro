#include "mainwindowcontroller.hpp"
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
        if(m_configuration.getIsFirstTimeOpen())
        {
            m_configuration.setIsFirstTimeOpen(false);
            m_configuration.save();
        }
        m_isOpened = true;
    }
}

void MainWindowController::onConfigurationChanged()
{

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
    m_accountAddedCallback(path);
}
