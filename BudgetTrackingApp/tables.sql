create table expenses
(
    id          serial primary key,
    description TEXT        not null,
    amount      integer     not null,
    created     TIMESTAMPtz not null,
    category    TEXT,
    user_id     integer REFERENCES users (id)
);

create table users
(
    id       serial
        primary key,
    username TEXT not null,
    password TEXT not null
);

-- 1st migration

create table categories
(
    id   serial primary key,
    name TEXT not null
);

create table user_categories
(
    category_id integer references categories,
    user_id     integer references users
);

alter table expenses 
add category_id integer references categories;

-- migration to Heroku
create table expenses
(
    id          serial
        primary key,
    description text             not null,
    amount      double precision not null,
    created     timestamp        not null,
    user_id     integer
        references users,
    category_id integer
        references categories
);

-- alter table expenses ??
--     owner to db;

create table users
(
    id       serial
        primary key,
    username text not null,
    password text not null
);

create table categories
(
    id   serial
        primary key,
    name text not null
);

create table user_categories
(
    category_id integer
        references categories,
    user_id     integer
        references users
);