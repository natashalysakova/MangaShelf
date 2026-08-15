using MangaShelf.BL.Contracts;
using MangaShelf.BL.Dto;
using MangaShelf.BL.Services.Parsing;
using MangaShelf.DAL.System.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace MangaShelf.Parser.Tests;

/// <summary>
/// Unit tests for JobStateTransitionPublisher event publishing behavior.
/// </summary>
[TestClass]
public class JobStateTransitionPublisherTests
{
    private Mock<ILogger<JobStateTransitionPublisher>> _loggerMock = null!;
    private JobStateTransitionPublisher _publisher = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<JobStateTransitionPublisher>>();
        _publisher = new JobStateTransitionPublisher(_loggerMock.Object);
    }

    [TestMethod]
    public void Constructor_InitializesWithNoHandlers()
    {
        // Assert
        Assert.AreEqual(0, _publisher.GetHandlerCount());
    }

    [TestMethod]
    public void Subscribe_AddsHandler()
    {
        // Arrange
        var handler = new Mock<IJobStateTransitionHandler>();

        // Act
        _publisher.Subscribe(handler.Object);

        // Assert
        Assert.AreEqual(1, _publisher.GetHandlerCount());
    }

    [TestMethod]
    public void Subscribe_WithNullHandler_ThrowsArgumentNullException()
    {
        // Act & Assert
        bool exceptionThrown = false;
        try
        {
            _publisher.Subscribe(null!);
        }
        catch (ArgumentNullException)
        {
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "ArgumentNullException should have been thrown");
    }

    [TestMethod]
    public void Subscribe_MultipleTimes_CountsCorrectly()
    {
        // Arrange
        var handler1 = new Mock<IJobStateTransitionHandler>();
        var handler2 = new Mock<IJobStateTransitionHandler>();
        var handler3 = new Mock<IJobStateTransitionHandler>();

        // Act
        _publisher.Subscribe(handler1.Object);
        _publisher.Subscribe(handler2.Object);
        _publisher.Subscribe(handler3.Object);

        // Assert
        Assert.AreEqual(3, _publisher.GetHandlerCount());
    }

    [TestMethod]
    public void Subscribe_SameHandlerTwice_CountsOnlyOnce()
    {
        // Arrange
        var handler = new Mock<IJobStateTransitionHandler>();

        // Act
        _publisher.Subscribe(handler.Object);
        _publisher.Subscribe(handler.Object);

        // Assert
        Assert.AreEqual(1, _publisher.GetHandlerCount());
    }

    [TestMethod]
    public void Unsubscribe_RemovesHandler()
    {
        // Arrange
        var handler = new Mock<IJobStateTransitionHandler>();
        _publisher.Subscribe(handler.Object);

        // Act
        _publisher.Unsubscribe(handler.Object);

        // Assert
        Assert.AreEqual(0, _publisher.GetHandlerCount());
    }

    [TestMethod]
    public void Unsubscribe_UnregisteredHandler_DoesNothing()
    {
        // Arrange
        var handler1 = new Mock<IJobStateTransitionHandler>();
        var handler2 = new Mock<IJobStateTransitionHandler>();
        _publisher.Subscribe(handler1.Object);

        // Act
        _publisher.Unsubscribe(handler2.Object);

        // Assert
        Assert.AreEqual(1, _publisher.GetHandlerCount());
    }

    [TestMethod]
    public async Task PublishAsync_CallsAllSubscribedHandlers()
    {
        // Arrange
        var handler1 = new Mock<IJobStateTransitionHandler>();
        var handler2 = new Mock<IJobStateTransitionHandler>();
        var handler3 = new Mock<IJobStateTransitionHandler>();

        handler1.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handler2.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handler3.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publisher.Subscribe(handler1.Object);
        _publisher.Subscribe(handler2.Object);
        _publisher.Subscribe(handler3.Object);

        var transition = new JobStateTransition
        {
            JobId = Guid.NewGuid(),
            FromState = RunStatus.Waiting,
            ToState = RunStatus.GatheringVolumes,
            Trigger = "StartGathering"
        };

        // Act
        await _publisher.PublishAsync(transition);

        // Assert
        handler1.Verify(h => h.HandleAsync(transition, It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.HandleAsync(transition, It.IsAny<CancellationToken>()), Times.Once);
        handler3.Verify(h => h.HandleAsync(transition, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task PublishAsync_WithNullTransition_ThrowsArgumentNullException()
    {
        // Act & Assert
        bool exceptionThrown = false;
        try
        {
            await _publisher.PublishAsync(null!);
        }
        catch (ArgumentNullException)
        {
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "ArgumentNullException should have been thrown");
    }

    [TestMethod]
    public async Task PublishAsync_WithHandlerException_ContinuesWithOtherHandlers()
    {
        // Arrange
        var handler1 = new Mock<IJobStateTransitionHandler>();
        var handler2 = new Mock<IJobStateTransitionHandler>();
        var handler3 = new Mock<IJobStateTransitionHandler>();

        handler1.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler 1 failed"));
        handler2.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        handler3.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publisher.Subscribe(handler1.Object);
        _publisher.Subscribe(handler2.Object);
        _publisher.Subscribe(handler3.Object);

        var transition = new JobStateTransition
        {
            JobId = Guid.NewGuid(),
            FromState = RunStatus.Waiting,
            ToState = RunStatus.GatheringVolumes,
            Trigger = "StartGathering"
        };

        // Act - should not throw, handlers 2 and 3 should still be called
        await _publisher.PublishAsync(transition);

        // Assert
        handler1.Verify(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()), Times.Once);
        handler2.Verify(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()), Times.Once);
        handler3.Verify(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task PublishAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var handler = new Mock<IJobStateTransitionHandler>();
        handler.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _publisher.Subscribe(handler.Object);

        var transition = new JobStateTransition
        {
            JobId = Guid.NewGuid(),
            FromState = RunStatus.Waiting,
            ToState = RunStatus.GatheringVolumes,
            Trigger = "StartGathering"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        bool exceptionThrown = false;
        try
        {
            await _publisher.PublishAsync(transition, cts.Token);
        }
        catch (OperationCanceledException)
        {
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "OperationCanceledException should have been thrown");
    }

    [TestMethod]
    public async Task PublishAsync_PassesCancellationTokenToHandlers()
    {
        // Arrange
        var handler = new Mock<IJobStateTransitionHandler>();
        CancellationToken? capturedToken = null;

        handler.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Callback<JobStateTransition, CancellationToken>((_, token) => capturedToken = token)
            .Returns(Task.CompletedTask);

        _publisher.Subscribe(handler.Object);

        var transition = new JobStateTransition
        {
            JobId = Guid.NewGuid(),
            FromState = RunStatus.Waiting,
            ToState = RunStatus.GatheringVolumes,
            Trigger = "StartGathering"
        };

        var cts = new CancellationTokenSource();

        // Act
        await _publisher.PublishAsync(transition, cts.Token);

        // Assert
        Assert.IsNotNull(capturedToken);
        Assert.AreEqual(cts.Token, capturedToken);
    }

    [TestMethod]
    public async Task PublishAsync_WithNoHandlers_CompletesSuccessfully()
    {
        // Arrange
        var transition = new JobStateTransition
        {
            JobId = Guid.NewGuid(),
            FromState = RunStatus.Waiting,
            ToState = RunStatus.GatheringVolumes,
            Trigger = "StartGathering"
        };

        // Act & Assert - should not throw
        await _publisher.PublishAsync(transition);
    }

    [TestMethod]
    public async Task PublishAsync_PreservesTransitionDetails()
    {
        // Arrange
        var handler = new Mock<IJobStateTransitionHandler>();
        JobStateTransition? capturedTransition = null;

        handler.Setup(h => h.HandleAsync(It.IsAny<JobStateTransition>(), It.IsAny<CancellationToken>()))
            .Callback<JobStateTransition, CancellationToken>((t, _) => capturedTransition = t)
            .Returns(Task.CompletedTask);

        _publisher.Subscribe(handler.Object);

        var jobId = Guid.NewGuid();
        var transition = new JobStateTransition
        {
            JobId = jobId,
            FromState = RunStatus.Waiting,
            ToState = RunStatus.GatheringVolumes,
            Trigger = "StartGathering",
            Context = "Test context",
            ExceptionDetails = "Test exception"
        };

        // Act
        await _publisher.PublishAsync(transition);

        // Assert
        Assert.IsNotNull(capturedTransition);
        Assert.AreEqual(jobId, capturedTransition.JobId);
        Assert.AreEqual(RunStatus.Waiting, capturedTransition.FromState);
        Assert.AreEqual(RunStatus.GatheringVolumes, capturedTransition.ToState);
        Assert.AreEqual("StartGathering", capturedTransition.Trigger);
        Assert.AreEqual("Test context", capturedTransition.Context);
        Assert.AreEqual("Test exception", capturedTransition.ExceptionDetails);
    }
}
