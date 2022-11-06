project('org.nickvision.money', ['cpp', 'c'], version: '2022.11.0', meson_version: '>= 0.63.0', default_options: ['warning_level=3', 'werror=false', 'cpp_std=c++20'])

gnome = import('gnome')

threads = dependency('Threads')
gtk = dependency('gtk4', version: '>= 4.8.0')
adwaita = dependency('libadwaita-1', version: '>= 1.2.0')
jsoncpp = dependency('jsoncpp')
dl = dependency('dl')
sqlitecpp = dependency('SQLiteCpp')
boost = dependency('boost')

subdir('src')
subdir('po')

executable('org.nickvision.money', sources, dependencies: [threads, adwaita, jsoncpp, dl, sqlitecpp, boost], install: true)
install_data(resources, install_dir: 'share/icons/hicolor/scalable/apps')
install_data(resources_symbolic, install_dir: 'share/icons/hicolor/symbolic/apps')
install_data(resources_actions, install_dir: 'share/icons/hicolor/scalable/actions')
install_data('org.nickvision.money.desktop', install_dir: 'share/applications')
install_data('org.nickvision.money.metainfo.xml', install_dir: 'share/metainfo')
gnome.post_install(gtk_update_icon_cache: true, update_desktop_database: true)