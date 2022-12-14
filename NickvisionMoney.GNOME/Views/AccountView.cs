using NickvisionMoney.Shared.Controllers;
using NickvisionMoney.Shared.Events;
using System;
using System.IO;
using System.Collections.Generic;

namespace NickvisionMoney.GNOME.Views;

public class AccountView
{
    private readonly Adw.TabPage _page;
    private readonly Adw.Flap _flap;
    private readonly Gtk.ScrolledWindow _scrollPane;
    private readonly Gtk.Box _paneBox;
    private readonly Gtk.Label _lblTotal;
    private readonly Adw.ActionRow _rowTotal;
    private readonly Gtk.Label _lblIncome;
    private readonly Gtk.CheckButton _chkIncome;
    private readonly Adw.ActionRow _rowIncome;
    private readonly Gtk.Label _lblExpense;
    private readonly Gtk.CheckButton _chkExpense;
    private readonly Adw.ActionRow _rowExpense;

    public AccountView(Gtk.Window parentWindow, Adw.TabView parentTabView, Gtk.ToggleButton btnFlapToggle) //AccountViewController controller
    {
        //Flap
        _flap = Adw.Flap.New();
        btnFlapToggle.BindProperty("active", _flap, "reveal-flap", (GObject.BindingFlags.Bidirectional | GObject.BindingFlags.SyncCreate));
        //Left Pane
        _scrollPane = Gtk.ScrolledWindow.New();
        _scrollPane.AddCssClass("background");
        _scrollPane.SetSizeRequest(350, -1);
        _flap.SetFlap(_scrollPane);
        //Pane Box
        _paneBox = Gtk.Box.New(Gtk.Orientation.Vertical, 10);
        _paneBox.SetHexpand(false);
        _paneBox.SetVexpand(true);
        _paneBox.SetMarginTop(10);
        _paneBox.SetMarginStart(10);
        _paneBox.SetMarginEnd(10);
        _paneBox.SetMarginBottom(10);
        _scrollPane.SetChild(_paneBox);
        //Account Total
        _lblTotal = Gtk.Label.New("");
        _lblTotal.SetValign(Gtk.Align.Center);
        _lblTotal.AddCssClass("accent");
        _lblTotal.AddCssClass("money-total");
        _rowTotal = Adw.ActionRow.New();
        //_rowTotal.SetTitle(controller.Localizer["Total"]);
        _rowTotal.AddSuffix(_lblTotal);
        //Account Income
        _lblIncome = Gtk.Label.New("");
        _lblIncome.SetValign(Gtk.Align.Center);
        _lblIncome.AddCssClass("sucess");
        _lblTotal.AddCssClass("money-income");
        _chkIncome = Gtk.CheckButton.New();
        _chkIncome.SetActive(true);
        _chkIncome.AddCssClass("selection-mode");
        //_chkIncome.OnToggled += () => controller.UpdateFilterValue(-3, _chkIncome.GetActive());
        _rowIncome = Adw.ActionRow.New();
        //_rowIncome.SetTitle(controller.Localizer["Income"]);
        _rowIncome.AddSuffix(_lblIncome);
        //Account Expense
        _lblExpense = Gtk.Label.New("");
        _lblExpense.SetValign(Gtk.Align.Center);
        _lblExpense.AddCssClass("error");
        _lblExpense.AddCssClass("money-expense");
        _chkExpense = Gtk.CheckButton.New();
        _chkExpense.SetActive(true);
        _chkExpense.AddCssClass("selection-mode");
        //_chkExpense.OnToggled += () => controller.UpdateFilterValue(-3, _chkExpense.GetActive());
        _rowExpense = Adw.ActionRow.New();
        //_rowExpense.SetTitle(controller.Localizer["Expense"]);
        _rowExpense.AddSuffix(_lblExpense);

        //Tab Page
        _page = parentTabView.Append(_flap);
        //_page.SetTitle(controller.GetAccountPath());
    }

    public Adw.TabPage GetPage() => _page;
}