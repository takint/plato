﻿using System;
using Plato.Internal.Models.Users;

namespace Plato.Reactions.Models
{

    
    public class Reaction : IReaction
    {
     
        public static readonly int PointsMultiplier = 1;

        public string Category { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Emoji { get; set; }

        public int Points { get; set; }
        
        public Sentiment Sentiment { get; set; }

        public ISimpleUser CreatedBy { get; set; } = new SimpleUser();

        public DateTimeOffset? CreatedDate { get; set; }
        
        public Reaction()
        {
        }

        public Reaction(string name)
        {
            this.Name = name;
        }

        public Reaction(
            string name,
            string description) : this(name)
        {
            this.Description = description;
        }

        public Reaction(
            string name,
            string description,
            string emoji) : this(name, description)
        {
            this.Emoji = emoji;
        }

        public Reaction(
            string name,
            string description,
            string emoji,
            int points) : this(name, description, emoji)
        {
            this.Points = points * PointsMultiplier; 
        }


        public Reaction(
            string name,
            string description,
            string emoji,
            int points,
            Sentiment sentiment) : this(name, description, emoji, points)
        {
            this.Sentiment = sentiment;
        }

    }

    public enum Sentiment
    {
        Positive = 1,
        Neutral = 0,
        Negative = -1
    }

}
