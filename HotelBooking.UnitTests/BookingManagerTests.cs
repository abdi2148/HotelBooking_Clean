using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.Infrastructure.Repositories;
using HotelBooking.UnitTests.Fakes;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        private Mock<IRepository<Booking>> bookingRepository;
        private Mock<IRepository<Room>> roomRepository;



        public BookingManagerTests()
        {
            bookingRepository = new Mock<IRepository<Booking>>();
            roomRepository = new Mock<IRepository<Room>>();

            var rooms = new List<Room>
            {
                new Room { Id=1, Description="Room 1" },
                new Room { Id=2, Description="Room 2" },
            };

            DateTime fullyOccupiedStartDate = DateTime.Today.AddDays(10);
            DateTime fullyOccupiedEndDate = DateTime.Today.AddDays(20);

            List<Booking> bookings = new List<Booking>
            {
                new Booking { Id=1, StartDate=fullyOccupiedStartDate, EndDate=fullyOccupiedEndDate, IsActive=true, CustomerId=1, RoomId=1 },
                new Booking { Id=2, StartDate=fullyOccupiedStartDate, EndDate=fullyOccupiedEndDate, IsActive=true, CustomerId=2, RoomId=2 },
            };

            roomRepository.Setup(x => x.GetAll()).Returns(rooms);
            bookingRepository.Setup(x => x.GetAll()).Returns(bookings);

            bookingManager = new BookingManager(bookingRepository.Object, roomRepository.Object);
        }

        [Fact]
        public void FindAvailableRoom_StartDateNotToday_ThrowsArgumentException()
        {
            // Arrange
            DateTime dateToday = DateTime.Today;
            DateTime dateTomorrow = DateTime.Today.AddDays(1);

            // Act
            Action act = () => bookingManager.FindAvailableRoom(dateToday, dateTomorrow);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FindAvailableRoom_StartDateBeforeToday_ThrowsArgumentException()
        {
            // Arrange
            DateTime dateYesterday = DateTime.Today.AddDays(-1);
            DateTime dateToday = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(dateYesterday, dateToday);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FindAvailableRoom_EndDateNotOlderThanStartDate_ThrowsArgumentException()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(1);
            DateTime dateEnd = DateTime.Today;

            // Act
            Action act = () => bookingManager.FindAvailableRoom(dateStart, dateEnd);

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(1);
            DateTime dateEnd = DateTime.Today.AddDays(2);

            // Act
            int roomId = bookingManager.FindAvailableRoom(dateStart, dateEnd);

            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public void CreateBooking_IncorrectStartDate_ReturnsFalse()
        {
            // Arrange
            Booking booking = new()
            {
                StartDate = DateTime.Today.AddDays(11),
                EndDate = DateTime.Today.AddDays(12)
            };

            // Act
            bool isCreated = bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(isCreated);
        }

        [Fact]
        public void CreateBooking_CorrectStartDate_ReturnsTrue()
        {
            // Arrange
            Booking booking = new()
            {
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2)
            };

            // Act
            bool isCreated = bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(isCreated);
        }

        [Fact]
        public void GetFullyOccupiedDates_StartDateIsMoreThanEndDate_ThrowArgumentException()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(5);
            DateTime endDate = DateTime.Today.AddDays(2);
            Action act = () => bookingManager.GetFullyOccupiedDates(startDate, endDate);
            //Act
            var rec = Record.Exception(act);
            //Assert
            Assert.IsType<ArgumentException>(rec);
        }

        [Fact]
        public void GetFullyOccupiedDates_StartDayAdd21EndDateAdd25_ReturnsEmptyList()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(21);
            DateTime endDate = DateTime.Today.AddDays(25);
            //Act
            List<DateTime> fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);
            //Assert
            Assert.Empty(fullyOccupiedDates);
        }

        [Fact]
        public void GetFullyOccupiedDates_StartDayAdd10EndDateAdd20_ReturnsListCount10()
        {
            //Arrange
            DateTime startDate = DateTime.Today.AddDays(10);
            DateTime endDate = DateTime.Today.AddDays(20);
            //Act
            List<DateTime> fullyOccupiedDates = bookingManager.GetFullyOccupiedDates(startDate, endDate);
            //Assert
            Assert.Equal(11, fullyOccupiedDates.Count);
        }

        [Fact]
        public void FindAvailableRoom_RoomNOTAvailable_MinusOne()
        {
            // Arrange
            DateTime dateStart = DateTime.Today.AddDays(10);
            DateTime dateEnd = DateTime.Today.AddDays(20);

            // Act
            int roomId = bookingManager.FindAvailableRoom(dateStart, dateEnd);

            // Assert
            Assert.Equal(-1, roomId);
        }
    }
}