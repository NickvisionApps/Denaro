#!/bin/bash

INSTALL_PREFIX="/usr"
if [ ! -z $1 ]
then
	INSTALL_PREFIX=$1
fi
echo Install prefix: $INSTALL_PREFIX

if [ ${PWD##*/} == "NickvisionMoney.GNOME" ]
then
	cd ..
fi

echo Installing icons...
mkdir -p $INSTALL_PREFIX/share/icons/hicolor/scalable/apps
cp ./NickvisionMoney.Shared/Resources/org.nickvision.money.svg $INSTALL_PREFIX/share/icons/hicolor/scalable/apps/
cp ./NickvisionMoney.Shared/Resources/org.nickvision.money-devel.svg $INSTALL_PREFIX/share/icons/hicolor/scalable/apps/
mkdir -p $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps
cp ./NickvisionMoney.Shared/Resources/bank-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp ./NickvisionMoney.Shared/Resources/larger-brush-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp ./NickvisionMoney.Shared/Resources/money-none-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp ./NickvisionMoney.Shared/Resources/moon-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp ./NickvisionMoney.Shared/Resources/org.nickvision.money-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp ./NickvisionMoney.Shared/Resources/sun-alt-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp ./NickvisionMoney.Shared/Resources/wallet2-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/

echo Installing GResource...
mkdir -p $INSTALL_PREFIX/share/org.nickvision.money
glib-compile-resources ./NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource.xml
mv ./NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource $INSTALL_PREFIX/share/org.nickvision.money/

echo Installing desktop file...
mkdir -p $INSTALL_PREFIX/share/applications
cp ./NickvisionMoney.GNOME/org.nickvision.money.desktop $INSTALL_PREFIX/share/applications/

echo Installing metainfo...
mkdir -p $INSTALL_PREFIX/share/metainfo
cp ./NickvisionMoney.GNOME/org.nickvision.money.metainfo.xml $INSTALL_PREFIX/share/metainfo/

echo Installing mime types...
mkdir -p $INSTALL_PREFIX/share/mime/packages
cp ./NickvisionMoney.GNOME/org.nickvision.money.extension.xml $INSTALL_PREFIX/share/mime/packages
update-mime-database $INSTALL_PREFIX/share/mime

echo Done!