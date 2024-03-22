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
         * @brief Creates a new PreferencesDialog.
         * @param controller The PreferencesViewController
         * @return PreferencesDialog* (The caller is NOT responsible for deleting the returned pointer)
         */
        static PreferencesDialog* create(const std::shared_ptr<Shared::Controllers::PreferencesViewController>& controller);
        /**
         * @brief Destructs a PreferencesDialog.
         */
        ~PreferencesDialog();
        /**
         * @brief Presents the PreferencesDialog.
         * @param parent The GtkWindow object of the parent window
         */
        void present(GtkWindow* parent) const;

    private:
        /**
         * @brief Constructs a PreferencesDialog.
         * @param controller The PreferencesViewController
         */
        PreferencesDialog(const std::shared_ptr<Shared::Controllers::PreferencesViewController>& controller);
        /**
         * @brief Handles when the dialog is closed. 
         */
        void onClosed();
        /**
         * @brief Applies the changes to the app's configuration object.
         */
        void applyChanges();
        /**
         * @brief Handles when the theme preference is changed. 
         */
        void onThemeChanged();
        std::shared_ptr<Shared::Controllers::PreferencesViewController> m_controller;
        GtkBuilder* m_builder;
        AdwPreferencesDialog* m_dialog;
    };
}

#endif //PREFERENCESDIALOG_H