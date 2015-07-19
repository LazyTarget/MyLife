using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharedLib.Tests
{
    [TestClass]
    public class DateExtensions_Tests
    {
        [TestMethod]
        public void GetFirstDayOfWeekAsSunday_WhenSaturday()
        {
            var now         = new DateTime(2015, 07, 11, 14, 23, 20);       // sunday
            var expected    = new DateTime(2015, 07, 05, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Sunday);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetFirstDayOfWeekAsSunday_WhenSunday()
        {
            var now         = new DateTime(2015, 07, 12, 14, 23, 20);       // sunday
            var expected    = new DateTime(2015, 07, 12, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Sunday);
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfWeekAsSunday_WhenMonday()
        {
            var now         = new DateTime(2015, 07, 13, 14, 23, 20);       // monday
            var expected    = new DateTime(2015, 07, 12, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Sunday);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetFirstDayOfWeekAsMonday_WhenMonday()
        {
            var now         = new DateTime(2015, 07, 13, 14, 23, 20);       // monday
            var expected    = new DateTime(2015, 07, 13, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Monday);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetFirstDayOfWeekAsMonday_WhenThursday()
        {
            var now         = new DateTime(2015, 07, 16, 14, 23, 20);       // thursday
            var expected    = new DateTime(2015, 07, 13, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Monday);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetFirstDayOfWeekAsMonday_WhenFriday()
        {
            var now         = new DateTime(2015, 07, 17, 14, 23, 20);       // friday
            var expected    = new DateTime(2015, 07, 13, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Monday);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetFirstDayOfWeekAsMonday_WhenSaturday()
        {
            var now         = new DateTime(2015, 07, 18, 14, 23, 20);       // saturday
            var expected    = new DateTime(2015, 07, 13, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Monday);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetFirstDayOfWeekAsMonday_WhenSunday()
        {
            var now         = new DateTime(2015, 07, 19, 14, 23, 20);       // sunday
            var expected    = new DateTime(2015, 07, 13, 0, 0, 0);
            var actual = now.GetFirstDayOfWeek(DayOfWeek.Monday);
            Assert.AreEqual(expected, actual);
        }



        
        [TestMethod]
        public void GetFirstDayOfMonth_December31st()
        {
            var now         = new DateTime(2014, 12, 31, 14, 23, 20);
            var expected    = new DateTime(2014, 12, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfMonth_January1st_Equals()
        {
            var now         = new DateTime(2015, 01, 01, 0, 0, 0);
            var expected    = new DateTime(2015, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetFirstDayOfMonth_Janunary1st()
        {
            var now         = new DateTime(2015, 01, 31, 14, 23, 20);
            var expected    = new DateTime(2015, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfMonth_February29th()
        {
            var now         = new DateTime(2012, 02, 29, 14, 23, 20);
            var expected    = new DateTime(2012, 02, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfMonth_July1st_Equals()
        {
            var now         = new DateTime(2015, 07, 01, 0, 0, 0);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfMonth_July1st()
        {
            var now         = new DateTime(2015, 07, 01, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetFirstDayOfMonth_July19th()
        {
            var now         = new DateTime(2015, 07, 19, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfMonth_July31th()
        {
            var now         = new DateTime(2015, 07, 31, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfMonth();
            Assert.AreEqual(expected, actual);
        }


        
        
        [TestMethod]
        public void GetFirstDayOfYear_December31st()
        {
            var now         = new DateTime(2014, 12, 31, 14, 23, 20);
            var expected    = new DateTime(2014, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfYear();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetFirstDayOfYear_January1st_Equals()
        {
            var now         = new DateTime(2014, 01, 01, 0, 0, 0);
            var expected    = new DateTime(2014, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfYear();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfYear_January1st()
        {
            var now         = new DateTime(2014, 01, 01, 14, 23, 20);
            var expected    = new DateTime(2014, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfYear();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfYear_February29th()
        {
            var now         = new DateTime(2012, 02, 29, 14, 23, 20);
            var expected    = new DateTime(2012, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfYear();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfYear_July31th()
        {
            var now         = new DateTime(2015, 07, 31, 14, 23, 20);
            var expected    = new DateTime(2015, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfYear();
            Assert.AreEqual(expected, actual);
        }


        
        
        
        [TestMethod]
        public void GetFirstDayOfQuarter_December31st()
        {
            var now         = new DateTime(2014, 12, 31, 14, 23, 20);
            var expected    = new DateTime(2014, 10, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetFirstDayOfQuarter_January1st_Equals()
        {
            var now         = new DateTime(2014, 01, 01, 0, 0, 0);
            var expected    = new DateTime(2014, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_January1st()
        {
            var now         = new DateTime(2014, 01, 01, 14, 23, 20);
            var expected    = new DateTime(2014, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_February29th()
        {
            var now         = new DateTime(2012, 02, 29, 14, 23, 20);
            var expected    = new DateTime(2012, 01, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_May3rd()
        {
            var now         = new DateTime(2012, 05, 03, 14, 23, 20);
            var expected    = new DateTime(2012, 04, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_June13th()
        {
            var now         = new DateTime(2012, 06, 13, 14, 23, 20);
            var expected    = new DateTime(2012, 04, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_June30th()
        {
            var now         = new DateTime(2012, 06, 30, 14, 23, 20);
            var expected    = new DateTime(2012, 04, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_July31th()
        {
            var now         = new DateTime(2015, 07, 31, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_August27th()
        {
            var now         = new DateTime(2015, 08, 27, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_October3rd()
        {
            var now         = new DateTime(2015, 10, 03, 14, 23, 20);
            var expected    = new DateTime(2015, 10, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_July1st()
        {
            var now         = new DateTime(2015, 09, 01, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_July1st_Equals()
        {
            var now         = new DateTime(2015, 07, 01, 0, 0, 0);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_September1st()
        {
            var now         = new DateTime(2015, 09, 01, 14, 23, 20);
            var expected    = new DateTime(2015, 07, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetFirstDayOfQuarter_November14th_Equals()
        {
            var now         = new DateTime(2015, 11, 14, 12, 56, 12);
            var expected    = new DateTime(2015, 10, 01, 0, 0, 0);
            var actual = now.GetFirstDayOfQuarter();
            Assert.AreEqual(expected, actual);
        }


    }
}
