create table statuses(
    id int,
    name text,
    primary key (id)
);

create table states(
    id int,
    name text,
    primary key (id)
);


insert into states (id, name) values (0,'Started');
insert into states (id, name) values (1,'ConfiguringDepth');
insert into states (id, name) values (4,'ConfiguringGroups');
insert into states (id, name) values (5,'ConfiguringChannel');
insert into states (id, name) values (2,'Ready');
insert into states (id, name) values (3,'Searching');


create table users(
    id bigint not null ,
    username text,
    firstname text,
    status int default 2,
    state int default 0,
    depth int default 3,
    search_in_groups bool default false not null,
    search_in_channels bool default true not null,
    registration timestamp default current_timestamp,
    update timestamp,
    primary key (id),
    foreign key (status) references statuses (id),
    foreign key (state) references states (id)
);



insert into statuses (id, name) values (-1,'master');
insert into statuses (id, name) values (0,'privileged');
insert into statuses (id, name) values (1,'beta_tester');
insert into statuses (id, name) values (2,'common');
insert into statuses (id, name) values (3,'banned');

create table messages (
    message_db_id bigserial,
    db_time timestamp default CURRENT_TIMESTAMP not null ,
    tg_time timestamp not null ,
    user_id bigint,
    chat_id bigint,
    message_number bigint,
    text text,
    primary key (message_db_id,db_time),
) partition by RANGE (db_time);


create table pages(
    id bigserial,
    time timestamp default current_timestamp,
    request_id text,
    page int,
    data jsonb,
    primary key (request_id,page)
);



create index users_ids_in_mess on messages using hash(user_id);
create index search_results_ind on pages (request_id,page);

CREATE OR REPLACE FUNCTION log_user(_user_id bigint,sender_username text,sender_first_name text, _state int,_search_in_groups bool,_search_in_channels bool, _depth int)
RETURNS table (status2 int,state2 int,search_in_groups2 bool, search_in_channels2 bool, depth2 int) as
$$
    begin
        if _state is null and _search_in_groups is null and _search_in_channels is null and _depth is null then

                return query insert into users (id, username, firstname) values (_user_id,sender_username,sender_first_name) on conflict on constraint users_pkey do update
                SET username=sender_username, firstname=sender_first_name, update=current_timestamp where excluded.id=users.id
                returning status, state,search_in_groups,search_in_channels, depth;
            else
                return query insert into users (id, username, firstname,state, search_in_groups,search_in_channels) values
                (_user_id,sender_username,sender_first_name, _state ,_search_in_groups ,_search_in_channels ) on conflict on constraint users_pkey do update
                SET username=sender_username, firstname=sender_first_name,state = _state, search_in_groups =_search_in_groups ,
                    depth=_depth,
                    search_in_channels =_search_in_channels, update=current_timestamp where excluded.id=users.id
        returning status, state,search_in_groups,search_in_channels, depth;
        end if;
    end;
$$ LANGUAGE plpgsql;


select log_user(0,'1','343',null,null,null);

CREATE OR REPLACE FUNCTION add_message(_user_id bigint, _chat_id bigint,_message_number bigint,_text text,_tg_time timestamp) RETURNS void as
$$
    begin
            insert into messages (user_id, chat_id, message_number,text, tg_time) values (_user_id, _chat_id, _message_number,_text,_tg_time)
            on conflict on constraint messages_pkey do nothing ;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION save_page(_page int, _search_guid text,_data jsonb) RETURNS void as
$$
    begin
            insert into pages (page, request_id, data) values (_page, _search_guid, _data)
            on conflict on constraint pages_pkey do nothing ;
    end;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_page(_page int, _search_guid text) RETURNS jsonb as
$$
    begin
        return (select data from pages where request_id=_search_guid and page = _page);
    end;
$$ LANGUAGE plpgsql;

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


CREATE TABLE search_results_2020_m07 PARTITION OF search_results FOR VALUES FROM ('2020-07-01') TO ('2020-08-01');
CREATE TABLE search_results_2020_m08 PARTITION OF search_results FOR VALUES FROM ('2020-08-01') TO ('2020-09-01');
CREATE TABLE search_results_2020_m09 PARTITION OF search_results FOR VALUES FROM ('2020-09-01') TO ('2020-10-01');
CREATE TABLE search_results_2020_m10 PARTITION OF search_results FOR VALUES FROM ('2020-10-01') TO ('2020-11-01');
CREATE TABLE search_results_2020_m11 PARTITION OF search_results FOR VALUES FROM ('2020-11-01') TO ('2020-12-01');
CREATE TABLE search_results_2020_m12 PARTITION OF search_results FOR VALUES FROM ('2020-12-01') TO ('2021-01-01');
CREATE TABLE search_results_2021_m01 PARTITION OF search_results FOR VALUES FROM ('2021-01-01') TO ('2021-02-01');
CREATE TABLE search_results_2021_m02 PARTITION OF search_results FOR VALUES FROM ('2021-02-01') TO ('2021-03-01');
CREATE TABLE search_results_2021_m03 PARTITION OF search_results FOR VALUES FROM ('2021-03-01') TO ('2021-04-01');
CREATE TABLE search_results_2021_m04 PARTITION OF search_results FOR VALUES FROM ('2021-04-01') TO ('2021-05-01');
CREATE TABLE search_results_2021_m05 PARTITION OF search_results FOR VALUES FROM ('2021-05-01') TO ('2021-06-01');
CREATE TABLE search_results_2021_m06 PARTITION OF search_results FOR VALUES FROM ('2021-06-01') TO ('2021-07-01');
CREATE TABLE search_results_2021_m07 PARTITION OF search_results FOR VALUES FROM ('2021-07-01') TO ('2021-08-01');
CREATE TABLE search_results_2021_m08 PARTITION OF search_results FOR VALUES FROM ('2021-08-01') TO ('2021-09-01');
CREATE TABLE search_results_2021_m09 PARTITION OF search_results FOR VALUES FROM ('2021-09-01') TO ('2021-10-01');
CREATE TABLE search_results_2021_m10 PARTITION OF search_results FOR VALUES FROM ('2021-10-01') TO ('2021-11-01');
CREATE TABLE search_results_2021_m11 PARTITION OF search_results FOR VALUES FROM ('2021-11-01') TO ('2021-12-01');
CREATE TABLE search_results_2021_m12 PARTITION OF search_results FOR VALUES FROM ('2021-12-01') TO ('2022-01-01');
CREATE TABLE search_results_2022_m01 PARTITION OF search_results FOR VALUES FROM ('2022-01-01') TO ('2022-02-01');
CREATE TABLE search_results_2022_m02 PARTITION OF search_results FOR VALUES FROM ('2022-02-01') TO ('2022-03-01');
CREATE TABLE search_results_2022_m03 PARTITION OF search_results FOR VALUES FROM ('2022-03-01') TO ('2022-04-01');
CREATE TABLE search_results_2022_m04 PARTITION OF search_results FOR VALUES FROM ('2022-04-01') TO ('2022-05-01');
CREATE TABLE search_results_2022_m05 PARTITION OF search_results FOR VALUES FROM ('2022-05-01') TO ('2022-06-01');
CREATE TABLE search_results_2022_m06 PARTITION OF search_results FOR VALUES FROM ('2022-06-01') TO ('2022-07-01');
CREATE TABLE search_results_2022_m07 PARTITION OF search_results FOR VALUES FROM ('2022-07-01') TO ('2022-08-01');
CREATE TABLE search_results_2022_m08 PARTITION OF search_results FOR VALUES FROM ('2022-08-01') TO ('2022-09-01');
CREATE TABLE search_results_2022_m09 PARTITION OF search_results FOR VALUES FROM ('2022-09-01') TO ('2022-10-01');
CREATE TABLE search_results_2022_m10 PARTITION OF search_results FOR VALUES FROM ('2022-10-01') TO ('2022-11-01');
CREATE TABLE search_results_2022_m11 PARTITION OF search_results FOR VALUES FROM ('2022-11-01') TO ('2022-12-01');
CREATE TABLE search_results_2022_m12 PARTITION OF search_results FOR VALUES FROM ('2022-12-01') TO ('2023-01-01');
CREATE TABLE search_results_2023_m01 PARTITION OF search_results FOR VALUES FROM ('2023-01-01') TO ('2023-02-01');
CREATE TABLE search_results_2023_m02 PARTITION OF search_results FOR VALUES FROM ('2023-02-01') TO ('2023-03-01');
CREATE TABLE search_results_2023_m03 PARTITION OF search_results FOR VALUES FROM ('2023-03-01') TO ('2023-04-01');
CREATE TABLE search_results_2023_m04 PARTITION OF search_results FOR VALUES FROM ('2023-04-01') TO ('2023-05-01');
CREATE TABLE search_results_2023_m05 PARTITION OF search_results FOR VALUES FROM ('2023-05-01') TO ('2023-06-01');
CREATE TABLE search_results_2023_m06 PARTITION OF search_results FOR VALUES FROM ('2023-06-01') TO ('2023-07-01');
CREATE TABLE search_results_2023_m07 PARTITION OF search_results FOR VALUES FROM ('2023-07-01') TO ('2023-08-01');
CREATE TABLE search_results_2023_m08 PARTITION OF search_results FOR VALUES FROM ('2023-08-01') TO ('2023-09-01');
CREATE TABLE search_results_2023_m09 PARTITION OF search_results FOR VALUES FROM ('2023-09-01') TO ('2023-10-01');
CREATE TABLE search_results_2023_m10 PARTITION OF search_results FOR VALUES FROM ('2023-10-01') TO ('2023-11-01');
CREATE TABLE search_results_2023_m11 PARTITION OF search_results FOR VALUES FROM ('2023-11-01') TO ('2023-12-01');
CREATE TABLE search_results_2023_m12 PARTITION OF search_results FOR VALUES FROM ('2023-12-01') TO ('2024-01-01');
CREATE TABLE search_results_2024_m01 PARTITION OF search_results FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
CREATE TABLE search_results_2024_m02 PARTITION OF search_results FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');
CREATE TABLE search_results_2024_m03 PARTITION OF search_results FOR VALUES FROM ('2024-03-01') TO ('2024-04-01');
CREATE TABLE search_results_2024_m04 PARTITION OF search_results FOR VALUES FROM ('2024-04-01') TO ('2024-05-01');
CREATE TABLE search_results_2024_m05 PARTITION OF search_results FOR VALUES FROM ('2024-05-01') TO ('2024-06-01');
CREATE TABLE search_results_2024_m06 PARTITION OF search_results FOR VALUES FROM ('2024-06-01') TO ('2024-07-01');
CREATE TABLE search_results_2024_m07 PARTITION OF search_results FOR VALUES FROM ('2024-07-01') TO ('2024-08-01');
CREATE TABLE search_results_2024_m08 PARTITION OF search_results FOR VALUES FROM ('2024-08-01') TO ('2024-09-01');
CREATE TABLE search_results_2024_m09 PARTITION OF search_results FOR VALUES FROM ('2024-09-01') TO ('2024-10-01');
CREATE TABLE search_results_2024_m10 PARTITION OF search_results FOR VALUES FROM ('2024-10-01') TO ('2024-11-01');
CREATE TABLE search_results_2024_m11 PARTITION OF search_results FOR VALUES FROM ('2024-11-01') TO ('2024-12-01');
CREATE TABLE search_results_2024_m12 PARTITION OF search_results FOR VALUES FROM ('2024-12-01') TO ('2025-01-01');
CREATE TABLE search_results_2025_m01 PARTITION OF search_results FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');
CREATE TABLE search_results_2025_m02 PARTITION OF search_results FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');
CREATE TABLE search_results_2025_m03 PARTITION OF search_results FOR VALUES FROM ('2025-03-01') TO ('2025-04-01');
CREATE TABLE search_results_2025_m04 PARTITION OF search_results FOR VALUES FROM ('2025-04-01') TO ('2025-05-01');
CREATE TABLE search_results_2025_m05 PARTITION OF search_results FOR VALUES FROM ('2025-05-01') TO ('2025-06-01');
CREATE TABLE search_results_2025_m06 PARTITION OF search_results FOR VALUES FROM ('2025-06-01') TO ('2025-07-01');
CREATE TABLE search_results_2025_m07 PARTITION OF search_results FOR VALUES FROM ('2025-07-01') TO ('2025-08-01');
CREATE TABLE search_results_2025_m08 PARTITION OF search_results FOR VALUES FROM ('2025-08-01') TO ('2025-09-01');
CREATE TABLE search_results_2025_m09 PARTITION OF search_results FOR VALUES FROM ('2025-09-01') TO ('2025-10-01');
CREATE TABLE search_results_2025_m10 PARTITION OF search_results FOR VALUES FROM ('2025-10-01') TO ('2025-11-01');
CREATE TABLE search_results_2025_m11 PARTITION OF search_results FOR VALUES FROM ('2025-11-01') TO ('2025-12-01');
CREATE TABLE search_results_2025_m12 PARTITION OF search_results FOR VALUES FROM ('2025-12-01') TO ('2026-01-01');
CREATE TABLE search_results_2026_m01 PARTITION OF search_results FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
CREATE TABLE search_results_2026_m02 PARTITION OF search_results FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
CREATE TABLE search_results_2026_m03 PARTITION OF search_results FOR VALUES FROM ('2026-03-01') TO ('2026-04-01');
CREATE TABLE search_results_2026_m04 PARTITION OF search_results FOR VALUES FROM ('2026-04-01') TO ('2026-05-01');
CREATE TABLE search_results_2026_m05 PARTITION OF search_results FOR VALUES FROM ('2026-05-01') TO ('2026-06-01');
CREATE TABLE search_results_2026_m06 PARTITION OF search_results FOR VALUES FROM ('2026-06-01') TO ('2026-07-01');
CREATE TABLE search_results_2026_m07 PARTITION OF search_results FOR VALUES FROM ('2026-07-01') TO ('2026-08-01');
CREATE TABLE search_results_2026_m08 PARTITION OF search_results FOR VALUES FROM ('2026-08-01') TO ('2026-09-01');
CREATE TABLE search_results_2026_m09 PARTITION OF search_results FOR VALUES FROM ('2026-09-01') TO ('2026-10-01');
CREATE TABLE search_results_2026_m10 PARTITION OF search_results FOR VALUES FROM ('2026-10-01') TO ('2026-11-01');
CREATE TABLE search_results_2026_m11 PARTITION OF search_results FOR VALUES FROM ('2026-11-01') TO ('2026-12-01');
CREATE TABLE search_results_2026_m12 PARTITION OF search_results FOR VALUES FROM ('2026-12-01') TO ('2027-01-01');
CREATE TABLE search_results_2027_m01 PARTITION OF search_results FOR VALUES FROM ('2027-01-01') TO ('2027-02-01');
CREATE TABLE search_results_2027_m02 PARTITION OF search_results FOR VALUES FROM ('2027-02-01') TO ('2027-03-01');
CREATE TABLE search_results_2027_m03 PARTITION OF search_results FOR VALUES FROM ('2027-03-01') TO ('2027-04-01');
CREATE TABLE search_results_2027_m04 PARTITION OF search_results FOR VALUES FROM ('2027-04-01') TO ('2027-05-01');
CREATE TABLE search_results_2027_m05 PARTITION OF search_results FOR VALUES FROM ('2027-05-01') TO ('2027-06-01');
CREATE TABLE search_results_2027_m06 PARTITION OF search_results FOR VALUES FROM ('2027-06-01') TO ('2027-07-01');
CREATE TABLE search_results_2027_m07 PARTITION OF search_results FOR VALUES FROM ('2027-07-01') TO ('2027-08-01');
CREATE TABLE search_results_2027_m08 PARTITION OF search_results FOR VALUES FROM ('2027-08-01') TO ('2027-09-01');
CREATE TABLE search_results_2027_m09 PARTITION OF search_results FOR VALUES FROM ('2027-09-01') TO ('2027-10-01');
CREATE TABLE search_results_2027_m10 PARTITION OF search_results FOR VALUES FROM ('2027-10-01') TO ('2027-11-01');
CREATE TABLE search_results_2027_m11 PARTITION OF search_results FOR VALUES FROM ('2027-11-01') TO ('2027-12-01');
CREATE TABLE search_results_2027_m12 PARTITION OF search_results FOR VALUES FROM ('2027-12-01') TO ('2028-01-01');
CREATE TABLE search_results_2028_m01 PARTITION OF search_results FOR VALUES FROM ('2028-01-01') TO ('2028-02-01');
CREATE TABLE search_results_2028_m02 PARTITION OF search_results FOR VALUES FROM ('2028-02-01') TO ('2028-03-01');
CREATE TABLE search_results_2028_m03 PARTITION OF search_results FOR VALUES FROM ('2028-03-01') TO ('2028-04-01');
CREATE TABLE search_results_2028_m04 PARTITION OF search_results FOR VALUES FROM ('2028-04-01') TO ('2028-05-01');
CREATE TABLE search_results_2028_m05 PARTITION OF search_results FOR VALUES FROM ('2028-05-01') TO ('2028-06-01');
CREATE TABLE search_results_2028_m06 PARTITION OF search_results FOR VALUES FROM ('2028-06-01') TO ('2028-07-01');
CREATE TABLE search_results_2028_m07 PARTITION OF search_results FOR VALUES FROM ('2028-07-01') TO ('2028-08-01');
CREATE TABLE search_results_2028_m08 PARTITION OF search_results FOR VALUES FROM ('2028-08-01') TO ('2028-09-01');
CREATE TABLE search_results_2028_m09 PARTITION OF search_results FOR VALUES FROM ('2028-09-01') TO ('2028-10-01');
CREATE TABLE search_results_2028_m10 PARTITION OF search_results FOR VALUES FROM ('2028-10-01') TO ('2028-11-01');
CREATE TABLE search_results_2028_m11 PARTITION OF search_results FOR VALUES FROM ('2028-11-01') TO ('2028-12-01');
CREATE TABLE search_results_2028_m12 PARTITION OF search_results FOR VALUES FROM ('2028-12-01') TO ('2029-01-01');
CREATE TABLE search_results_2029_m01 PARTITION OF search_results FOR VALUES FROM ('2029-01-01') TO ('2029-02-01');
CREATE TABLE search_results_2029_m02 PARTITION OF search_results FOR VALUES FROM ('2029-02-01') TO ('2029-03-01');
CREATE TABLE search_results_2029_m03 PARTITION OF search_results FOR VALUES FROM ('2029-03-01') TO ('2029-04-01');
CREATE TABLE search_results_2029_m04 PARTITION OF search_results FOR VALUES FROM ('2029-04-01') TO ('2029-05-01');
CREATE TABLE search_results_2029_m05 PARTITION OF search_results FOR VALUES FROM ('2029-05-01') TO ('2029-06-01');
CREATE TABLE search_results_2029_m06 PARTITION OF search_results FOR VALUES FROM ('2029-06-01') TO ('2029-07-01');
CREATE TABLE search_results_2029_m07 PARTITION OF search_results FOR VALUES FROM ('2029-07-01') TO ('2029-08-01');
CREATE TABLE search_results_2029_m08 PARTITION OF search_results FOR VALUES FROM ('2029-08-01') TO ('2029-09-01');
CREATE TABLE search_results_2029_m09 PARTITION OF search_results FOR VALUES FROM ('2029-09-01') TO ('2029-10-01');
CREATE TABLE search_results_2029_m10 PARTITION OF search_results FOR VALUES FROM ('2029-10-01') TO ('2029-11-01');
CREATE TABLE search_results_2029_m11 PARTITION OF search_results FOR VALUES FROM ('2029-11-01') TO ('2029-12-01');
CREATE TABLE search_results_2029_m12 PARTITION OF search_results FOR VALUES FROM ('2029-12-01') TO ('2030-01-01');