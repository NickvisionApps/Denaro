#include <gtest/gtest.h>
#include "models/color.h"

using namespace Nickvision::Money::Shared::Models;

TEST(ColorTests, Color1)
{
    Color c{ "rgb(123,234,3)" };
    ASSERT_TRUE(c);
    ASSERT_EQ(c.getR(), 123);
    ASSERT_EQ(c.getG(), 234);
    ASSERT_EQ(c.getB(), 3);
    ASSERT_EQ(c.getA(), 255);
}

TEST(ColorTests, Color2)
{
    Color c{ "123,234,3" };
    ASSERT_TRUE(c);
    ASSERT_EQ(c.getR(), 123);
    ASSERT_EQ(c.getG(), 234);
    ASSERT_EQ(c.getB(), 3);
    ASSERT_EQ(c.getA(), 255);
}

TEST(ColorTests, Color3)
{
    Color c{ "rgba(56,126,206,100)" };
    ASSERT_TRUE(c);
    ASSERT_EQ(c.getR(), 56);
    ASSERT_EQ(c.getG(), 126);
    ASSERT_EQ(c.getB(), 206);
    ASSERT_EQ(c.getA(), 100);
}

TEST(ColorTests, Color4)
{
    Color c{ "56,126,206,100" };
    ASSERT_TRUE(c);
    ASSERT_EQ(c.getR(), 56);
    ASSERT_EQ(c.getG(), 126);
    ASSERT_EQ(c.getB(), 206);
    ASSERT_EQ(c.getA(), 100);
}

TEST(ColorTests, Color5)
{
    Color c{ "#96f542aa" };
    ASSERT_TRUE(c);
    ASSERT_EQ(c.getR(), 150);
    ASSERT_EQ(c.getG(), 245);
    ASSERT_EQ(c.getB(), 66);
    ASSERT_EQ(c.getA(), 170);
}

TEST(ColorTests, Color6)
{
    Color c{ "#96f542uu" };
    ASSERT_FALSE(c);
}

TEST(ColorTests, Color7)
{
    Color c{ "270,100,6" };
    ASSERT_FALSE(c);
}

TEST(ColorTests, RGBAString1)
{
    Color c{ "#96f542aa" };
    ASSERT_EQ(c.toRGBAString(true), "rgba(150,245,66,170)");
}

TEST(ColorTests, RGBHexString1)
{
    Color c{ "#96f542aa" };
    ASSERT_EQ(c.toRGBHexString(), "#96f542");
}