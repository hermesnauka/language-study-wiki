using SecureLearning.Backend.Sessions;

namespace SecureLearning.Backend.Tests;

public class RoomServiceTests
{
    private readonly RoomService rooms = new();

    [Fact]
    public void CreateRoomReturnsFirstAnonymousSession()
    {
        var first = rooms.CreateRoom();

        Assert.False(string.IsNullOrEmpty(first.SessionId));
        Assert.False(string.IsNullOrEmpty(first.RoomCode));
        Assert.True(rooms.BelongsToRoom(first.RoomCode, first.SessionId));
    }

    [Fact]
    public void SecondParticipantJoinsAndBecomesThePeerOfTheFirst()
    {
        var first = rooms.CreateRoom();

        var second = rooms.Join(first.RoomCode);

        Assert.Equal(first.RoomCode, second.RoomCode);
        Assert.NotEqual(first.SessionId, second.SessionId);
        Assert.Equal([second], rooms.PeersOf(first.RoomCode, first.SessionId));
        Assert.Equal([first], rooms.PeersOf(first.RoomCode, second.SessionId));
    }

    [Fact]
    public void ThirdParticipantCannotJoinAFullRoom()
    {
        var first = rooms.CreateRoom();
        rooms.Join(first.RoomCode);

        Assert.Throws<RoomFullException>(() => rooms.Join(first.RoomCode));
    }

    [Fact]
    public void JoiningAnUnknownRoomCodeFails()
    {
        Assert.Throws<RoomNotFoundException>(() => rooms.Join("NOSUCHROOM"));
    }

    [Fact]
    public void StrangerDoesNotBelongToARoomItNeverJoined()
    {
        var first = rooms.CreateRoom();

        Assert.False(rooms.BelongsToRoom(first.RoomCode, "some-other-session-id"));
    }
}
