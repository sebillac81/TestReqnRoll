Feature: Envio y recepcion de mensaje

Scenario: Produce and consume a message
Given The Pulsar client is created
When The producer send a message "Hello Pulsar!"
Then The consumer receive a message "Hello Pulsar!"