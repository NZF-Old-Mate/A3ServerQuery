using System;
using System.Collections.Generic;

namespace DataStorage
{
    public class User
    {
        public int Uid { get; set; } //internal use only
        public string? A3ProfileName { get; set; } //return from Server Query
        public string? DiscordUsername { get; set; } //Add manually for now, for future compatibility with Discord bots
        public IList<DateTime>? DatesOnline { get; set; } //All dates this user has been online
        public DateTime? IsMember { get; set; } //if null then not a member
        public DateTime? IsRecruit { get; set; } //if null then not a recruit, if both null then raise a flag
        public DateTime? IsCM { get; set; } //internal purposes and forward compatibility
        public DateTime? IsAdmin { get; set; } //internal purposes and forward compatibility
        public string[]? A3ProfileAliases { get; set; } //So users aren't logged twice under different profile names

    }

    public class Session
    {
        public DateTime SessionDateTime { get; set; }
        public List<string>? OnlineUsersA3ProfileNames { get; set; }

    }
}