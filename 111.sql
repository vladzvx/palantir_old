create database tg_data_miner template template0

create table public.chats (
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    update_time timestamp default null,
    last_message_id bigint default 1,
    name text,
    username text,
    is_group bool default false,
    is_channel bool default false,
    description text,
    pair_id bigint references chats (id) default null,
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
    primary key (id,chat_id),
    foreign key (chat_id) references chats (id)
);


CREATE OR REPLACE FUNCTION add_message(_message_timestamp timestamp,_message_id bigint, _chat_id bigint, _user_id bigint,
    _reply_to bigint,_thread_start bigint,_media_group_id bigint,_forward_from_id BIGINT,_forward_from_message_id bigint,
     _text text, _media jsonb, _formatting jsonb) RETURNS void as
$$
    declare

    begin
        if (select count (id) from messages where id = _message_id and chat_id=_chat_id)=0 then
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
        end if;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_user(_user_id bigint,sender_username text,sender_first_name text, sender_last_name text) RETURNS void as
$$
    declare

    begin
        if (select count (id) from users where id = _user_id)=0 then
            insert into users (id, first_name, last_name, username) values (_user_id,sender_first_name,sender_last_name,sender_username);
        end if;
    end;
$$ LANGUAGE plpgsql;

