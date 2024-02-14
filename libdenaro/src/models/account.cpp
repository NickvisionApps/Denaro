#include "models/account.h"
#include <chrono>
#include <format>
#include <libnick/database/sqlstatement.h>
#include <libnick/localization/gettext.h>
#include "models/customcurrency.h"

using namespace Nickvision::Database;

namespace Nickvision::Money::Shared::Models
{
    Account::Account(std::filesystem::path path)
        : m_path{ path.replace_extension(".nmoney") },
        m_loggedIn{ false },
        m_database{ m_path, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE },
        m_metadata{ m_path.stem().string(), AccountType::Checking }
    {

    }

    bool Account::isEncrypted() const
    {
        return m_database.isEncrypted();
    }

    bool Account::login(const std::string& password)
    {
        if(!m_loggedIn)
        {
            //Unlock database
            if(!m_database.unlock(password))
            {
                return false;
            }
            //Setup metadata table
            m_database.exec("CREATE TABLE IF NOT EXISTS metadata (id INTEGER PRIMARY KEY, name TEXT, type INTEGER, useCustomCurrency INTEGER, customSymbol TEXT, customCode TEXT, defaultTransactionType INTEGER, showGroupsList INTEGER, sortFirstToLast INTEGER, sortTransactionsBy INTEGER, customDecimalSeparator TEXT, customGroupSeparator TEXT, customDecimalDigits INTEGER, showTagsList INTEGER, transactionRemindersThreshold INTEGER, customAmountStyle INTEGER)");
            m_database.exec("ALTER TABLE metadata ADD COLUMN sortTransactionsBy INTEGER");
            m_database.exec("ALTER TABLE metadata ADD COLUMN customDecimalSeparator TEXT");
            m_database.exec("ALTER TABLE metadata ADD COLUMN customGroupSeparator TEXT");
            m_database.exec("ALTER TABLE metadata ADD COLUMN customDecimalDigits INTEGER");
            m_database.exec("ALTER TABLE metadata ADD COLUMN showTagsList INTEGER");
            m_database.exec("ALTER TABLE metadata ADD COLUMN transactionRemindersThreshold INTEGER");
            m_database.exec("ALTER TABLE metadata ADD COLUMN customAmountStyle INTEGER");
            //Setup groups table
            m_database.exec("CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT, rgba TEXT)");
            m_database.exec("ALTER TABLE groups ADD COLUMN rgba TEXT");
            //Setup transactions table
            m_database.exec("CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT, receipt TEXT, repeatFrom INTEGER, repeatEndDate TEXT, useGroupColor INTEGER, notes TEXT, tags TEXT)");
            m_database.exec("ALTER TABLE transactions ADD COLUMN gid INTEGER");
            m_database.exec("ALTER TABLE transactions ADD COLUMN rgba TEXT");
            m_database.exec("ALTER TABLE transactions ADD COLUMN receipt TEXT");
            m_database.exec("ALTER TABLE transactions ADD COLUMN repeatFrom INTEGER");
            m_database.exec("ALTER TABLE transactions ADD COLUMN repeatEndDate TEXT");
            m_database.exec("ALTER TABLE transactions ADD COLUMN useGroupColor INTEGER");
            m_database.exec("ALTER TABLE transactions ADD COLUMN notes TEXT");
            m_database.exec("ALTER TABLE transactions ADD COLUMN tags TEXT");
            //Get metadata
            SqlStatement statement{ m_database.createStatement("SELECT * FROM metadata where id = 0") };
            if(statement.step())
            {
                m_metadata.setName(statement.getColumnString(1));
                m_metadata.setType(static_cast<AccountType>(statement.getColumnInt(2)));
                m_metadata.setUseCustomCurrency(statement.getColumnBool(3));
                CustomCurrency curr{ statement.getColumnString(4), statement.getColumnString(5) };
                curr.setDecimalSeparator(statement.getColumnString(10));
                curr.setGroupSeparator(statement.getColumnString(11));
                curr.setDecimalDigits(statement.getColumnInt(12));
                curr.setAmountStyle(static_cast<AmountStyle>(statement.getColumnInt(15)));
                m_metadata.setCustomCurrency(curr);
                m_metadata.setDefaultTransactionType(static_cast<TransactionType>(statement.getColumnInt(6)));
                m_metadata.setTransactionRemindersThreshold(static_cast<RemindersThreshold>(statement.getColumnInt(14)));
                m_metadata.setShowGroupsList(statement.getColumnBool(7));
                m_metadata.setShowTagsList(statement.getColumnBool(13));
                m_metadata.setSortTransactionsBy(static_cast<SortBy>(statement.getColumnInt(9)));
                m_metadata.setSortFirstToLast(statement.getColumnBool(8));
            }
            else
            {
                SqlStatement newStatement{ m_database.createStatement("INSERT INTO metadata (id, name, type, useCustomCurrency, customSymbol, customCode, defaultTransactionType, showGroupsList, sortFirstToLast, sortTransactionsBy, customDecimalSeparator, customGroupSeparator, customDecimalDigits, showTagsList, transactionRemindersThreshold, customAmountStyle) VALUES (0, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)") };
                newStatement.bind(1, m_metadata.getName());
                newStatement.bind(2, static_cast<int>(m_metadata.getType()));
                newStatement.bind(3, m_metadata.getUseCustomCurrency());
                newStatement.bind(4, m_metadata.getCustomCurrency().getSymbol());
                newStatement.bind(5, m_metadata.getCustomCurrency().getCode());
                newStatement.bind(10, m_metadata.getCustomCurrency().getDecimalSeparator());
                newStatement.bind(11, m_metadata.getCustomCurrency().getGroupSeparator());
                newStatement.bind(12, m_metadata.getCustomCurrency().getDecimalDigits());
                newStatement.bind(15, static_cast<int>(m_metadata.getCustomCurrency().getAmountStyle()));
                newStatement.bind(6, static_cast<int>(m_metadata.getDefaultTransactionType()));
                newStatement.bind(14, static_cast<int>(m_metadata.getTransactionRemindersThreshold()));
                newStatement.bind(7, m_metadata.getShowGroupsList());
                newStatement.bind(13, m_metadata.getShowTagsList());
                newStatement.bind(9, static_cast<int>(m_metadata.getSortTransactionsBy()));
                newStatement.bind(8, m_metadata.getSortFirstToLast());
                newStatement.step();
            }
            m_loggedIn = true;
        }
        return m_loggedIn;
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
        SqlStatement statement{ m_database.createStatement("UPDATE metadata SET name = ?, type = ?, useCustomCurrency = ?, customSymbol = ?, customCode = ?, defaultTransactionType = ?, showGroupsList = ?, sortFirstToLast = ?, sortTransactionsBy = ?, customDecimalSeparator = ?, customGroupSeparator = ?, customDecimalDigits = ?, showTagsList = ?, transactionRemindersThreshold = ?, customAmountStyle = ? WHERE id = 0") };
        statement.bind(1, m_metadata.getName());
        statement.bind(2, static_cast<int>(m_metadata.getType()));
        statement.bind(3, m_metadata.getUseCustomCurrency());
        statement.bind(4, m_metadata.getCustomCurrency().getSymbol());
        statement.bind(5, m_metadata.getCustomCurrency().getCode());
        statement.bind(10, m_metadata.getCustomCurrency().getDecimalSeparator());
        statement.bind(11, m_metadata.getCustomCurrency().getGroupSeparator());
        statement.bind(12, m_metadata.getCustomCurrency().getDecimalDigits());
        statement.bind(15, static_cast<int>(m_metadata.getCustomCurrency().getAmountStyle()));
        statement.bind(6, static_cast<int>(m_metadata.getDefaultTransactionType()));
        statement.bind(14, static_cast<int>(m_metadata.getTransactionRemindersThreshold()));
        statement.bind(7, m_metadata.getShowGroupsList());
        statement.bind(13, m_metadata.getShowTagsList());
        statement.bind(9, static_cast<int>(m_metadata.getSortTransactionsBy()));
        statement.bind(8, m_metadata.getSortFirstToLast());
        statement.step();
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

    std::vector<std::pair<std::string, std::string>> Account::getTransactionReminders() const
    {
        if(m_metadata.getTransactionRemindersThreshold() == RemindersThreshold::Never)
        {
            return {};
        }
        std::vector<std::pair<std::string, std::string>> reminders;
        std::chrono::year_month_day today{ std::chrono::floor<std::chrono::days>(std::chrono::system_clock::now()) };
        std::string todayString{ std::to_string(unsigned(today.month())) + "/" + std::to_string(unsigned(today.day())) + "/" + std::to_string(int(today.year())) };

        return reminders;
    }

    unsigned int Account::getNextAvailableGroupId() const
    {
        SqlStatement statement{ m_database.createStatement("SELECT MIN(id + 1) AS next_id FROM groups WHERE NOT EXISTS (SELECT 1 FROM groups AS g2 WHERE g2.id = groups.id + 1)") };
        statement.step();
        return statement.getColumnInt(0);
    }

    unsigned int Account::getNextAvailableTransactionId() const
    {
        SqlStatement statement{ m_database.createStatement("SELECT MIN(id + 1) AS next_id FROM transactions WHERE NOT EXISTS (SELECT 1 FROM transactions AS t2 WHERE t2.id = transactions.id + 1)") };
        statement.step();
        return statement.getColumnInt(0);
    }

    void Account::changePassword(const std::string& password)
    {
        m_database.changePassword(password);
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
            Transaction expense{ getNextAvailableTransactionId() };
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
        Transaction income{ getNextAvailableTransactionId() };
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