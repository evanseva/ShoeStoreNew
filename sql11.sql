\c postgres;
DROP DATABASE IF EXISTS evans;
CREATE DATABASE evans
    ENCODING = 'UTF8'
    LC_COLLATE = 'Russian_Russia.1251'
    LC_CTYPE = 'Russian_Russia.1251'
    TEMPLATE = template0;
\c evans;

CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE manufacturers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    category_id INT REFERENCES categories(id),
    description TEXT,
    manufacturer_id INT REFERENCES manufacturers(id),
    supplier VARCHAR(200),
    price DECIMAL(10,2) NOT NULL,
    quantity INT NOT NULL,
    discount INT DEFAULT 0,
    image_path VARCHAR(500)
);

CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    article VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    pickup_address VARCHAR(300),
    order_date DATE,
    issue_date DATE
);

INSERT INTO categories (name) VALUES ('Кроссовки'), ('Ботинки'), ('Сандалии'), ('Сапоги'), ('Туфли');
INSERT INTO manufacturers (name) VALUES ('Nike'), ('Adidas'), ('Reebok'), ('Puma'), ('New Balance');

INSERT INTO products (name, category_id, manufacturer_id, supplier, price, quantity, discount, image_path) VALUES
('Кроссовки Air Max', 1, 1, 'СпортТовары', 8500, 25, 10, 'Images/1.jpg'),
('Ботинки зимние', 2, 2, 'Зимний Мир', 12000, 5, 20, 'Images/2.jpg'),
('Сандалии летние', 3, 3, 'Летний Сезон', 3500, 0, 5, 'Images/3.jpg'),
('Кроссовки беговые', 1, 2, 'СпортМастер', 7200, 15, 0, 'Images/4.jpg'),
('Ботинки рабочие', 2, 1, 'СпецОбувь', 9500, 8, 15, 'Images/5.jpg'),
('Кроссовки повседневные', 1, 4, 'Городская Обувь', 4500, 30, 0, 'Images/6.jpg'),
('Сапоги резиновые', 4, 5, 'Рыболов', 2800, 12, 0, 'Images/7.jpg'),
('Туфли классические', 5, 3, 'Обувь Плюс', 6800, 7, 30, 'Images/8.jpg'),
('Ботинки треккинговые', 2, 4, 'Активный Отдых', 11000, 3, 25, 'Images/9.jpg'),
('Кроссовки детские', 1, 5, 'Детский Мир', 3200, 20, 10, 'Images/10.jpg');

INSERT INTO orders (article, status, pickup_address, order_date) VALUES
('ORD-001', 'Новый', 'ул. Ленина, 10', '2025-01-15'),
('ORD-002', 'В обработке', 'пр. Мира, 25', '2025-01-18'),
('ORD-003', 'Готов к выдаче', 'ул. Гагарина, 5', '2025-01-20');

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO app;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app;