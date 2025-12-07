namespace uchat_server.Args;

struct Option(string name, string shortName, string description) {
    public readonly string name = name;
    public readonly string shortName = shortName;
    public readonly string description = description;

    public bool HasSameName(string name) => 
        name == this.name || name == shortName;
}
