using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
using Nickvision.Aura;
using NickvisionMoney.Shared.Helpers;
using NickvisionMoney.Shared.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a transaction is validated
/// </summary>
[Flags]
public enum TransactionCheckStatus
{
    Valid = 1,
    EmptyDescription = 2,
    InvalidAmount = 4,
    InvalidRepeatEndDate = 8,
    CannotAccessReceipt = 16
}

/// <summary>
/// A controller for a TransactionDialog
/// </summary>
public class TransactionDialogController : IDisposable
{
    private bool _disposed;
    private readonly Dictionary<uint, Transaction> _transactions;
    private readonly Dictionary<uint, Group> _groups;

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The transaction represented by the controller
    /// </summary>
    public Transaction Transaction { get; init; }
    /// <summary>
    /// The default color for transactions
    /// </summary>
    public string DefaultTransactionColor { get; init; }
    /// <summary>
    /// The list of tags in the account
    /// </summary>
    public List<string> AccountTags { get; init; }
    /// <summary>
    /// Whether or not this transaction can be copied
    /// </summary>
    public bool CanCopy { get; init; }
    /// <summary>
    /// Whether or not the dialog is editing a transaction
    /// </summary>
    public bool IsEditing { get; init; }
    /// <summary>
    /// Whether or not there was a request to make a copy of transaction
    /// </summary>
    public bool CopyRequested { get; set; }
    /// <summary>
    /// The original repeat interval of a transaction
    /// </summary>
    public TransactionRepeatInterval OriginalRepeatInterval { get; init; }
    /// <summary>
    /// The CultureInfo to use when displaying a number string
    /// </summary>
    public CultureInfo CultureForNumberString { get; init; }

    /// <summary>
    /// Whether to use native digits
    /// </summary>
    public bool UseNativeDigits => Models.Configuration.Current.UseNativeDigits; // Full name is required to avoid error because of ambiguous reference (there's also SixLabors.ImageSharp.Configuration)
    /// <summary>
    /// Decimal Separator Inserting
    /// <summary>
    public InsertSeparator InsertSeparator => Models.Configuration.Current.InsertSeparator; // Full name is required to avoid error because of ambiguous reference (there's also SixLabors.ImageSharp.Configuration)
    /// <summary>
    /// The list of group names
    /// </summary>
    public List<string> GroupNames => _groups.Values.OrderBy(x => x.Name == _("Ungrouped") ? " " : x.Name).Select(x => x.Name).ToList();

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="transaction">The Transaction object represented by the controller</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="accountTags">The list of tags in the account</param>
    /// <param name="transactions">The list of transactions in the account</param>
    /// <param name="canCopy">Whether or not the transaction can be copied</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="cultureNumber">The CultureInfo to use for the amount string</param>
    internal TransactionDialogController(Transaction transaction, Dictionary<uint, Transaction> transactions, Dictionary<uint, Group> groups, List<string> accountTags, bool canCopy, string transactionDefaultColor, CultureInfo cultureNumber)
    {
        _disposed = false;
        DefaultTransactionColor = transactionDefaultColor;
        _transactions = transactions;
        _groups = groups;
        Transaction = (Transaction)transaction.Clone();
        AccountTags = new List<string>(accountTags);
        CanCopy = canCopy;
        IsEditing = canCopy;
        CopyRequested = false;
        OriginalRepeatInterval = Transaction.RepeatInterval;
        CultureForNumberString = cultureNumber;
        if (string.IsNullOrWhiteSpace(Transaction.RGBA))
        {
            Transaction.RGBA = DefaultTransactionColor;
        }
        AccountTags.Remove(AccountTags[0]); //remove untagged
    }

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="id">The id of the new transaction</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="accountTags">The list of tags in the account</param>
    /// <param name="transactions">The list of transactions in the account</param>
    /// <param name="transactionDefaultType">A default type for the transaction</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="cultureNumber">The CultureInfo to use for the amount string</param>
    internal TransactionDialogController(uint id, Dictionary<uint, Transaction> transactions, Dictionary<uint, Group> groups, List<string> accountTags, TransactionType transactionDefaultType, string transactionDefaultColor, CultureInfo cultureNumber)
    {
        _disposed = false;
        DefaultTransactionColor = transactionDefaultColor;
        _transactions = transactions;
        _groups = groups;
        AccountTags = new List<string>(accountTags);
        Transaction = new Transaction(id);
        CanCopy = false;
        IsEditing = false;
        CopyRequested = false;
        OriginalRepeatInterval = Transaction.RepeatInterval;
        CultureForNumberString = cultureNumber;
        //Set Defaults For New Transaction
        Transaction.Type = transactionDefaultType;
        Transaction.RGBA = DefaultTransactionColor;
        AccountTags.Remove(AccountTags[0]); //remove untagged
    }

    /// <summary>
    /// Finalizes the TransactionDialogController
    /// </summary>
    ~TransactionDialogController() => Dispose(false);

    /// <summary>
    /// The repeat interval index used by GUI
    /// </summary>
    public uint RepeatIntervalIndex => (uint)Transaction.RepeatInterval switch
    {
        0 => (uint)Transaction.RepeatInterval,
        1 => (uint)Transaction.RepeatInterval,
        2 => (uint)Transaction.RepeatInterval,
        7 => 3,
        _ => (uint)Transaction.RepeatInterval + 1
    };

    /// <summary>
    /// Frees resources used by the TransactionDialogController object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the TransactionDialogController object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            if (IsEditing)
            {
                Transaction.Dispose();
            }
            var jpgPath = $"{UserDirectories.ApplicationCache}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
            if (File.Exists(jpgPath))
            {
                File.Delete(jpgPath);
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Gets a list of suggestions to finish a description
    /// </summary>
    /// <param name="description">The description to get suggestions for</param>
    /// <returns>The list of suggestions and their subtext and transactions</returns>
    public List<(string, string, Transaction)> GetDescriptionSuggestions(string description)
    {
        return _transactions
            .Where(x => FuzzySharp.Fuzz.PartialRatio(x.Value.Description.ToLower().Normalize(NormalizationForm.FormKD), description.ToLower().Normalize(NormalizationForm.FormKD)) > 75)
            .GroupBy(x => x.Value.Description.ToLower())
            .Select(x =>
            {
                var first = x.FirstOrDefault(y => y.Value.GroupId != -1, x.First()).Value;
                return (first.Description, first.GroupId != -1 ? $"{_("Group")}: {_groups[(uint)first.GroupId].Name}" : "", first);
            })
            .OrderByDescending(x => FuzzySharp.Fuzz.PartialRatio(x.Item1.ToLower().Normalize(NormalizationForm.FormKD), description.ToLower().Normalize(NormalizationForm.FormKD)))
            .Take(5).ToList();
    }

    /// <summary>
    /// Gets the name of a group from a group id
    /// </summary>
    /// <param name="id">The id of the group</param>
    /// <returns>The name of the group</returns>
    public string GetGroupNameFromId(uint id)
    {
        try
        {
            return _groups[id].Name;
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Gets an Image object for a file
    /// </summary>
    /// <param name="path">The image file path</param>
    /// <returns>Image?</returns>
    public async Task<Image?> GetImageFromPathAsync(string? path)
    {
        if (File.Exists(path))
        {
            if (Path.GetExtension(path).ToLower() == ".jpeg" || Path.GetExtension(path).ToLower() == ".jpg" || Path.GetExtension(path).ToLower() == ".png")
            {
                return await Image.LoadAsync(path);
            }
            if (Path.GetExtension(path).ToLower() == ".pdf")
            {
                using var library = DocLib.Instance;
                using var docReader = library.GetDocReader(path, new PageDimensions(1080, 1920));
                using var pageReader = docReader.GetPageReader(0);
                return Image.LoadPixelData<Bgra32>(pageReader.GetImage(new NaiveTransparencyRemover(255, 255, 255)), pageReader.GetPageWidth(), pageReader.GetPageHeight());
            }
        }
        return null;
    }

    /// <summary>
    /// Opens the receipt image of the transaction in the default viewer application
    /// </summary>
    public async Task OpenReceiptImageAsync()
    {
        if (Transaction.Receipt != null)
        {
            var jpgPath = $"{UserDirectories.ApplicationCache}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
            await Transaction.Receipt.SaveAsJpegAsync(jpgPath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("explorer", $"\"{jpgPath}\"") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo("xdg-open", $"\"{jpgPath}\""));
            }
        }
    }

    /// <summary>
    /// Updates the Transaction object
    /// </summary>
    /// <param name="date">The new DateOnly object</param>
    /// <param name="description">The new description</param>
    /// <param name="type">The new TransactionType</param>
    /// <param name="selectedRepeat">The new selected repeat index</param>
    /// <param name="groupName">The new Group name</param>
    /// <param name="rgba">The new rgba string</param>
    /// <param name="useGroupColor">Whether or not to use the group's color instead of the transaction's color</param>
    /// <param name="tags">List of transaction tags</param>
    /// <param name="amountString">The new amount string</param>
    /// <param name="receipt">The new receipt image</param>
    /// <param name="repeatEndDate">The new repeat end date DateOnly object</param>
    /// <param name="notes">The new notes</param>
    /// <returns>TransactionCheckStatus</returns>
    public TransactionCheckStatus UpdateTransaction(DateOnly date, string description, TransactionType type, int selectedRepeat, string groupName, string rgba, bool useGroupColor, List<string> tags, string amountString, Image? receipt, DateOnly? repeatEndDate, string notes)
    {
        TransactionCheckStatus result = 0;
        var amount = 0m;
        if (string.IsNullOrWhiteSpace(description))
        {
            result |= TransactionCheckStatus.EmptyDescription;
        }
        try
        {
            amount = decimal.Parse(amountString.ReplaceNativeDigits(CultureForNumberString), NumberStyles.Currency, CultureForNumberString.NumberFormat);
        }
        catch
        {
            result |= TransactionCheckStatus.InvalidAmount;
        }
        if (amount <= 0)
        {
            result |= TransactionCheckStatus.InvalidAmount;
        }
        if (repeatEndDate.HasValue && repeatEndDate.Value <= date)
        {
            result |= TransactionCheckStatus.InvalidRepeatEndDate;
        }
        if (result != 0)
        {
            return result;
        }
        Transaction.Date = date;
        Transaction.Description = description;
        Transaction.Type = type;
        if (selectedRepeat == 3)
        {
            selectedRepeat = 7;
        }
        else if (selectedRepeat > 3)
        {
            selectedRepeat -= 1;
        }
        Transaction.RepeatInterval = (TransactionRepeatInterval)selectedRepeat;
        Transaction.Amount = amount;
        Transaction.GroupId = groupName == _("Ungrouped") ? -1 : (int)_groups.FirstOrDefault(x => x.Value.Name == groupName).Key;
        Transaction.RGBA = rgba;
        Transaction.UseGroupColor = useGroupColor;
        Transaction.Tags = tags;
        if (Transaction.Receipt != receipt)
        {
            Transaction.Receipt?.Dispose();
            Transaction.Receipt = receipt;
        }
        if (Transaction.RepeatInterval == TransactionRepeatInterval.Never)
        {
            Transaction.RepeatFrom = -1;
        }
        else if (Transaction.RepeatInterval != OriginalRepeatInterval)
        {
            Transaction.RepeatFrom = 0;
        }
        Transaction.RepeatEndDate = Transaction.RepeatInterval == TransactionRepeatInterval.Never ? null : repeatEndDate;
        Transaction.Notes = notes;
        return TransactionCheckStatus.Valid;
    }
}
