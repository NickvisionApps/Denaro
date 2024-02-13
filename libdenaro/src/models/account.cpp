#include "models/account.h"
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
        m_metadata{ m_path.stem().string(), AccountType::Checking },
        m_nextAvailableGroupId{ 1 },
        m_nextAvailableTransactionId{ 1 }
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
            //Set password if needed
            if(m_database.isEncrypted())
            {
                if(!m_database.unlock(password))
                {
                    return false;
                }
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