using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PoEMonitor.Helpers
{
    public sealed class ScrollIntoViewBehavior : Behavior<DataGrid>
        {
            protected override void OnAttached()
            {
                base.OnAttached();
                AssociatedObject.SelectionChanged += ScrollIntoView;
            }

            protected override void OnDetaching()
            {
                AssociatedObject.SelectionChanged -= ScrollIntoView;
                base.OnDetaching();
            }

            private void ScrollIntoView(object o, SelectionChangedEventArgs e)
            {
                var b = (DataGrid)o;

                if (b == null)
                {
                    return;
                }

                if (b.SelectedItem == null)
                {
                    return;
                }

                b.UpdateLayout();

                b.ScrollIntoView(b.SelectedItem);
            }
    }
}