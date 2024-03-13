#include "models/account.h"
#include <algorithm>
#include <format>
#include <fstream>
#include <regex>
#include <libnick/app/aura.h>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/database/sqlstatement.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/localization/gettext.h>
#include <matplot/matplot.h>
#include <rapidcsv.h>
#include "helpers/datehelpers.h"
#include "models/currency.h"

using namespace Nickvision::App;
using namespace Nickvision::Database;
using namespace Nickvision::Filesystem;

namespace Nickvision::Money::Shared::Models
{
    Account::Account(std::filesystem::path path)
        : m_path{ path.replace_extension(".nmoney") },
        m_loggedIn{ false },
        m_database{ m_path, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE },
        m_metadata{ m_path.stem().string(), AccountType::Checking }
    {

    }

    const std::filesystem::path& Account::getPath() const
    {
        return m_path;
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
            //Register fixdate sql function
            m_database.registerFunction("fixdate", [](SqlContext& context)
            {
                context.result(DateHelpers::toIsoDateString(context.getArgs()[0].getString()));
            }, 1);
            //Setup metadata table
            m_database.exec("CREATE TABLE IF NOT EXISTS metadata (id INTEGER PRIMARY KEY, name TEXT, type INTEGER, useCustomCurrency INTEGER, customSymbol TEXT, customCode TEXT, defaultTransactionType INTEGER, showGroupsList INTEGER, sortFirstToLast INTEGER, sortTransactionsBy INTEGER, customDecimalSeparator TEXT, customGroupSeparator TEXT, customDecimalDigits INTEGER, showTagsList INTEGER, transactionRemindersThreshold INTEGER, customAmountStyle INTEGER)");
            m_database.exec("ALTER TABLE metadata ADD COLUMN IF NOT EXISTS sortTransactionsBy INTEGER, ADD COLUMN IF NOT EXISTS customDecimalSeparator TEXT, ADD COLUMN IF NOT EXISTS customGroupSeparator TEXT, ADD COLUMN IF NOT EXISTS customDecimalDigits INTEGER, ADD COLUMN IF NOT EXISTS showTagsList INTEGER, ADD COLUMN IF NOT EXISTS transactionRemindersThreshold INTEGER, ADD COLUMN IF NOT EXISTS customAmountStyle INTEGER");
            //Setup groups table
            m_database.exec("CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT, rgba TEXT)");
            m_database.exec("ALTER TABLE groups ADD COLUMN rgba TEXT");
            //Setup transactions table
            m_database.exec("CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT, receipt TEXT, repeatFrom INTEGER, repeatEndDate TEXT, useGroupColor INTEGER, notes TEXT, tags TEXT)");
            m_database.exec("ALTER TABLE transactions ADD COLUMN IF NOT EXISTS gid INTEGER, ADD COLUMN IF NOT EXISTS rgba TEXT, ADD COLUMN IF NOT EXISTS receipt TEXT, ADD COLUMN IF NOT EXISTS repeatFrom INTEGER, ADD COLUMN IF NOT EXISTS repeatEndDate TEXT, ADD COLUMN IF NOT EXISTS useGroupColor INTEGER, ADD COLUMN IF NOT EXISTS notes TEXT, ADD COLUMN IF NOT EXISTS tags TEXT");
            //Get metadata
            SqlStatement statement{ m_database.createStatement("SELECT * FROM metadata where id = 0") };
            if(statement.step())
            {
                m_metadata.setName(statement.getColumnString(1));
                m_metadata.setType(static_cast<AccountType>(statement.getColumnInt(2)));
                m_metadata.setUseCustomCurrency(statement.getColumnBool(3));
                Currency curr{ statement.getColumnString(4), statement.getColumnString(5) };
                curr.setDecimalSeparator(statement.getColumnString(10)[0]);
                curr.setGroupSeparator(statement.getColumnString(11)[0]);
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
            //Get groups
            Group ungrouped{ 0 };
            ungrouped.setName(_("Ungrouped"));
            ungrouped.setDescription(_("Transactions without a group"));
            m_groups.emplace(std::make_pair(0, ungrouped));
            SqlStatement groupsStatement{ m_database.createStatement("SELECT * FROM groups") };
            while(groupsStatement.step())
            {
                Group g{ groupsStatement.getColumnInt(0) };
                g.setName(groupsStatement.getColumnString(1));
                g.setDescription(groupsStatement.getColumnString(2));
                g.setColor({ groupsStatement.getColumnString(3) });
                m_groups.emplace(std::make_pair(g.getId(), g));
            }
            //Get transactions and tags
            SqlStatement transactionsStatement{ m_database.createStatement("SELECT * FROM transactions") };
            while(transactionsStatement.step())
            {
                Transaction t{ transactionsStatement.getColumnInt(0) };
                t.setDate(DateHelpers::fromUSDateString(transactionsStatement.getColumnString(1)));
                t.setDescription(transactionsStatement.getColumnString(2));
                t.setType(static_cast<TransactionType>(transactionsStatement.getColumnInt(3)));
                t.setRepeatInterval(static_cast<TransactionRepeatInterval>(transactionsStatement.getColumnInt(4)));
                t.setAmount(transactionsStatement.getColumnDouble(5));
                t.setGroupId(transactionsStatement.getColumnInt(6));
                t.setColor({ transactionsStatement.getColumnString(7) });
                t.setReceipt({ transactionsStatement.getColumnString(8) });
                t.setRepeatFrom(transactionsStatement.getColumnInt(9));
                t.setRepeatEndDate(DateHelpers::fromUSDateString(transactionsStatement.getColumnString(10)));
                t.setUseGroupColor(transactionsStatement.getColumnBool(11));
                t.setNotes(transactionsStatement.getColumnString(12));
                for(const std::string& tag : StringHelpers::split(transactionsStatement.getColumnString(13), ","))
                {
                    t.addTag(tag);
                    if(std::find(m_tags.begin(), m_tags.end(), tag) == m_tags.end())
                    {
                        m_tags.push_back(tag);
                    }
                }
                m_transactions.emplace(std::make_pair(t.getId(), t));
            }
            //Sync repeat transactions
            syncRepeatTransactions();
            m_loggedIn = true;
        }
        return m_loggedIn;
    }

    bool Account::changePassword(const std::string& password)
    {
        return m_database.changePassword(password);
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
        statement.bind(10, std::to_string(m_metadata.getCustomCurrency().getDecimalSeparator()));
        statement.bind(11, std::to_string(m_metadata.getCustomCurrency().getGroupSeparator()));
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

    const std::unordered_map<int, Group>& Account::getGroups() const
    {
        return m_groups;
    }

    const std::vector<std::string>& Account::getTags() const
    {
        return m_tags;
    }

    const std::unordered_map<int, Transaction>& Account::getTransactions() const
    {
        return m_transactions;
    }

    std::vector<std::tuple<std::string, double, std::string>> Account::getTransactionReminders() const
    {
        if(m_metadata.getTransactionRemindersThreshold() == RemindersThreshold::Never)
        {
            return {};
        }
        std::vector<std::tuple<std::string, double, std::string>> reminders;
        boost::gregorian::date today{ boost::gregorian::day_clock::local_day() };
        boost::gregorian::date threshold{ today };
        std::string when;
        switch(m_metadata.getTransactionRemindersThreshold())
        {
        case RemindersThreshold::OneDayBefore:
            threshold += boost::gregorian::days{ 1 };
            when = _("Tomorrow");
            break;
        case RemindersThreshold::OneWeekBefore:
            threshold += boost::gregorian::weeks{ 1 };
            when = _("One week from now");
            break;
        case RemindersThreshold::OneMonthBefore:
            threshold += boost::gregorian::months{ 1 };
            when = _("One month from now");
            break;
        case RemindersThreshold::TwoMonthsBefore:
            threshold += boost::gregorian::months{ 2 };
            when = _("Two months from now");
            break;
        }
        SqlStatement statement{ m_database.createStatement("SELECT * FROM transactions WHERE repeatFrom = -1 AND fixdata(date) > ? UNION SELECT t1.* FROM transactions t1 JOIN (SELECT repeatFrom, MAX(fixdate(date)) AS highest_date FROM transactions WHERE repeatFrom > 0 GROUP BY repeatFrom) t2 ON t1.repeatFrom = t2.repeatFrom AND fixdate(t1.date) = t2.highest_date") };
        statement.bind(1, boost::gregorian::to_iso_string(today));
        while(statement.step())
        {
            boost::gregorian::date upcoming{ DateHelpers::fromUSDateString(statement.getColumnString(1)) };
            if(statement.getColumnInt(9) != -1)
            {
                switch(static_cast<TransactionRepeatInterval>(statement.getColumnInt(4)))
                {
                case TransactionRepeatInterval::Daily:
                    upcoming += boost::gregorian::days{ 1 };
                    break;
                case TransactionRepeatInterval::Weekly:
                    upcoming += boost::gregorian::weeks{ 1 };
                    break;
                case TransactionRepeatInterval::Biweekly:
                    upcoming += boost::gregorian::weeks{ 2 };
                    break;
                case TransactionRepeatInterval::Monthly:
                    upcoming += boost::gregorian::months{ 1 };
                    break;
                case TransactionRepeatInterval::Quarterly:
                    upcoming += boost::gregorian::months{ 3 };
                    break;
                case TransactionRepeatInterval::Yearly:
                    upcoming += boost::gregorian::years{ 1 };
                    break;
                case TransactionRepeatInterval::Biyearly:
                    upcoming += boost::gregorian::years{ 2 };
                    break;
                }
            }
            if(upcoming <= threshold)
            {
                reminders.push_back({ statement.getColumnString(2), statement.getColumnDouble(5), when });
            }
        }
        return reminders;
    }

    int Account::getNextAvailableGroupId() const
    {
        SqlStatement statement{ m_database.createStatement("SELECT IFNULL(MIN(id + 1), 1) AS next_id FROM groups WHERE NOT EXISTS (SELECT 1 FROM groups AS g2 WHERE g2.id = groups.id + 1)") };
        statement.step();
        return statement.getColumnInt(0);
    }

    int Account::getNextAvailableTransactionId() const
    {
        SqlStatement statement{ m_database.createStatement("SELECT IFNULL(MIN(id + 1), 1) AS next_id FROM transactions WHERE NOT EXISTS (SELECT 1 FROM transactions AS t2 WHERE t2.id = transactions.id + 1)") };
        statement.step();
        return statement.getColumnInt(0);
    }

    double Account::getIncome(const std::vector<int>& transactionIds) const
    {
        std::string query{ "SELECT sum(amount) FROM transactions WHERE type = 0" };
        for(size_t i = 0; i < transactionIds.size(); i++)
        {
            if(i == 0)
            {
                query += " AND id IN (";
            }
            query += std::to_string(transactionIds[i]);
            if(i != transactionIds.size() - 1)
            {
                query += ",";
            }
            else
            {
                query += ")";
            }
        }
        SqlStatement statement{ m_database.createStatement(query) };
        statement.step();
        return statement.getColumnDouble(0);
    }

    double Account::getExpense(const std::vector<int>& transactionIds) const
    {
        std::string query{ "SELECT sum(amount) FROM transactions WHERE type = 1" };
        for(size_t i = 0; i < transactionIds.size(); i++)
        {
            if(i == 0)
            {
                query += " AND id IN (";
            }
            query += std::to_string(transactionIds[i]);
            if(i != transactionIds.size() - 1)
            {
                query += ",";
            }
            else
            {
                query += ")";
            }
        }
        SqlStatement statement{ m_database.createStatement(query) };
        statement.step();
        return statement.getColumnDouble(0);
    }

    double Account::getTotal(const std::vector<int>& transactionIds) const
    {
        std::string query{ "SELECT sum(modified_amount) FROM (SELECT amount, CASE WHEN type = 0 THEN amount ELSE (amount * -1) END AS modified_amount FROM transactions" };
        for(size_t i = 0; i < transactionIds.size(); i++)
        {
            if(i == 0)
            {
                query += " WHERE id IN (";
            }
            query += std::to_string(transactionIds[i]);
            if(i != transactionIds.size() - 1)
            {
                query += ",";
            }
            else
            {
                query += ")";
            }
        }
        query += ")";
        SqlStatement statement{ m_database.createStatement(query) };
        statement.step();
        return statement.getColumnDouble(0);
    }

    double Account::getGroupIncome(const Group& group, const std::vector<int>& transactionIds) const
    {
        std::string query{ "SELECT sum(amount) FROM transactions WHERE type = 0 AND gid = " + std::to_string(group.getId()) };
        for(size_t i = 0; i < transactionIds.size(); i++)
        {
            if(i == 0)
            {
                query += " AND id IN (";
            }
            query += std::to_string(transactionIds[i]);
            if(i != transactionIds.size() - 1)
            {
                query += ",";
            }
            else
            {
                query += ")";
            }
        }
        SqlStatement statement{ m_database.createStatement(query) };
        statement.step();
        return statement.getColumnDouble(0);
    }

    double Account::getGroupExpense(const Group& group, const std::vector<int>& transactionIds) const
    {
        std::string query{ "SELECT sum(amount) FROM transactions WHERE type = 1 AND gid = " + std::to_string(group.getId()) };
        for(size_t i = 0; i < transactionIds.size(); i++)
        {
            if(i == 0)
            {
                query += " AND id IN (";
            }
            query += std::to_string(transactionIds[i]);
            if(i != transactionIds.size() - 1)
            {
                query += ",";
            }
            else
            {
                query += ")";
            }
        }
        SqlStatement statement{ m_database.createStatement(query) };
        statement.step();
        return statement.getColumnDouble(0);
    }

    double Account::getGroupTotal(const Group& group, const std::vector<int>& transactionIds) const
    {
        std::string query{ "SELECT sum(modified_amount) FROM (SELECT amount, CASE WHEN type = 0 THEN amount ELSE (amount * -1) END AS modified_amount FROM transactions WHERE gid = " + std::to_string(group.getId()) };
        for(size_t i = 0; i < transactionIds.size(); i++)
        {
            if(i == 0)
            {
                query += " AND id IN (";
            }
            query += std::to_string(transactionIds[i]);
            if(i != transactionIds.size() - 1)
            {
                query += ",";
            }
            else
            {
                query += ")";
            }
        }
        query += ")";
        SqlStatement statement{ m_database.createStatement(query) };
        statement.step();
        return statement.getColumnDouble(0);
    }

    bool Account::addGroup(const Group& group)
    {
        if(m_groups.contains(group.getId()) || group.getId() <= 0)
        {
            return false;
        }
        //Group names must be unique
        for(const std::pair<const int, Group>& pair : m_groups)
        {
            if(pair.second.getName() == group.getName())
            {
                return false;
            }
        }
        SqlStatement statement{ m_database.createStatement("INSERT INTO groups (id, name, description, rgba) VALUES (?,?,?,?)") };
        statement.bind(1, group.getId());
        statement.bind(2, group.getName());
        statement.bind(3, group.getDescription());
        statement.bind(4, group.getColor().toRGBAHexString());
        if(!statement.step())
        {
            m_groups.emplace(std::make_pair(group.getId(), group));
            return true;
        }
        return false;
    }

    bool Account::updateGroup(const Group& group)
    {
        if(!m_groups.contains(group.getId()))
        {
            return false;
        }
        //Group names must be unique
        for(const std::pair<const int, Group>& pair : m_groups)
        {
            if(pair.second.getName() == group.getName() && pair.second.getId() != group.getId())
            {
                return false;
            }
        }
        SqlStatement statement{ m_database.createStatement("UPDATE groups SET name = ?, description = ?, rgba = ? WHERE id = ?") };
        statement.bind(1, group.getName());
        statement.bind(2, group.getDescription());
        statement.bind(3, group.getColor().toRGBAHexString());
        statement.bind(4, group.getId());
        if(!statement.step())
        {
            m_groups.at(group.getId()) = group;
            return true;
        }
        return false;
    }

    std::pair<bool, std::vector<int>> Account::deleteGroup(const Group& group)
    {
        if(!m_groups.contains(group.getId()))
        {
            return { false, {} };
        }
        //Get transactions belonging to the group and remove their association with the group
        std::vector<int> belongingTransactions;
        SqlStatement updateBelongingStatement{ m_database.createStatement("UPDATE transactions SET gid = -1, useGroupId = 0 WHERE gid = ?") };
        updateBelongingStatement.bind(1, group.getId());
        if(!updateBelongingStatement.step())
        {
            for(std::pair<const int, Transaction>& pair : m_transactions)
            {
                if(pair.second.getGroupId() == group.getId())
                {
                    belongingTransactions.push_back(pair.first);
                    pair.second.setGroupId(-1);
                    pair.second.setUseGroupColor(false);
                }
            }
        }
        //Delete group
        SqlStatement deleteGroupStatement{ m_database.createStatement("DELETE FROM groups WHERE id = ?") };
        deleteGroupStatement.bind(1, group.getId());
        if(!deleteGroupStatement.step())
        {
            m_groups.erase(group.getId());
            return { !deleteGroupStatement.step(), belongingTransactions };
        }
        return { false, {} };
    }

    bool Account::addTransaction(const Transaction& transaction)
    {
        if(m_transactions.contains(transaction.getId()) || transaction.getId() <= 0)
        {
            return false;
        }
        SqlStatement statement{ m_database.createStatement("INSERT INTO transactions (id, date, description, type, repeat, amount, gid, rgba, receipt, repeatFrom, repeatEndDate, useGroupColor, notes, tags) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?)") };
        statement.bind(1, transaction.getId());
        statement.bind(2, DateHelpers::toUSDateString(transaction.getDate()));
        statement.bind(3, transaction.getDescription());
        statement.bind(4, static_cast<int>(transaction.getType()));
        statement.bind(5, static_cast<int>(transaction.getRepeatInterval()));
        statement.bind(6, transaction.getAmount());
        statement.bind(7, transaction.getGroupId());
        statement.bind(8, transaction.getColor().toRGBAHexString());
        statement.bind(9, transaction.getReceipt().toString());
        statement.bind(10, transaction.getRepeatFrom());
        statement.bind(11, DateHelpers::toUSDateString(transaction.getRepeatEndDate()));
        statement.bind(12, transaction.getUseGroupColor());
        statement.bind(13, transaction.getNotes());
        std::string tags;
        for(size_t i = 0; i < transaction.getTags().size(); i++)
        {
            const std::string& tag{ transaction.getTags()[i] };
            tags += tag;
            if(i < transaction.getTags().size() - 1)
            {
                tags += ",";
            }
            if(std::find(m_tags.begin(), m_tags.end(), tag) == m_tags.end())
            {
                m_tags.push_back(tag);
            }
        }
        statement.bind(14, tags);
        if(!statement.step())
        {
            m_transactions.emplace(std::make_pair(transaction.getId(), transaction));
            if(transaction.getRepeatInterval() != TransactionRepeatInterval::Never && transaction.getRepeatFrom() == 0)
            {
                syncRepeatTransactions();
            }
            return true;
        }
        return false;
    }

    bool Account::updateTransaction(const Transaction& transaction, bool updateGenerated)
    {
        if(!m_transactions.contains(transaction.getId()))
        {
            return false;
        }
        SqlStatement statement{ m_database.createStatement("UPDATE transactions SET date = ?, description = ?, type = ?, repeat = ?, amount = ?, gid = ?, rgba = ?, receipt = ?, repeatFrom = ?, repeatEndDate = ?, useGroupColor = ?, notes = ?, tags = ? WHERE id = ?") };
        statement.bind(1, DateHelpers::toUSDateString(transaction.getDate()));
        statement.bind(2, transaction.getDescription());
        statement.bind(3, static_cast<int>(transaction.getType()));
        statement.bind(4, static_cast<int>(transaction.getRepeatInterval()));
        statement.bind(5, transaction.getAmount());
        statement.bind(6, transaction.getGroupId());
        statement.bind(7, transaction.getColor().toRGBAHexString());
        statement.bind(8, transaction.getReceipt().toString());
        statement.bind(9, transaction.getRepeatFrom());
        statement.bind(10, DateHelpers::toUSDateString(transaction.getRepeatEndDate()));
        statement.bind(11, transaction.getUseGroupColor());
        statement.bind(12, transaction.getNotes());
        std::string tags;
        for(size_t i = 0; i < transaction.getTags().size(); i++)
        {
            const std::string& tag{ transaction.getTags()[i] };
            tags += tag;
            if(i < transaction.getTags().size() - 1)
            {
                tags += ",";
            }
            if(std::find(m_tags.begin(), m_tags.end(), tag) == m_tags.end())
            {
                m_tags.push_back(tag);
            }
        }
        statement.bind(13, tags);
        statement.bind(14, transaction.getId());
        if(!statement.step())
        {
            m_transactions.at(transaction.getId()) = transaction;
            if(transaction.getRepeatFrom() == 0) //source repeat transaction
            {
                if(updateGenerated)
                {
                    SqlStatement updateStatement{ m_database.createStatement("UPDATE transactions SET description = ?, type = ?, repeat = ?, amount = ?, gid = ?, rgba = ?, receipt = ?, repeatEndDate = ?, useGroupColor = ?, notes = ?, tags = ? WHERE repeatFrom = ?") };
                    updateStatement.bind(1, transaction.getDescription());
                    updateStatement.bind(2, static_cast<int>(transaction.getType()));
                    updateStatement.bind(3, static_cast<int>(transaction.getRepeatInterval()));
                    updateStatement.bind(4, transaction.getAmount());
                    updateStatement.bind(5, transaction.getGroupId());
                    updateStatement.bind(6, transaction.getColor().toRGBAHexString());
                    updateStatement.bind(7, transaction.getReceipt().toString());
                    updateStatement.bind(8, DateHelpers::toUSDateString(transaction.getRepeatEndDate()));
                    updateStatement.bind(9, transaction.getUseGroupColor());
                    updateStatement.bind(10, transaction.getNotes());
                    updateStatement.bind(11, tags);
                    updateStatement.bind(12, transaction.getId());
                    if(!updateStatement.step())
                    {
                        for(std::pair<const int, Transaction>& pair : m_transactions)
                        {
                            if(pair.second.getRepeatFrom() == transaction.getId())
                            {
                                pair.second.setDescription(transaction.getDescription());
                                pair.second.setType(transaction.getType());
                                pair.second.setRepeatInterval(transaction.getRepeatInterval());
                                pair.second.setAmount(transaction.getAmount());
                                pair.second.setGroupId(transaction.getGroupId());
                                pair.second.setColor(transaction.getColor());
                                pair.second.setReceipt(transaction.getReceipt());
                                pair.second.setRepeatEndDate(transaction.getRepeatEndDate());
                                pair.second.setUseGroupColor(transaction.getUseGroupColor());
                                pair.second.setNotes(transaction.getNotes());
                                pair.second.setTags(transaction.getTags());
                            }
                        }
                    }
                }
                else
                {
                    SqlStatement disassociateStatement{ m_database.createStatement("UPDATE transactions SET repeat = 0, repeatFrom = -1, repeatEndDate = '' WHERE repeatFrom = ?") };
                    disassociateStatement.bind(1, transaction.getId());
                    if(!disassociateStatement.step())
                    {
                        for(std::pair<const int, Transaction>& pair : m_transactions)
                        {
                            if(pair.second.getRepeatFrom() == transaction.getId())
                            {
                                pair.second.setRepeatInterval(TransactionRepeatInterval::Never);
                                pair.second.setRepeatFrom(-1);
                                pair.second.setRepeatEndDate({});
                            }
                        }
                    }
                }
                syncRepeatTransactions();
            }
            return true;
        }
        return false;
    }

    bool Account::deleteTransaction(const Transaction& transaction, bool deleteGenerated)
    {
        if(!m_transactions.contains(transaction.getId()))
        {
            return false;
        }
        SqlStatement statement{ m_database.createStatement("DELETE FROM transactions WHERE id = ?") };
        statement.bind(1, transaction.getId());
        if(!statement.step())
        {
            if(transaction.getRepeatFrom() == 0) //source repeat transaction
            {
                if(deleteGenerated)
                {
                    SqlStatement deleteStatement{ m_database.createStatement("DELETE FROM transactions WHERE repeatFrom = ?") };
                    deleteStatement.bind(1, transaction.getId());
                    if(!deleteStatement.step())
                    {
                        for (std::unordered_map<int, Transaction>::iterator it = m_transactions.begin(); it != m_transactions.end();) 
                        {
                            if(it->second.getRepeatFrom() == transaction.getId())
                            {
                                it = m_transactions.erase(it);
                            }
                            else
                            {
                                it++;
                            }
                        }
                    }
                }
                else
                {
                    SqlStatement disassociateStatement{ m_database.createStatement("UPDATE transactions SET repeat = 0, repeatFrom = -1, repeatEndDate = '' WHERE repeatFrom = ?") };
                    disassociateStatement.bind(1, transaction.getId());
                    if(!disassociateStatement.step())
                    {
                        for(std::pair<const int, Transaction>& pair : m_transactions)
                        {
                            if(pair.second.getRepeatFrom() == transaction.getId())
                            {
                                pair.second.setRepeatInterval(TransactionRepeatInterval::Never);
                                pair.second.setRepeatFrom(-1);
                                pair.second.setRepeatEndDate({});
                            }
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    bool Account::deleteGeneratedTransactions(int sourceId)
    {
        SqlStatement statement{ m_database.createStatement("DELETE FROM transactions WHERE repeatFrom = ?") };
        statement.bind(1, sourceId);
        if(!statement.step())
        {
            for (std::unordered_map<int, Transaction>::iterator it = m_transactions.begin(); it != m_transactions.end();) 
            {
                if(it->second.getRepeatFrom() == sourceId)
                {
                    it = m_transactions.erase(it);
                }
                else
                {
                    it++;
                }
            }
            return true;
        }
        return false;
    }

    bool Account::syncRepeatTransactions()
    {
        m_database.exec("BEGIN TRANSACTION");
        //Delete repeat transactions if the date from the original transaction was changed to a smaller date
        SqlStatement deleteExtra{ m_database.createStatement("DELETE FROM transactions WHERE repeatFrom > 0 AND (SELECT fixDate(date) FROM transactions WHERE id = repeatFrom) < fixDate(date)") };
        if(!deleteExtra.step())
        {
            for (std::unordered_map<int, Transaction>::iterator it = m_transactions.begin(); it != m_transactions.end();) 
            {
                if(it->second.getRepeatFrom() > 0)
                {
                    if(it->second.getDate() < m_transactions.at(it->second.getRepeatFrom()).getDate())
                    {
                        m_transactions.erase(it->first);
                        continue;
                    }
                }
                it++;
            }
        }
        //TODO: Add missing repeat transactions up until today
        m_database.exec("COMMIT");
        return true;
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

    ImportResult Account::importFromFile(const std::filesystem::path& path, const Color& defaultTransactionColor, const Color& defaultGroupColor)
    {
        if(!std::filesystem::exists(path))
        {
            return {};
        }
        std::string extension{ StringHelpers::toLower(path.extension().string()) };
        if(extension == ".csv")
        {
            return importFromCSV(path, defaultTransactionColor, defaultGroupColor);
        }
        else if(extension == ".ofx" || extension == ".ofc")
        {
            return importFromOFX(path, defaultTransactionColor);
        }
        else if(extension == ".qif")
        {
            return importFromQIF(path, defaultTransactionColor, defaultGroupColor);
        }
        return {};
    }

    bool Account::exportToCSV(const std::filesystem::path& path, const std::vector<int>& filteredIds) const
    {
        /**
         * CSV Header:
         * ID;Date;Description;Type;RepeatInterval;RepeatFrom;RepeatEndDate;Amount;RGBA;UseGroupColor;Group;GroupName;GroupDescription;GroupRGBA
         */
        rapidcsv::Document csv{ {}, rapidcsv::LabelParams(), rapidcsv::SeparatorParams(';'), rapidcsv::ConverterParams(true) };
        csv.SetColumnName(0, "ID");
        csv.SetColumnName(1, "Date");
        csv.SetColumnName(2, "Description");
        csv.SetColumnName(3, "Type");
        csv.SetColumnName(4, "RepeatInterval");
        csv.SetColumnName(5, "RepeatFrom");
        csv.SetColumnName(6, "RepeatEndDate");
        csv.SetColumnName(7, "Amount");
        csv.SetColumnName(8, "RGBA");
        csv.SetColumnName(9, "UseGroupColor");
        csv.SetColumnName(10, "Group");
        csv.SetColumnName(11, "GroupName");
        csv.SetColumnName(12, "GroupDescription");
        csv.SetColumnName(13, "GroupRGBA");
        for(const std::pair<const int, Transaction>& pair : m_transactions)
        {
            if(filteredIds.empty() || std::find(filteredIds.begin(), filteredIds.end(), pair.first) != filteredIds.end())
            {
                csv.SetCell<int>(0, pair.first, pair.first);
                csv.SetCell<std::string>(1, pair.first, DateHelpers::toUSDateString(pair.second.getDate()));
                csv.SetCell<std::string>(2, pair.first, pair.second.getDescription());
                csv.SetCell<int>(3, pair.first, static_cast<int>(pair.second.getType()));
                csv.SetCell<int>(4, pair.first, static_cast<int>(pair.second.getRepeatInterval()));
                csv.SetCell<int>(5, pair.first, pair.second.getRepeatFrom());
                csv.SetCell<std::string>(6, pair.first, DateHelpers::toUSDateString(pair.second.getRepeatEndDate()));
                csv.SetCell<double>(7, pair.first, pair.second.getAmount());
                csv.SetCell<std::string>(8, pair.first, pair.second.getColor().toRGBAHexString());
                csv.SetCell<int>(9, pair.first, pair.second.getUseGroupColor());
                if(pair.second.getGroupId() != -1)
                {
                    const Group& group{ m_groups.at(pair.second.getGroupId()) };
                    csv.SetCell<int>(10, pair.first, group.getId());
                    csv.SetCell<std::string>(11, pair.first, group.getName());
                    csv.SetCell<std::string>(12, pair.first, group.getDescription());
                    csv.SetCell<std::string>(13, pair.first, group.getColor().toRGBAHexString());
                }
            }
        }
        csv.Save(path.string());
        return true;
    }

    bool Account::exportToPDF(const std::filesystem::path& path, const std::string& password, const std::vector<int>& filteredIds) const
    {
        //TODO: Implement
        return false;
    }

    std::vector<std::uint8_t> Account::generateGraph(GraphType type, bool darkMode, const std::vector<int>& filteredIds, int width, int height) const
    {
        std::string tempPath{ StringHelpers::replace((UserDirectories::getApplicationCache() / "TEMP_DENARO_GRAPH.png").string(), "\\", "/") }; //gnuplot accepts paths with only / as separator
        matplot::figure_handle figure{ matplot::figure(true) };
        //Set graph size
        if(width != -1 && height != -1)
        {
            figure->size(static_cast<unsigned int>(width), static_cast<unsigned int>(height));
        }
        //TODO: Support dark mode
        //Render graph
        if(type == GraphType::IncomeExpensePie)
        {
            std::vector<double> values{ getIncome(filteredIds), getExpense(filteredIds) };
            std::vector<std::string> labels{ _("Income"), _("Expense") };
            matplot::circles_handle pie{ figure->current_axes()->pie(values, labels) };
            pie->labels()->color(darkMode ? "white" : "black");
            //TODO: Change pie colors
        }
        else if(type == GraphType::IncomeExpensePerGroup)
        {
            //TODO: Implement
        }
        else if(type == GraphType::IncomeExpenseOverTime)
        {
            //TODO: Implement
        }
        else if(type == GraphType::IncomeByGroup)
        {
            //TODO: Implement
        }
        else if(type == GraphType::ExpenseByGroup)
        {
            //TODO: Implement
        }
        //Get graph bytes
        figure->save(tempPath);
        figure->~figure_type();
        std::ifstream in{ tempPath, std::ios_base::binary };
        std::vector<std::uint8_t> bytes{ std::istreambuf_iterator<char>(in), std::istreambuf_iterator<char>() };
        in.close();
        std::filesystem::remove(tempPath);
        return bytes;
        //TODO: Fix segfault on Linux
    }

    ImportResult Account::importFromCSV(const std::filesystem::path& path, const Color& defaultTransactionColor, const Color& defaultGroupColor)
    {
        /**
         * CSV Header:
         * ID;Date;Description;Type;RepeatInterval;RepeatFrom;RepeatEndDate;Amount;RGBA;UseGroupColor;Group;GroupName;GroupDescription;GroupRGBA
         */
        rapidcsv::Document csv{ path.string(), rapidcsv::LabelParams(0, -1), rapidcsv::SeparatorParams(';'), rapidcsv::ConverterParams(true), rapidcsv::LineReaderParams(true, '#', true) };
        if(csv.GetColumnCount() != 14)
        {
            return {};
        }
        ImportResult result;
        m_database.exec("BEGIN TRANSACTION");
        for(size_t i = 0; i < csv.GetRowCount(); i++)
        {
            if(m_transactions.contains(csv.GetCell<int>(0, i))) //Transaction IDs must be unique
            {
                continue;
            }
            Transaction t{ csv.GetCell<int>(0, i) };
            t.setDate(DateHelpers::fromUSDateString(csv.GetCell<std::string>(1, i)));
            t.setDescription(csv.GetCell<std::string>(2, i));
            t.setType(static_cast<TransactionType>(csv.GetCell<int>(3, i)));
            t.setRepeatInterval(static_cast<TransactionRepeatInterval>(csv.GetCell<int>(4, i)));
            t.setRepeatFrom(csv.GetCell<int>(5, i));
            t.setRepeatEndDate(DateHelpers::fromUSDateString(csv.GetCell<std::string>(6, i)));
            t.setAmount(csv.GetCell<double>(7, i));
            if(t.getAmount() < 0)
            {
                t.setAmount(t.getAmount() * -1);
            }
            t.setColor({ csv.GetCell<std::string>(8, i) });
            if(!t.getColor())
            {
                t.setColor(defaultTransactionColor);
            }
            t.setUseGroupColor(static_cast<bool>(csv.GetCell<int>(9, i)));
            t.setGroupId(csv.GetCell<int>(10, i));
            if(t.getGroupId() != -1 && !m_groups.contains(t.getGroupId()))
            {
                Group g{ t.getGroupId() };
                g.setName(csv.GetCell<std::string>(11, i));
                if(!g.getName().empty()) //Group names must not be empty
                {
                    g.setDescription(csv.GetCell<std::string>(12, i));
                    g.setColor({ csv.GetCell<std::string>(13, i) });
                    if(!g.getColor())
                    {
                        g.setColor(defaultGroupColor);
                    }
                    if(addGroup(g))
                    {
                        result.addGroup(g.getId());
                    }
                }
            }
            if(addTransaction(t))
            {
                result.addTransaction(t.getId());
            }
        }
        m_database.exec("COMMIT");
        return result;
    }

    ImportResult Account::importFromOFX(const std::filesystem::path& path, const Color& defaultTransactionColor)
    {
        ImportResult result;
        //Read all lines from ofx file
        std::ifstream file{ path };
        std::string ofx;
        if(file.is_open())
        {
            for(std::string line; std::getline(file, line);)
            {
                size_t r{ line.find("\r") };
                if(r != std::string::npos)
                {
                    line.erase(r);
                }
                ofx += line + "\n";
            }
        }
        //Parse ofx file
        if(!ofx.empty())
        {
            m_database.exec("BEGIN TRANSACTION");
            std::regex transactionRegex{ "<STMTTRN>([\\s\\S]+?)</STMTTRN>" };
            std::regex transactionInfoRegex{ "<(.+)>(.+)(\\s|</.+>|$)" };
            //Find all transaction blocks (<STMTTRN>...</STMTTRN>)
            for(std::sregex_iterator it{ ofx.begin(), ofx.end(), transactionRegex }, end{}; it != end; it++)
            {
                Transaction transaction{ getNextAvailableTransactionId() };
                bool nameSet{ false };
                bool dateSet{ false };
                std::string info{ it->str(1) };
                bool transactionValid{ true };
                //Populate transaction info
                transaction.setColor(defaultTransactionColor);
                for(std::sregex_iterator it2{ info.begin(), info.end(), transactionInfoRegex }; it2 != end; it2++)
                {
                    std::string tag{ it2->str(1) };
                    std::string content{ it2->str(2) };
                    // Name
                    if(((tag == "NAME") || (tag == "MEMO")) && !nameSet)
                    {
                        transaction.setDescription(content);
                        nameSet = true;
                    }
                    // Amount
                    if(tag == "TRNAMT")
                    {
                        try
                        {
                            double amount{ std::stod(content) };
                            transaction.setAmount(amount < 0 ? amount * -1 : amount);
                            transaction.setType(amount >= 0 ? TransactionType::Income : TransactionType::Expense);
                        }
                        catch(...)
                        {
                            transactionValid = false;
                            break;
                        }
                    }
                    // Date
                    if((tag == "DTPOSTED" || tag == "DTUSER") && !dateSet)
                    {
                        try
                        {
                            transaction.setDate(boost::gregorian::from_undelimited_string(content));
                        }
                        catch(...)
                        {
                            transactionValid = false;
                            break;
                        }
                        dateSet = true;
                    }
                }
                //Add new transaction
                if(transactionValid && addTransaction(transaction))
                {
                    result.addTransaction(transaction.getId());
                }
            }
            m_database.exec("COMMIT");
        }
        return result;
    }

    ImportResult Account::importFromQIF(const std::filesystem::path& path, const Color& defaultTransactionColor, const Color& defaultGroupColor)
    {
        ImportResult result;
        //Parse qif file
        std::ifstream file{ path };
        std::unique_ptr<Transaction> transaction{ std::make_unique<Transaction>(getNextAvailableTransactionId()) };
        transaction->setColor(defaultTransactionColor);
        if(file.is_open())
        {
            bool amountSet{ false };
            bool descriptionSet{ false };
            bool transactionValid{ true };
            m_database.exec("BEGIN TRANSACTION");
            for(std::string line; std::getline(file, line);)
            {
                if(line.empty() || line[0] == '!')
                {
                    continue;
                }
                //Add transaction
                if(line[0] == '^')
                {
                    if(amountSet && descriptionSet && transactionValid)
                    {
                        if(addTransaction(*transaction))
                        {
                            result.addTransaction(transaction->getId());
                        }
                    }
                    transaction.reset();
                    transaction = std::make_unique<Transaction>(getNextAvailableTransactionId());
                    transaction->setColor(defaultTransactionColor);
                    amountSet = false;
                    descriptionSet = false;
                    transactionValid = true;
                }
                //Date
                else if(line[0] == 'D')
                {
                    try
                    {
                        transaction->setDate(DateHelpers::fromUSDateString(line.substr(1)));
                    }
                    catch(...)
                    {
                        transactionValid = false;
                        break;
                    }
                }
                //Amount
                else if(line[0] == 'T')
                {
                    try
                    {
                        double amount{ std::stod(line.substr(1)) };
                        transaction->setAmount(amount < 0 ? amount * -1 : amount);
                        transaction->setType(amount >= 0 ? TransactionType::Income : TransactionType::Expense);
                        amountSet = true;
                    }
                    catch(...)
                    {
                        transactionValid = false;
                        break;
                    }
                }
                //Description
                else if((line[0] == 'P' || line[0] == 'M') && !descriptionSet)
                {
                    transaction->setDescription(line.substr(1));
                    descriptionSet = true;
                }
                //Group
                else if(line[0] == 'L')
                {
                    std::string name{ line.substr(1) };
                    if(name.empty())
                    {
                        continue;
                    }
                    if(name[0] == '[' && name[name.size() - 1] == ']') //remove [] if surrounding group name
                    {
                        name = name.substr(1, name.size() - 2);
                    }
                    Group g{ getNextAvailableGroupId() };
                    g.setName(name);
                    g.setColor(defaultGroupColor);
                    if(addGroup(g))
                    {
                        result.addGroup(g.getId());
                        transaction->setGroupId(g.getId());
                    }
                    else
                    {
                        for(const std::pair<const int, Group>& pair : m_groups)
                        {
                            if(pair.second.getName() == g.getName())
                            {
                                transaction->setGroupId(pair.first);
                                break;
                            }
                        }
                    }
                }
            }
            m_database.exec("COMMIT");
        }
        return result;
    }

    Account::operator bool() const
    {
        return m_loggedIn;
    }
}