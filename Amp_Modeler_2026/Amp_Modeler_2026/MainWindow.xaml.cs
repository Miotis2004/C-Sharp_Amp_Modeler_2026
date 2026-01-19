using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Amp_Modeler_2026
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ViewModels.MainViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new ViewModels.MainViewModel();
            // WinUI 3 doesn't automatically inherit DataContext to the Window content in the same way as WPF sometimes implies,
            // but setting it on the Root element (Grid usually) is safest, or via x:Bind to {x:Bind ViewModel}.
            // For simple binding:
            ((FrameworkElement)Content).DataContext = ViewModel;

            this.Closed += (s, e) => ViewModel.Dispose();
        }
    }
}
