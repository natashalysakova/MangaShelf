CREATE USER 'manga-db-user'@'%' IDENTIFIED BY 'e!UwF9b4kMU_JbnL';

CREATE DATABASE IF NOT EXISTS `manga-shelf-db`;
GRANT ALL PRIVILEGES ON `manga-shelf-db`.* TO 'manga-db-user'@'%';

CREATE DATABASE IF NOT EXISTS `manga-system-db`;
GRANT ALL PRIVILEGES ON `manga-system-db`.* TO 'manga-db-user'@'%';

CREATE DATABASE IF NOT EXISTS `accounts-db`;
GRANT ALL PRIVILEGES ON `accounts-db`.* TO 'manga-db-user'@'%';