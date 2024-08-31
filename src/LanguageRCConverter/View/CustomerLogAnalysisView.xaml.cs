using LanguageRCConverter.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LanguageRCConverter.View
{
    /// <summary>
    /// CustomerLogAnalysis.xaml 的交互逻辑
    /// </summary>
    public partial class CustomerLogAnalysisView : UserControl
    {
        public CustomerLogAnalysisView()
        {
            InitializeComponent();

            this.DataContext = new CustomerLogAnalysisViewModel();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
