using System;
using System.ComponentModel.DataAnnotations;
using MangaApplication.DTOs;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace MangaApplication.Entities 
{
    public class Rating 
    {
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TotalScore { get; set;}
        public int RatingQuantity { get; set;}

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal AverageScore {get; set;}
        public Rating()
        {
            this.TotalScore = 0;
            this.RatingQuantity = 0;
            this.AverageScore = 0;
        }
        public Rating(decimal TotalScore, int RatingQuantity)
        {
            this.TotalScore = TotalScore;
            this.RatingQuantity = RatingQuantity;
            this.AverageScore = 0;
        }
        public void UpdateRating(int score)
        {
            this.TotalScore += score;
            this.RatingQuantity++;
            this.AverageScore = Convert.ToDecimal(TotalScore / RatingQuantity);
        }

        public void RemoveRating(int score) {
            this.TotalScore -= score;
            this.RatingQuantity--;
            if (RatingQuantity <= 0) 
            {
                this.AverageScore = 0;
            }
            else {
                this.AverageScore = Convert.ToDecimal(TotalScore / RatingQuantity);
            }
        }
       
    }
    public record MangaAuthor(Guid AuthorId, string AuthorName);
    public class Manga
    {
        public Guid Id { get; init; }
        public string Title { get; init; }  
        public string Summary { get; init; }
        public MangaAuthor Author { get; init; }
        public string Status { get; init; }
        public string ImageUrl { get; init; }
        public IEnumerable<Guid> TagIds;
        public Rating? Rating { get; init; }
        // public IEnumerable<Chapter> Chapters { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
        [BsonElement("follows")]
        public int Follows { get; set; }
        
    }
}