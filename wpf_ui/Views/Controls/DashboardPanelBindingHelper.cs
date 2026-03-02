using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Media;

namespace ToolKHBrowser.Views.Controls
{
    public static class DashboardPanelBindingHelper
    {
        public static void BindToggleTargets(FrameworkElement panelRoot, FrameworkElement hostRoot)
        {
            if (panelRoot == null || hostRoot == null)
            {
                return;
            }

            panelRoot.ApplyTemplate();
            BindNow(panelRoot, hostRoot);

            // Rebind once after layout to catch toggles in templated/late-loaded visual trees.
            panelRoot.Dispatcher.BeginInvoke(
                new Action(() => BindNow(panelRoot, hostRoot)),
                DispatcherPriority.Loaded);
        }

        private static void BindNow(FrameworkElement panelRoot, FrameworkElement hostRoot)
        {
            foreach (ToggleButton panelToggle in FindToggles(panelRoot))
            {
                if (panelToggle?.Tag is string targetName && !string.IsNullOrWhiteSpace(targetName))
                {
                    if (hostRoot.FindName(targetName) is ToggleButton hostToggle)
                    {
                        BindToggle(panelToggle, hostToggle);
                    }
                }
            }
        }

        private static void BindToggle(ToggleButton source, ToggleButton target)
        {
            try
            {
                BindingOperations.SetBinding(
                    source,
                    ToggleButton.IsCheckedProperty,
                    new Binding(nameof(ToggleButton.IsChecked))
                    {
                        Source = target,
                        Mode = BindingMode.TwoWay
                    });

                BindingOperations.SetBinding(
                    source,
                    UIElement.IsEnabledProperty,
                    new Binding(nameof(UIElement.IsEnabled))
                    {
                        Source = target,
                        Mode = BindingMode.OneWay
                    });

                if (source.ToolTip == null)
                {
                    source.ToolTip = targetNameFallback(target);
                }
            }
            catch (Exception)
            {
            }
        }

        private static string targetNameFallback(FrameworkElement element)
        {
            return string.IsNullOrWhiteSpace(element?.Name) ? "Toggle" : element.Name;
        }

        private static System.Collections.Generic.IEnumerable<ToggleButton> FindToggles(DependencyObject root)
        {
            if (root == null)
            {
                yield break;
            }

            var seen = new HashSet<ToggleButton>();
            foreach (ToggleButton toggle in FindTogglesLogical(root, seen))
            {
                yield return toggle;
            }

            foreach (ToggleButton toggle in FindTogglesVisual(root, seen))
            {
                yield return toggle;
            }
        }

        private static IEnumerable<ToggleButton> FindTogglesLogical(DependencyObject root, HashSet<ToggleButton> seen)
        {
            if (root is ToggleButton directToggle && seen.Add(directToggle))
            {
                yield return directToggle;
            }

            foreach (object childObj in LogicalTreeHelper.GetChildren(root))
            {
                if (childObj is DependencyObject child)
                {
                    if (child is FrameworkElement fe)
                    {
                        fe.ApplyTemplate();
                    }

                    foreach (ToggleButton nested in FindTogglesLogical(child, seen))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private static IEnumerable<ToggleButton> FindTogglesVisual(DependencyObject root, HashSet<ToggleButton> seen)
        {
            if (root is ToggleButton directToggle && seen.Add(directToggle))
            {
                yield return directToggle;
            }

            int childCount = 0;
            try
            {
                childCount = VisualTreeHelper.GetChildrenCount(root);
            }
            catch (Exception)
            {
            }

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                foreach (ToggleButton nested in FindTogglesVisual(child, seen))
                {
                    yield return nested;
                }
            }
        }
    }
}
