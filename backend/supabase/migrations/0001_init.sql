-- AI遠距離介護コーディネーター 初期スキーマ
-- 重要: 全テーブルで RLS を有効化し、family_id スコープでアクセスを制限する。
-- 医療情報・連絡先は application 層で AES-256 暗号化済みバイト列を保存する。

create extension if not exists "uuid-ossp";
create extension if not exists "pgcrypto";
create extension if not exists "vector";

create type family_role as enum ('primary', 'sibling', 'spouse', 'observer');
create type subscription_tier as enum ('free', 'standard', 'premium');
create type message_kind as enum ('chat', 'note', 'system', 'care_event');
create type review_kind as enum ('summary', 'doctor_pack', 'mediation', 'decision');
create type review_status as enum ('pending', 'approved', 'rejected', 'edited');

create table families (
    id uuid primary key default uuid_generate_v4(),
    name text not null,
    subscription_tier subscription_tier not null default 'free',
    created_at timestamptz not null default now()
);

create table family_members (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    user_id uuid not null,
    display_name text not null,
    role family_role not null,
    joined_at timestamptz not null default now(),
    unique (family_id, user_id)
);
create index family_members_user_idx on family_members (user_id);

create table parents (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    display_name text not null,
    date_of_birth date,
    address_region text,
    care_level text,
    care_manager_name text,
    care_manager_contact_enc bytea,
    notes text,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);
create index parents_family_idx on parents (family_id);

create table medications (
    id uuid primary key default uuid_generate_v4(),
    parent_id uuid not null references parents(id) on delete cascade,
    name text not null,
    dosage text,
    schedule text,
    prescribed_by text,
    started_on date,
    ended_on date,
    notes_enc bytea
);
create index medications_parent_idx on medications (parent_id);

create table medical_events (
    id uuid primary key default uuid_generate_v4(),
    parent_id uuid not null references parents(id) on delete cascade,
    family_id uuid not null references families(id) on delete cascade,
    occurred_at timestamptz not null,
    kind text not null,
    summary_enc bytea not null,
    source_member_id uuid references family_members(id) on delete set null
);
create index medical_events_parent_idx on medical_events (parent_id, occurred_at desc);

create table messages (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    sender_member_id uuid references family_members(id) on delete set null,
    kind message_kind not null default 'chat',
    body_enc bytea not null,
    created_at timestamptz not null default now()
);
create index messages_family_idx on messages (family_id, created_at desc);

create table care_records (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    parent_id uuid not null references parents(id) on delete cascade,
    occurred_at timestamptz not null,
    category text not null,
    body_enc bytea not null,
    created_at timestamptz not null default now()
);
create index care_records_family_idx on care_records (family_id, occurred_at desc);

create table decisions (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    topic text not null,
    status text not null default 'open',
    options_json jsonb not null default '[]',
    chosen_option_id text,
    decided_at timestamptz,
    created_at timestamptz not null default now()
);

create table tasks (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    assignee_member_id uuid references family_members(id) on delete set null,
    title text not null,
    due_at timestamptz,
    status text not null default 'open',
    created_at timestamptz not null default now()
);

create table agent_runs (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    agent_name text not null,
    session_id text,
    input_summary text,
    output_text text,
    confidence numeric(4, 3),
    cost_usd numeric(8, 5),
    duration_ms integer,
    created_at timestamptz not null default now()
);
create index agent_runs_family_idx on agent_runs (family_id, created_at desc);

create table human_review_queue (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    agent_run_id uuid references agent_runs(id) on delete set null,
    kind review_kind not null,
    payload jsonb not null,
    status review_status not null default 'pending',
    reviewed_by text,
    reviewed_at timestamptz,
    edited_payload jsonb,
    created_at timestamptz not null default now()
);
create index human_review_queue_status_idx on human_review_queue (status, created_at);

create table feedback_labels (
    id uuid primary key default uuid_generate_v4(),
    family_id uuid not null references families(id) on delete cascade,
    agent_run_id uuid references agent_runs(id) on delete set null,
    member_id uuid references family_members(id) on delete set null,
    label text not null,
    note text,
    created_at timestamptz not null default now()
);

create table subscriptions (
    family_id uuid primary key references families(id) on delete cascade,
    revenuecat_app_user_id text not null,
    tier subscription_tier not null,
    started_at timestamptz not null,
    renews_at timestamptz,
    cancelled_at timestamptz,
    updated_at timestamptz not null default now()
);

create table care_knowledge_embeddings (
    id uuid primary key default uuid_generate_v4(),
    source text not null,
    chunk text not null,
    embedding vector(1536) not null,
    created_at timestamptz not null default now()
);
create index care_knowledge_embeddings_ivf on care_knowledge_embeddings
    using ivfflat (embedding vector_cosine_ops) with (lists = 100);

-- RLS
alter table families enable row level security;
alter table family_members enable row level security;
alter table parents enable row level security;
alter table medications enable row level security;
alter table medical_events enable row level security;
alter table messages enable row level security;
alter table care_records enable row level security;
alter table decisions enable row level security;
alter table tasks enable row level security;
alter table agent_runs enable row level security;
alter table human_review_queue enable row level security;
alter table feedback_labels enable row level security;
alter table subscriptions enable row level security;

-- 家族メンバーのみ自家族のレコードを参照可能
create policy "members_select_own_family" on families
    for select using (
        id in (select family_id from family_members where user_id = auth.uid())
    );

create policy "members_select_family_members" on family_members
    for select using (
        family_id in (select family_id from family_members where user_id = auth.uid())
    );

create policy "members_modify_self_only" on family_members
    for all using (user_id = auth.uid())
    with check (user_id = auth.uid());

-- 共通: family_id 越境禁止のテンプレ
create policy "scope_parents" on parents for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_medications" on medications for all
    using (parent_id in (
        select id from parents where family_id in (
            select family_id from family_members where user_id = auth.uid()
        )
    ));

create policy "scope_medical_events" on medical_events for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_messages" on messages for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_care_records" on care_records for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_decisions" on decisions for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_tasks" on tasks for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_agent_runs" on agent_runs for select
    using (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_feedback" on feedback_labels for all
    using (family_id in (select family_id from family_members where user_id = auth.uid()))
    with check (family_id in (select family_id from family_members where user_id = auth.uid()));

create policy "scope_subscriptions" on subscriptions for select
    using (family_id in (select family_id from family_members where user_id = auth.uid()));

-- human_review_queue は管理者のみがアクセス（サービスロール経由）
-- → RLS は有効にしつつ、デフォルトでは誰も触れない（service_role はバイパス）
