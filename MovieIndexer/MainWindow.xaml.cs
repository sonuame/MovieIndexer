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

namespace MovieIndexer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        mDatabaseEntities db = new mDatabaseEntities();
        List<ScanPath> ScanPaths = new List<ScanPath>();
        public MainWindow()
        {
            InitializeComponent();
            ScanPaths = db.ScanPaths.ToList();
        }

        private void btnUpdateDB_Click(object sender, RoutedEventArgs e)
        {
            List<Movy> Movies = new List<Movy>();
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

            grdMovies.AutoGenerateColumns = true;
            grdMovies.ItemsSource = Movies;


        }
    }
}
