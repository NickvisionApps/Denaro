#include "helpers/datehelpers.h"
#include <iomanip>
#include <sstream>
#include <libnick/helpers/stringhelpers.h>

namespace Nickvision::Money::Shared
{
    boost::gregorian::date DateHelpers::fromUSDateString(const std::string& date)
    {
        std::vector<std::string> splits { StringHelpers::split<std::string>(date, "/") };
        if(splits.size() != 3)
        {
            return {};
        }
        return { static_cast<unsigned short>(std::stoul(splits[2])), static_cast<unsigned short>(std::stoul(splits[0])), static_cast<unsigned short>(std::stoul(splits[1])) };
    }

    std::string DateHelpers::toUSDateString(const boost::gregorian::date& date, bool pad)
    {
        if(date.is_not_a_date())
        {
            return "";
        }
        std::stringstream builder;
        if(pad)
        {
            builder << std::setfill('0') << std::setw(2);
        }
        builder << date.month().as_number() << "/";
        if(pad)
        {
            builder << std::setfill('0') << std::setw(2);
        }
        builder << date.day() << "/";
        if(pad)
        {
            builder << std::setfill('0') << std::setw(4);
        }
        builder << date.year();
        return builder.str();
    }

    std::string DateHelpers::toIsoDateString(const std::string& date)
    {
        if(!date.empty())
        {
            std::vector<std::string> splits { StringHelpers::split<std::string>(date, "/") };
            if(splits.size() == 3)
            {
                std::stringstream builder;
                builder << std::setfill('0') << std::setw(4) << splits[2];
                builder << std::setfill('0') << std::setw(2) << splits[0];
                builder << std::setfill('0') << std::setw(2) << splits[1];
                return builder.str();
            }
        }
        return "00000000";
    }
}