package com.platform.securelearning.session;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

import java.util.List;
import org.junit.jupiter.api.Test;

class RoomServiceTest {

    private final RoomService roomService = new RoomService();

    @Test
    void createRoomReturnsFirstAnonymousSession() {
        Session first = roomService.createRoom();

        assertThat(first.sessionId()).isNotBlank();
        assertThat(first.roomCode()).isNotBlank();
        assertThat(roomService.belongsToRoom(first.roomCode(), first.sessionId())).isTrue();
    }

    @Test
    void secondParticipantCanJoinAndBecomesThePeerOfTheFirst() {
        Session first = roomService.createRoom();

        Session second = roomService.join(first.roomCode());

        assertThat(second.roomCode()).isEqualTo(first.roomCode());
        assertThat(second.sessionId()).isNotEqualTo(first.sessionId());
        assertThat(roomService.peersOf(first.roomCode(), first.sessionId()))
                .containsExactly(second);
        assertThat(roomService.peersOf(first.roomCode(), second.sessionId()))
                .containsExactly(first);
    }

    @Test
    void thirdParticipantCannotJoinAFullRoom() {
        Session first = roomService.createRoom();
        roomService.join(first.roomCode());

        assertThatThrownBy(() -> roomService.join(first.roomCode()))
                .isInstanceOf(RoomFullException.class);
    }

    @Test
    void joiningAnUnknownRoomCodeFails() {
        assertThatThrownBy(() -> roomService.join("NOSUCHROOM"))
                .isInstanceOf(RoomNotFoundException.class);
    }

    @Test
    void peersOfAnUnknownRoomCodeFails() {
        assertThatThrownBy(() -> roomService.peersOf("NOSUCHROOM", "irrelevant"))
                .isInstanceOf(RoomNotFoundException.class);
    }

    @Test
    void aStrangerDoesNotBelongToARoomItNeverJoined() {
        Session first = roomService.createRoom();

        assertThat(roomService.belongsToRoom(first.roomCode(), "some-other-session-id")).isFalse();
    }

    @Test
    void roomCodesAreUnpredictableAcrossCreations() {
        List<String> codes = List.of(roomService.createRoom().roomCode(),
                roomService.createRoom().roomCode(), roomService.createRoom().roomCode());

        assertThat(codes).doesNotHaveDuplicates();
    }
}
