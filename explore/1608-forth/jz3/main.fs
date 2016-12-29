\ application setup and main loop
\ assumes that the BME280 sensor is connected to PB6..PB7

0 constant DEBUG  \ 0 = send RF packets, 1 = display on serial port
10 constant RATE  \ seconds between readings

: show-readings ( vprev vcc tint lux humi pres temp -- )
  hwid hex. ." = "
  . ." °cC, " . ." Pa, " . ." %cRH, "
  . ." lux, "  . ." °C, " . ." => " . ." mV " ;

: send-packet ( vprev vcc tint lux humi pres temp -- )
  2 <pkt  hwid u+>  n+> 6 0 do u+> loop  pkt>rf ;

: low-power-sleep
  rf69-sleep
  -adc \ only-msi
  RATE 0 do stop1s loop
  hsi-on adc-init ;

: main
  2.1MHz  1000 systick-hz  lptim-init i2c-init adc-init

  8686 rf69.freq ! 6 rf69.group ! 62 rf69.nodeid !
  rf69-init 16 rf69-power

  bme-init drop bme-calib
  tsl-init drop

  begin
    led-off 

    adc-vcc                      ( vprev )
    low-power-sleep
    adc-vcc adc-temp             ( vprev vcc tint )
    tsl-data  bme-data bme-calc  ( vprev vcc tint lux humi pres temp )

    led-on

    DEBUG if
      show-readings cr 1 ms
    else
      send-packet
    then
  key? until ;
