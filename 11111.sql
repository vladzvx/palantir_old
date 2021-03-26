create table public.patients(
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    full_name text,
    name text,
    sex text,
    bith_date timestamp,
    primary key (id)
);

create table public.concepts(
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    level int,
    levelb int,
    name text,
    patient_id bigint,
    primary key (id),
    foreign key (patient_id) references patients (id)
);

create table public.protocols(
    id text,
    Type bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    patient_id bigint,
    primary key (id),
    foreign key (patient_id) references patients (id)
);

create table public.subjects(
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    level int,
    levelb int,
    patient_id bigint,
    primary key (id),
    foreign key (patient_id) references patients (id)
);

create table public.facts(
    id bigint,
    adding_time timestamp default CURRENT_TIMESTAMP,
    level int,
    levelb int,
    primary key (id)
);


create table subject_facts_links(
    id bigserial,
    adding_time timestamp default CURRENT_TIMESTAMP,
    subject_id bigint,
    fact_id bigint,
    primary key (id),
    foreign key (subject_id) references subjects (id),
    foreign key (fact_id) references facts (id)
);

create table facts_facts_links(
    id bigserial,
    adding_time timestamp default CURRENT_TIMESTAMP,
    parent_id bigint,
    child_id bigint,
    primary key (id),
    foreign key (parent_id) references facts (id),
    foreign key (child_id) references facts (id)
);


CREATE OR REPLACE FUNCTION patients_check() RETURNS trigger as
$$
    begin
        if exists(select 1 from public.patients where patients.id=new.id) then
            return null;
        end if;
        return new;
    end
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION facts_check() RETURNS trigger as
$$
    begin
        if exists(select 1 from facts where facts.id=new.id) then
            return null;
        end if;
        return new;
    end
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION subjects_check() RETURNS trigger as
$$
    begin
        if exists(select 1 from subjects where  subjects.id=new.id) then
            return null;
        end if;
        return new;
    end
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION concepts_check() RETURNS trigger as
$$
    begin
        if exists(select 1 from concepts where  concepts.id=new.id) then
            return null;
        end if;
        return new;
    end
$$ LANGUAGE plpgsql;

CREATE TRIGGER on_insert_to_facts before INSERT on public.facts FOR EACH ROW execute PROCEDURE facts_check();
CREATE TRIGGER on_insert_to_facts before INSERT on public.patients FOR EACH ROW execute PROCEDURE patients_check();
CREATE TRIGGER on_insert_to_facts before INSERT on public.concepts FOR EACH ROW execute PROCEDURE concepts_check();
CREATE TRIGGER on_insert_to_facts before INSERT on public.subjects FOR EACH ROW execute PROCEDURE subjects_check();




create or replace function add_patient(_id bigint, _full_name text, _name text,_bith_date timestamp) returns void as
$$
    begin
        insert into patients (id, full_name, name, sex, bith_date) VALUES (_id,_full_name,_name,_bith_date);
    end
$$ LANGUAGE plpgsql;

create or replace function add_protocol(_id text, _patient_id bigint, _type bigint) returns void as
$$
    begin
        insert into protocols (id, patient_id, Type) VALUES (_id,_patient_id,_type);
    end
$$ LANGUAGE plpgsql;

create or replace function add_subject(_id bigint, _level int, _levelb int,_patient_id bigint) returns void as
$$
    begin
        insert into subjects (id, level, levelb, patient_id) VALUES (_id,_level,_levelb,_patient_id);
    end
$$ LANGUAGE plpgsql;

create or replace function add_fact(_id bigint, _level int, _levelb int,_parent_fact_id bigint,_subject_id bigint) returns void as
$$
    begin
        insert into facts (id, level, levelb) VALUES (_id,_level,_levelb);
    end
$$ LANGUAGE plpgsql;

create or replace function add_concept(_id bigint, _level int, _levelb int,_name text,_patient_id bigint) returns void as
$$
    begin
        insert into concepts (id, level, levelb, name, patient_id) values (_id,_level,_levelb,_name,_patient_id);
    end
$$ LANGUAGE plpgsql;

