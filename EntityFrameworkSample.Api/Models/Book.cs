namespace EntityFrameworkSample.Api.Models
{
    public class Book
    {
        public Book(int bookId, string title, Author author, int authorId)
        {
            BookId = bookId;
            Title = title;
            Author = author;
            AuthorId = authorId;
        }

        public int BookId { get; set; }
        public string Title { get; set; }
        public Author Author { get; set; }
        public int AuthorId { get; set; }
    }
}