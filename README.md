# EntityFrameworkSample

Comprehensive Testing of Entity Framework Entities and Relationships within a .NET Core API.

## Features

- [ ] create/update entity with a one-to-one relationship
- [ ] create/update entity with a one-to-many relationship
- [ ] add item to one-to-many relationship
- [ ] delete item from one-to-many relationship

## Motivation

Use case: saving an entity **with chages to child entites/relationships** in a .Net API using Entity Framework.

The motivation to create this project is because most examples in documentation shows saving connected entities that share the same DbContext and most of them do not apply to APIs.

APIs and Web applications usually receives a *detached/disconnected* entity from the client (or a DTO). The problem is that with disconnected entities the DbContext does not have the information about what was added/updated/deleted.

Example: Let's say we are saving a blog and its posts. In real world scenarios, we may have changed the blog's name, deleted one of the blog's posts, modified a post's content or even deleted a post or any combination of that. Adding or updating a blog/post is simple, but handling deletes is where things get tricky.

[Microsoft's documentation](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes) suggests using the soft delete strategy. But considering that not all applications can actually change clients to implement soft deletes, it is not a viable solution to everyone.

Here is a piece of code from the official documentation that illustrates a [graph diff](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes) to handle deletes:

```csharp
public static void InsertUpdateOrDeleteGraph(BloggingContext context, Blog blog)
{
    var existingBlog = context.Blogs
        .Include(b => b.Posts)
        .FirstOrDefault(b => b.BlogId == blog.BlogId);

    if (existingBlog == null)
    {
        context.Add(blog);
    }
    else
    {
        context.Entry(existingBlog).CurrentValues.SetValues(blog);
        foreach (var post in blog.Posts)
        {
            var existingPost = existingBlog.Posts
                .FirstOrDefault(p => p.PostId == post.PostId);

            if (existingPost == null)
            {
                existingBlog.Posts.Add(post);
            }
            else
            {
                context.Entry(existingPost).CurrentValues.SetValues(post);
            }
        }

        foreach (var post in existingBlog.Posts)
        {
            if (!blog.Posts.Any(p => p.PostId == post.PostId))
            {
                context.Remove(post);
            }
        }
    }

    context.SaveChanges();
}
```

As we can see, It's a lot of repetitive code. Imagine that you have tens or even hundreds of different entities, each with its own relationships and having to repeat this code over you codebase. It's far from ideal and simple, considering that other ORM frameworks are far more simple.

Microsoft's documentations also suggests using [TrackGraph](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#trackgraph).

Here is an example:

```csharp
public static void SaveAnnotatedGraph(DbContext context, object rootEntity)
{
    context.ChangeTracker.TrackGraph(
        rootEntity,
        n =>
        {
            var entity = (EntityBase)n.Entry.Entity;
            n.Entry.State = entity.IsNew
                ? EntityState.Added
                : entity.IsChanged
                    ? EntityState.Modified
                    : entity.IsDeleted
                        ? EntityState.Deleted
                        : EntityState.Unchanged;
        });

    context.SaveChanges();
}
```

It puts the responsibility of figuring out what actually was added/updated/deleted in the hands of the developer instead of the framework. It is hard work to figure out what added/updated/deleted and provide a fairly bug-free experience.

I'm not keen to writing repetitive, boiler-plate code that is prone to error just to keep track of changes. 

I'm more inclined to have a standard "automated" solution within the framework or in some community library that handles an entity *and* child entitites, handling a mixed state of inserts/updates/deletes.

Maybe something that build up on TrackGraph but behind the scenes does a db query and figures out what has added/updated/deleted. We will always have edge cases, but a general solution built on good practices would be a nice addition to the framework.

For reference, some open source packages have gone this direction, like:
[EF-Core-Simple-Graph-Update](https://github.com/WahidBitar/EF-Core-Simple-Graph-Update)
[Detached-Mapper](https://github.com/leonardoporro/Detached-Mapper)
[EF-Core-Simple-Graph-Update](https://github.com/WahidBitar/EF-Core-Simple-Graph-Update/blob/f71a103bda2593b85bd6415374a13f9eacc08394/src/Diwink.Extensions.EntityFrameworkCore/DbContextExtensions.cs#L40C46-L40C46)

There is an issue in Entity Framework's GitHub repository called [Turn-key Disconnected Entities Solution](https://github.com/dotnet/efcore/issues/5536) that *might* provide a solution to solve this problem, but currenty it has few upvotes and has been postponed many times by the EF team.

## References

[Turn-key Disconnected Entities Solution #5536](https://github.com/dotnet/efcore/issues/5536)

[Disconnected Entities - Handling Deletes](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes)

[Change Tracking Mixed-State Graphs in EF Core](https://www.codemag.com/Article/2205041/Change-Tracking-Mixed-State-Graphs-in-EF-Core)

[Integration Tests in ASP.NET Core - Customize WebApplicationFactory - Microsoft](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#customize-webapplicationfactory) and [CustomWebApplicationFactory.cs](https://github.com/dotnet/AspNetCore.Docs.Samples/blob/main/test/integration-tests/IntegrationTestsSample/tests/RazorPagesProject.Tests/CustomWebApplicationFactory.cs).
