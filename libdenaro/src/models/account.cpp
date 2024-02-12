#include "models/account.h"
#include <libnick/localization/gettext.h>

namespace Nickvision::Money::Shared::Models
{
    Account::Account(const std::filesystem::path& path)
        : m_path{ path },
        m_loggedIn{ false },
        m_database{ nullptr },
        m_isEncrypted{ false },
        m_metadata{ path.stem().string(), AccountType::Checking },
        m_tags{ _("Untagged") },
        m_nextAvailableGroupId{ 1 },
        m_nextAvailableTransactionId{ 1 }
    {
        //Get database 
        sqlite3* database{ nullptr };
        if(sqlite3_open_v2(m_path.string().c_str(), &database, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, nullptr) == SQLITE_OK)
        {
            //Determine if account is encrypted
            if(sqlite3_exec(database, "PRAGMA schema_version", nullptr, nullptr, nullptr) != SQLITE_OK)
            {
                m_isEncrypted = true;
            }
            m_database = { database, [](sqlite3* sql)
            {
                sqlite3_close(sql);
            }};
        }
    }

    bool Account::isEncrypted() const
    {
        return m_isEncrypted;
    }

    bool Account::login(const std::string& password)
    {
        if(!m_loggedIn)
        {
            //Set password if needed
            if(m_isEncrypted)
            {
                sqlite3_key(m_database.get(), password.c_str(), static_cast<int>(password.size()));
            }
            if(sqlite3_exec(m_database.get(), "PRAGMA schema_version", nullptr, nullptr, nullptr) == SQLITE_OK)
            {
                //TODO: Setup metadata table
                //TODO: Setup groups table
                //TODO: Setup transactions table
                //TODO: Get metadata
                //TODO: Get groups
                //TODO: Get transactions
                //TODO: Sync repeats
                m_loggedIn = true;
            }
        }
        return m_loggedIn;
    }

    Account::operator bool() const
    {
        return m_loggedIn;
    }
}