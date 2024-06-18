#ifndef ACCOUNTPAGE_H
#define ACCOUNTPAGE_H

#include <memory>
#include <string>
#include <adwaita.h>
#include "controllers/accountviewcontroller.h"

namespace Nickvision::Money::GNOME::Views
{
    /**
     * @brief A page for displaying an account
     */
    class AccountPage
    {
    public:
        /**
         * @brief Constructs an AccountPage
         * @param controller The AccountViewController
         * @param parent The parent window
         */
        AccountPage(const std::shared_ptr<Shared::Controllers::AccountViewController>& controller, GtkWindow* parent);
        /**
         * @brief Destructs an AccountView
         */
        ~AccountPage();
        /**
         * @brief Gets the gobj of the control
         * @return AdwViewStack*
         */
        AdwViewStack* gobj();
        /**
         * @brief Gets the title of the page
         * @return The page title
         */
        const std::string& getTitle() const;

    private:
        /**
         * @brief Opens the TransactionDialog to create a new transaction.
         */
        void newTransaction();
        /**
         * @brief Opens the GroupDialog to create a new group.
         */
        void newGroup();
        /**
         * @brief Opens the TransferDialog.
         */
        void transfer();
        /**
         * @brief Prompts the user to select a file to import.
         */
        void importFromFile();
        /**
         * @brief Prompts the user to export to a CSV file.
         */
        void exportToCSV();
        /**
         * @brief Prompts the user to export to a PDF file.
         */
        void exportToPDF();
        /**
         * @brief Opens the AccountSettingsDialog.
         */
        void accountSettings();
        std::shared_ptr<Shared::Controllers::AccountViewController> m_controller;
        GtkBuilder* m_builder;
        GtkWindow* m_parent;
        AdwViewStack* m_page;
        GSimpleActionGroup* m_actionGroup;
    };
}

#endif //ACCOUNTPAGE_H
