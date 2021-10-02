create database test_db;
create database sessions;

drop database if exists test_db;
select count(message_db_id) from messages;

create table public.chats (
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    update_time timestamp default CURRENT_TIMESTAMP,
    getting_last_message_timestamp timestamp default null,
    last_time_checked timestamp default null,
    last_message_id bigint default 1,
    name text,
    username text,
    pair_username text,
    is_group bool default false,
    is_channel bool default false,
    description text,
    pair_id bigint default null,
    pair_id_checked bool default null,
	banned bool default false,
    primary key (id)
);

create table public.users (
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    update_time timestamp default CURRENT_TIMESTAMP,
    first_name text,
    last_name text,
    username text,
    bio text,
    primary key (id)
);

create table public.messages(
    message_db_id bigserial,
    adding_time timestamp default CURRENT_TIMESTAMP,
    message_timestamp timestamp not null,
    id bigint not null,
    chat_id bigint,
    user_id bigint default null,
    reply_to bigint default null,
    thread_start bigint default null,
    media_group_id bigint default null,
    forward_from_id bigint,
    forward_from_message_id bigint,
    text text default null,
    media jsonb,
    formatting jsonb,
    media_costyl text,
    formatting_costyl text,
    primary key (message_timestamp, chat_id, id),
)  PARTITION BY RANGE (message_timestamp);

create table chats_buffer(
    id bigint,
    getting_last_message_timestamp timestamp, 
    last_message_id bigint,
    primary key (id)
)

create index messages_db_id ON messages (message_db_id);

CREATE TABLE messages_2014_m01 PARTITION OF messages FOR VALUES FROM ('2014-01-01') TO ('2014-02-01');
CREATE TABLE messages_2014_m02 PARTITION OF messages FOR VALUES FROM ('2014-02-01') TO ('2014-03-01');
CREATE TABLE messages_2014_m03 PARTITION OF messages FOR VALUES FROM ('2014-03-01') TO ('2014-04-01');
CREATE TABLE messages_2014_m04 PARTITION OF messages FOR VALUES FROM ('2014-04-01') TO ('2014-05-01');
CREATE TABLE messages_2014_m05 PARTITION OF messages FOR VALUES FROM ('2014-05-01') TO ('2014-06-01');
CREATE TABLE messages_2014_m06 PARTITION OF messages FOR VALUES FROM ('2014-06-01') TO ('2014-07-01');
CREATE TABLE messages_2014_m07 PARTITION OF messages FOR VALUES FROM ('2014-07-01') TO ('2014-08-01');
CREATE TABLE messages_2014_m08 PARTITION OF messages FOR VALUES FROM ('2014-08-01') TO ('2014-09-01');
CREATE TABLE messages_2014_m09 PARTITION OF messages FOR VALUES FROM ('2014-09-01') TO ('2014-10-01');
CREATE TABLE messages_2014_m10 PARTITION OF messages FOR VALUES FROM ('2014-10-01') TO ('2014-11-01');
CREATE TABLE messages_2014_m11 PARTITION OF messages FOR VALUES FROM ('2014-11-01') TO ('2014-12-01');
CREATE TABLE messages_2014_m12 PARTITION OF messages FOR VALUES FROM ('2014-12-01') TO ('2015-01-01');
CREATE TABLE messages_2015_m01 PARTITION OF messages FOR VALUES FROM ('2015-01-01') TO ('2015-02-01');
CREATE TABLE messages_2015_m02 PARTITION OF messages FOR VALUES FROM ('2015-02-01') TO ('2015-03-01');
CREATE TABLE messages_2015_m03 PARTITION OF messages FOR VALUES FROM ('2015-03-01') TO ('2015-04-01');
CREATE TABLE messages_2015_m04 PARTITION OF messages FOR VALUES FROM ('2015-04-01') TO ('2015-05-01');
CREATE TABLE messages_2015_m05 PARTITION OF messages FOR VALUES FROM ('2015-05-01') TO ('2015-06-01');
CREATE TABLE messages_2015_m06 PARTITION OF messages FOR VALUES FROM ('2015-06-01') TO ('2015-07-01');
CREATE TABLE messages_2015_m07 PARTITION OF messages FOR VALUES FROM ('2015-07-01') TO ('2015-08-01');
CREATE TABLE messages_2015_m08 PARTITION OF messages FOR VALUES FROM ('2015-08-01') TO ('2015-09-01');
CREATE TABLE messages_2015_m09 PARTITION OF messages FOR VALUES FROM ('2015-09-01') TO ('2015-10-01');
CREATE TABLE messages_2015_m10 PARTITION OF messages FOR VALUES FROM ('2015-10-01') TO ('2015-11-01');
CREATE TABLE messages_2015_m11 PARTITION OF messages FOR VALUES FROM ('2015-11-01') TO ('2015-12-01');
CREATE TABLE messages_2015_m12 PARTITION OF messages FOR VALUES FROM ('2015-12-01') TO ('2016-01-01');
CREATE TABLE messages_2016_m01 PARTITION OF messages FOR VALUES FROM ('2016-01-01') TO ('2016-02-01');
CREATE TABLE messages_2016_m02 PARTITION OF messages FOR VALUES FROM ('2016-02-01') TO ('2016-03-01');
CREATE TABLE messages_2016_m03 PARTITION OF messages FOR VALUES FROM ('2016-03-01') TO ('2016-04-01');
CREATE TABLE messages_2016_m04 PARTITION OF messages FOR VALUES FROM ('2016-04-01') TO ('2016-05-01');
CREATE TABLE messages_2016_m05 PARTITION OF messages FOR VALUES FROM ('2016-05-01') TO ('2016-06-01');
CREATE TABLE messages_2016_m06 PARTITION OF messages FOR VALUES FROM ('2016-06-01') TO ('2016-07-01');
CREATE TABLE messages_2016_m07 PARTITION OF messages FOR VALUES FROM ('2016-07-01') TO ('2016-08-01');
CREATE TABLE messages_2016_m08 PARTITION OF messages FOR VALUES FROM ('2016-08-01') TO ('2016-09-01');
CREATE TABLE messages_2016_m09 PARTITION OF messages FOR VALUES FROM ('2016-09-01') TO ('2016-10-01');
CREATE TABLE messages_2016_m10 PARTITION OF messages FOR VALUES FROM ('2016-10-01') TO ('2016-11-01');
CREATE TABLE messages_2016_m11 PARTITION OF messages FOR VALUES FROM ('2016-11-01') TO ('2016-12-01');
CREATE TABLE messages_2016_m12 PARTITION OF messages FOR VALUES FROM ('2016-12-01') TO ('2017-01-01');
CREATE TABLE messages_2017_m01 PARTITION OF messages FOR VALUES FROM ('2017-01-01') TO ('2017-02-01');
CREATE TABLE messages_2017_m02 PARTITION OF messages FOR VALUES FROM ('2017-02-01') TO ('2017-03-01');
CREATE TABLE messages_2017_m03 PARTITION OF messages FOR VALUES FROM ('2017-03-01') TO ('2017-04-01');
CREATE TABLE messages_2017_m04 PARTITION OF messages FOR VALUES FROM ('2017-04-01') TO ('2017-05-01');
CREATE TABLE messages_2017_m05 PARTITION OF messages FOR VALUES FROM ('2017-05-01') TO ('2017-06-01');
CREATE TABLE messages_2017_m06 PARTITION OF messages FOR VALUES FROM ('2017-06-01') TO ('2017-07-01');
CREATE TABLE messages_2017_m07 PARTITION OF messages FOR VALUES FROM ('2017-07-01') TO ('2017-08-01');
CREATE TABLE messages_2017_m08 PARTITION OF messages FOR VALUES FROM ('2017-08-01') TO ('2017-09-01');
CREATE TABLE messages_2017_m09 PARTITION OF messages FOR VALUES FROM ('2017-09-01') TO ('2017-10-01');
CREATE TABLE messages_2017_m10 PARTITION OF messages FOR VALUES FROM ('2017-10-01') TO ('2017-11-01');
CREATE TABLE messages_2017_m11 PARTITION OF messages FOR VALUES FROM ('2017-11-01') TO ('2017-12-01');
CREATE TABLE messages_2017_m12 PARTITION OF messages FOR VALUES FROM ('2017-12-01') TO ('2018-01-01');
CREATE TABLE messages_2018_m01 PARTITION OF messages FOR VALUES FROM ('2018-01-01') TO ('2018-02-01');
CREATE TABLE messages_2018_m02 PARTITION OF messages FOR VALUES FROM ('2018-02-01') TO ('2018-03-01');
CREATE TABLE messages_2018_m03 PARTITION OF messages FOR VALUES FROM ('2018-03-01') TO ('2018-04-01');
CREATE TABLE messages_2018_m04 PARTITION OF messages FOR VALUES FROM ('2018-04-01') TO ('2018-05-01');
CREATE TABLE messages_2018_m05 PARTITION OF messages FOR VALUES FROM ('2018-05-01') TO ('2018-06-01');
CREATE TABLE messages_2018_m06 PARTITION OF messages FOR VALUES FROM ('2018-06-01') TO ('2018-07-01');
CREATE TABLE messages_2018_m07 PARTITION OF messages FOR VALUES FROM ('2018-07-01') TO ('2018-08-01');
CREATE TABLE messages_2018_m08 PARTITION OF messages FOR VALUES FROM ('2018-08-01') TO ('2018-09-01');
CREATE TABLE messages_2018_m09 PARTITION OF messages FOR VALUES FROM ('2018-09-01') TO ('2018-10-01');
CREATE TABLE messages_2018_m10 PARTITION OF messages FOR VALUES FROM ('2018-10-01') TO ('2018-11-01');
CREATE TABLE messages_2018_m11 PARTITION OF messages FOR VALUES FROM ('2018-11-01') TO ('2018-12-01');
CREATE TABLE messages_2018_m12 PARTITION OF messages FOR VALUES FROM ('2018-12-01') TO ('2019-01-01');
CREATE TABLE messages_2019_m01 PARTITION OF messages FOR VALUES FROM ('2019-01-01') TO ('2019-02-01');
CREATE TABLE messages_2019_m02 PARTITION OF messages FOR VALUES FROM ('2019-02-01') TO ('2019-03-01');
CREATE TABLE messages_2019_m03 PARTITION OF messages FOR VALUES FROM ('2019-03-01') TO ('2019-04-01');
CREATE TABLE messages_2019_m04 PARTITION OF messages FOR VALUES FROM ('2019-04-01') TO ('2019-05-01');
CREATE TABLE messages_2019_m05 PARTITION OF messages FOR VALUES FROM ('2019-05-01') TO ('2019-06-01');
CREATE TABLE messages_2019_m06 PARTITION OF messages FOR VALUES FROM ('2019-06-01') TO ('2019-07-01');
CREATE TABLE messages_2019_m07 PARTITION OF messages FOR VALUES FROM ('2019-07-01') TO ('2019-08-01');
CREATE TABLE messages_2019_m08 PARTITION OF messages FOR VALUES FROM ('2019-08-01') TO ('2019-09-01');
CREATE TABLE messages_2019_m09 PARTITION OF messages FOR VALUES FROM ('2019-09-01') TO ('2019-10-01');
CREATE TABLE messages_2019_m10 PARTITION OF messages FOR VALUES FROM ('2019-10-01') TO ('2019-11-01');
CREATE TABLE messages_2019_m11 PARTITION OF messages FOR VALUES FROM ('2019-11-01') TO ('2019-12-01');
CREATE TABLE messages_2019_m12 PARTITION OF messages FOR VALUES FROM ('2019-12-01') TO ('2020-01-01');
CREATE TABLE messages_2020_m01 PARTITION OF messages FOR VALUES FROM ('2020-01-01') TO ('2020-02-01');
CREATE TABLE messages_2020_m02 PARTITION OF messages FOR VALUES FROM ('2020-02-01') TO ('2020-03-01');
CREATE TABLE messages_2020_m03 PARTITION OF messages FOR VALUES FROM ('2020-03-01') TO ('2020-04-01');
CREATE TABLE messages_2020_m04 PARTITION OF messages FOR VALUES FROM ('2020-04-01') TO ('2020-05-01');
CREATE TABLE messages_2020_m05 PARTITION OF messages FOR VALUES FROM ('2020-05-01') TO ('2020-06-01');
CREATE TABLE messages_2020_m06 PARTITION OF messages FOR VALUES FROM ('2020-06-01') TO ('2020-07-01');
CREATE TABLE messages_2020_m07 PARTITION OF messages FOR VALUES FROM ('2020-07-01') TO ('2020-08-01');
CREATE TABLE messages_2020_m08 PARTITION OF messages FOR VALUES FROM ('2020-08-01') TO ('2020-09-01');
CREATE TABLE messages_2020_m09 PARTITION OF messages FOR VALUES FROM ('2020-09-01') TO ('2020-10-01');
CREATE TABLE messages_2020_m10 PARTITION OF messages FOR VALUES FROM ('2020-10-01') TO ('2020-11-01');
CREATE TABLE messages_2020_m11 PARTITION OF messages FOR VALUES FROM ('2020-11-01') TO ('2020-12-01');
CREATE TABLE messages_2020_m12 PARTITION OF messages FOR VALUES FROM ('2020-12-01') TO ('2021-01-01');
CREATE TABLE messages_2021_m01 PARTITION OF messages FOR VALUES FROM ('2021-01-01') TO ('2021-02-01');
CREATE TABLE messages_2021_m02 PARTITION OF messages FOR VALUES FROM ('2021-02-01') TO ('2021-03-01');
CREATE TABLE messages_2021_m03 PARTITION OF messages FOR VALUES FROM ('2021-03-01') TO ('2021-04-01');
CREATE TABLE messages_2021_m04 PARTITION OF messages FOR VALUES FROM ('2021-04-01') TO ('2021-05-01');
CREATE TABLE messages_2021_m05 PARTITION OF messages FOR VALUES FROM ('2021-05-01') TO ('2021-06-01');
CREATE TABLE messages_2021_m06 PARTITION OF messages FOR VALUES FROM ('2021-06-01') TO ('2021-07-01');
CREATE TABLE messages_2021_m07 PARTITION OF messages FOR VALUES FROM ('2021-07-01') TO ('2021-08-01');
CREATE TABLE messages_2021_m08 PARTITION OF messages FOR VALUES FROM ('2021-08-01') TO ('2021-09-01');
CREATE TABLE messages_2021_m09 PARTITION OF messages FOR VALUES FROM ('2021-09-01') TO ('2021-10-01');
CREATE TABLE messages_2021_m10 PARTITION OF messages FOR VALUES FROM ('2021-10-01') TO ('2021-11-01');
CREATE TABLE messages_2021_m11 PARTITION OF messages FOR VALUES FROM ('2021-11-01') TO ('2021-12-01');
CREATE TABLE messages_2021_m12 PARTITION OF messages FOR VALUES FROM ('2021-12-01') TO ('2022-01-01');
CREATE TABLE messages_2022_m01 PARTITION OF messages FOR VALUES FROM ('2022-01-01') TO ('2022-02-01');
CREATE TABLE messages_2022_m02 PARTITION OF messages FOR VALUES FROM ('2022-02-01') TO ('2022-03-01');
CREATE TABLE messages_2022_m03 PARTITION OF messages FOR VALUES FROM ('2022-03-01') TO ('2022-04-01');
CREATE TABLE messages_2022_m04 PARTITION OF messages FOR VALUES FROM ('2022-04-01') TO ('2022-05-01');
CREATE TABLE messages_2022_m05 PARTITION OF messages FOR VALUES FROM ('2022-05-01') TO ('2022-06-01');
CREATE TABLE messages_2022_m06 PARTITION OF messages FOR VALUES FROM ('2022-06-01') TO ('2022-07-01');
CREATE TABLE messages_2022_m07 PARTITION OF messages FOR VALUES FROM ('2022-07-01') TO ('2022-08-01');
CREATE TABLE messages_2022_m08 PARTITION OF messages FOR VALUES FROM ('2022-08-01') TO ('2022-09-01');
CREATE TABLE messages_2022_m09 PARTITION OF messages FOR VALUES FROM ('2022-09-01') TO ('2022-10-01');
CREATE TABLE messages_2022_m10 PARTITION OF messages FOR VALUES FROM ('2022-10-01') TO ('2022-11-01');
CREATE TABLE messages_2022_m11 PARTITION OF messages FOR VALUES FROM ('2022-11-01') TO ('2022-12-01');
CREATE TABLE messages_2022_m12 PARTITION OF messages FOR VALUES FROM ('2022-12-01') TO ('2023-01-01');
CREATE TABLE messages_2023_m01 PARTITION OF messages FOR VALUES FROM ('2023-01-01') TO ('2023-02-01');
CREATE TABLE messages_2023_m02 PARTITION OF messages FOR VALUES FROM ('2023-02-01') TO ('2023-03-01');
CREATE TABLE messages_2023_m03 PARTITION OF messages FOR VALUES FROM ('2023-03-01') TO ('2023-04-01');
CREATE TABLE messages_2023_m04 PARTITION OF messages FOR VALUES FROM ('2023-04-01') TO ('2023-05-01');
CREATE TABLE messages_2023_m05 PARTITION OF messages FOR VALUES FROM ('2023-05-01') TO ('2023-06-01');
CREATE TABLE messages_2023_m06 PARTITION OF messages FOR VALUES FROM ('2023-06-01') TO ('2023-07-01');
CREATE TABLE messages_2023_m07 PARTITION OF messages FOR VALUES FROM ('2023-07-01') TO ('2023-08-01');
CREATE TABLE messages_2023_m08 PARTITION OF messages FOR VALUES FROM ('2023-08-01') TO ('2023-09-01');
CREATE TABLE messages_2023_m09 PARTITION OF messages FOR VALUES FROM ('2023-09-01') TO ('2023-10-01');
CREATE TABLE messages_2023_m10 PARTITION OF messages FOR VALUES FROM ('2023-10-01') TO ('2023-11-01');
CREATE TABLE messages_2023_m11 PARTITION OF messages FOR VALUES FROM ('2023-11-01') TO ('2023-12-01');
CREATE TABLE messages_2023_m12 PARTITION OF messages FOR VALUES FROM ('2023-12-01') TO ('2024-01-01');
CREATE TABLE messages_2024_m01 PARTITION OF messages FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
CREATE TABLE messages_2024_m02 PARTITION OF messages FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');
CREATE TABLE messages_2024_m03 PARTITION OF messages FOR VALUES FROM ('2024-03-01') TO ('2024-04-01');
CREATE TABLE messages_2024_m04 PARTITION OF messages FOR VALUES FROM ('2024-04-01') TO ('2024-05-01');
CREATE TABLE messages_2024_m05 PARTITION OF messages FOR VALUES FROM ('2024-05-01') TO ('2024-06-01');
CREATE TABLE messages_2024_m06 PARTITION OF messages FOR VALUES FROM ('2024-06-01') TO ('2024-07-01');
CREATE TABLE messages_2024_m07 PARTITION OF messages FOR VALUES FROM ('2024-07-01') TO ('2024-08-01');
CREATE TABLE messages_2024_m08 PARTITION OF messages FOR VALUES FROM ('2024-08-01') TO ('2024-09-01');
CREATE TABLE messages_2024_m09 PARTITION OF messages FOR VALUES FROM ('2024-09-01') TO ('2024-10-01');
CREATE TABLE messages_2024_m10 PARTITION OF messages FOR VALUES FROM ('2024-10-01') TO ('2024-11-01');
CREATE TABLE messages_2024_m11 PARTITION OF messages FOR VALUES FROM ('2024-11-01') TO ('2024-12-01');
CREATE TABLE messages_2024_m12 PARTITION OF messages FOR VALUES FROM ('2024-12-01') TO ('2025-01-01');
CREATE TABLE messages_2025_m01 PARTITION OF messages FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');
CREATE TABLE messages_2025_m02 PARTITION OF messages FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');
CREATE TABLE messages_2025_m03 PARTITION OF messages FOR VALUES FROM ('2025-03-01') TO ('2025-04-01');
CREATE TABLE messages_2025_m04 PARTITION OF messages FOR VALUES FROM ('2025-04-01') TO ('2025-05-01');
CREATE TABLE messages_2025_m05 PARTITION OF messages FOR VALUES FROM ('2025-05-01') TO ('2025-06-01');
CREATE TABLE messages_2025_m06 PARTITION OF messages FOR VALUES FROM ('2025-06-01') TO ('2025-07-01');
CREATE TABLE messages_2025_m07 PARTITION OF messages FOR VALUES FROM ('2025-07-01') TO ('2025-08-01');
CREATE TABLE messages_2025_m08 PARTITION OF messages FOR VALUES FROM ('2025-08-01') TO ('2025-09-01');
CREATE TABLE messages_2025_m09 PARTITION OF messages FOR VALUES FROM ('2025-09-01') TO ('2025-10-01');
CREATE TABLE messages_2025_m10 PARTITION OF messages FOR VALUES FROM ('2025-10-01') TO ('2025-11-01');
CREATE TABLE messages_2025_m11 PARTITION OF messages FOR VALUES FROM ('2025-11-01') TO ('2025-12-01');
CREATE TABLE messages_2025_m12 PARTITION OF messages FOR VALUES FROM ('2025-12-01') TO ('2026-01-01');
CREATE TABLE messages_2026_m01 PARTITION OF messages FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
CREATE TABLE messages_2026_m02 PARTITION OF messages FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
CREATE TABLE messages_2026_m03 PARTITION OF messages FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE messages_2026_m04 PARTITION OF messages FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE messages_2026_m05 PARTITION OF messages FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE messages_2026_m06 PARTITION OF messages FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');
CREATE TABLE messages_2026_m07 PARTITION OF messages FOR VALUES FROM ('2026-07-01') TO ('2026-08-01');
CREATE TABLE messages_2026_m08 PARTITION OF messages FOR VALUES FROM ('2026-08-01') TO ('2026-09-01');
CREATE TABLE messages_2026_m09 PARTITION OF messages FOR VALUES FROM ('2026-09-01') TO ('2026-10-01');
CREATE TABLE messages_2026_m10 PARTITION OF messages FOR VALUES FROM ('2026-10-01') TO ('2026-11-01');
CREATE TABLE messages_2026_m11 PARTITION OF messages FOR VALUES FROM ('2026-11-01') TO ('2026-12-01');
CREATE TABLE messages_2026_m12 PARTITION OF messages FOR VALUES FROM ('2026-12-01') TO ('2027-01-01');
CREATE TABLE messages_2027_m01 PARTITION OF messages FOR VALUES FROM ('2027-01-01') TO ('2027-02-01');
CREATE TABLE messages_2027_m02 PARTITION OF messages FOR VALUES FROM ('2027-02-01') TO ('2027-03-01');
CREATE TABLE messages_2027_m03 PARTITION OF messages FOR VALUES FROM ('2027-03-01') TO ('2027-04-01');
CREATE TABLE messages_2027_m04 PARTITION OF messages FOR VALUES FROM ('2027-04-01') TO ('2027-05-01');
CREATE TABLE messages_2027_m05 PARTITION OF messages FOR VALUES FROM ('2027-05-01') TO ('2027-06-01');
CREATE TABLE messages_2027_m06 PARTITION OF messages FOR VALUES FROM ('2027-06-01') TO ('2027-07-01');
CREATE TABLE messages_2027_m07 PARTITION OF messages FOR VALUES FROM ('2027-07-01') TO ('2027-08-01');
CREATE TABLE messages_2027_m08 PARTITION OF messages FOR VALUES FROM ('2027-08-01') TO ('2027-09-01');
CREATE TABLE messages_2027_m09 PARTITION OF messages FOR VALUES FROM ('2027-09-01') TO ('2027-10-01');
CREATE TABLE messages_2027_m10 PARTITION OF messages FOR VALUES FROM ('2027-10-01') TO ('2027-11-01');
CREATE TABLE messages_2027_m11 PARTITION OF messages FOR VALUES FROM ('2027-11-01') TO ('2027-12-01');
CREATE TABLE messages_2027_m12 PARTITION OF messages FOR VALUES FROM ('2027-12-01') TO ('2028-01-01');
CREATE TABLE messages_2028_m01 PARTITION OF messages FOR VALUES FROM ('2028-01-01') TO ('2028-02-01');
CREATE TABLE messages_2028_m02 PARTITION OF messages FOR VALUES FROM ('2028-02-01') TO ('2028-03-01');
CREATE TABLE messages_2028_m03 PARTITION OF messages FOR VALUES FROM ('2028-03-01') TO ('2028-04-01');
CREATE TABLE messages_2028_m04 PARTITION OF messages FOR VALUES FROM ('2028-04-01') TO ('2028-05-01');
CREATE TABLE messages_2028_m05 PARTITION OF messages FOR VALUES FROM ('2028-05-01') TO ('2028-06-01');
CREATE TABLE messages_2028_m06 PARTITION OF messages FOR VALUES FROM ('2028-06-01') TO ('2028-07-01');
CREATE TABLE messages_2028_m07 PARTITION OF messages FOR VALUES FROM ('2028-07-01') TO ('2028-08-01');
CREATE TABLE messages_2028_m08 PARTITION OF messages FOR VALUES FROM ('2028-08-01') TO ('2028-09-01');
CREATE TABLE messages_2028_m09 PARTITION OF messages FOR VALUES FROM ('2028-09-01') TO ('2028-10-01');
CREATE TABLE messages_2028_m10 PARTITION OF messages FOR VALUES FROM ('2028-10-01') TO ('2028-11-01');
CREATE TABLE messages_2028_m11 PARTITION OF messages FOR VALUES FROM ('2028-11-01') TO ('2028-12-01');
CREATE TABLE messages_2028_m12 PARTITION OF messages FOR VALUES FROM ('2028-12-01') TO ('2029-01-01');
CREATE TABLE messages_2029_m01 PARTITION OF messages FOR VALUES FROM ('2029-01-01') TO ('2029-02-01');
CREATE TABLE messages_2029_m02 PARTITION OF messages FOR VALUES FROM ('2029-02-01') TO ('2029-03-01');
CREATE TABLE messages_2029_m03 PARTITION OF messages FOR VALUES FROM ('2029-03-01') TO ('2029-04-01');
CREATE TABLE messages_2029_m04 PARTITION OF messages FOR VALUES FROM ('2029-04-01') TO ('2029-05-01');
CREATE TABLE messages_2029_m05 PARTITION OF messages FOR VALUES FROM ('2029-05-01') TO ('2029-06-01');
CREATE TABLE messages_2029_m06 PARTITION OF messages FOR VALUES FROM ('2029-06-01') TO ('2029-07-01');
CREATE TABLE messages_2029_m07 PARTITION OF messages FOR VALUES FROM ('2029-07-01') TO ('2029-08-01');
CREATE TABLE messages_2029_m08 PARTITION OF messages FOR VALUES FROM ('2029-08-01') TO ('2029-09-01');
CREATE TABLE messages_2029_m09 PARTITION OF messages FOR VALUES FROM ('2029-09-01') TO ('2029-10-01');
CREATE TABLE messages_2029_m10 PARTITION OF messages FOR VALUES FROM ('2029-10-01') TO ('2029-11-01');
CREATE TABLE messages_2029_m11 PARTITION OF messages FOR VALUES FROM ('2029-11-01') TO ('2029-12-01');
CREATE TABLE messages_2029_m12 PARTITION OF messages FOR VALUES FROM ('2029-12-01') TO ('2030-01-01');


CREATE OR REPLACE FUNCTION add_message(_message_timestamp timestamp,_message_id bigint, _chat_id bigint, _user_id bigint,
    _reply_to bigint,_thread_start bigint,_media_group_id bigint,_forward_from_id BIGINT,_forward_from_message_id bigint,
     _text text, _media jsonb, _formatting jsonb) RETURNS void as
$$
    begin
           if _message_timestamp>'2021-05-01 00:00:00.000000' then
            insert into public.messages (message_timestamp,
                                         id,
                                         chat_id,
                                         user_id,
                                         reply_to,
                                         thread_start,
                                         media_group_id,
                                         forward_from_id,
                                         forward_from_message_id,
                                         text,
                                         media,
                                         formatting, vectorised_text_my_default) values (_message_timestamp,
                                                             _message_id,
                                                             _chat_id,
                                                             _user_id,
                                                             _reply_to,
                                                             _thread_start,
                                                             _media_group_id,
                                                             _forward_from_id,
                                                             _forward_from_message_id,
                                                             _text,
                                                             _media,
                                                             _formatting, to_tsvector('my_default', _text)) on conflict do nothing;
            else
                    insert into public.messages (message_timestamp,
                                         id,
                                         chat_id,
                                         user_id,
                                         reply_to,
                                         thread_start,
                                         media_group_id,
                                         forward_from_id,
                                         forward_from_message_id,
                                         text,
                                         media,
                                         formatting, parsed) values (_message_timestamp,
                                                             _message_id,
                                                             _chat_id,
                                                             _user_id,
                                                             _reply_to,
                                                             _thread_start,
                                                             _media_group_id,
                                                             _forward_from_id,
                                                             _forward_from_message_id,
                                                             _text,
                                                             _media,
                                                             _formatting,false) on conflict do nothing;
        end if;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_user(_user_id bigint,sender_username text,sender_first_name text, sender_last_name text) RETURNS void as
$$
    begin
            insert into users (id, first_name, last_name, username) values (_user_id,sender_first_name,sender_last_name,sender_username);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_chat(_chat_id bigint,_title text,_username text, pair_chat_id bigint,_is_group bool,_is_channel bool,_finder text) RETURNS void as
$$
    begin
            insert into chats (id, name , username, pair_id, is_group,is_channel, finders) values (_chat_id,_title,_username,pair_chat_id,_is_group,_is_channel, ARRAY[_finder]);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION set_chat_checked(_chat_id bigint) RETURNS void as
$$
    begin
            update chats SET getting_last_message_timestamp = current_timestamp where _chat_id=chats.id;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION check_user(user_id bigint) RETURNS bool as
$$
    begin
        return exists(select 1 from  users where id=user_id);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION check_chat(chat_id bigint) RETURNS bool as
$$
    begin
        return exists(select 1 from  chats where id=chat_id);
    end;
$$ LANGUAGE plpgsql;

create  or replace function  get_unupdated_chats() returns table (_id bigint,
                                                                            _pair_id bigint,
                                                                            _last_message_id bigint,
                                                                            _getting_last_message_timestamp timestamp,
                                                                            _pair_id_checked bool,
                                                                            _username text,
                                                                            _pair_username text,
                                                                            _finders text[]) as
$$
    begin
        return query update chats set last_time_checked = current_timestamp, has_actual_order=true where
            (has_actual_order is null or not has_actual_order)
                and finders is not null
                and (is_group or (is_channel and pair_id_checked))
                and not banned
                returning id,
                pair_id,
                last_message_id,
                getting_last_message_timestamp,
                pair_id_checked,
                username,
                pair_username,
                    finders;
    end;
$$LANGUAGE plpgsql;

create  or replace function  get_chats_for_history(dt timestamp) returns table (_id bigint,
                                                                            _pair_id bigint,
                                                                            _last_message_id bigint,
                                                                            _getting_last_message_timestamp timestamp,
                                                                            _pair_id_checked bool,
                                                                            _username text,
                                                                            _pair_username text) as
$$
    begin
        return query update chats set last_time_checked = current_timestamp  where getting_last_message_timestamp is null and (last_time_checked is null or last_time_checked <dt) and not banned
                returning id,
                pair_id,
                last_message_id,
                getting_last_message_timestamp,
                pair_id_checked,
                username,
                pair_username;
    end;
$$LANGUAGE plpgsql;

create  or replace function  get_last_message_id(_chat_id bigint) returns bigint as
$$
    begin
        return (select max(id) from messages where chat_id=_chat_id);
    end;
$$LANGUAGE plpgsql;

create  or replace function  get_groups_for_history(dt timestamp) returns table (_id bigint,
                                                                            _pair_id bigint,
                                                                            _last_message_id bigint,
                                                                            _getting_last_message_timestamp timestamp,
                                                                            _pair_id_checked bool,
                                                                            _username text,
                                                                            _pair_username text) as
$$
    begin
        return query update chats set last_time_checked = current_timestamp  where getting_last_message_timestamp is null and is_group and (last_time_checked is null or last_time_checked <dt) and not banned
                returning id,
                pair_id,
                last_message_id,
                getting_last_message_timestamp,
                pair_id_checked,
                username,
                pair_username;
    end;
$$LANGUAGE plpgsql;

create  or replace function  get_unrequested_channels(dt timestamp) returns table (_id bigint,
                                                                            _pair_id bigint,
                                                                            _last_message_id bigint,
                                                                            _getting_last_message_timestamp timestamp,
                                                                            _pair_id_checked bool,
                                                                            _username text,
                                                                            _pair_username text) as
$$
    begin
        return query update chats set last_time_checked = current_timestamp  where chats.pair_id_checked=false and (last_time_checked is null or last_time_checked <dt) and not banned
                returning id,
                pair_id,
                last_message_id,
                getting_last_message_timestamp,
                pair_id_checked,
                username,
                pair_username;
    end;
$$LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION message_protect() RETURNS trigger as
$$
    begin
            update chats SET getting_last_message_timestamp = CURRENT_TIMESTAMP, last_message_id=new.id where chats.id=new.chat_id;
            return null;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION users_protect() RETURNS trigger as
$$
    begin
        if not exists(select 1 from users where id = new.id) then
            return new;
        else
            return null;
        end if;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION chats_protect() RETURNS trigger as
$$
    begin
        if not exists(select 1 from chats where id = new.id) then
            if new.is_channel then
                new.pair_id_checked = false;
            end if;
            update public.chats set pair_id_checked=true, pair_id=new.id, pair_username=new.username where id = new.pair_id;
            new.pair_username = (select username from chats where chats.id=new.pair_id);
            return new;
        else
            if new.id=0 THEN
                update public.chats set pair_id_checked=true where id = new.pair_id;
            end if;
			update chats set finders=finders||new.finders where id = new.id and not new.finders[0]='' and not new.finders[0] = ANY (finders);
			update chats set finders=new.finders where id = new.id and (finders is null or not array_length(finders,1)>0);
            return null;
        end if;
    end;
$$ LANGUAGE plpgsql;



CREATE TRIGGER on_insert_to_messages after INSERT on public.messages FOR EACH ROW execute PROCEDURE message_protect();
CREATE TRIGGER on_insert_to_users before INSERT on public.users FOR EACH ROW execute PROCEDURE users_protect();
CREATE TRIGGER on_insert_to_chats before INSERT on public.chats FOR EACH ROW execute PROCEDURE chats_protect();

insert into chats (id,username,is_channel) values (1264079104, 'ssleg', true);


create index messages_user_id_index on messages (user_id);
create index messages_chat_id_index on messages (chat_id);



create table search_tests(
    username text,
    id bigint,
    chat_id bigint,
    text text,
    vectorised_text tsvector
);

create or replace function vect() returns void as
$$
    begin
        update search_tests set vectorised_text=to_tsvector('russian',text) where vectorised_text is null;
    end;
$$ LANGUAGE plpgsql;


create or replace function simple_search(request text, lim int) returns table (link text) as
$$
    begin
        return query SELECT 'https://t.me/'||COALESCE(username,'c/'||(chat_id::text))||'/'||id::text from search_tests
        WHERE to_tsquery('russian',request)::tsquery @@ vectorised_text
        LIMIT lim;
    end;
$$ LANGUAGE plpgsql;

create or replace function simple_search_selective(request text, lim int,chats bigint[]) returns table (link text) as
$$
    begin
        return query SELECT 'https://t.me/'||COALESCE(username,'c/'||(chat_id::text))||'/'||id::text from search_tests
        WHERE to_tsquery('russian',request)::tsquery @@ vectorised_text and array_position(chats, chat_id)
        LIMIT lim;
    end;
$$ LANGUAGE plpgsql;



create extension rum;
create extension rusmorph;
create extension hunspell_ru_ru_aot;


create table temp(
    id bigint,
    data1 bigint
);


CREATE TEXT SEARCH CONFIGURATION public.my_default ( COPY = pg_catalog.russian );
ALTER TEXT SEARCH CONFIGURATION public.my_default
    DROP MAPPING FOR email, url,host, url_path, sfloat, float, int,uint;

CREATE TEXT SEARCH CONFIGURATION public.combo ( COPY = pg_catalog.russian );
alter text search configuration public.combo alter mapping for word with rusmorph, russian_aot_hunspell;
ALTER TEXT SEARCH CONFIGURATION public.combo
    DROP MAPPING FOR email, url,host, url_path, sfloat, float, int,uint;

alter table messages add column parsed bool default true;
alter table messages add column vectorised_text_combo tsvector;
alter table messages add column vectorised_text_my_default tsvector;



CREATE OR REPLACE FUNCTION create_fulltext_data(count int) RETURNS bool as
$$
    declare
        min_id bigint;
        value bigint;
        value2 bigint;
        result bool;
    begin
        min_id = (select min(data1) from temp where id=0);
        value=min_id+count;
        update messages set vectorised_text_combo=parse_combo(replace(text)),
                            vectorised_text_my_default=parse_default(replace(text)),
                            parsed=true where not parsed and message_db_id>=min_id and message_db_id<value;
        value2 = (select max(message_db_id) from  messages);
        result = true;
        if value>(value2) then
            value = value2;
            result = false;
        end if;
        update temp set data1=value where id=0;
        return  result;
    end;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION force_parse() RETURNS void as
$$
    declare
        min_id bigint;
    begin
        min_id = (select min(data1) from temp where id=0);
        update messages set vectorised_text_combo='', vectorised_text_my_default=parse_default(replace(text)),
                            parsed=true where not parsed and message_db_id=min_id;
        update temp set data1=min_id+1 where id=0;
    end;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION parse_combo(text text) RETURNS tsvector as
$$
    begin
        return to_tsvector('combo', text);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION parse_default(text text) RETURNS tsvector as
$$
    begin
        return to_tsvector('my_default', text);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION replace(text text) RETURNS text as
$$
    declare
        pattern text;
    begin
        pattern = '[^0-9абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ ,abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ\.:/?\^\\=\-_%\$#@!\n"&]*';
        return regexp_replace(text, pattern, '','g');
    end;
$$ LANGUAGE plpgsql;

drop trigger on_insert_to_messages ON messages;

CREATE OR REPLACE FUNCTION update_last_message(_chat_id bigint,_last_message_id bigint) RETURNS void as
$$
    begin
        if exists(select id from chats where id=_chat_id) then
            update chats_buffer set getting_last_message_timestamp=CURRENT_TIMESTAMP, last_message_id=_last_message_id where id=_chat_id;
        else
            insert into chats_buffer (id,getting_last_message_timestamp,last_message_id) values (_chat_id,CURRENT_TIMESTAMP,_last_message_id);
        end if;

    end;
$$ LANGUAGE plpgsql;


create  or replace function  ban() returns bool as
$$
    declare
        result bool;
        _id bigint;
        pattern text;
    begin
        pattern='[абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ]';
        _id = (select id from chats where banchecked is null limit 1);
        if _id is not null THEN
            update chats set banchecked = true where id=_id;
            if  exists(select 1 from messages where chat_id=_id) and not exists(select 1 from messages where chat_id=_id and text ~ pattern) then
                update chats set banned = true where id=_id;
            end if;
            return true;
        else
            return false;
        end if;
    end;
$$LANGUAGE plpgsql;

alter table chats add column banchecked bool default null;

CREATE OR REPLACE FUNCTION order_executed(_chat_id bigint,_finder text) RETURNS void as
$$
    begin
        update chats set has_actual_order=false where id=_chat_id ;
        update chats set finders=finders||array[_finder] where
                                       id=_chat_id and not _finder=ANY(finders) and not _finder='';
    end;
$$ LANGUAGE plpgsql;


create  or replace function  get_chats() returns table (_id bigint,
                                                                            _pair_id bigint,
                                                                            _last_message_id bigint,
                                                                            _pair_last_message_id bigint,
                                                                            _getting_last_message_timestamp timestamp,
                                                                            _pair_id_checked bool,
                                                                            _username text,
                                                                            _pair_username text) as
$$
    begin
        return query update chats set last_time_checked = current_timestamp, has_actual_order=true where has_actual_order is null or not has_actual_order
                returning id,
                pair_id,
                last_message_id,
                (select last_message_id from chats where id=pair_id),
                getting_last_message_timestamp,
                pair_id_checked,
                username,
                pair_username;
    end;
$$LANGUAGE plpgsql;

alter table chats add column finders text[]


create or replace function search_name_period(request text,dt1 timestamp,dt2 timestamp, lim int, _is_group bool, _is_channel bool) returns table (url text, te text) as
$$
    declare
        query tsquery;
    begin
        query=to_tsquery('my_default',replace(request))::tsquery;
        if (_is_group and _is_channel) then
            return query
                with sel as(
                SELECT username, chat_id, messages.id as id, messages.text as te,(vectorised_text_my_default <=> query)::float4 as rank from messages inner join chats c on c.id = messages.chat_id
            WHERE query @@ vectorised_text_my_default and message_timestamp>=dt1 and message_timestamp<dt2 order by rank
            LIMIT lim) select ('https://t.me/'||COALESCE(sel.username,'c/'||(sel.chat_id::text))||'/'||sel.id)::text, sel.te from sel;
        else
            return query
            with sel as(
                SELECT username, chat_id, messages.id as id, messages.text as te,(vectorised_text_my_default <=> query)::float4 as rank  from messages inner join chats c on c.id = messages.chat_id
            WHERE query @@ vectorised_text_my_default and message_timestamp>=dt1 and message_timestamp<dt2 and c.is_channel=_is_channel and c.is_group=_is_group order by rank
            LIMIT lim) select ('https://t.me/'||COALESCE(sel.username,'c/'||(sel.chat_id::text))||'/'||sel.id)::text, sel.te from sel;
        end if;
    end;
$$ LANGUAGE plpgsql;

create or replace function search_period(request text,dt1 timestamp,dt2 timestamp, lim int, _is_group bool, _is_channel bool) returns table (url text, te text) as
$$
    declare
        query tsquery;
    begin
        query=to_tsquery('combo',replace(request))::tsquery;
        if (_is_group and _is_channel) then
            return query
                with sel as(
                SELECT username, chat_id, messages.id as id, messages.text as te,(vectorised_text_combo <=> query)::float4 as rank from messages inner join chats c on c.id = messages.chat_id
            WHERE query @@ vectorised_text_combo and message_timestamp>=dt1 and message_timestamp<dt2 order by rank
            LIMIT lim) select ('https://t.me/'||COALESCE(sel.username,'c/'||(sel.chat_id::text))||'/'||sel.id)::text, sel.te from sel;
        else
            return query
            with sel as(
                SELECT username, chat_id, messages.id as id, messages.text as te,(vectorised_text_combo <=> query)::float4 as rank  from messages inner join chats c on c.id = messages.chat_id
            WHERE query @@ vectorised_text_combo and message_timestamp>=dt1 and message_timestamp<dt2 and c.is_channel=_is_channel and c.is_group=_is_group order by rank
            LIMIT lim) select ('https://t.me/'||COALESCE(sel.username,'c/'||(sel.chat_id::text))||'/'||sel.id)::text, sel.te from sel;
        end if;
    end;
$$ LANGUAGE plpgsql;

create or replace function search_in_channel(request text,dt1 timestamp,dt2 timestamp, lim int, _is_group bool, _is_channel bool, ids bigint[] ) returns table (link text, text text) as
$$
    begin
        if _is_group and _is_channel then
        return query SELECT ('https://t.me/'||COALESCE(username,'c/'||(chat_id::text))||'/'||messages.id)::text, messages.text from messages inner join chats c on c.id = messages.chat_id
        WHERE to_tsquery('combo',request)::tsquery @@ vectorised_text_combo and message_timestamp>=dt1 and message_timestamp<dt2 and chat_id=ANY(ids)
        LIMIT lim;
            else
                    return query SELECT ('https://t.me/'||COALESCE(username,'c/'||(chat_id::text))||'/'||messages.id)::text, messages.text from messages inner join chats c on c.id = messages.chat_id
        WHERE to_tsquery('combo',request)::tsquery @@ vectorised_text_combo and message_timestamp>=dt1 and message_timestamp<dt2 and c.is_channel=_is_channel and c.is_group=_is_group and chat_id=ANY(ids)
        LIMIT lim;
        end if;
    end;
$$ LANGUAGE plpgsql;

create or replace function get_user_messages(_user_id bigint, lim int) returns table (link text, text text) as
$$
    begin
        return query SELECT ('https://t.me/'||COALESCE(username,'c/'||(chat_id::text))||'/'||messages.id)::text, messages.text from messages inner join chats c on c.id = messages.chat_id
        WHERE user_id=_user_id LIMIT lim;
    end;
$$ LANGUAGE plpgsql;

create index mess_combo_index ON messages  using rum(vectorised_text_combo);
create index mess_my_default_index ON messages  using rum(vectorised_text_my_default);
create index chats_is_group on chats (is_group);
create index chats_is_channel on chats (is_channel);

create table Requests(
    id bigserial,
    finder text,
    counter int,
    banned bool,
    primary key (id)
)


create index messages_id_index on messages (chat_id,id);

create or replace function last_messages_processing() returns void as
$$
    declare
        current_id bigint;
        next_current_id bigint;
    begin
        current_id=(select max(data1) from temp where id=1);
        next_current_id = (select max(message_db_id) from messages);
        update chats c set test = (select max(id) from messages m where chat_id=c.id and message_db_id>current_id) where not banned;
        update temp set data1=next_current_id where id=1;
    end;
$$ LANGUAGE plpgsql;


create table queries(
    id bigserial,
    query tsquery,
    client_id bigint,
    primary key (id)
);


create table spotter(
    id bigserial,
    time timestamp default current_timestamp not null ,
    query_id bigint,
    requester_id text,
    client_id bigint not null,
    link text,
    preview_text text,
    data tsvector,
    need_trigger bool not null default true,
    primary key (id),
    foreign key (query_id) references queries(id)
);

create or replace function add_query(request text, _client_id bigint) returns void as
$$
    begin
        insert into queries (query,client_id) values (to_tsquery('my_default',request), _client_id);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION spotter_protect() RETURNS trigger as
$$
    DECLARE
        temp text;
    begin
        if not new.need_trigger then
            temp = pg_notify('test','"link":'||new.link||'; "text": '||new.preview_text||'"client":'||new.client_id||'"requester":'||new.requester_id||'"query_id":'||new.query_id);
            return new;
        end if;
        insert into spotter(query_id, requester_id, client_id, data,need_trigger,preview_text,link,teal_time)  (
        with q as(
        select id,query,client_id from public.queries),
        res as(
            select (q.query @@ new.data) as result, q.id as q_id, q.client_id as client_id,new.requester_id as requester_id, new.data as data, new.teal_time as rt from q
        ) select res.q_id,res.requester_id,res.client_id,res.data,false,new.preview_text,new.link,res.rt from res where result);

        return null;
    end;
$$ LANGUAGE plpgsql;
drop trigger on_insert_to_spotter on spotter;
CREATE TRIGGER on_insert_to_spotter before INSERT on public.spotter FOR EACH ROW execute PROCEDURE spotter_protect();

CREATE OR REPLACE FUNCTION after_messages() RETURNS trigger as
$$
    DECLARE
        temp text;
    begin
        if EXTRACT(EPOCH FROM (current_timestamp - new.message_timestamp))<43200 then
                    with sel as (select username,name,pair_username,id,new.text as text,new.id as mess_id, new.message_timestamp as real_time from chats where id= new.chat_id)
          insert into spotter(requester_id, client_id,data,need_trigger,link,preview_text,teal_time) values
      ('bot',0,new.vectorised_text_my_default,true,
       (select ('https://t.me/'||COALESCE(sel.username,'c/'||(sel.id::text))||'/'||sel.mess_id)::text from sel),
       substring(new.text,0,200),(select sel.real_time from sel));

       end if;
        return null;
    end;
$$ LANGUAGE plpgsql;
drop trigger after_messages_insert on public.messages ;
CREATE TRIGGER after_messages_insert after INSERT on public.messages FOR EACH ROW execute PROCEDURE after_messages();






















