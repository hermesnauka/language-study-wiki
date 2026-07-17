package com.platform.securelearning.session;

import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import java.time.Instant;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.WebMvcTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.test.web.servlet.MockMvc;

@WebMvcTest(SessionController.class)
class SessionControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @MockBean
    private RoomService roomService;

    @Test
    void createRoomReturnsTheNewSession() throws Exception {
        when(roomService.createRoom()).thenReturn(new Session("session-a", "ROOM1", Instant.now()));

        mockMvc.perform(post("/api/rooms"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.roomCode").value("ROOM1"))
                .andExpect(jsonPath("$.sessionId").value("session-a"));
    }

    @Test
    void joiningAFullRoomReturns409() throws Exception {
        when(roomService.join("ROOM1")).thenThrow(new RoomFullException("ROOM1"));

        mockMvc.perform(post("/api/rooms/ROOM1/join")).andExpect(status().isConflict());
    }

    @Test
    void joiningAnUnknownRoomReturns404() throws Exception {
        when(roomService.join("NOPE")).thenThrow(new RoomNotFoundException("NOPE"));

        mockMvc.perform(post("/api/rooms/NOPE/join")).andExpect(status().isNotFound());
    }
}
