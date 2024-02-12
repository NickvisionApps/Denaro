#include "models/account.h"
#include <format>
#include <libnick/localization/gettext.h>
#include "models/customcurrency.h"

namespace Nickvision::Money::Shared::Models
{
    Account::Account(const std::filesystem::path& path)
        : m_path{ path },
        m_loggedIn{ false },
        m_database{ nullptr },
        m_isEncrypted{ false },
        m_metadata{ path.stem().string(), AccountType::Checking },
        m_nextAvailableGroupId{ 1 },
        m_nextAvailableTransactionId{ 1 }
    {
        if(m_path.extension() != ".nmoney")
        {
            m_path.replace_extension(".nmoney");
        }
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
                //Setup metadata table
                sqlite3_exec(m_database.get(), "CREATE TABLE IF NOT EXISTS metadata (id INTEGER PRIMARY KEY, name TEXT, type INTEGER, useCustomCurrency INTEGER, customSymbol TEXT, customCode TEXT, defaultTransactionType INTEGER, showGroupsList INTEGER, sortFirstToLast INTEGER, sortTransactionsBy INTEGER, customDecimalSeparator TEXT, customGroupSeparator TEXT, customDecimalDigits INTEGER, showTagsList INTEGER, transactionRemindersThreshold INTEGER, customAmountStyle INTEGER)", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN sortTransactionsBy INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN customDecimalSeparator TEXT", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN customGroupSeparator TEXT", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN customDecimalDigits INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN showTagsList INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN transactionRemindersThreshold INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE metadata ADD COLUMN customAmountStyle INTEGER", nullptr, nullptr, nullptr);
                //Setup groups table
                sqlite3_exec(m_database.get(), "CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT, rgba TEXT)", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE groups ADD COLUMN rgba TEXT", nullptr, nullptr, nullptr);
                //Setup transactions table
                sqlite3_exec(m_database.get(), "CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT, receipt TEXT, repeatFrom INTEGER, repeatEndDate TEXT, useGroupColor INTEGER, notes TEXT, tags TEXT)", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN gid INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN rgba TEXT", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN receipt TEXT", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN repeatFrom INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN repeatEndDate TEXT", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN useGroupColor INTEGER", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN notes TEXT", nullptr, nullptr, nullptr);
                sqlite3_exec(m_database.get(), "ALTER TABLE transactions ADD COLUMN tags TEXT", nullptr, nullptr, nullptr);
                //Get metadata
                sqlite3_stmt* metadata;
                sqlite3_prepare_v2(m_database.get(), "SELECT * FROM metadata where id = 0", -1, &metadata, nullptr);
                if(sqlite3_step(metadata) == SQLITE_ROW)
                {
                    m_metadata.setName((const char*)sqlite3_column_text(metadata, 1));
                    m_metadata.setType(static_cast<AccountType>(sqlite3_column_int(metadata, 2)));
                    m_metadata.setUseCustomCurrency(static_cast<bool>(sqlite3_column_int(metadata, 3)));
                    CustomCurrency curr{ (const char*)sqlite3_column_text(metadata, 4), (const char*)sqlite3_column_text(metadata, 5) };
                    curr.setDecimalSeparator((const char*)sqlite3_column_text(metadata, 10));
                    curr.setGroupSeparator((const char*)sqlite3_column_text(metadata, 11));
                    curr.setDecimalDigits(sqlite3_column_int(metadata, 12));
                    curr.setAmountStyle(static_cast<AmountStyle>(sqlite3_column_int(metadata, 15)));
                    m_metadata.setCustomCurrency(curr);
                    m_metadata.setDefaultTransactionType(static_cast<TransactionType>(sqlite3_column_int(metadata, 6)));
                    m_metadata.setTransactionRemindersThreshold(static_cast<RemindersThreshold>(sqlite3_column_int(metadata, 14)));
                    m_metadata.setShowGroupsList(static_cast<bool>(sqlite3_column_int(metadata, 7)));
                    m_metadata.setShowTagsList(static_cast<bool>(sqlite3_column_int(metadata, 13)));
                    m_metadata.setSortTransactionsBy(static_cast<SortBy>(sqlite3_column_int(metadata, 9)));
                    m_metadata.setSortFirstToLast(static_cast<bool>(sqlite3_column_int(metadata, 8)));
                }
                else
                {
                    sqlite3_stmt* newMetadata;
                    sqlite3_prepare_v2(m_database.get(), "INSERT INTO metadata (id, name, type, useCustomCurrency, customSymbol, customCode, defaultTransactionType, showGroupsList, sortFirstToLast, sortTransactionsBy, customDecimalSeparator, customGroupSeparator, customDecimalDigits, showTagsList, transactionRemindersThreshold, customAmountStyle) VALUES (0, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", -1, &metadata, nullptr);
                    sqlite3_bind_text(newMetadata, 1, m_metadata.getName().c_str(), -1, SQLITE_TRANSIENT);
                    sqlite3_bind_int(newMetadata, 2, static_cast<int>(m_metadata.getType()));
                    sqlite3_bind_int(newMetadata, 3, static_cast<int>(m_metadata.getUseCustomCurrency()));
                    sqlite3_bind_text(newMetadata, 4, m_metadata.getCustomCurrency().getSymbol().c_str(), -1, SQLITE_TRANSIENT);
                    sqlite3_bind_text(newMetadata, 5, m_metadata.getCustomCurrency().getCode().c_str(), -1, SQLITE_TRANSIENT);
                    sqlite3_bind_text(newMetadata, 10, m_metadata.getCustomCurrency().getDecimalSeparator().c_str(), -1, SQLITE_TRANSIENT);
                    sqlite3_bind_text(newMetadata, 11, m_metadata.getCustomCurrency().getGroupSeparator().c_str(), -1, SQLITE_TRANSIENT);
                    sqlite3_bind_int(newMetadata, 12, m_metadata.getCustomCurrency().getDecimalDigits());
                    sqlite3_bind_int(newMetadata, 15, static_cast<int>(m_metadata.getCustomCurrency().getAmountStyle()));
                    sqlite3_bind_int(newMetadata, 6, static_cast<int>(m_metadata.getDefaultTransactionType()));
                    sqlite3_bind_int(newMetadata, 14, static_cast<int>(m_metadata.getTransactionRemindersThreshold()));
                    sqlite3_bind_int(newMetadata, 7, static_cast<int>(m_metadata.getShowGroupsList()));
                    sqlite3_bind_int(newMetadata, 13, static_cast<int>(m_metadata.getShowTagsList()));
                    sqlite3_bind_int(newMetadata, 9, static_cast<int>(m_metadata.getSortTransactionsBy()));
                    sqlite3_bind_int(newMetadata, 8, static_cast<int>(m_metadata.getSortFirstToLast()));
                    sqlite3_step(newMetadata);
                    sqlite3_finalize(newMetadata);
                }
                sqlite3_finalize(metadata);
                m_loggedIn = true;
            }
        }
        return m_loggedIn;
    }

    bool Account::load()
    {
        if(!m_loggedIn)
        {
            return false;
        }
        //Clear existing data from memory
        m_groups.clear();
        m_tags.clear();
        m_transactions.clear();
        m_transactionReminders.clear();
        m_nextAvailableGroupId = 1;
        m_nextAvailableTransactionId = 1;
        //TODO: Get groups
        //TODO: Get tags and transactions
        m_tags.push_back(_("Untagged"));
        syncRepeatTransactions();
        calculateTransactionReminders();
        return true;
    }

    const AccountMetadata& Account::getMetadata() const
    {
        return m_metadata;
    }

    void Account::setMetadata(const AccountMetadata& metadata)
    {
        if(!m_loggedIn)
        {
            return;
        }
        m_metadata = metadata;
        //TODO: Update database
    }

    const std::unordered_map<unsigned int, Group>& Account::getGroups() const
    {
        return m_groups;
    }

    const std::vector<std::string>& Account::getTags() const
    {
        return m_tags;
    }

    const std::unordered_map<unsigned int, Transaction>& Account::getTransactions() const
    {
        return m_transactions;
    }

    const std::vector<std::pair<std::string, std::string>>& Account::getTransactionReminders() const
    {
        return m_transactionReminders;
    }

    unsigned int Account::getNextAvailableGroupId() const
    {
        return m_nextAvailableGroupId;
    }

    unsigned int Account::getNextAvailableTransactionId() const
    {
        return m_nextAvailableTransactionId;
    }

    std::optional<Transaction> Account::sendTransfer(const Transfer& transfer, const Color& color)
    {
        if(!m_loggedIn || transfer.getSourceAccountPath() != m_path)
        {
            return std::nullopt;
        }
        Account accountToSend{ transfer.getDestinationAccountPath() };
        if(accountToSend.login(transfer.getDestinationAccountPassword()))
        {
            accountToSend.receiveTransfer(transfer, color);
            Transaction expense{ m_nextAvailableTransactionId };
            expense.setDescription(std::vformat(_("Transfer to {}"), std::make_format_args(accountToSend.getMetadata().getName())));
            expense.setType(TransactionType::Expense);
            expense.setAmount(transfer.getSourceAmount());
            expense.setColor(color);
            addTransaction(expense);
            return expense;
        }
        return std::nullopt;
    }

    std::optional<Transaction> Account::receiveTransfer(const Transfer& transfer, const Color& color)
    {
        if(!m_loggedIn)
        {
            return std::nullopt;
        }
        Transaction income{ m_nextAvailableTransactionId };
        income.setDescription(std::vformat(_("Transfer from {}"), std::make_format_args(transfer.getSourceAccountName())));
        income.setType(TransactionType::Income);
        income.setAmount(transfer.getDestinationAmount());
        income.setColor(color);
        addTransaction(income);
        return income;
    }

    Account::operator bool() const
    {
        return m_loggedIn;
    }
}