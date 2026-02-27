using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace ToolKHBrowser.Views.Controls
{
    public static class DashboardPanelBindingHelper
    {
        public static void BindToggleTargets(FrameworkElement panelRoot, FrameworkElement owner)
        {
            if (panelRoot == null || owner == null)
            {
                return;
            }

            foreach (ToggleButton panelToggle in FindPanelToggles(panelRoot))
            {
                if (!(panelToggle.Tag is string targetName) || string.IsNullOrWhiteSpace(targetName))
                {
                    continue;
                }

                if (!(owner.FindName(targetName) is ToggleButton sourceToggle))
                {
                    continue;
                }

                BindingOperations.SetBinding(
                    panelToggle,
                    ToggleButton.IsCheckedProperty,
                    new Binding(nameof(ToggleButton.IsChecked))
                    {
                        Source = sourceToggle,
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    });
            }
        }

        private static IEnumerable<ToggleButton> FindPanelToggles(FrameworkElement panelRoot)
        {
            if (panelRoot == null)
            {
                return Enumerable.Empty<ToggleButton>();
            }

            // Logical tree works even when a UserControl is collapsed/not yet rendered.
            var logical = FindLogicalChildren<ToggleButton>(panelRoot).ToList();
            if (logical.Count > 0)
            {
                return logical;
            }

            // Fallback for templates/controls that only appear in the visual tree.
            return FindVisualChildren<ToggleButton>(panelRoot);
        }

        private static IEnumerable<T> FindLogicalChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                yield break;
            }

            foreach (object child in LogicalTreeHelper.GetChildren(dependencyObject))
            {
                if (!(child is DependencyObject childObject))
                {
                    continue;
                }

                if (childObject is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (T nested in FindLogicalChildren<T>(childObject))
                {
                    yield return nested;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
            {
                yield break;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, childIndex);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (T nestedChild in FindVisualChildren<T>(child))
                {
                    yield return nestedChild;
                }
            }
        }
    }
}
