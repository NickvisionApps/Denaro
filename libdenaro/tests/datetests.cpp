#include <gtest/gtest.h>
#include "helpers/datehelpers.h"

using namespace Nickvision::Money::Shared;

TEST(DateTests, FromUs1)
{
    boost::gregorian::date d{ DateHelpers::fromUSDateString("12/3/2056") };
    ASSERT_EQ(d.month(), 12);
    ASSERT_EQ(d.day(), 3);
    ASSERT_EQ(d.year(), 2056);
}

TEST(DateTests, FromUs2)
{
    boost::gregorian::date d{ DateHelpers::fromUSDateString("4/25/2024") };
    ASSERT_EQ(d.month(), 4);
    ASSERT_EQ(d.day(), 25);
    ASSERT_EQ(d.year(), 2024);
}

TEST(DateTests, ToUs1)
{
    boost::gregorian::date d{ 1995, 4, 5 };
    ASSERT_EQ(DateHelpers::toUSDateString(d), "4/5/1995");
}

TEST(DateTests, ToUs2)
{
    boost::gregorian::date d{ 2015, 10, 14 };
    ASSERT_EQ(DateHelpers::toUSDateString(d), "10/14/2015");
}

TEST(DateTests, ToUs3)
{
    boost::gregorian::date d{ 1995, 4, 5 };
    ASSERT_EQ(DateHelpers::toUSDateString(d, true), "04/05/1995");
}

TEST(DateTests, ToIso1)
{
    ASSERT_EQ(DateHelpers::toIsoDateString("10/14/2015"), "20151014");
}

TEST(DateTests, ToIso2)
{
    ASSERT_EQ(DateHelpers::toIsoDateString("4/5/1995"), "19950405");
}