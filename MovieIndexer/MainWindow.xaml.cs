using System;
using System.Collections.Generic;
using System.Data;
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
using System.IO;
using System.Collections.ObjectModel;

namespace MovieIndexer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        mDatabaseEntities db = new mDatabaseEntities();
        List<ScanPath> ScanPaths = new List<ScanPath>();
        List<Movy> Movies = new List<Movy>();
        
        public MainWindow()
        {
            InitializeComponent();
            ScanPaths = db.ScanPaths.ToList();
            grdMovies.AutoGenerateColumns = true;
            LoadGrid();
        }

        private void btnUpdateDB_Click(object sender, RoutedEventArgs e)
        {
            
            foreach (ScanPath item in ScanPaths)
            {
                Movies.AddRange(Directory.EnumerateDirectories(item.Path, "*.*", SearchOption.TopDirectoryOnly).Select(m=>new Movy() {
                    Id = Movies.Count + 1, 
                    Location = item.Location,
                    Path = m,
                    Title = System.IO.Path.GetFileNameWithoutExtension(m)
                }));
            }

            db.Movies.RemoveRange(db.Movies);
            db.Movies.AddRange(Movies);
            db.SaveChanges();

            LoadGrid();

        }

        private void grdMovies_Loaded(object sender, RoutedEventArgs e)
        {
            lblStat.Content = "Total Movies - " + grdMovies.Items.Count;
        }

        public async void LoadGrid()
        {
            await Task<List<Movy>>.Run(() => {
                Movies = db.Movies.ToList();
            });
            grdMovies.ItemsSource = Movies;
            lblStat.Content = "Total Movies - " + grdMovies.Items.Count;
        }

        private void txtTitle_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                grdMovies.ItemsSource = Movies.Where(m=>m.Title.ToLower().Contains(txtTitle.Text.ToLower()));
            }
        }

        private void grdMovies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Movy m = grdMovies.SelectedItem as Movy;
            System.Diagnostics.Process.Start(m.Path);
        }
    }
}
