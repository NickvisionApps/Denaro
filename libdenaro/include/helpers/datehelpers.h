#ifndef DATEHELPERS_H
#define DATEHELPERS_H

#include <string>
#include <boost/date_time/gregorian/gregorian.hpp>

namespace Nickvision::Money::Shared::DateHelpers
{
    /**
     * @brief Parses a US date string (MM/DD/YYYY) into a date object.
     * @param date The US date string
     * @return The parsed date object
     */
    boost::gregorian::date fromUSDateString(const std::string& date);
    /**
     * @brief Gets a US date string (MM/DD/YYYY) representation of the date object.
     * @param date The date object
     * @return The US date string representation
     */
    std::string toUSDateString(const boost::gregorian::date& date);
    /**
     * @brief Gets an ISO date string (YYYYMMDD) from a US date string (MM/DD/YYYY).
     * @param date The US date string
     * @return The ISO date string 
     */
    std::string toIsoDateString(const std::string& date);
}

#endif //DATEHELPERS_H