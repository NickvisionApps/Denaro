#!/bin/bash

INSTALL_PREFIX="/usr"
if [ ! -z $1 ]
then
	INSTALL_PREFIX=$1
fi
echo Install prefix: $INSTALL_PREFIX

SOURCE_PREFIX="."
if [ ${PWD##*/} == "NickvisionMoney.GNOME" ]
then
	SOURCE_PREFIX=".."
fi

echo Installing icons...
mkdir -p $INSTALL_PREFIX/share/icons/hicolor/scalable/apps
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/org.nickvision.money.svg $INSTALL_PREFIX/share/icons/hicolor/scalable/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/org.nickvision.money-devel.svg $INSTALL_PREFIX/share/icons/hicolor/scalable/apps/
mkdir -p $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/bank-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/larger-brush-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/money-none-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/moon-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/org.nickvision.money-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/sun-alt-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/
cp $SOURCE_PREFIX/NickvisionMoney.Shared/Resources/wallet2-symbolic.svg $INSTALL_PREFIX/share/icons/hicolor/symbolic/apps/

echo Installing GResource...
mkdir -p $INSTALL_PREFIX/share/org.nickvision.money
glib-compile-resources $SOURCE_PREFIX/NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource.xml
mv $SOURCE_PREFIX/NickvisionMoney.GNOME/Resources/org.nickvision.money.gresource $INSTALL_PREFIX/share/org.nickvision.money/

echo Installing desktop file...
mkdir -p $INSTALL_PREFIX/share/applications
cp $SOURCE_PREFIX/NickvisionMoney.GNOME/org.nickvision.money.desktop $INSTALL_PREFIX/share/applications/

echo Installing metainfo...
mkdir -p $INSTALL_PREFIX/share/metainfo
cp $SOURCE_PREFIX/NickvisionMoney.GNOME/org.nickvision.money.metainfo.xml $INSTALL_PREFIX/share/metainfo/

echo Done!