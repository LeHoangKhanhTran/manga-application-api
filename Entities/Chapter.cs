using System;
namespace MangaApplication.Entities 
{
    public class Chapter 
    {
        public Guid Id { get; init; }
        public string ChapterNumber { get; init; }
        public string ChapterTitle { get; init; }
        public string[] PageUrls { get; init; }
        public DateTimeOffset CreatedDate { get; init; }
    }
}