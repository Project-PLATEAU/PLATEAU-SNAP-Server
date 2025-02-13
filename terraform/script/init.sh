#!/bin/bash
timedatectl set-timezone Asia/Tokyo
localectl set-locale LANG=ja_JP.UTF-8
localectl set-keymap jp106
dnf update -y
dnf install -y postgresql16
dnf install -y docker
systemctl start docker
gpasswd -a $(whoami) docker
chgrp docker /var/run/docker.sock
service docker restart
systemctl enable docker
curl -L "https://github.com/docker/compose/releases/download/v2.32.1/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose
ln -s /usr/local/bin/docker-compose /usr/bin/docker-compose
dnf install -y java-11-amazon-corretto-headless
cd ~
wget https://github.com/3dcitydb/importer-exporter/releases/download/v5.5.0/3DCityDB-Importer-Exporter-5.5.0.zip
wget https://github.com/3dcitydb/plugin-ade-manager/releases/download/v2.3.0/plugin-ade-manager-2.3.0.zip
wget https://github.com/3dcitydb/iur-ade-citydb/releases/download/v2.1.0/iur-ade-2.1.0.zip
wget https://github.com/3dcitydb/3dcitydb/releases/download/v4.4.1/3dcitydb-4.4.1.zip
unzip 3DCityDB-Importer-Exporter-5.5.0.zip
unzip plugin-ade-manager-2.3.0.zip
unzip iur-ade-2.1.0.zip
unzip 3dcitydb-4.4.1.zip
mv 3DCityDB-Importer-Exporter-5.5.0 3DCityDB-Importer-Exporter
mv plugin-ade-manager-2.3.0 3DCityDB-Importer-Exporter/plugins/plugin-ade-manager
mv iur-ade-2.1.0 3DCityDB-Importer-Exporter/ade-extensions/iur-ade
mv 3dcitydb-4.4.1 3DCityDB-Importer-Exporter/3dcitydb
