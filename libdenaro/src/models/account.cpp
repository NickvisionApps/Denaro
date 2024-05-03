#include "models/account.h"
#include <algorithm>
#include <chrono>
#include <format>
#include <fstream>
#include <regex>
#include <thread>
#include <libnick/app/aura.h>
#include <libnick/filesystem/userdirectories.h>
#include <libnick/helpers/stringhelpers.h>
#include <libnick/localization/gettext.h>
#include <matplot/matplot.h>
#include <rapidcsv.h>
#include "helpers/currencyhelpers.h"
#include "helpers/datehelpers.h"
#include "models/currency.h"

using namespace Nickvision::App;
using namespace Nickvision::Filesystem;

namespace Nickvision::Money::Shared::Models
{
    Account::Account(std::filesystem::path path)
        : m_path{ path.replace_extension(".nmoney") },
        m_loggedIn{ false },
        m_repository{ m_path },
        m_metadata{ m_path.stem().string(), AccountType::Checking }
    {
        
    }

    const std::filesystem::path& Account::getPath() const
    {
        return m_path;
    }

    bool Account::isEncrypted() const
    {
        return m_repository.isEncrypted();
    }

    bool Account::login(const std::string& password, const Color& defaultGroupColor)
    {
        if(!m_loggedIn)
        {
            if(!m_repository.login(password))
            {
                return false;
            }
            //Load metadata into memory
            m_metadata = m_repository.getMetadata();
            //Load groups into memory
            m_groups = m_repository.getGroups();
            Group ungrouped{ -1 };
            ungrouped.setName(_("Ungrouped"));
            ungrouped.setDescription(_("Transactions without a group"));
            ungrouped.setColor(defaultGroupColor);
            m_groups.emplace(std::make_pair(ungrouped.getId(), ungrouped));
            //Load tags into memory
            m_tags = m_repository.getTags();
            //Load transactions into memory
            for(const std::pair<const int, Transaction>& pair : m_repository.getTransactions())
            {
                m_transactions.emplace(pair);
                m_groups.at(pair.second.getGroupId()).updateBalance(pair.second);
            }
            //Sync repeat transactions
            syncRepeatTransactions();
            m_loggedIn = true;
        }
        return m_loggedIn;
    }

    bool Account::changePassword(const std::string& password)
    {
        return m_repository.changePassword(password);
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
        m_repository.setMetadata(m_metadata);
    }

    const Currency& Account::getCurrency() const
    {
        if(m_metadata.getUseCustomCurrency())
        {
            return m_metadata.getCustomCurrency();
        }
        else
        {
            return CurrencyHelpers::getSystemCurrency();
        }
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

    std::vector<TransactionReminder> Account::getTransactionReminders() const
    {
        if(m_metadata.getTransactionRemindersThreshold() == RemindersThreshold::Never)
        {
            return {};
        }
        std::vector<TransactionReminder> reminders;
        //Calculate threshold date
        boost::gregorian::date threshold{ boost::gregorian::day_clock::local_day() };
        switch(m_metadata.getTransactionRemindersThreshold())
        {
        case RemindersThreshold::OneDayBefore:
            threshold += boost::gregorian::days{ 1 };
            break;
        case RemindersThreshold::OneWeekBefore:
            threshold += boost::gregorian::weeks{ 1 };
            break;
        case RemindersThreshold::OneMonthBefore:
            threshold += boost::gregorian::months{ 1 };
            break;
        case RemindersThreshold::TwoMonthsBefore:
            threshold += boost::gregorian::months{ 2 };
            break;
        }
        //Add reminders for future transactions
        for(const Transaction& transaction : m_repository.getFutureTransactions(threshold))
        {
            TransactionReminder reminder{ transaction.getDescription(), transaction.getDate() };
            reminder.setAmount(transaction.getAmount(), transaction.getType(), getCurrency());
            reminders.push_back(reminder);
        }
        return reminders;
    }

    int Account::getNextAvailableGroupId() const
    {
        if(m_groups.empty())
        {
            return 1;
        }
        for(size_t i = 1; i <= m_groups.size(); i++)
        {
            if(!m_groups.contains(i))
            {
                return i;
            }
        }
        return m_groups.size() + 1;
    }

    int Account::getNextAvailableTransactionId() const
    {
        if(m_transactions.empty())
        {
            return 1;
        }
        for(size_t i = 1; i <= m_transactions.size(); i++)
        {
            if(!m_transactions.contains(i))
            {
                return i;
            }
        }
        return m_transactions.size() + 1;
    }

    double Account::getIncome(const std::vector<int>& transactionIds) const
    {
        double income{ 0 };
        for(const std::pair<const int, Transaction>& pair : m_transactions)
        {
            if(!transactionIds.empty() && std::find(transactionIds.begin(), transactionIds.end(), pair.first) == transactionIds.end())
            {
                continue;
            }
            if(pair.second.getType() == TransactionType::Income)
            {
                income += pair.second.getAmount();
            }
        }
        return income;
    }

    double Account::getExpense(const std::vector<int>& transactionIds) const
    {
        double expense{ 0 };
        for(const std::pair<const int, Transaction>& pair : m_transactions)
        {
            if(!transactionIds.empty() && std::find(transactionIds.begin(), transactionIds.end(), pair.first) == transactionIds.end())
            {
                continue;
            }
            if(pair.second.getType() == TransactionType::Expense)
            {
                expense += pair.second.getAmount();
            }
        }
        return expense;
    }

    double Account::getTotal(const std::vector<int>& transactionIds) const
    {
        double total{ 0 };
        for(const std::pair<const int, Transaction>& pair : m_transactions)
        {
            if(!transactionIds.empty() && std::find(transactionIds.begin(), transactionIds.end(), pair.first) == transactionIds.end())
            {
                continue;
            }
            if(pair.second.getType() == TransactionType::Income)
            {
                total += pair.second.getAmount();
            }
            else if(pair.second.getType() == TransactionType::Expense)
            {
                total -= pair.second.getAmount();
            }
        }
        return total;
    }

    bool Account::addGroup(const Group& group)
    {
        if(m_groups.contains(group.getId()) || group.getId() <= 0)
        {
            return false;
        }
        //Group names must be unique
        if(std::find_if(m_groups.begin(), m_groups.end(), [&group](const std::pair<const int, Group>& pair) 
        { 
            return pair.second.getName() == group.getName(); 
        }) != m_groups.end())
        {
            return false;
        }
        if(m_repository.addGroup(group))
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
        if(std::find_if(m_groups.begin(), m_groups.end(), [&group](const std::pair<const int, Group>& pair) 
        { 
            return pair.second.getName() == group.getName(); 
        }) != m_groups.end())
        {
            return false;
        }
        if(m_repository.updateGroup(group))
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
        if(m_repository.deleteGroup(group))
        {
            //Get transactions belonging to the group and remove their association with the group
            std::vector<int> belongingTransactions;
            for(std::pair<const int, Transaction>& pair : m_transactions)
            {
                if(pair.second.getGroupId() == group.getId())
                {
                    belongingTransactions.push_back(pair.first);
                    pair.second.setGroupId(-1);
                    pair.second.setUseGroupColor(false);
                }
            }
            //Delete group
            m_groups.erase(group.getId());
            return { true, belongingTransactions };
        }
        return { false, {} };
    }

    bool Account::addTransaction(const Transaction& transaction)
    {
        if(m_transactions.contains(transaction.getId()) || transaction.getId() <= 0)
        {
            return false;
        }
        if(m_repository.addTransaction(transaction))
        {
            for(const std::string& tag : transaction.getTags())
            {
                if(std::find(m_tags.begin(), m_tags.end(), tag) == m_tags.end())
                {
                    m_tags.push_back(tag);
                }
            }
            m_transactions.emplace(std::make_pair(transaction.getId(), transaction));
            m_groups.at(transaction.getGroupId()).updateBalance(transaction);
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
        if(m_repository.updateTransaction(transaction, updateGenerated))
        {
            for(const std::string& tag : transaction.getTags())
            {
                if(std::find(m_tags.begin(), m_tags.end(), tag) == m_tags.end())
                {
                    m_tags.push_back(tag);
                }
            }
            if(m_transactions.at(transaction.getId()).getGroupId() != transaction.getGroupId())
            {
                m_groups.at(m_transactions.at(transaction.getId()).getGroupId()).updateBalance(transaction, true);
            }
            m_transactions.at(transaction.getId()) = transaction;
            m_groups.at(transaction.getGroupId()).updateBalance(transaction);
            if(transaction.getRepeatFrom() == 0) //source repeat transaction
            {
                if(updateGenerated)
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
                else
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
        if(m_repository.deleteTransaction(transaction, deleteGenerated))
        {
            m_groups.at(transaction.getGroupId()).updateBalance(transaction, true);
            if(transaction.getRepeatFrom() == 0) //source repeat transaction
            {
                if(deleteGenerated)
                {
                    for (std::unordered_map<int, Transaction>::iterator it = m_transactions.begin(); it != m_transactions.end();) 
                    {
                        if(it->second.getRepeatFrom() == transaction.getId())
                        {
                            it = m_transactions.erase(it);
                            m_groups.at(it->second.getGroupId()).updateBalance(it->second, true);
                        }
                        else
                        {
                            it++;
                        }
                    }
                }
                else
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
            return true;
        }
        return false;
    }

    bool Account::deleteGeneratedTransactions(int sourceId)
    {
        if(m_repository.deleteGeneratedTransactions(sourceId))
        {
            for (std::unordered_map<int, Transaction>::iterator it = m_transactions.begin(); it != m_transactions.end();) 
            {
                if(it->second.getRepeatFrom() == sourceId)
                {
                    it = m_transactions.erase(it);
                    m_groups.at(it->second.getGroupId()).updateBalance(it->second, true);
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
        m_repository.beginTransaction();
        //TODO
        //Delete repeat transactions if the date from the original transaction was changed to a smaller date
        /**
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
        */
        m_repository.commitTransaction();
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
        while(!std::filesystem::exists(tempPath))
        {
            std::this_thread::sleep_for(std::chrono::seconds(1));
        }
        std::ifstream in{ tempPath, std::ios_base::binary };
        std::vector<std::uint8_t> bytes{ std::istreambuf_iterator<char>(in), std::istreambuf_iterator<char>() };
        in.close();
        std::filesystem::remove(tempPath);
        return bytes;
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
        m_repository.beginTransaction();
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
        m_repository.commitTransaction();
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
            m_repository.beginTransaction();
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
            m_repository.commitTransaction();
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
            m_repository.beginTransaction();
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
            m_repository.commitTransaction();
        }
        return result;
    }

    Account::operator bool() const
    {
        return m_loggedIn;
    }
}
