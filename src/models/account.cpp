#include "account.hpp"
#include <chrono>
#include <filesystem>
#include <fstream>
#include <regex>
#include <sstream>
#include <vector>
#include <boost/date_time/gregorian/gregorian.hpp>

using namespace NickvisionMoney::Models;

std::vector<std::string> split(const std::string& s, const std::string& delim)
{
    std::vector<std::string> result;
    size_t last{ 0 };
    size_t next{ s.find(delim) };
    while(next != std::string::npos)
    {
        result.push_back(s.substr(last, next - last));
        last = next + delim.length();
        next = s.find(delim, last);
    }
    result.push_back(s.substr(last));
    return result;
}

unsigned int stoui(const std::string& str, size_t* idx = nullptr, int base = 10)
{
    unsigned long ui{ std::stoul(str, idx, base) };
    if (ui > UINT_MAX)
    {
        return UINT_MAX;
    }
    return ui;
}

Account::Account(const std::string& path) : m_path{ path }, m_db{ std::make_shared<SQLite::Database>(m_path, SQLite::OPEN_READWRITE | SQLite::OPEN_CREATE) }
{
    //Load Groups
    m_db->exec("CREATE TABLE IF NOT EXISTS groups (id INTEGER PRIMARY KEY, name TEXT, description TEXT)");
    //Load Transactions
    m_db->exec("CREATE TABLE IF NOT EXISTS transactions (id INTEGER PRIMARY KEY, date TEXT, description TEXT, type INTEGER, repeat INTEGER, amount TEXT, gid INTEGER, rgba TEXT)");
    try
    {
        m_db->exec("ALTER TABLE transactions ADD COLUMN gid INTEGER");
    }
    catch(...) { }
    try
    {
        m_db->exec("ALTER TABLE transactions ADD COLUMN rgba TEXT");
    }
    catch(...) { }
    // Queries
    SQLite::Statement qryGetAllGroups{ *m_db, "SELECT g.*, CAST(COALESCE(SUM(IIF(t.type=1, -t.amount, t.amount)), 0) AS TEXT) FROM groups g LEFT JOIN transactions t ON t.gid = g.id GROUP BY g.id;" };
    while(qryGetAllGroups.executeStep())
    {
        Group group{ (unsigned int)qryGetAllGroups.getColumn(0).getInt() };
        group.setName(qryGetAllGroups.getColumn(1).getString());
        group.setDescription(qryGetAllGroups.getColumn(2).getString());
        group.setBalance(boost::multiprecision::cpp_dec_float_50(qryGetAllGroups.getColumn(3).getString()));
        m_groups.insert({ group.getId(), group });
    }
    SQLite::Statement qryGetAllTransactions{ *m_db, "SELECT * FROM transactions" };
    while(qryGetAllTransactions.executeStep())
    {
        Transaction transaction{ (unsigned int)qryGetAllTransactions.getColumn(0).getInt() };
        transaction.setDate(boost::gregorian::from_string(qryGetAllTransactions.getColumn(1).getString()));
        transaction.setDescription(qryGetAllTransactions.getColumn(2).getString());
        transaction.setType(static_cast<TransactionType>(qryGetAllTransactions.getColumn(3).getInt()));
        transaction.setRepeatInterval(static_cast<RepeatInterval>(qryGetAllTransactions.getColumn(4).getInt()));
        transaction.setAmount(boost::multiprecision::cpp_dec_float_50(qryGetAllTransactions.getColumn(5).getString()));
        transaction.setGroupId(qryGetAllTransactions.getColumn(6).getInt());
        std::string rgba{ qryGetAllTransactions.getColumn(7).getString() };
        if(!rgba.empty())
        {
            transaction.setRGBA(qryGetAllTransactions.getColumn(7).getString());
        }
        m_transactions.insert({ transaction.getId(), transaction });
    }
    //Repeat Needed Transactions
    size_t i{ 0 };
    size_t startingSize{ m_transactions.size() };
    std::map<unsigned int, Transaction>::iterator it{ m_transactions.begin() };
    while(i != startingSize)
    {
        Transaction transaction{ it->second };
        if(transaction.getRepeatInterval() != RepeatInterval::Never)
        {
            bool repeatNeeeded{ false };
            boost::gregorian::date today{ boost::gregorian::day_clock::local_day() };
            if(transaction.getRepeatInterval() == RepeatInterval::Daily)
            {
                if(today >= transaction.getDate() + boost::gregorian::date_duration(1))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Weekly)
            {
                if(today >= transaction.getDate() + boost::gregorian::date_duration(7))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Monthly)
            {
                if(today >= transaction.getDate() + boost::gregorian::months(1))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Quarterly)
            {
                if(today >= transaction.getDate() + boost::gregorian::months(4))
                {
                    repeatNeeeded = true;
                }
            }
            else if(transaction.getRepeatInterval() == RepeatInterval::Yearly)
            {
                if(today >= transaction.getDate() + boost::gregorian::years(1))
                {
                    repeatNeeeded = true;
                }
            }
            else
            {
                if(today >= transaction.getDate() + boost::gregorian::years(2))
                {
                    repeatNeeeded = true;
                }
            }
            if(repeatNeeeded)
            {
                Transaction newTransaction{ getNextAvailableTransactionId() };
                newTransaction.setDate(today);
                newTransaction.setDescription(transaction.getDescription());
                newTransaction.setType(transaction.getType());
                newTransaction.setRepeatInterval(transaction.getRepeatInterval());
                newTransaction.setAmount(transaction.getAmount());
                newTransaction.setGroupId(transaction.getGroupId());
                newTransaction.setRGBA(transaction.getRGBA());
                addTransaction(newTransaction);
                transaction.setRepeatInterval(RepeatInterval::Never);
                updateTransaction(transaction);
            }
        }
        i++;
        it++;
    }
}

const std::string& Account::getPath() const
{
    return m_path;
}

const std::map<unsigned int, Group>& Account::getGroups() const
{
    return m_groups;
}

std::optional<Group> Account::getGroupById(unsigned int id) const
{
    try
    {
        return m_groups.at(id);
    }
    catch (...)
    {
        return std::nullopt;
    }
}

unsigned int Account::getNextAvailableGroupId() const
{
    if(m_groups.empty())
    {
        return 1;
    }
    else
    {
        return m_groups.rbegin()->first + 1;
    }
}

bool Account::addGroup(const Group& group)
{
    SQLite::Statement qryInsert{ *m_db, "INSERT INTO groups (id, name, description) VALUES (?, ?, ?)" };
    qryInsert.bind(1, group.getId());
    qryInsert.bind(2, group.getName());
    qryInsert.bind(3, group.getDescription());
    if(qryInsert.exec() > 0)
    {
        m_groups.insert({ group.getId(), group });
        return true;
    }
    return false;
}

bool Account::updateGroup(const Group& group)
{
    SQLite::Statement qryUpdate{ *m_db, "UPDATE groups SET name = ?, description = ? WHERE id = " + std::to_string(group.getId()) };
    qryUpdate.bind(1, group.getName());
    qryUpdate.bind(2, group.getDescription());
    if(qryUpdate.exec() > 0)
    {
        m_groups[group.getId()] = group;
        return true;
    }
    return false;
}

bool Account::deleteGroup(unsigned int id)
{
    if(m_db->exec("DELETE FROM groups WHERE id = " + std::to_string(id)) > 0)
    {
        m_groups.erase(id);
        for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
        {
            if(pair.second.getGroupId() == (int)id)
            {
                Transaction transaction{ pair.second };
                transaction.setGroupId(-1);
                updateTransaction(transaction);
            }
        }
        return true;
    }
    return false;
}

const std::map<unsigned int, Transaction>& Account::getTransactions() const
{
    return m_transactions;
}

std::optional<Transaction> Account::getTransactionById(unsigned int id) const
{
    try
    {
        return m_transactions.at(id);
    }
    catch (...)
    {
        return std::nullopt;
    }
}

unsigned int Account::getNextAvailableTransactionId() const
{
    if(m_transactions.empty())
    {
        return 1;
    }
    else
    {
        return m_transactions.rbegin()->first + 1;
    }
}

bool Account::addTransaction(const Transaction& transaction)
{
    SQLite::Statement qryInsert{ *m_db, "INSERT INTO transactions (id, date, description, type, repeat, amount, gid, rgba) VALUES (?, ?, ?, ?, ?, ?, ?, ?)" };
    std::stringstream strAmount;
    strAmount << transaction.getAmount();
    qryInsert.bind(1, transaction.getId());
    qryInsert.bind(2, boost::gregorian::to_iso_extended_string(transaction.getDate()));
    qryInsert.bind(3, transaction.getDescription());
    qryInsert.bind(4, static_cast<int>(transaction.getType()));
    qryInsert.bind(5, static_cast<int>(transaction.getRepeatInterval()));
    qryInsert.bind(6, strAmount.str());
    qryInsert.bind(7, transaction.getGroupId());
    qryInsert.bind(8, transaction.getRGBA());
    if(qryInsert.exec() > 0)
    {
        m_transactions.insert({ transaction.getId(), transaction });
        if(transaction.getGroupId() != -1)
        {
            updateGroupAmounts();
        }
        return true;
    }
    return false;
}

bool Account::updateTransaction(const Transaction& transaction)
{
    SQLite::Statement qryUpdate{ *m_db, "UPDATE transactions SET date = ?, description = ?, type = ?, repeat = ?, amount = ?, gid = ?, rgba = ? WHERE id = " + std::to_string(transaction.getId()) };
    std::stringstream strAmount;
    strAmount << transaction.getAmount();
    qryUpdate.bind(1, boost::gregorian::to_iso_extended_string(transaction.getDate()));
    qryUpdate.bind(2, transaction.getDescription());
    qryUpdate.bind(3, static_cast<int>(transaction.getType()));
    qryUpdate.bind(4, static_cast<int>(transaction.getRepeatInterval()));
    qryUpdate.bind(5, strAmount.str());
    qryUpdate.bind(6, transaction.getGroupId());
    qryUpdate.bind(7, transaction.getRGBA());
    if(qryUpdate.exec() > 0)
    {
        m_transactions[transaction.getId()] = transaction;
        updateGroupAmounts();
        return true;
    }
    return false;
}

bool Account::deleteTransaction(unsigned int id)
{
    if(m_db->exec("DELETE FROM transactions WHERE id = " + std::to_string(id)) > 0)
    {
        bool updateGroups{ m_transactions.at(id).getGroupId() != -1 };
        m_transactions.erase(id);
        if(updateGroups)
        {
            updateGroupAmounts();
        }
        return true;
    }
    return false;
}

boost::multiprecision::cpp_dec_float_50 Account::getIncome() const
{
    boost::multiprecision::cpp_dec_float_50 income{ 0.00 };
    for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
    {
        if(pair.second.getType() == TransactionType::Income)
        {
            income += pair.second.getAmount();
        }
    }
    return income;
}

boost::multiprecision::cpp_dec_float_50 Account::getExpense() const
{
    boost::multiprecision::cpp_dec_float_50 expense{ 0.00 };
    for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
    {
        if(pair.second.getType() == TransactionType::Expense)
        {
            expense += pair.second.getAmount();
        }
    }
    return expense;
}

boost::multiprecision::cpp_dec_float_50 Account::getTotal() const
{
    boost::multiprecision::cpp_dec_float_50 total{ 0.00 };
    for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
    {
        total += pair.second.getType() == TransactionType::Income ? pair.second.getAmount() : (pair.second.getAmount() * -1);
    }
    return total;
}

bool Account::exportAsCSV(const std::string& path)
{
    std::ofstream file{ path };
    if(file.is_open())
    {
        file << "ID;Date;Description;Type;RepeatInterval;Amount;RGBA;Group;GroupName;GroupDescription\n";
        for(const std::pair<const unsigned int, Transaction>& pair : m_transactions)
        {
            file << pair.second.getId() << ";";
            file << boost::gregorian::to_iso_extended_string(pair.second.getDate()) << ";";
            file << pair.second.getDescription() << ";";
            file << static_cast<int>(pair.second.getType()) << ";";
            file << static_cast<int>(pair.second.getRepeatInterval()) << ";";
            file << pair.second.getAmount() << ";";
            file << pair.second.getRGBA() << ";";
            file << pair.second.getGroupId() << ";";
            if (pair.second.getGroupId() != -1)
            {
                file << m_groups.at(pair.second.getGroupId()).getName() << ";";
                file << m_groups.at(pair.second.getGroupId()).getDescription() << "\n";
            }
            else
            {
                file << ";\n";
            }
        }
        return true;
    }
    return false;
}

int Account::importFromFile(const std::string& path)
{
    if (path.ends_with(".csv"))
    {
        return importFromCSV(path);
    }
    else if (path.ends_with(".ofx"))
    {
        return importFromOFX(path);
    }
    else if (path.ends_with(".qif"))
    {
        return importFromQIF(path);
    }

    return -1;
}

int Account::importFromCSV(const std::string& path)
{
    int imported{ 0 };
    std::ifstream file{ path };
    if(file.is_open())
    {
        std::string line;
        while(getline(file, line))
        {
            //Separate fields by ;
            std::vector<std::string> fields{ split(line, ";") };
            if(fields.size() != 10)
            {
                continue;
            }
            //Get Id
            unsigned int id{ 0 };
            try
            {
                id = stoui(fields[0]);
            }
            catch(...)
            {
                continue;
            }
            if(getTransactionById(id) != std::nullopt)
            {
                continue;
            }
            //Get Date
            boost::gregorian::date date;
            try
            {
                date = boost::gregorian::from_string(fields[1]);
            }
            catch(...)
            {
                continue;
            }
            //Get Description
            const std::string& description{ fields[2] };
            //Get Type
            TransactionType type{ TransactionType::Income };
            try
            {
                int value{ std::stoi(fields[3]) };
                if(value != 0 && value != 1)
                {
                    continue;
                }
                type = static_cast<TransactionType>(value);
            }
            catch(...)
            {
                continue;
            }
            //Get Repeat Interval
            RepeatInterval repeatInterval{ RepeatInterval::Never };
            try
            {
                int value{ std::stoi(fields[4]) };
                if(value < 0 && value > 6)
                {
                    continue;
                }
                repeatInterval = static_cast<RepeatInterval>(value);
            }
            catch(...)
            {
                continue;
            }
            //Get Amount
            boost::multiprecision::cpp_dec_float_50 amount;
            try
            {
                amount = boost::multiprecision::cpp_dec_float_50(fields[5]);
            }
            catch(...)
            {
                continue;
            }
            //Get RGBA
            const std::string& rgba{ fields[6] };
            //Get Group Id
            int gid{ 0 };
            try
            {
                gid = stoui(fields[7]);
            }
            catch(...)
            {
                continue;
            }
            //Get Group Name
            const std::string& groupName{ fields[8] };
            //Get Group Description
            const std::string& groupDescription{ fields[9] };
            //Add Group if needed
            if (getGroupById(gid) == std::nullopt && gid != -1)
            {
                Group group{ (unsigned int) gid };
                group.setName(groupName);
                group.setDescription(groupDescription);
                addGroup(group);
            }
            //Add Transaction
            Transaction transaction{ id };
            transaction.setDate(date);
            transaction.setDescription(description);
            transaction.setType(type);
            transaction.setRepeatInterval(repeatInterval);
            transaction.setAmount(amount);
            transaction.setGroupId(gid);
            transaction.setRGBA(rgba);
            addTransaction(transaction);
            imported++;
        }
    }
    return imported;
}

int Account::importFromOFX(const std::string& path)
{
    int imported{ 0 };
    unsigned int nextId{ getNextAvailableTransactionId() };
    const std::regex xmlTagRegex("^<([^/>]+|/STMTTRN)>(?:[+-])?([^<]+)");
    std::smatch xmlTagMatch;
    Transaction *transaction{ nullptr };
    bool skipTransaction{ false };

    std::ifstream file{ path };
    if (file.is_open())
    {
        std::string line;
        while (getline(file, line))
        {
            // Extract tag and content
            if (std::regex_match(line, xmlTagMatch, xmlTagRegex))
            {
                const std::string& tag{ xmlTagMatch[1].str() };
                std::string content{ xmlTagMatch[2].str() };

                const long unsigned int found = content.find('\r');
                if (found != std::string::npos)
                {
                    content.erase(found);
                }

                // Add previous transaction
                if (tag == "/STMTTRN" && transaction != nullptr)
                {
                    if (!skipTransaction)
                    {
                        addTransaction(*transaction);
                        imported++;
                    }
                    skipTransaction = false;
                    delete transaction;
                }
                // New transaction
                if (tag == "STMTTRN")
                {
                    transaction = new Transaction(nextId);
                    nextId++;
                }
                if (!skipTransaction)
                {
                    // Type
                    if (tag == "TRNTYPE")
                    {
                        if (content == "CREDIT")
                        {
                            transaction->setType(TransactionType::Income);
                        }
                        else if (content == "DEBIT")
                        {
                            transaction->setType(TransactionType::Expense);
                        }
                        else
                        {
                            // Unkown entry
                            skipTransaction = true;
                            continue;
                        }
                    }
                    // Name
                    if (tag == "MEMO")
                    {
                        transaction->setDescription(content);
                    }
                    // Amount
                    if (tag == "TRNAMT")
                    {
                        try
                        {
                            transaction->setAmount(boost::multiprecision::cpp_dec_float_50(content));
                        }
                        catch (...)
                        {
                            skipTransaction = true;
                            continue;
                        }
                    }
                    // Date
                    if (tag == "DTPOSTED")
                    {
                        try
                        {
                            transaction->setDate(boost::gregorian::from_undelimited_string(content));
                        }
                        catch (...)
                        {
                            skipTransaction = true;
                            continue;
                        }
                    }
                }
            }
        }
    }

    return imported;
}

int Account::importFromQIF(const std::string& path)
{
    int imported{ 0 };
    unsigned int nextId{ getNextAvailableTransactionId() };
    std::ifstream file{ path };
    Transaction *transaction{ new Transaction(nextId) };
    bool skipTransaction{ false };
    if (file.is_open())
    {
        std::string line;
        while (getline(file, line))
        {
            const long unsigned int found = line.find('\r');
            if (found != std::string::npos)
            {
                line.erase(found);
            }
            if (line.length() == 0)
            {
                continue;
            }
            // Add the previous transaction
            if (line[0] == '^')
            {
                if (!skipTransaction)
                {
                    addTransaction(*transaction);
                    imported++;
                }
                delete transaction;
                skipTransaction = false;
                nextId++;
                transaction = new Transaction{ nextId };
            }
            // Date
            if (line[0] == 'D')
            {
                try
                {
                    const int year{ atoi(line.substr(7, 4).c_str()) };
                    const int month{ atoi(line.substr(4, 2).c_str()) };
                    const int day{ atoi(line.substr(1, 2).c_str()) };
                    transaction->setDate(boost::gregorian::date(year, month, day));
                }
                catch (...)
                {
                    skipTransaction = true;
                    continue;
                }
            }
            // Amount and Type
            if (line[0] == 'T')
            {
                try
                {
                    transaction->setType(line[1] == '-' ? TransactionType::Expense : TransactionType::Income);
                    transaction->setAmount(boost::multiprecision::cpp_dec_float_50(line.substr(2)));
                }
                catch (...)
                {
                    skipTransaction = true;
                    continue;
                }
                
            }
            // Description
            if (line[0] == 'P')
            {
                transaction->setDescription(line.substr(1));
            }
        }
        delete transaction;
    }
    return imported;
}

void Account::updateGroupAmounts()
{
    //Query for balance in SQL
    SQLite::Statement qryGetGroupsBalance{ *m_db, "SELECT g.id, CAST(COALESCE(SUM(IIF(t.type=1, -t.amount, t.amount)), 0) AS TEXT) FROM transactions t RIGHT JOIN groups g on g.id = t.gid GROUP BY g.id;" };
    while(qryGetGroupsBalance.executeStep())
    {
        m_groups.at(qryGetGroupsBalance.getColumn(0).getInt()).setBalance(boost::multiprecision::cpp_dec_float_50(qryGetGroupsBalance.getColumn(1).getString()));
    }
}
