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
    primary key (message_db_id, message_timestamp),
    foreign key (chat_id) references chats (id)
)  PARTITION BY RANGE (message_timestamp);


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
                                         formatting) values (_message_timestamp,
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
                                                             _formatting);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_user(_user_id bigint,sender_username text,sender_first_name text, sender_last_name text) RETURNS void as
$$
    begin
            insert into users (id, first_name, last_name, username) values (_user_id,sender_first_name,sender_last_name,sender_username);
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_chat(_chat_id bigint,_title text,_username text, pair_chat_id bigint,_is_group bool,_is_channel bool) RETURNS void as
$$
    begin
            insert into chats (id, name , username, pair_id, is_group,is_channel) values (_chat_id,_title,_username,pair_chat_id,_is_group,_is_channel);
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

create  or replace function  get_unupdated_chats(dt timestamp) returns table (_id bigint,
                                                                            _pair_id bigint,
                                                                            _last_message_id bigint,
                                                                            _getting_last_message_timestamp timestamp,
                                                                            _pair_id_checked bool,
                                                                            _username text,
                                                                            _pair_username text) as
$$
    begin
        return query update chats set last_time_checked = current_timestamp  where (getting_last_message_timestamp<dt) and (last_time_checked is null or last_time_checked <dt) and not banned
                returning id,
                pair_id,
                last_message_id,
                getting_last_message_timestamp,
                pair_id_checked,
                username,
                pair_username;
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
