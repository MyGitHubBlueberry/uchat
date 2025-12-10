### Uchat
# Build
In foler with solution:
```bash
dotnet build uchat.sln -m Release
```
### Client
# Usage
```bash
./uchat [options]
```
### Server
# Usage:
```bash
./uchat_server [options]
```
# Options:
```
-p, --port <num>    Specifies server port (Required)
-d, --daemon        Starts server in background (detached)
-h, --help          Prints this message
```

# Examples:
Show help message
```bash
./uchat_server -h
```

Launch server deamon at port 8100:
```bash
./uchat_server -p 8100 -d
```

Launch server tied to the console on port 6789:
```bash
./uchat_server -p 6789
```
