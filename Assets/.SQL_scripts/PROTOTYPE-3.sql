INSERT INTO waves (id) VALUES (2);

INSERT INTO rooms (id, name, room_type, location_type, "key") 
VALUES (2, 'Forest Battle Arena', 0, 0, 'Room2');

INSERT INTO wave_enemies (wave_id, unit_id, count) 
VALUES (2, 3, 3);

INSERT INTO wave_room (wave_id, room_id, "order") 
VALUES (2, 2, 1);

CREATE TABLE IF NOT EXISTS locations(
    type INTEGER PRIMARY KEY,
    min_room_count INTEGER NOT NULL,
    max_room_count INTEGER NOT NULL
);

INSERT INTO locations ("type", min_room_count, max_room_count)
VALUES (0, 1, 3);