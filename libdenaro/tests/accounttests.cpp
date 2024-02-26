#include <gtest/gtest.h>
#include <filesystem>
#include <memory>
#include <libnick/app/aura.h>
#include "models/account.h"

using namespace Nickvision::App;
using namespace Nickvision::Money::Shared::Models;

class AccountTest : public testing::Test
{
public:
    static std::filesystem::path m_accountPath;
    static std::unique_ptr<Account> m_account;
    static std::string m_password;

    static void SetUpTestSuite()
    {
        Aura::getActive().init("org.nickvision.money", "Nickvision Denaro", "Denaro");
        m_account = std::make_unique<Account>(m_accountPath);
    }

    static void TearDownTestSuite()
    {
        m_account.reset();
        std::filesystem::remove(m_accountPath);
    }
};

std::filesystem::path AccountTest::m_accountPath = "account.nmoney";
std::unique_ptr<Account> AccountTest::m_account = nullptr;
std::string AccountTest::m_password = "abc72356";

TEST_F(AccountTest, Login)
{
    ASSERT_FALSE(m_account->isEncrypted());
    ASSERT_TRUE(m_account->login(""));
}

TEST_F(AccountTest, EncryptedAccount)
{
    ASSERT_TRUE(m_account->changePassword(m_password));
    ASSERT_TRUE(m_account->isEncrypted());
}

TEST_F(AccountTest, UpdateMetadata)
{
    AccountMetadata metadata{ m_account->getMetadata() };
    metadata.setName("Piggy Bank");
    metadata.setType(AccountType::Savings);
    ASSERT_NO_THROW(m_account->setMetadata(metadata));
    ASSERT_TRUE(m_account->getMetadata().getName() == "Piggy Bank");
    ASSERT_TRUE(m_account->getMetadata().getType() == AccountType::Savings);
}

TEST_F(AccountTest, ImportTestAccountCSV)
{
    ImportResult result{ m_account->importFromFile(Aura::getActive().getExecutableDirectory() / "DenaroTestAccount1.csv", {}, {}) };
    ASSERT_EQ(result.getNewTransactionIds().size(), 1498);
    ASSERT_EQ(result.getNewGroupIds().size(), 2);
    ASSERT_TRUE(m_account->getTotal() > 0);
    ASSERT_TRUE(m_account->getIncome() != m_account->getExpense());
}

TEST_F(AccountTest, ExportTestAccountCSV)
{
    std::filesystem::path exportPath{ Aura::getActive().getExecutableDirectory() / "export.csv" };
    ASSERT_TRUE(m_account->exportToCSV(exportPath, {}, {}));
    ASSERT_TRUE(std::filesystem::exists(exportPath));
    std::filesystem::remove(exportPath);
}

TEST_F(AccountTest, UpdateTransaction2)
{
    Transaction t{ m_account->getTransactions().at(2) };
    t.setDescription("New and improved description");
    t.setAmount(100);
    ASSERT_TRUE(m_account->updateTransaction(t));
}

TEST_F(AccountTest, CheckUpdatedTransaction2)
{
    Transaction t{ m_account->getTransactions().at(2) };
    ASSERT_TRUE(t.getDescription() == "New and improved description");
    ASSERT_TRUE(t.getAmount() == 100);
}

TEST_F(AccountTest, ImportTestAccountOFX)
{
    ImportResult result{ m_account->importFromFile(Aura::getActive().getExecutableDirectory() / "DenaroTestAccount2.ofx", {}, {}) };
    ASSERT_EQ(result.getNewTransactionIds().size(), 3);
    const Transaction& t1{ m_account->getTransactions().at(result.getNewTransactionIds()[0]) };
    const Transaction& t2{ m_account->getTransactions().at(result.getNewTransactionIds()[1]) };
    const Transaction& t3{ m_account->getTransactions().at(result.getNewTransactionIds()[2]) };
    ASSERT_EQ(t1.getDescription(), "Lunch");
    ASSERT_EQ(t1.getAmount(), 25.0);
    ASSERT_EQ(t1.getType(), TransactionType::Expense);
    ASSERT_EQ(t1.getDate(), boost::gregorian::date(2023, 1, 27));
    ASSERT_EQ(t2.getDescription(), "Pay day");
    ASSERT_EQ(t2.getAmount(), 300);
    ASSERT_EQ(t2.getType(), TransactionType::Income);
    ASSERT_EQ(t2.getDate(), boost::gregorian::date(2023, 1, 28));
    ASSERT_EQ(t3.getDescription(), "Phone bill");
    ASSERT_EQ(t3.getAmount(), 75.0);
    ASSERT_EQ(t3.getType(), TransactionType::Expense);
    ASSERT_EQ(t3.getDate(), boost::gregorian::date(2023, 1, 29));
}

TEST_F(AccountTest, ImportTestAccountQIF)
{
    ImportResult result{ m_account->importFromFile(Aura::getActive().getExecutableDirectory() / "DenaroTestAccount3.qif", {}, {}) };
    ASSERT_EQ(result.getNewGroupIds().size(), 1);
    const Group& g1{ m_account->getGroups().at(result.getNewGroupIds()[0]) };
    ASSERT_EQ(g1.getName(), "Food");
    ASSERT_EQ(result.getNewTransactionIds().size(), 2);
    const Transaction& t1{ m_account->getTransactions().at(result.getNewTransactionIds()[0]) };
    const Transaction& t2{ m_account->getTransactions().at(result.getNewTransactionIds()[1]) };
    ASSERT_EQ(t1.getDescription(), "Pay day");
    ASSERT_EQ(t1.getAmount(), 600);
    ASSERT_EQ(t1.getType(), TransactionType::Income);
    ASSERT_EQ(t1.getDate(), boost::gregorian::date(2023, 1, 22));
    ASSERT_EQ(t1.getGroupId(), 1);
    ASSERT_EQ(t2.getDescription(), "Some food");
    ASSERT_EQ(t2.getAmount(), 50);
    ASSERT_EQ(t2.getType(), TransactionType::Expense);
    ASSERT_EQ(t2.getDate(), boost::gregorian::date(2023, 1, 22));
    ASSERT_EQ(t2.getGroupId(), g1.getId());
}