using System.Collections.Generic;
using System;

namespace uchat.Models;

public record User {
    public int id { get; init; } // todo maybe this id should handle db
    string name { get; set; }
    string? image { get; init; } //todo change type
    //todo maybe contacts shold be a db table
    List<int> contacts { get; set; } = new List<int>(); //todo maybe store only ids
    //todo maybe chats are in db as well
    //
    //phone number?
    //registration date?

    List<Chat> chats { get; set; } = new List<Chat>();
    
    public User(string name) {
        this.name = name;
        this.id = 0; //Guid.NewGuid(); or db handles ids
    }

    public bool AddContact(User user) {
        // maybe make it void function
        //
        // if user is in contacts
        //  return false
        // else add to contacts
        // return true
        return true;
    }

    public bool RemoveContact(User user) {
        return true;
    }

    public void CreateChat(User user) {
    }

    public void DeleteChat(int id) {
    }

    public void LeaveGroupChat() {}
    public void DeleteGroupChat() {} // owner only
}
