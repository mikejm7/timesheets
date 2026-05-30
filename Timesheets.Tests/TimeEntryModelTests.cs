using System;
using Timesheets.Shared;
using Xunit;

namespace Timesheets.Tests
{
    public class TimeEntryModelTests
    {
        [Fact]
        public void Constructor_InitializesDefaultValuesCorrectly()
        {
            // Arrange & Act
            var model = new TimeEntryModel();

            // Assert
            Assert.Equal("Draft", model.Status);
            Assert.Equal(0, model.RT);
            Assert.Equal(0, model.OT);
            Assert.Equal(0, model.DT);
            Assert.Equal(0, model.Travel);
            Assert.Equal(0, model.TotalHours);
        }

        [Fact]
        public void SettingHourProperties_DoesNotAutomaticallyUpdateTotalHours()
        {
            // Arrange
            var model = new TimeEntryModel();

            // Act
            model.RT = 5;
            model.OT = 2;
            model.DT = 1;
            model.Travel = 3;

            // Assert
            Assert.Equal(5, model.RT);
            Assert.Equal(2, model.OT);
            Assert.Equal(1, model.DT);
            Assert.Equal(3, model.Travel);
            // TotalHours should remain 0 as it is a passive DTO
            Assert.Equal(0, model.TotalHours);
        }
    }
}
