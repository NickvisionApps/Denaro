#include "models/accountrepository.h"
#include <libnick/database/sqlstatement.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/localization/gettext.h>
#include "helpers/datehelpers.h"

using namespace Nickvision::Database;

namespace Nickvision::Money::Shared::Models
{
    AccountRepository::AccountRepository(const std::filesystem::path& path)
        : m_database{ path, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE },
        m_transactionInProcess{ false }
    {
        
    }

    bool AccountRepository::isEncrypted() const
    {
        return m_database.isEncrypted();
    }

    bool AccountRepository::login(const std::string& password)
    {
        //Unlock database
        if(!m_database.unlock(password))
        {
            return false;
        }
        //Register fixdata sql function
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
        return true;
    }

    bool AccountRepository::changePassword(const std::string& password)
    {
        return m_database.changePassword(password);
    }

    bool AccountRepository::beginTransaction()
    {
        if(m_transactionInProcess)
        {
            return false;
        }
        if(m_database.exec("BEGIN TRANSACTION"))
        {
            m_transactionInProcess = true;
        }
        return m_transactionInProcess;
    }

    bool AccountRepository::commitTransaction()
    {
        if(!m_transactionInProcess)
        {
            return false;
        }
        if(m_database.exec("COMMIT"))
        {
            m_transactionInProcess = false;
        }
        return !m_transactionInProcess;
    }

    AccountMetadata AccountRepository::getMetadata() const
    {
        AccountMetadata metadata{ "", AccountType::Checking };
        SqlStatement statement{ m_database.createStatement("SELECT * FROM metadata where id = 0") };
        if(statement.step())
        {
            metadata.setName(statement.getColumnString(1));
            metadata.setType(static_cast<AccountType>(statement.getColumnInt(2)));
            metadata.setUseCustomCurrency(statement.getColumnBool(3));
            Currency curr{ statement.getColumnString(4), statement.getColumnString(5) };
            curr.setDecimalSeparator(statement.getColumnString(10)[0]);
            curr.setGroupSeparator(statement.getColumnString(11)[0]);
            curr.setDecimalDigits(statement.getColumnInt(12));
            curr.setAmountStyle(static_cast<AmountStyle>(statement.getColumnInt(15)));
            metadata.setCustomCurrency(curr);
            metadata.setDefaultTransactionType(static_cast<TransactionType>(statement.getColumnInt(6)));
            metadata.setTransactionRemindersThreshold(static_cast<RemindersThreshold>(statement.getColumnInt(14)));
            metadata.setShowGroupsList(statement.getColumnBool(7));
            metadata.setShowTagsList(statement.getColumnBool(13));
            metadata.setSortTransactionsBy(static_cast<SortBy>(statement.getColumnInt(9)));
            metadata.setSortFirstToLast(statement.getColumnBool(8));
        }
        else
        {
            SqlStatement newStatement{ m_database.createStatement("INSERT INTO metadata (id, name, type, useCustomCurrency, customSymbol, customCode, defaultTransactionType, showGroupsList, sortFirstToLast, sortTransactionsBy, customDecimalSeparator, customGroupSeparator, customDecimalDigits, showTagsList, transactionRemindersThreshold, customAmountStyle) VALUES (0, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)") };
            newStatement.bind(1, metadata.getName());
            newStatement.bind(2, static_cast<int>(metadata.getType()));
            newStatement.bind(3, metadata.getUseCustomCurrency());
            newStatement.bind(4, metadata.getCustomCurrency().getSymbol());
            newStatement.bind(5, metadata.getCustomCurrency().getCode());
            newStatement.bind(10, metadata.getCustomCurrency().getDecimalSeparator());
            newStatement.bind(11, metadata.getCustomCurrency().getGroupSeparator());
            newStatement.bind(12, metadata.getCustomCurrency().getDecimalDigits());
            newStatement.bind(15, static_cast<int>(metadata.getCustomCurrency().getAmountStyle()));
            newStatement.bind(6, static_cast<int>(metadata.getDefaultTransactionType()));
            newStatement.bind(14, static_cast<int>(metadata.getTransactionRemindersThreshold()));
            newStatement.bind(7, metadata.getShowGroupsList());
            newStatement.bind(13, metadata.getShowTagsList());
            newStatement.bind(9, static_cast<int>(metadata.getSortTransactionsBy()));
            newStatement.bind(8, metadata.getSortFirstToLast());
            newStatement.step();
        }
        return metadata;
    }

    void AccountRepository::setMetadata(const AccountMetadata& metadata)
    {
        SqlStatement statement{ m_database.createStatement("UPDATE metadata SET name = ?, type = ?, useCustomCurrency = ?, customSymbol = ?, customCode = ?, defaultTransactionType = ?, showGroupsList = ?, sortFirstToLast = ?, sortTransactionsBy = ?, customDecimalSeparator = ?, customGroupSeparator = ?, customDecimalDigits = ?, showTagsList = ?, transactionRemindersThreshold = ?, customAmountStyle = ? WHERE id = 0") };
        statement.bind(1, metadata.getName());
        statement.bind(2, static_cast<int>(metadata.getType()));
        statement.bind(3, metadata.getUseCustomCurrency());
        statement.bind(4, metadata.getCustomCurrency().getSymbol());
        statement.bind(5, metadata.getCustomCurrency().getCode());
        statement.bind(10, std::to_string(metadata.getCustomCurrency().getDecimalSeparator()));
        statement.bind(11, std::to_string(metadata.getCustomCurrency().getGroupSeparator()));
        statement.bind(12, metadata.getCustomCurrency().getDecimalDigits());
        statement.bind(15, static_cast<int>(metadata.getCustomCurrency().getAmountStyle()));
        statement.bind(6, static_cast<int>(metadata.getDefaultTransactionType()));
        statement.bind(14, static_cast<int>(metadata.getTransactionRemindersThreshold()));
        statement.bind(7, metadata.getShowGroupsList());
        statement.bind(13, metadata.getShowTagsList());
        statement.bind(9, static_cast<int>(metadata.getSortTransactionsBy()));
        statement.bind(8, metadata.getSortFirstToLast());
        statement.step();
    }

    std::unordered_map<int, Group> AccountRepository::getGroups() const
    {
        std::unordered_map<int, Group> groups;
        SqlStatement groupsStatement{ m_database.createStatement("SELECT * FROM groups") };
        while(groupsStatement.step())
        {
            Group g{ groupsStatement.getColumnInt(0) };
            g.setName(groupsStatement.getColumnString(1));
            g.setDescription(groupsStatement.getColumnString(2));
            g.setColor({ groupsStatement.getColumnString(3) });
            groups.emplace(std::make_pair(g.getId(), g));
        }
        return groups;
    }

    std::vector<std::string> AccountRepository::getTags() const
    {
        std::vector<std::string> tags;
        SqlStatement tagsStatement{ m_database.createStatement("SELECT tags FROM transactions") };
        while(tagsStatement.step())
        {
            for(const std::string& tag : StringHelpers::split(tagsStatement.getColumnString(0), ","))
            {
                if(std::find(tags.begin(), tags.end(), tag) == tags.end())
                {
                    tags.push_back(tag);
                }
            }
        }
        return tags;
    }

    std::unordered_map<int, Transaction> AccountRepository::getTransactions() const
    {
        std::unordered_map<int, Transaction> transactions;
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
            }
            transactions.emplace(std::make_pair(t.getId(), t));
        }
        return transactions;
    }

    std::vector<Transaction> AccountRepository::getUpcomingTransactions(const boost::gregorian::date& threshold) const
    {
        std::vector<Transaction> transactions;
        boost::gregorian::date today{ boost::gregorian::day_clock::local_day() };
        SqlStatement statement{ m_database.createStatement("SELECT * FROM transactions WHERE repeatFrom = -1 AND fixdata(date) > ? UNION SELECT t1.* FROM transactions t1 JOIN (SELECT repeatFrom, MAX(fixdate(date)) AS highest_date FROM transactions WHERE repeatFrom > 0 GROUP BY repeatFrom) t2 ON t1.repeatFrom = t2.repeatFrom AND fixdate(t1.date) = t2.highest_date") };
        statement.bind(1, boost::gregorian::to_iso_string(today));
        while(statement.step())
        {
            Transaction t{ statement.getColumnInt(0) };
            t.setDate(DateHelpers::fromUSDateString(statement.getColumnString(1)));
            t.setDescription(statement.getColumnString(2));
            t.setType(static_cast<TransactionType>(statement.getColumnInt(3)));
            t.setRepeatInterval(static_cast<TransactionRepeatInterval>(statement.getColumnInt(4)));
            t.setAmount(statement.getColumnDouble(5));
            t.setGroupId(statement.getColumnInt(6));
            t.setColor({ statement.getColumnString(7) });
            t.setReceipt({ statement.getColumnString(8) });
            t.setRepeatFrom(statement.getColumnInt(9));
            t.setRepeatEndDate(DateHelpers::fromUSDateString(statement.getColumnString(10)));
            t.setUseGroupColor(statement.getColumnBool(11));
            t.setNotes(statement.getColumnString(12));
            for(const std::string& tag : StringHelpers::split(statement.getColumnString(13), ","))
            {
                t.addTag(tag);
            }
            boost::gregorian::date upcoming{ t.getDate() };
            if(t.getRepeatFrom() != -1)
            {
                switch(t.getRepeatInterval())
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
                transactions.push_back(t);
            }
        }
        return transactions;
    }

    bool AccountRepository::addGroup(const Group& group)
    {
        SqlStatement statement{ m_database.createStatement("INSERT INTO groups (id, name, description, rgba) VALUES (?, ?, ?, ?)") };
        statement.bind(1, group.getId());
        statement.bind(2, group.getName());
        statement.bind(3, group.getDescription());
        statement.bind(4, group.getColor().toRGBAHexString());
        return !statement.step();
    }

    bool AccountRepository::updateGroup(const Group& group)
    {
        SqlStatement statement{ m_database.createStatement("UPDATE groups SET name = ?, description = ?, rgba = ? WHERE id = ?") };
        statement.bind(1, group.getName());
        statement.bind(2, group.getDescription());
        statement.bind(3, group.getColor().toRGBAHexString());
        statement.bind(4, group.getId());
        return !statement.step();
    }

    bool AccountRepository::deleteGroup(const Group& group)
    {
        SqlStatement statement{ m_database.createStatement("DELETE FROM groups WHERE id = ?") };
        statement.bind(1, group.getId());
        if(!statement.step())
        {
            SqlStatement updateStatement{ m_database.createStatement("UPDATE transactions SET gid = -1, useGroupColor = 0 WHERE gid = ?") };
            updateStatement.bind(1, group.getId());
            return !updateStatement.step();
        }
        return false;
    }

    bool AccountRepository::addTransaction(const Transaction& transaction)
    {
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
        }
        statement.bind(14, tags);
        return !statement.step();
    }

    bool AccountRepository::updateTransaction(const Transaction& transaction, bool updateGenerated)
    {
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
        }
        statement.bind(13, tags);
        statement.bind(14, transaction.getId());
        if(!statement.step())
        {
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
                    return !updateStatement.step();
                }
                else
                {
                    SqlStatement disassociateStatement{ m_database.createStatement("UPDATE transactions SET repeat = 0, repeatFrom = -1, repeatEndDate = '' WHERE repeatFrom = ?") };
                    disassociateStatement.bind(1, transaction.getId());
                    return !disassociateStatement.step();
                }
            }
            return true;
        }
        return false;
    }

    bool AccountRepository::deleteTransaction(const Transaction& transaction, bool deleteGenerated)
    {
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
                    return !deleteStatement.step();
                }
                else
                {
                    SqlStatement disassociateStatement{ m_database.createStatement("UPDATE transactions SET repeat = 0, repeatFrom = -1, repeatEndDate = '' WHERE repeatFrom = ?") };
                    disassociateStatement.bind(1, transaction.getId());
                    return !disassociateStatement.step();
                }
            }
            return true;
        }
        return false;
    }

    bool AccountRepository::deleteGeneratedTransactions(int sourceId)
    {
        SqlStatement statement{ m_database.createStatement("DELETE FROM transactions WHERE repeatFrom = ?") };
        statement.bind(1, sourceId);
        return !statement.step();
    }
}