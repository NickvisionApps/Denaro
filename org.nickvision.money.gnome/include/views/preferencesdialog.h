#ifndef PREFERENCESDIALOG_H
#define PREFERENCESDIALOG_H

#include <memory>
#include <adwaita.h>
#include "controllers/preferencesviewcontroller.h"

namespace Nickvision::Money::GNOME::Views
{
    /**
     * @brief The preferences dialog for the application. 
     */
    class PreferencesDialog
    {
    public:
        /**
         * @brief Constructs a PreferencesDialog.
         * @param controller The PreferencesViewController
         * @param parent The GtkWindow object of the parent window
         */
        PreferencesDialog(const std::shared_ptr<Shared::Controllers::PreferencesViewController>& controller, GtkWindow* parent);
        /**
         * @brief Destructs a PreferencesDialog.
         */
        ~PreferencesDialog();
        /**
         * @brief Shows the PreferencesDialog and waits for it to close. 
         */
        void run();
        /**
         * @brief Handles when the theme preference is changed. 
         */
        void onThemeChanged();

    private:
        std::shared_ptr<Shared::Controllers::PreferencesViewController> m_controller;
        GtkBuilder* m_builder;
        AdwPreferencesWindow* m_dialog;
    };
}

#endif //PREFERENCESDIALOG_H