# ExpandDialSticks

## Access point configuration
1. sudo nano /etc/network/interfaces
2. Comment everything
3. sudo apt-get install -y hostapd dnsmasq
4. sudo nano /etc/dhcpcd.conf
5. at the end add 
    - interface wlan0
    - static ip_address=192.168.0.10/24
    - nohook wpa_supplicant
4. sudo nano /etc/sysctl.conf
5. uncomment net.ipv4.ip_forward=1
6. sudo nano /etc/dnsmasq.conf
    - interface=wlan0
    - dhcp-range=192.168.0.11, 192.168.0.255, 255.255.255.0, 24h
7. sudo nano /etc/default/hostapd
8. uncomment DEAMON_CONF="/etc/hostapd/hostapd.conf"
9. sudo nano /etc/hostapd/hostapd.conf
    - interface=wlan0
    - driver=nl80211
    - hw_mode=g
    - channel=6
    - wmm_enabled=0
    - macaddr_acl=0
    - auth_algs=1
    - ignore_broadcast_ssid=0
    - wpa=2
    - wpa_key_mgmt=WPA-PSK
    - wpa_pairwise=TKIP
    - rsn_pairwise=CCMP
    - ssid=ExpanDialSticks
    - wpa_passphrase=04081992

# Enable I2C and VNC
10. sudo raspi-config
    - Interface Settings
        - enable i2c
        - enable VNC
        
# Python Dependencies for I2C
11. test i2c with sudo i2cdetect -y 1
12. sudo apt-get install -y python-smbus
13. pip3 install smbus2

# Mosquitto installation
14. sudo apt-get install -y mosquitto mosquitto-clients
15. test with mosquitto_sub -h 192.168.0.10 -t "test"
16. text with mosquitto_pub -h 192.168.0.10 -t "test" -m "Hello"

# Autorun script at startup
17. sudo nano /etc/xdg/autostart/myapp.desktop
    - [Desktop Entry]
    - Exec=lxterminal --command "/bin/bash -c 'sudo python3 /home/pi/Documents/ExpanDialSticks.py; /bin/bash'"

    
VNC @ 192.168.0.10:5900 with pi/04081992
MQTT @ 192.168.0.10:1883

