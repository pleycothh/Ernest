﻿using App1.Dao.DataRetrieval;
using App1.Dao.Impl;
using App1.Models;
using App1.TableCreators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddStudentForm : Page
    {
        public static string DatabaseName = "students.db";
        public StudentDao studentDao = new StudentDao(DatabaseName);
        public CourseDao courseDao = new CourseDao(DatabaseName);
        public NetworkDao networkDao = new NetworkDao(DatabaseName);
        public DataRetrievalClass dataRetrievalClass = new DataRetrievalClass(DatabaseName);

        public AddStudentForm()
        {
            this.InitializeComponent();
        }

        void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        void SaveStudentButton_Click(object sender, RoutedEventArgs e)
        {
            string newStudentNumber = this.sNumber.Text.Trim();
            string fName = this.FirstName.Text.Trim();
            string lName = this.LastName.Text.Trim();
           
            if (studentDao.FindById(newStudentNumber) != null)
            {
                this.ErrorMessage.Text = "Student with this sNumber already exists";
            }
            else
            {
                Student student =
                new Student.Builder().WithId(newStudentNumber).WithFirstName(fName).WithLastName(lName).Build();
                studentDao.Save(student);
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private bool IsValidName(string name)
        {
            string strRegex = @"^[a-zA-Z]+(([',. -][a-zA-Z ])?[a-zA-Z]*)*$";

            Regex re = new Regex(strRegex);

            if (re.IsMatch(name))
                return (true);
            else
                return (false);
        }

        private bool IsValidStudentNumber(string sNumber)
        {
            string strRegex = @"([s]\d{7})";

            Regex re = new Regex(strRegex);

            if (re.IsMatch(sNumber))
                return (true);
            else
                return (false);
        }
    }
}
