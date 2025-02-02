﻿using App1.Dao.Impl;
using App1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace App1.Parser
{
    public class ExcelParser
    {
        private StudentDao StudentDao;
        private CourseDao CourseDao;
        private NetworkDao NetworkDao;

        public ExcelParser(StudentDao studentDao, CourseDao courseDao, NetworkDao networkDao)
        {
            StudentDao = studentDao;
            CourseDao = courseDao;
            NetworkDao = networkDao;
        }

        /*
         * The function reads data from Excel file and saves every cell into a temporary object (Obj class)
         * */
        public List<Obj> ReadFromExcel(string inputString)
        {
            List<string[]> lines = SplitString(inputString);
            List<Obj> _data = new List<Obj>();

            foreach (string[] line in lines)
            {
                Obj obj = new Obj();
                obj.StudentId = line[0];
                obj.FirstName = line[1];
                obj.LastName = line[2];
                obj.ChildCourse = line[3];
                _data.Add(obj);
            }

            _data.RemoveAt(0);

            return _data;
        }

        /*
         * The function writes data into the database
         * */
        public void WriteToDatabase(List<Obj> _data)
        {
            if(StudentDao.CheckIfStudentATableIsEmpty() == 0) // checking if students table does not exist and insert new data
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    CourseDao.Save(new Course.Builder().WithId(_data[i].ChildCourse).Build());

                    Student student = new Student.Builder().WithId(_data[i].StudentId)
                            .WithFirstName(_data[i].FirstName).WithLastName(_data[i].LastName).Build();
                    StudentDao.Save(student);

                    CourseDao.SaveStudentCourse(_data[i].StudentId, _data[i].ChildCourse);
                }
            } else // if database already has data, update the database with new entries and remove students who dropped out
            {
                List<Obj> existingStudents = GetAllExistingStudents(); //getting all students from the db
                List<Obj> intersectStudents = _data.Select(i => i).Intersect(existingStudents).ToList(); //intersecting the existing students with new students
                List<Obj> unitedList = intersectStudents.Union(_data).ToList(); // union the intersected students with new students

                CourseDao.EraseDatabase(); // removing existing data from the database

                for (int i = 0; i < unitedList.Count; i++)
                {
                    CourseDao.Save(new Course.Builder().WithId(_data[i].ChildCourse).Build());

                    Student student = new Student.Builder().WithId(unitedList[i].StudentId)
                            .WithFirstName(unitedList[i].FirstName).WithLastName(unitedList[i].LastName).Build();
                    StudentDao.Save(student);

                    CourseDao.SaveStudentCourse(unitedList[i].StudentId, unitedList[i].ChildCourse);

                    if(StudentDao.FindById(student.Id) == null) //if student does not exist, remove their networks from NETWORKS table
                    {
                        NetworkDao.DeleteByStudentId(student.Id);
                    }
                }
            }
            
        }

        /*
         * The function splits a row from Excel file into a list of strings
         * */
        private List<string[]> SplitString(string inputString)
        {
            List<string[]> result = new List<string[]>();

            string[] lines = inputString.Split(Environment.NewLine,
                            StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                result.Add(line.Split(','));
            }

            return result;
        }

        /*
         * The function gets all existing students from the database
         * */
        private List<Obj> GetAllExistingStudents() 
        {
            List<Student> existingStudents = StudentDao.FindAll();
            List<Obj> result = new List<Obj>();

            foreach(Student student in existingStudents)
            {
               for (int i = 0; i < student.Courses.Count; i++)
                {
                    Obj obj = new Obj();
                    obj.StudentId = student.Id;
                    obj.FirstName = student.FirstName;
                    obj.LastName = student.LastName;
                    obj.ChildCourse = student.Courses[i].ToString();
                    result.Add(obj);
                }
            }

            return result;
        }
    }

         /*
         * This class is used to store data from each row of Excel file
         * */
    public class Obj
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ChildCourse { get; set; }
    }

}
