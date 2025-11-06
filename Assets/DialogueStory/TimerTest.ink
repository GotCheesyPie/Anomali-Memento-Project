#speaker: Aldrin
"Laper banget deh."
#speaker: Sera
"Kalo ada makanan sih aku mau milih..."
-> Pilihan

=== Pilihan ===
#speaker: Sera
"Hmm, apa ya..." #timer: 10 #timeout_index: 2
* [Seblak]
    -> Seblak
* [Nasi Padang]
    -> Naspad
* [ ] -> WaktuHabis
    
=== Seblak ===
#speaker: Sera
"Seblak enak sih tapi gasuka pedes."
-> END

=== Naspad ===
#speaker: Sera
"Naspad enak sih tapi gasuka pedes."
-> END

=== WaktuHabis ===
#speaker: Sera
"Ah ga kepikiran, laper banget soalnya."
-> END