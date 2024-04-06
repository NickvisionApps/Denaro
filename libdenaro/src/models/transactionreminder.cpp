#include "models/transactionreminder.h"
#include <format>
#include <libnick/localization/gettext.h>
#include "helpers/currencyhelpers.h"

using namespace Nickvision::Money::Shared;

namespace Nickvision::Money::Shared::Models
{
    TransactionReminder::TransactionReminder(const std::string& description, const boost::gregorian::date& when)
        : m_description{ description },
        m_amount{ 0 },
        m_amountString{ "0" }
    {
        setWhen(when);
    }

    const std::string& TransactionReminder::getDescription() const
    {
        return m_description;
    }

    void TransactionReminder::setDescription(const std::string& description)
    {
        m_description = description;
    }

    double TransactionReminder::getAmount() const
    {
        return m_amount;
    }

    const std::string& TransactionReminder::getAmountString() const
    {
        return m_amountString;
    }

    void TransactionReminder::setAmount(double amount, TransactionType type, const Currency& currency)
    {
        m_amount = type == TransactionType::Income ? amount : amount * -1;
        m_amountString = CurrencyHelpers::toAmountString(m_amount, currency);
    }

    const boost::gregorian::date& TransactionReminder::getWhen() const
    {
        return m_when;
    }

    const std::string& TransactionReminder::getWhenString() const
    {
        return m_whenString;
    }

    void TransactionReminder::setWhen(const boost::gregorian::date& when)
    {
        m_when = when;
        boost::gregorian::date today{ boost::gregorian::day_clock::local_day() };
        if(m_when == today + boost::gregorian::days{ 1 })
        {
            m_whenString = _("Tomorrow");
        }
        else if(m_when == today + boost::gregorian::weeks{ 1 })
        {
            m_whenString = _("One week from now");
        }
        else if(m_when == today + boost::gregorian::months{ 1 })
        {
            m_whenString = _("One month from now");
        }
        else if(m_when == today + boost::gregorian::months{ 2 })
        {
            m_whenString = _("Two months from now");
        }
        else
        {
            m_whenString = std::vformat(_("{} days from now"), std::make_format_args((m_when - today).days()));
        }
    }
}