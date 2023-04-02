# Nostr Bot

This is an example Nostr bot. 
It is a simple bot that can be used as a starting point for your own bot. 

It reacts to direct messages (DM) or public mentions. OpenAI Chat API generates every reply. 

Dialogue context is preserved in SQLite database and retrieved per every communication thread (DMs per sender, mentions per sender or parent event). 