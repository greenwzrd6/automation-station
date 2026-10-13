using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Actions;

namespace AutomationStation.Unit.Tests.Infrastructure.Actions
{
    public class ActionCatalogTests
    {
        private readonly ActionCatalog _ac = new();

        [Fact]
        public void TryGet_ValidActionValidTargetSystem_ReturnsEntry()
        {
            // Arrange
            var actionType = "CreatePlacement";
            var targetSystem = TargetSystem.Kanban;

            // Act
            var result = _ac.TryGet(
                actionType, 
                targetSystem, 
                out var entry);

            // Assert
            Assert.True(result);
            Assert.Equal(actionType, entry.ActionType);
            Assert.Equal(targetSystem, entry.TargetSystem);
        }

        [Theory]
        [InlineData("CreatePlacement", TargetSystem.TojSystem)]
        [InlineData("CreateTask", TargetSystem.Kanban)]
        [InlineData("UpdateTaskStatus", TargetSystem.Kanban)]
        public void TryGet_ValidActionWrongTargetSystem_ReturnsFalse(
            string actionType,
            TargetSystem targetSystem)
        {
            // Act
            var result = _ac.TryGet(
                actionType,
                targetSystem,
                out var entry);

            // Assert
            Assert.False(result);
            Assert.Null(entry);
        }

        [Fact]
        public void TryGet_InvalidAction_ReturnsFalse()
        {
            // Arrange
            var actionType = "InvalidAction";
            var targetSystem = TargetSystem.Kanban;

            // Act
            var result = _ac.TryGet(
                actionType, 
                targetSystem, 
                out var entry);

            // Assert
            Assert.False(result);
            Assert.Null(entry);
        }

        [Theory]
        [InlineData("CreatePlacement", TargetSystem.Kanban, 100, 1)]
        [InlineData("CreateTask", TargetSystem.TojSystem, 10, 1)]
        [InlineData("UpdateTaskStatus", TargetSystem.TojSystem, 100, 1)]
        public void TryGet_ValidEntry_ReturnsCorrectConfiguration(
            string actionType,
            TargetSystem targetSystem,
            int permittedActions,
            int rateLimitWindow)
        {
            // Act
            var result = _ac.TryGet(
                actionType,
                targetSystem,
                out var entry);

            // Assert
            Assert.True(result);
            Assert.Equal(actionType, entry.ActionType);
            Assert.Equal(targetSystem, entry.TargetSystem);
            Assert.Equal(permittedActions, entry.PermittedActions);
            Assert.Equal(TimeSpan.FromMinutes(rateLimitWindow), entry.RateLimitWindow);
        }

        [Theory]
        [InlineData("CreatePlacement")]
        [InlineData("createplacement")]
        [InlineData("CREATEPLACEMENT")]
        [InlineData("cReAtEpLaCeMeNt")]
        public void TryGet_ActionTypeIsCaseInsensitive_ReturnsEntry(
            string actionType)
        {
            // Arrange
            var targetSystem = TargetSystem.Kanban;

            // Act
            var result = _ac.TryGet(
                actionType,
                targetSystem,
                out var entry);

            // Assert
            Assert.True(result);
            Assert.Equal("CreatePlacement", entry.ActionType);
            Assert.Equal(targetSystem, entry.TargetSystem);
        }
    }
}
