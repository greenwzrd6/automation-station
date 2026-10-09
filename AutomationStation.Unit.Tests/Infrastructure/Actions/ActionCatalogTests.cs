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
            var result = _ac.TryGet(actionType, targetSystem, out var entry);

            // Assert
            Assert.True(result);
            Assert.Equal(actionType, entry.ActionType);
            Assert.Equal(targetSystem, entry.TargetSystem);
        }

        [Fact]
        public void TryGet_ValidActionInvalidTargetSystem_ReturnsFalse()
        {
            // Arrange
            var actionType = "CreatePlacement";
            var targetSystem = TargetSystem.TojSystem;

            // Act
            var result = _ac.TryGet(actionType, targetSystem, out var entry);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryGet_InvalidAction_ReturnsFalse()
        {
            // Arrange
            var actionType = "InvalidAction";
            var targetSystem = TargetSystem.Kanban;

            // Act
            var result = _ac.TryGet(actionType, targetSystem, out var entry);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryGet_CorrectProperties_ReturnsEntries()
        {
            // Arrange
            var actionType1 = "CreatePlacement";
            var actionType2 = "CreateTask";
            var actionType3 = "UpdateTaskStatus";

            var targetSystem = TargetSystem.Kanban;
            var targetSystem2 = TargetSystem.TojSystem;

            // Act
            var result1 = _ac.TryGet(actionType1, targetSystem, out var entry);
            var result2 = _ac.TryGet(actionType2, targetSystem2, out var entry2);
            var result3 = _ac.TryGet(actionType3, targetSystem2, out var entry3);

            // Assert
            Assert.True(result1);
            Assert.Equal(100, entry.PermittedActions);
            Assert.Equal(TimeSpan.FromMinutes(1), entry.RateLimitWindow);

            Assert.True(result2);
            Assert.Equal(10, entry2.PermittedActions);
            Assert.Equal(TimeSpan.FromMinutes(1), entry2.RateLimitWindow);

            Assert.True(result3);
            Assert.Equal(100, entry3.PermittedActions);
            Assert.Equal(TimeSpan.FromMinutes(1), entry3.RateLimitWindow);
        }
    }
}
