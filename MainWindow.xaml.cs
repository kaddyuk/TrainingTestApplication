using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrainingTestApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> ExpandedExpanders { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ExpandedExpanders = new List<string>();
        }


        private void expander_OnExpandedStateChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is Expander expander)) return;

            var cvg = expander.DataContext as CollectionViewGroup;

            string viewGroupId = FormViewGroupIdentifier(cvg, null);

            switch (e.RoutedEvent.Name)
            {
                case "Expanded":
                    ExpandedExpanders.Add(viewGroupId);
                    break;
                case "Collapsed":
                    ExpandedExpanders.Remove(viewGroupId);
                    break;
            }
        }

        public static string FormViewGroupIdentifier(CollectionViewGroup collectionViewGroup, string sufix)
        {
            string formViewGroupIdentifier = collectionViewGroup.Name + sufix;
            CollectionViewGroup parentgroup = GetParent(collectionViewGroup);
            if (parentgroup == null)
            {
                return formViewGroupIdentifier;
            }
            else
            {
                return FormViewGroupIdentifier(parentgroup, "@" + formViewGroupIdentifier);
            }
        }

        private static CollectionViewGroup GetParent(CollectionViewGroup collectionViewGroup)
        {
            Type type = collectionViewGroup.GetType();
            if (type.Name == "CollectionViewGroupRoot")
            {//if we are at the root level return null as there is no parent
                return null;
            }

            CollectionViewGroup parentgroup
                = type.GetProperty("Parent", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(collectionViewGroup, null) as CollectionViewGroup;
            return parentgroup;
        }
    }
}
