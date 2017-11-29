HexChain
--------
P2P Networking service that will recieve transactions and process them as "blocks" within a modified block chain messaging system. The hash of each block is used in subsequent blocks to create a historically enriched hash on every transaction processed into a block. The hash contains information about the transaction and provides a high level of encryption. It also prevents man in the middle attacks when sending and recieving transactions since the hash must be known for the entire historical sequence to be able to create a new hash that would be considered valid. 

Jinx
----
A interface program to the HexChain for windows that could transmit transactions into the HexChain service. It can also interpret the last block on the HexChain and present it to the user for parsing.


TODO:
add software checksum to the hash encyrption
Jinx application using locally named pipes.

- priority based on last time a value was updated.
- this could change based on sources to pick between faster sources that may havemore reliable data?