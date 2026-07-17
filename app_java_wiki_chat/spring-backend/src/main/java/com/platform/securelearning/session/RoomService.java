package com.platform.securelearning.session;

import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import org.springframework.stereotype.Service;

/**
 * Pairs anonymous sessions into 2-party rooms (US-3.1). Rooms are held only in
 * memory — see {@link Room} for why nothing here is persisted.
 */
@Service
public class RoomService {

    private final Map<String, Room> rooms = new ConcurrentHashMap<>();

    public Session createRoom() {
        Room room = new Room(RoomCodeGenerator.newCode());
        rooms.put(room.roomCode(), room);
        return room.addParticipant();
    }

    public Session join(String roomCode) {
        Room room = rooms.get(roomCode);
        if (room == null) {
            throw new RoomNotFoundException(roomCode);
        }
        return room.addParticipant();
    }

    public boolean belongsToRoom(String roomCode, String sessionId) {
        Room room = rooms.get(roomCode);
        return room != null && room.contains(sessionId);
    }

    public List<Session> peersOf(String roomCode, String sessionId) {
        Room room = rooms.get(roomCode);
        if (room == null) {
            throw new RoomNotFoundException(roomCode);
        }
        return room.peersOf(sessionId);
    }
}
