#include <memory>
#include <curlpp/cURLpp.hpp>
#include <gtkmm.h>
#include "views/mainwindow.h"

int main(int argc, char* argv[])
{
    std::shared_ptr<Gtk::Application> application = Gtk::Application::create("org.nickvision.money");
    cURLpp::initialize();
    int result = application->make_window_and_run<NickvisionMoney::Views::MainWindow>(argc, argv);
    cURLpp::terminate();
    return result;
}
