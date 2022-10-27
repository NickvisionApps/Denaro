#pragma once

#include <string>
#include "../models/account.hpp"

namespace NickvisionMoney::Controllers
{
	class AccountViewController
	{
	public:
		AccountViewController(const std::string& path);
		const std::string& getAccountPath() const;

	private:
		NickvisionMoney::Models::Account m_account;
	};
}