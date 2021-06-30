using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.SQLite;
using Microsoft.Win32;


namespace LibraryApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SQLiteConnection m_dbConnection;

        public struct Student
        {
            public string Name;
            public DateTime IssueDate;
            public DateTime ReturnDate;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файл базы данных (*.db)  |  *.db;";
            Nullable<bool> result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string db_name = openFileDialog.FileName;

                m_dbConnection = new SQLiteConnection("Data Source=" + db_name + ";Version=3;");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                m_dbConnection.Open();

                // SQL запрос выводит имя одного самого популярного автора.
                // Т.к. в SQLite нет фильтра TOP, нельзя вывести несколько одинкого популярных авторов,
                // используя параметр WITH TIES, используется фильтр LIMIT

                string sql = @"SELECT Author
                               FROM Books
                               JOIN Report ON Books.nBook = Report.nBook
                               WHERE IssueDate BETWEEN '2021-01-01' AND '2022-01-01'
                               GROUP BY Author
                               ORDER BY count(*) DESC
                               LIMIT 1";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                    AutorLabel.Content = "Имя самого популярного автора: " + reader["Author"].ToString();

                reader.Close();
                m_dbConnection.Close();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // "злостным читателем" является студент, не сдававший книгу наибольшее количество времени

            try
            {
                m_dbConnection.Open();

                // проходит выборка полей из 2-ух таблиц: студент, дата выдачи и дата возврата

                string sql = @"SELECT Students.Student, Report.IssueDate, Report.ReturnDate
                               FROM Report
                               JOIN Students ON Report.nStudent = Students.nStudent";

                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();

                Student student;

                TimeSpan dateDifference = new();
                string badStudent = "";

                while (reader.Read())
                {
                    student.Name = reader["Student"].ToString();
                    student.IssueDate = DateTime.Parse(reader["IssueDate"].ToString());

                    if (reader["ReturnDate"].ToString() != "")
                    {
                        student.ReturnDate = DateTime.Parse(reader["ReturnDate"].ToString());
                    }
                    else // если дата возврата пустая, то ставится нынешняя дата
                    {
                        student.ReturnDate = DateTime.Today;
                    }
                    
                    if (dateDifference.CompareTo(student.ReturnDate - student.IssueDate) < 0)
                    {
                        dateDifference = student.ReturnDate - student.IssueDate;

                        badStudent = student.Name;
                    }

                    StudentLabel.Content = "Имя самого злостного читателя: " + badStudent;                
                }                    

                reader.Close();
                m_dbConnection.Close();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
