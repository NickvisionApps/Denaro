using Docnet.Core;
using Docnet.Core.Converters;
using Docnet.Core.Models;
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
using System.Threading.Tasks;

namespace NickvisionMoney.Shared.Controllers;

/// <summary>
/// Statuses for when a transaction is validated
/// </summary>
public enum TransactionCheckStatus
{
    Valid = 0,
    EmptyDescription,
    InvalidAmount
}

/// <summary>
/// A controller for a TransactionDialog
/// </summary>
public class TransactionDialogController : IDisposable
{
    /// <summary>
    /// The localizer to get translated strings from
    /// </summary>
    public Localizer Localizer { get; init; }
    /// <summary>
    /// The transaction represented by the controller
    /// </summary>
    public Transaction Transaction { get; init; }
    /// <summary>
    /// The groups in the account
    /// </summary>
    public Dictionary<uint, string> Groups { get; init; }
    /// <summary>
    /// Whether or not the dialog was accepted (response)
    /// </summary>
    public bool Accepted { get; set; }
    /// <summary>
    /// A default color for the transaction
    /// </summary>
    public string TransactionDefaultColor { get; init; }

    /// <summary>
    /// Constructs a TransactionDialogController
    /// </summary>
    /// <param name="transaction">The Transaction object represented by the controller</param>
    /// <param name="groups">The list of groups in the account</param>
    /// <param name="transactionDefaultColor">A default color for the transaction</param>
    /// <param name="localizer">The Localizer of the app</param>
    public TransactionDialogController(Transaction transaction, Dictionary<uint, string> groups, string transactionDefaultColor, Localizer localizer)
    {
        Localizer = localizer;
        Transaction = transaction;
        Groups = groups;
        Accepted = false;
        TransactionDefaultColor = transactionDefaultColor;
    }

    /// <summary>
    /// Frees resources used by the TransactionDialogController object
    /// </summary>
    public void Dispose()
    {
        var jpgPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
        if (Path.Exists(jpgPath))
        {
            File.Delete(jpgPath);
        }
    }

    /// <summary>
    /// Opens the receipt image of the transaction in the default viewer application
    /// </summary>
    /// <param name="receiptPath">A possible path of a new receipt image</param>
    public async Task OpenReceiptImageAsync(string? receiptPath = null)
    {
        var image = default(Image);
        if (receiptPath != null)
        {
            if (Path.Exists(receiptPath))
            {
                if (Path.GetExtension(receiptPath) == ".jpeg" || Path.GetExtension(receiptPath) == ".jpg")
                {
                    image = await Image.LoadAsync(receiptPath);
                }
                else if (Path.GetExtension(receiptPath) == ".pdf")
                {
                    image = ConvertPDFToJPEG(receiptPath);
                }
                else
                {
                    image = null;
                }
            }
            else
            {
                image = null;
            }
        }
        else
        {
            image = Transaction.Receipt;
        }
        if(image != null)
        {
            var jpgPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}{Path.DirectorySeparatorChar}Denaro_ViewReceipt_TEMP.jpg";
            await image.SaveAsJpegAsync(jpgPath);
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("explorer", $"\"{jpgPath}\"") { CreateNoWindow = true });
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start(new ProcessStartInfo("xdg-open", jpgPath));
            }
        }
    }

    /// <summary>
    /// Updates the Transaction object
    /// </summary>
    /// <param name="date">The new DateOnly object</param>
    /// <param name="description">The new description</param>
    /// <param name="type">The new TransactionType</param>
    /// <param name="repeat">The new TransactionRepeatInterval</param>
    /// <param name="groupName">The new Group name</param>
    /// <param name="rgba">The new rgba string</param>
    /// <param name="amountString">The new amount string</param>
    /// <param name="receiptPath">The new receipt image path</param>
    /// <returns>TransactionCheckStatus</returns>
    public async Task<TransactionCheckStatus> UpdateTransactionAsync(DateOnly date, string description, TransactionType type, TransactionRepeatInterval repeat, string groupName, string rgba, string amountString, string? receiptPath)
    {
        var amount = 0m;
        if(string.IsNullOrEmpty(description))
        {
            return TransactionCheckStatus.EmptyDescription;
        }
        try
        {
            amount = decimal.Parse(amountString, NumberStyles.Currency);
        }
        catch
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        if (amount <= 0)
        {
            return TransactionCheckStatus.InvalidAmount;
        }
        Transaction.Date = date;
        Transaction.Description = description;
        Transaction.Type = type;
        Transaction.RepeatInterval = repeat;
        Transaction.Amount = amount;
        Transaction.GroupId = groupName == "Ungrouped" ? -1 : (int)Groups.FirstOrDefault(x => x.Value == groupName).Key;
        Transaction.RGBA = rgba;
        if(receiptPath != null)
        {
            if (Path.Exists(receiptPath))
            {
                if (Path.GetExtension(receiptPath) == ".jpeg" || Path.GetExtension(receiptPath) == ".jpg")
                {
                    Transaction.Receipt = await Image.LoadAsync(receiptPath);
                }
                else if (Path.GetExtension(receiptPath) == ".pdf")
                {
                    Transaction.Receipt = ConvertPDFToJPEG(receiptPath);
                }
                else
                {
                    Transaction.Receipt = null;
                }
            }
            else
            {
                Transaction.Receipt = null;
            }
        }
        return TransactionCheckStatus.Valid;
    }

    /// <summary>
    /// Converts a PDF
    /// </summary>
    /// <param name="pathToPDF"></param>
    /// <returns></returns>
    private Image ConvertPDFToJPEG(string pathToPDF)
    {
        using var library = DocLib.Instance;
        using var docReader = library.GetDocReader(pathToPDF, new PageDimensions(1080, 1920));
        using var pageReader = docReader.GetPageReader(0);
        return Image.LoadPixelData<Bgra32>(pageReader.GetImage(new NaiveTransparencyRemover(120, 120, 0)), pageReader.GetPageWidth(), pageReader.GetPageHeight());
    }
}
