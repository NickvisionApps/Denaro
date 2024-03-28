#include <gtest/gtest.h>
#include <filesystem>
#include <memory>
#include <libnick/helpers/webhelpers.h>
#include "models/receipt.h"

using namespace Nickvision;
using namespace Nickvision::Money::Shared::Models;

class ReceiptTest : public testing::Test
{
public:
    static std::filesystem::path m_receiptPath;
    static std::filesystem::path m_receiptPath2;
    static std::unique_ptr<Receipt> m_receipt;

    static void SetUpTestSuite()
    {
        WebHelpers::downloadFile("https://upload.wikimedia.org/wikipedia/commons/thumb/4/47/PNG_transparency_demonstration_1.png/280px-PNG_transparency_demonstration_1.png", m_receiptPath);
    }

    static void TearDownTestSuite() 
    {
        std::filesystem::remove(m_receiptPath);
        std::filesystem::remove(m_receiptPath2);
    }
};

std::filesystem::path ReceiptTest::m_receiptPath = "a.png";
std::filesystem::path ReceiptTest::m_receiptPath2 = "b.png";
std::unique_ptr<Receipt> ReceiptTest::m_receipt = nullptr;

TEST_F(ReceiptTest, SetupReceiptObject)
{
    ASSERT_NO_THROW(m_receipt = std::make_unique<Receipt>(m_receiptPath));
}

TEST_F(ReceiptTest, EnsureReceiptObject)
{
    ASSERT_TRUE(m_receipt->operator bool());
    ASSERT_EQ(m_receipt->getType(), ReceiptType::PNG);
    ASSERT_TRUE(!m_receipt->toString().empty());
}

TEST_F(ReceiptTest, SaveToDisk)
{
    ASSERT_NO_THROW(m_receipt->saveToDisk(m_receiptPath2));
}