using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using DesktopAppVendingMachines.ViewModels;

namespace DesktopAppVendingMachines
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var fullName = data.GetType().FullName!;
            var viewName = fullName
                .Replace(".ViewModels.", ".Views.")
                .Replace("ViewModel", "View");

            var type = Type.GetType(viewName);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + viewName };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}