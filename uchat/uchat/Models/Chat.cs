using System;
using System.Collections.Generic;

namespace uchat.Models;

public record Chat {
    string id; //db might handle this
    public Chat(User a, User b) {
        id = String.Format("{}-{}", a.id, b.id); //todo change to something better
    }
}

public record GroupChat(string name) {
    int id;
    string name = name;
    string? picture;
    string? description;
    List<int> participants = new List<int>();// by ids 
    int ownerId;

    public void Add(int id) {}
    public void Remove(int id) {}
    public void Delete() {}
}
