using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Models;
using System;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The ShortcutsDialog for the application
/// </summary>
public class ShortcutsDialog
{
    private readonly Gtk.Builder _builder;
    private readonly Gtk.ShortcutsWindow _window;

    public ShortcutsDialog(MainWindowController controller, Gtk.Window parent)
    {
    	string xml = $@"<?xml version='1.0' encoding='UTF-8'?>
    <interface>
        <object class='GtkShortcutsWindow' id='dialog'>
            <property name='default-width'>600</property>
            <property name='default-height'>500</property>
            <property name='modal'>true</property>
            <property name='resizable'>true</property>
            <property name='destroy-with-parent'>false</property>
            <property name='hide-on-close'>true</property>
            <child>
                <object class='GtkShortcutsSection'>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ controller.Localizer["Account"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["NewAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;N</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["OpenAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;O</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["CloseAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;W</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ controller.Localizer["AccountActions"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["Transfer"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;T</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["Export"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;E</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["Import"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;I</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ controller.Localizer["Group"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["NewGroup"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;G</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ controller.Localizer["Transaction"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["NewTransaction"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;&lt;Shift&gt;N</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ controller.Localizer["Application"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["Preferences"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;comma</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ controller.Localizer["KeyboardShortcuts"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;question</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ string.Format(controller.Localizer["About"], controller.AppInfo.ShortName) }</property>
                                    <property name='accelerator'>F1</property>
                                </object>
                            </child>
                        </object>
                    </child>
                </object>
            </child>
        </object>
    </interface>";

        _builder = Gtk.Builder.NewFromString(xml, -1);
        _window = (Gtk.ShortcutsWindow)_builder.GetObject("dialog");
        _window.SetTransientFor(parent);
    }

    public void Show() => _window.Show();
}