CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY NOT NULL,
    email TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    username TEXT NOT NULL UNIQUE,
    icon TEXT,
    display_name TEXT,
    is_admin BOOLEAN DEFAULT FALSE,
    is_banned BOOLEAN DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS nest (
    id UUID PRIMARY KEY NOT NULL,
    title VARCHAR(100) NOT NULL UNIQUE,
    display_name TEXT,
    icon TEXT,
    moderator_id UUID,
    CONSTRAINT fk_nest_moderator
        FOREIGN KEY (moderator_id)
        REFERENCES users(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS posts (
    id UUID PRIMARY KEY NOT NULL,
    title TEXT NOT NULL,
    content TEXT NOT NULL,
    author_id UUID,
    is_deleted BOOLEAN DEFAULT FALSE,
    CONSTRAINT fk_posts_author
        FOREIGN KEY (author_id)
        REFERENCES users(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS media (
    id UUID PRIMARY KEY NOT NULL,
    posts_id UUID,
    content_url TEXT,
    CONSTRAINT fk_media_post
        FOREIGN KEY (posts_id)
        REFERENCES posts(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS comment (
    id UUID PRIMARY KEY NOT NULL,
    posts_id UUID,
    author_id UUID,
    content TEXT,
    CONSTRAINT fk_comment_post
        FOREIGN KEY (posts_id)
        REFERENCES posts(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_comment_author
        FOREIGN KEY (author_id)
        REFERENCES users(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE IF NOT EXISTS usersessions (
    id UUID PRIMARY KEY NOT NULL,
    user_id UUID NOT NULL,
    token TEXT UNIQUE NOT NULL,
    issued_on TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT fk_session_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
