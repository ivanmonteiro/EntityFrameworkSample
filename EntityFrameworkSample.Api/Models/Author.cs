
namespace EntityFrameworkSample.Api.Models
{
    public class Author
    {
        public Author(int authorId, string firstName, string lastName)
        {
            AuthorId = authorId;
            FirstName = firstName;
            LastName = lastName;
        }

        public int AuthorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}