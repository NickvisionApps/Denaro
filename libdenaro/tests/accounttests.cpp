#include <gtest/gtest.h>
#include <filesystem>
#include <memory>
#include "models/account.h"

using namespace Nickvision::Money::Shared::Models;

class AccountTest : public testing::Test
{
public:
    static std::filesystem::path m_accountPath;
    static std::unique_ptr<Account> m_account;
    static std::string m_password;

    static void SetUpTestSuite()
    {
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
}