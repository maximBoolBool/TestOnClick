INSERT INTO waves (id) VALUES (1);

INSERT INTO rooms (id, name, room_type, location_type) 
VALUES (1, 'Forest Battle Arena', 0, 0);

INSERT INTO wave_enemies (wave_id, unit_id, count) 
VALUES (1, 3, 1);

INSERT INTO wave_room (wave_id, room_id, "order") 
VALUES (1, 1, 1);