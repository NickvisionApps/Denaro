#include "models/color.h"
#include <iomanip>
#include <limits>
#include <sstream>
#include <libnick/helpers/stringhelpers.h>

namespace Nickvision::Money::Shared::Models
{
    Color::Color()
        : m_r{ 0 },
        m_g{ 0 },
        m_b{ 0 },
        m_a{ 0 }
    {
        
    }

    Color::Color(unsigned char r, unsigned char g, unsigned char b)
        : m_r{ r },
        m_g{ g },
        m_b{ b },
        m_a{ 255 }
    {

    }

    Color::Color(unsigned char r, unsigned char g, unsigned char b, unsigned char a)
        : m_r{ r },
        m_g{ g },
        m_b{ b },
        m_a{ a }
    {

    }

    Color::Color(const std::string& s)
        : m_r{ 0 },
        m_g{ 0 },
        m_b{ 0 },
        m_a{ 0 }
    {
        if(s.empty())
        {
            return;
        }
        unsigned char max{ std::numeric_limits<unsigned char>::max() };
        unsigned char min{ std::numeric_limits<unsigned char>::min() };
        if(s[0] == '#')
        {
            if(s.length() == 7) //#rrggbb
            {
                try
                {
                    int r{ std::stoi(s.substr(1, 2), 0, 16) };
                    int g{ std::stoi(s.substr(3, 2), 0, 16) };
                    int b{ std::stoi(s.substr(5, 2), 0, 16) };
                    if(r >= min && r <= max && g >= min && g <= max && b >= min && b <= max)
                    {
                        m_r = static_cast<unsigned char>(r);
                        m_g = static_cast<unsigned char>(g);
                        m_b = static_cast<unsigned char>(b);
                        m_a = 255;
                    }
                }
                catch(...) 
                {  
                    m_r = 0;
                    m_g = 0;
                    m_b = 0;
                    m_a = 0;
                }
            }
            else if(s.length() == 9) //#rrggbbaa
            {
                try
                {
                    int r{ std::stoi(s.substr(1, 2), 0, 16) };
                    int g{ std::stoi(s.substr(3, 2), 0, 16) };
                    int b{ std::stoi(s.substr(5, 2), 0, 16) };
                    int a{ std::stoi(s.substr(7, 2), 0, 16) };
                    if(r >= min && r <= max && g >= min && g <= max && b >= min && b <= max && a >= min && a <= max)
                    {
                        m_r = static_cast<unsigned char>(r);
                        m_g = static_cast<unsigned char>(g);
                        m_b = static_cast<unsigned char>(b);
                        m_a = static_cast<unsigned char>(a);
                    }
                }
                catch(...) 
                {  
                    m_r = 0;
                    m_g = 0;
                    m_b = 0;
                    m_a = 0;
                }
            }
        }
        else
        {
            std::vector<std::string> values{ StringHelpers::split<std::string>(s, ",") };
            if(values.size() == 3)
            {
                if(values[0].substr(0, 4) == "rgb(")
                {
                    values[0] = values[0].substr(4);
                    values[2] = values[2].substr(0, values[2].size() - 1);
                }
                try
                {
                    int r{ std::stoi(values[0]) };
                    int g{ std::stoi(values[1]) };
                    int b{ std::stoi(values[2]) };
                    if(r >= min && r <= max && g >= min && g <= max && b >= min && b <= max)
                    {
                        m_r = static_cast<unsigned char>(r);
                        m_g = static_cast<unsigned char>(g);
                        m_b = static_cast<unsigned char>(b);
                        m_a = 255;
                    }
                }
                catch(...) 
                {  
                    m_r = 0;
                    m_g = 0;
                    m_b = 0;
                    m_a = 0;
                }
            }
            else if(values.size() == 4)
            {
                if(values[0].substr(0, 5) == "rgba(")
                {
                    values[0] = values[0].substr(5);
                    values[3] = values[3].substr(0, values[3].size() - 1);
                }
                try
                {
                    int r{ std::stoi(values[0]) };
                    int g{ std::stoi(values[1]) };
                    int b{ std::stoi(values[2]) };
                    int a{ std::stoi(values[3]) };
                    if(r >= min && r <= max && g >= min && g <= max && b >= min && b <= max && a >= min && a <= max)
                    {
                        m_r = static_cast<unsigned char>(r);
                        m_g = static_cast<unsigned char>(g);
                        m_b = static_cast<unsigned char>(b);
                        m_a = static_cast<unsigned char>(a);
                    }
                }
                catch(...) 
                {  
                    m_r = 0;
                    m_g = 0;
                    m_b = 0;
                    m_a = 0;
                }
            }
        }
    }

    bool Color::empty() const
    {
        return m_r == 0 && m_g == 0 && m_b == 0 && m_a == 0;
    }

    unsigned char Color::getR() const
    {
        return m_r;
    }

    void Color::setR(unsigned char r)
    {
        m_r = r;
    }

    unsigned char Color::getG() const
    {
        return m_g;
    }

    void Color::setG(unsigned char g)
    {
        m_g = g;
    }

    unsigned char Color::getB() const
    {
        return m_b;
    }

    void Color::setB(unsigned char b)
    {
        m_b = b;
    }

    unsigned char Color::getA() const
    {
        return m_a;
    }

    void Color::setA(unsigned char a)
    {
        m_a = a;
    }

    std::string Color::toRGBString(bool header) const
    {
        std::stringstream builder;
        if(header)
        {
            builder << "rgb(";
        }
        builder << +m_r << ",";
        builder << +m_g << ",";
        builder << +m_b;
        if(header)
        {
            builder << ")";
        }
        return builder.str();
    }

    std::string Color::toRGBAString(bool header) const
    {
        std::stringstream builder;
        if(header)
        {
            builder << "rgba(";
        }
        builder << +m_r << ",";
        builder << +m_g << ",";
        builder << +m_b << ",";
        builder << +m_a;
        if(header)
        {
            builder << ")";
        }
        return builder.str();
    }

    std::string Color::toRGBHexString() const
    {
        std::stringstream builder;
        builder << "#";
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_r;
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_g;
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_b;
        return builder.str();
    }

    std::string Color::toRGBAHexString() const
    {
        std::stringstream builder;
        builder << "#";
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_r;
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_g;
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_b;
        builder << std::setfill ('0') << std::setw(2) << std::hex << +m_a;
        return builder.str();
    }

    bool Color::operator==(const Color& compare)
    {
        return m_r == compare.m_r && m_g == compare.m_g && m_b == compare.m_b && m_a == compare.m_a;
    }

    bool Color::operator!=(const Color& compare)
    {
        return !operator==(compare);
    }

    Color::operator bool() const
    {
        return !empty();
    }
}