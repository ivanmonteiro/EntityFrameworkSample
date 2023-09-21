# EntityFrameworkSample

Comprehensive Testing of Entity Framework Entities and Relationships within a .NET Core API.

## Features

[] create/update entity with a one-to-one relationship
[] create/update entity with a one-to-many relationship
[] add item to one-to-many relationship
[] delete item from one-to-many relationship

## Motivation

A real world scenario: an API that saves an entity with relationships. To simplify, let's say we are saving a blog and its posts. If we only added post to the blog, it is fairly simple. But in real world scenarios, we may have changed the blog's name, deleted a post, modified a post or even deleted a post. Adding or updating a blog/post is simple, but handling deletes is where things get tricky.

In APIs and Web applications we receive a *detached* entity from the client (or a DTO). We also usually don't use the same dbcontext that was used to query the entity. That entity may have relationships. If we are using detached entities, then the responsibility of figure ou what was added/updated/deleted is shifted to the developer.

[This doc](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes) suggests using the soft delete strategy. But considering that not all applications can actually change clients to implement soft deletes, it is not the best solution.

Alternatively it suggests doing a "graph diff" with an example very verbose for my liking. I'm more inclined to have a standard "automated" solution within the framework or in some community library that handles an entity WITH child entitites, handling inserts/updates/deletes in relationships.

Here is a piece of code from the official documentation that illustrates a "graph diff":
```
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

[TrackGraph](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#trackgraph) also puts the responsibility of figuring out what actually was added/updated/deleted in the hands of the developer. Maybe something that build up on TrackGraph but behind the scenes does a db query and figures out "behind the scenes" what has added/updated/deleted. We will always have edge cases, but a general solution built on good practices would be a nice addition to the framework.

For reference, some open source packages have gone this direction, like:
[EF-Core-Simple-Graph-Update](https://github.com/WahidBitar/EF-Core-Simple-Graph-Update)
[Detached-Mapper](https://github.com/leonardoporro/Detached-Mapper)
[EF-Core-Simple-Graph-Update](https://github.com/WahidBitar/EF-Core-Simple-Graph-Update/blob/f71a103bda2593b85bd6415374a13f9eacc08394/src/Diwink.Extensions.EntityFrameworkCore/DbContextExtensions.cs#L40C46-L40C46)

## References

[Turn-key Disconnected Entities Solution #5536](https://github.com/dotnet/efcore/issues/5536)

[Disconnected Entities - Handling Deletes](https://learn.microsoft.com/en-us/ef/core/saving/disconnected-entities#handling-deletes)

[Change Tracking Mixed-State Graphs in EF Core](https://www.codemag.com/Article/2205041/Change-Tracking-Mixed-State-Graphs-in-EF-Core)

[Integration Tests in ASP.NET Core - Customize WebApplicationFactory - Microsoft](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0#customize-webapplicationfactory) and [CustomWebApplicationFactory.cs](https://github.com/dotnet/AspNetCore.Docs.Samples/blob/main/test/integration-tests/IntegrationTestsSample/tests/RazorPagesProject.Tests/CustomWebApplicationFactory.cs).
