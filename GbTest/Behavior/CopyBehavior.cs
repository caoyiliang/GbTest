using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace GbTest.Behavior
{
    internal class CopyBehavior : Behavior<TextBlock>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            base.OnAttached();
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetDataObject(((TextBlock)sender).Text);
                MessageBox.Show("已复制");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
