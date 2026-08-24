-- scripts/init.sql

CREATE TABLE IF NOT EXISTS tasks (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    done BOOLEAN NOT NULL DEFAULT FALSE
);

-- Seed initial tasks if table is empty
INSERT INTO tasks (title, done)
SELECT 'Buy groceries', false
WHERE NOT EXISTS (SELECT 1 FROM tasks);

INSERT INTO tasks (title, done)
SELECT 'Read EF Core documentation', false
WHERE NOT EXISTS (SELECT 1 FROM tasks);

INSERT INTO tasks (title, done)
SELECT 'Complete Docker Assignment', true
WHERE NOT EXISTS (SELECT 1 FROM tasks);