CREATE TABLE IF NOT EXISTS units (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    health_points INTEGER NOT NULL,
    active_action_points INTEGER NOT NULL,
    reaction_action_points INTEGER NOT NULL,
    agility INTEGER NOT NULL,
    melee_skill INTEGER NOT NULL,
    defend_skill INTEGER NOT NULL,
    side INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS waves (
    id INTEGER PRIMARY KEY AUTOINCREMENT
);

CREATE TABLE IF NOT EXISTS rooms (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    room_type INTEGER NOT NULL,
    location_type INTEGER NOT NULL
);

CREATE TABLE wave_enemies (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    wave_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    count INTEGER NOT NULL,
    FOREIGN KEY (wave_id) REFERENCES waves(Id),
    FOREIGN KEY (unit_id) REFERENCES units(Id)
);

CREATE TABLE wave_room (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    wave_id INTEGER NOT NULL,
    room_id INTEGER NOT NULL,
    [order] INTEGER NOT NULL,
    FOREIGN KEY (wave_id) REFERENCES waves(Id),
    FOREIGN KEY (room_id) REFERENCES rooms(Id)
);