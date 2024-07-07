namespace MangaApplication.Entities;
public class UserRating
{
    public UserRating(Guid mangaId, int score) 
    {
        this.MangaId = mangaId;
        this.Score = score;
    }
    public Guid MangaId { get; set; }
    public int Score { get; set; }
}
public class User 
{
    public Guid Id;
    public string Username;
    public string Password;
    public string Email;
    public string AvatarUrl;
    public string Role;
    public IEnumerable<Guid> MangaFollows;
    public IEnumerable<UserRating> UserRatings;

}