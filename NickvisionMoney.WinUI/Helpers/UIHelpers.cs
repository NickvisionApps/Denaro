using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace NickvisionMoney.WinUI.Helpers;

/// <summary>
/// Extension methods for WinUI
/// </summary>
public static class UIHelpers
{
    /// <summary>
    /// Finds children of a certain type in the parent object
    /// </summary>
    /// <typeparam name="T">The type of the children to find</typeparam>
    /// <param name="parent">The parent object</param>
    /// <returns>A list of children found of the provided type</returns>
    public static IEnumerable<T> FindChildrenOfType<T>(this DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T hit)
            {
                yield return hit;
            }
            foreach (T? grandChild in FindChildrenOfType<T>(child))
            {
                yield return grandChild;
            }
        }
    }
}
