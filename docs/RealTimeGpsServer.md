# RealTimeGpsServer

A lightweight server component designed to stream real-time GPS data to connected clients over a network protocol. It handles incoming client connections, manages data streams, and provides lifecycle control for starting and stopping the server.

## API

### `RealTimeGpsServer`
The default constructor for the `RealTimeGpsServer` class. Initializes a new instance of the server with default configuration settings. No network resources are allocated until `StartAsync` is called.

### `StartAsync`
