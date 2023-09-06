using NickvisionMoney.GNOME.Helpers;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static NickvisionMoney.Shared.Helpers.Gettext;

namespace NickvisionMoney.GNOME.Controls;

public class CurrencyConverterDialog : Adw.Window
{
    private readonly string _iconName;
    private readonly bool _useNativeDigits;
    private string[]? _currencies;
    
    [Gtk.Connect] private readonly Gtk.Box _loadingBox;
    [Gtk.Connect] private readonly Adw.ComboRow _sourceCurrencyRow;
    [Gtk.Connect] private readonly Adw.ComboRow _resultCurrencyRow;
    [Gtk.Connect] private readonly Adw.EntryRow _sourceAmountRow;
    [Gtk.Connect] private readonly Adw.EntryRow _resultAmountRow;
    
    /// <summary>
    /// Constructs a CurrencyConverterDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon for the dialog</param>
    /// <param name="useNativeDigits">Whether or not to use native digits when displaying the result amount</param>
    private CurrencyConverterDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, bool useNativeDigits) : base(builder.GetPointer("_root"), false)
    {
        _iconName = iconName;
        _useNativeDigits = useNativeDigits;
        builder.Connect(this);
        //Dialog Settings
        SetTransientFor(parent);
        SetIconName(_iconName);
        _sourceCurrencyRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _sourceAmountRow.SetTitle(_currencies[_sourceCurrencyRow.GetSelected()]);
            }
        };
        _sourceAmountRow.OnNotify += async (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                await OnAmountRowChangedAsync();
            }
        };
        _resultCurrencyRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected-item")
            {
                _resultAmountRow.SetTitle(_currencies[_resultCurrencyRow.GetSelected()]);
            }
        };
    }
    
    /// <summary>
    /// Constructs a CurrencyConverterDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">The name of the icon for the dialog</param>
    /// <param name="useNativeDigits">Whether or not to use native digits when displaying the result amount</param>
    public CurrencyConverterDialog(Gtk.Window parent, string iconName, bool useNativeDigits) : this(Builder.FromFile("currency_converter_dialog.ui"), parent, iconName, useNativeDigits)
    {
    }

    /// <summary>
    /// Presents the dialog
    /// </summary>
    public async Task PresentAsync()
    {
        base.Present();
        _loadingBox.SetVisible(true);
        _currencies = (await CurrencyConversionService.GetConversionRatesAsync("USD") ?? new Dictionary<string, decimal>()).Keys.ToArray();
        if (_currencies.Length == 0)
        {
            var messageDialog = Adw.MessageDialog.New(this, _("Error"), _("Unable to load currency data. Please try again. If the error still persists, report a bug."));
            messageDialog.SetIconName(_iconName);
            messageDialog.AddResponse("ok", _("OK"));
            messageDialog.SetDefaultResponse("ok");
            messageDialog.SetCloseResponse("ok");
            messageDialog.OnResponse += async (ss, exx) =>
            {
                messageDialog.Destroy();
                Close();
            };
            messageDialog.Present();
        }
        var currenciesList = Gtk.StringList.New(_currencies);
        _loadingBox.SetVisible(false);
        _sourceCurrencyRow.SetModel(currenciesList);
        _resultCurrencyRow.SetModel(currenciesList);
        _resultCurrencyRow.SetSelected(1);
        _sourceAmountRow.SetText("1");
        _sourceAmountRow.SetPosition(-1);
    }

    /// <summary>
    /// Occurs when the source amount row changes
    /// </summary>
    private async Task OnAmountRowChangedAsync()
    {
        if (string.IsNullOrEmpty(_sourceAmountRow.GetText()))
        {
            _resultAmountRow.SetText("");
        }
        else
        {
            _loadingBox.SetVisible(true);
            if (decimal.TryParse(_sourceAmountRow.GetText(), out var amount))
            {
                var res = await CurrencyConversionService.ConvertAsync(_currencies![_sourceCurrencyRow.GetSelected()], amount, _currencies[_resultCurrencyRow.GetSelected()]);
                if (res != null)
                {
                    _resultAmountRow.SetText(res.ResultAmount.ToAmountString(CultureInfo.CurrentCulture, _useNativeDigits, false));
                }
            }
            _loadingBox.SetVisible(false);
        }
    }
}