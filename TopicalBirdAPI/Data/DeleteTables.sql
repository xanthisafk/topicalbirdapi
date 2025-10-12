-- Reset all tables
DO $$
BEGIN
    RAISE NOTICE 'Dropping all tables...';
END $$;

DROP TABLE IF EXISTS usersessions CASCADE;
DROP TABLE IF EXISTS comment CASCADE;
DROP TABLE IF EXISTS media CASCADE;
DROP TABLE IF EXISTS posts CASCADE;
DROP TABLE IF EXISTS nest CASCADE;
DROP TABLE IF EXISTS users CASCADE;

DO $$
BEGIN
    RAISE NOTICE 'All tables dropped successfully.';
END $$;
