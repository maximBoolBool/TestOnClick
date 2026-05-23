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

CREATE TABLE IF NOT EXISTS enemy_wave (
    id INTEGER PRIMARY KEY,
    wave_id INTEGER NOT NULL,
    room_id INTEGER NOT NULL,
    FOREIGN KEY (wave_id) REFERENCES waves(id),
    FOREIGN KEY (room_id) REFERENCES rooms(id)
);

CREATE TABLE IF NOT EXISTS wave_enemies (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    wave_id INTEGER NOT NULL,
    unit_id INTEGER NOT NULL,
    count INTEGER NOT NULL,
    FOREIGN KEY (wave_id) REFERENCES waves(id),
    FOREIGN KEY (unit_id) REFERENCES units(id)
);

CREATE TABLE IF NOT EXISTS room_wave (
    id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    room_id INTEGER NOT NULL,
    wave_id INTEGER NOT NULL,
    [order] INTEGER NOT NULL,
    
    FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE CASCADE,
    FOREIGN KEY (wave_id) REFERENCES waves(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_enemy_wave_wave_id ON enemy_wave(wave_id);
CREATE INDEX IF NOT EXISTS idx_enemy_wave_room_id ON enemy_wave(room_id);
CREATE INDEX IF NOT EXISTS idx_wave_enemies_wave_id ON wave_enemies(wave_id);
CREATE INDEX IF NOT EXISTS idx_wave_enemies_unit_id ON wave_enemies(unit_id);