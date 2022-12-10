using NickvisionMoney.Shared.Models;
using NickvisionMoney.Shared.Helpers;
using System;

namespace NickvisionMoney.GNOME.Views;

/// <summary>
/// The ShortcutsDialog for the application
/// </summary>
public class ShortcutsDialog
{
    private readonly Gtk.Builder _builder;
    private readonly Gtk.ShortcutsWindow _window;

    public ShortcutsDialog(Localizer localizer, Gtk.Window parent)
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
                            <property name='title'>{ localizer["ShortcutsAccount"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsNewAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;N</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsOpenAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;O</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsCloseAccount"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;W</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["ShortcutsAccountActions"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsTransfer"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;T</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsExport"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;E</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsImport"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;I</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["ShortcutsGroup"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsNewGroup"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;G</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["ShortcutsTransaction"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsNewTransaction"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;&lt;Shift&gt;N</property>
                                </object>
                            </child>
                        </object>
                    </child>
                    <child>
                        <object class='GtkShortcutsGroup'>
                            <property name='title'>{ localizer["ShortcutsApplication"] }</property>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsPreferences"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;comma</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsShortcuts"] }</property>
                                    <property name='accelerator'>&lt;Control&gt;question</property>
                                </object>
                            </child>
                            <child>
                                <object class='GtkShortcutsShortcut'>
                                    <property name='title'>{ localizer["ShortcutsAbout"] }</property>
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
        _window.Show();
    }
}