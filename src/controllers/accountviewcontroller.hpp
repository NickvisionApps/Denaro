#pragma once

#include <string>
#include "transactiondialogcontroller.hpp"
#include "../models/account.hpp"
#include "../models/transaction.hpp"

namespace NickvisionMoney::Controllers
{
	class AccountViewController
	{
	public:
		AccountViewController(const std::string& path, const std::string& currencySymbol);
		const std::string& getAccountPath() const;
		std::string getAccountTotalString() const;
		std::string getAccountIncomeString() const;
		std::string getAccountExpenseString() const;
		TransactionDialogController createTransactionDialogController() const;
		TransactionDialogController createTransactionDialogController(const NickvisionMoney::Models::Transaction& transaction) const;

	private:
		std::string m_currencySymbol;
		NickvisionMoney::Models::Account m_account;
	};
}