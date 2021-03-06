﻿namespace Dashing.Testing.Tests {
    using System.Collections.Generic;
    using System.Linq;

    using Dashing.Testing.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class InMemoryEngineTests {
        [Fact]
        public void ItWorks() {
            var session = this.GetSession();
            var firstComment = session.Query<Comment>().Fetch(c => c.Post.Author).Single(c => c.CommentId == 1);
            Assert.NotNull(firstComment);
            Assert.Equal(1, firstComment.Post.PostId);
            Assert.Equal(1, firstComment.Post.Author.UserId);
            Assert.Equal(2, firstComment.User.UserId);
            Assert.Null(firstComment.User.Username);
            Assert.Equal("Bob", firstComment.Post.Author.Username);
            Assert.Equal("My First Post", firstComment.Post.Title);
        }

        [Fact]
        public void CollectionsWork() {
            var session = this.GetSession();
            var posts = session.Query<Post>().FetchMany(p => p.Comments).ThenFetch(c => c.User).ToArray();
            Assert.Equal(3, posts.Length);
            Assert.Equal(3, posts.First().Comments.Count);
            Assert.Equal("Mark", posts.First().Comments.First().User.Username);
            Assert.Equal(posts.First().Comments.First().Post.PostId, posts.First().PostId);
            Assert.Null(posts.First().Comments.First().Post.Title);
            Assert.NotNull(posts.First().Title);
        }

        [Fact]
        public void WhereNotFetchedWorks() {
            var session = this.GetSession();
            var comments = session.Query<Comment>().Fetch(c => c.Post).Where(c => c.Post.Author.UserId == 1);
            Assert.True(comments.First().Post.Author.UserId == 1);
            Assert.True(comments.First().Post.Author.Username == null);
        }

        [Fact]
        public void TestConfigWorks() {
            var config = new TestConfiguration(true);
            using (var session = config.BeginSession()) {
                session.Insert(new Post() { Title = "Foo" });
                Assert.Equal("Foo", session.Get<Post>(1).Title);
            }
        }

        [Fact]
        public void WhereWorks() {
            var session = this.GetSession();
            var comments = session.Query<Comment>().Where(c => c.Post.Title.Contains("Second"));
            Assert.Equal(3, comments.Count());
        }

        [Fact]
        public void WhereOpequalityWorks() {
            var session = this.GetSession();
            var blog = new Blog { BlogId = 1 };
            var postWithBlog1 = session.Query<Post>().FirstOrDefault(p => p.Blog == blog);
            Assert.NotNull(postWithBlog1);
        }

        [Fact]
        public void OrderByWorks() {
            var session = this.GetSession();
            var posts = session.Query<Post>().OrderByDescending(p => p.Title).ToList();
            Assert.Equal(3, posts.Count);
            Assert.Equal("My Second Post", posts.First().Title);
        }

        [Fact]
        public void MultipleOrderByWorks() {
            var session = this.GetSession();
            var users = session.Query<User>().OrderBy(u => u.IsEnabled).OrderByDescending(u => u.Username).ToList();
            Assert.Equal(3, users.Count);
            Assert.Equal("James", users.ElementAt(0).Username);
            Assert.Equal("Mark", users.ElementAt(1).Username);
            Assert.Equal("Bob", users.ElementAt(2).Username);
        }

        [Fact]
        public void UpdateWorks() {
            var session = this.GetSession();
            session.Update<User>(u => u.EmailAddress = "Hazar", u => u.IsEnabled);
            var users = session.Query<User>().ToList();
            
            Assert.Equal(3, users.Count);
            Assert.Equal(2, users.Count(u => u.IsEnabled && u.EmailAddress == "Hazar"));
            Assert.Equal(1, users.Count(u => !u.IsEnabled && u.Username == "James"));
        }

        [Fact]
        public void DeleteWorks() {
            var session = this.GetSession();
            session.Delete<User>(u => u.IsEnabled);
            var users = session.Query<User>().ToList();

            Assert.Equal(1, users.Count);
            Assert.Equal(1, users.Count(u => !u.IsEnabled && u.Username == "James"));
        }

        [Fact]
        public void EmptyCollectionContainsWorks() {
            var session = this.GetSession();
            var blogIds = new List<int>();
            var posts = session.Query<Post>().Where(p => blogIds.Contains(p.Blog.BlogId));
            Assert.Equal(0, posts.Count());
        }

        [Fact]
        public void OrWithEmptyCollectionWorks() {
            var session = this.GetSession();
            var blogIds = new List<int>();
            var author = new User { UserId = 1 };
            var posts = session.Query<Post>().Where(p => p.Author == author || blogIds.Contains(p.Blog.BlogId));
            Assert.Equal(2, posts.Count());
        }

        [Fact]
        public void OrWithEmptyCollectionOnNullRelationshipWorks() {
            var session = this.GetSession();
            var blogIds = new List<int>();
            var author = new User { UserId = 2 };
            var posts = session.Query<Post>().Where(p => p.Author == author || blogIds.Contains(p.Blog.BlogId));
            Assert.Equal(1, posts.Count());
        }

        [Fact]
        public void DeleteAllWorks() {
            var session = this.GetSession();
            Assert.Equal(3, session.Query<User>().Count());
            session.DeleteAll<User>();
            Assert.Equal(0, session.Query<User>().Count());
        }

        [Fact]
        public void UpdateAllWorks() {
            var session = this.GetSession();
            Assert.Equal(2, session.Query<User>().Count(u => u.IsEnabled));
            session.UpdateAll<User>(u => u.IsEnabled = false);
            Assert.Equal(0, session.Query<User>().Count(u => u.IsEnabled));
        }

        [Fact]
        public void InsertLongWorks() {
            var config = new TestConfiguration(true);
            using (var session = config.BeginSession()) {
                var thing = new ThingWithLongPrimaryKey { Name = "Foo" };
                var inserts = session.Insert(thing);
                Assert.Equal(1, inserts);
                Assert.Equal(1, thing.Id);

                var secondThing = new ThingWithLongPrimaryKey { Name = "Bar" };
                var insertsSecond = session.Insert(secondThing);
                Assert.Equal(1, insertsSecond);
                Assert.Equal(2, secondThing.Id);
            }
        }

        private ISession GetSession() {
            var engine = new InMemoryEngine() { Configuration = new TestConfiguration() };
            var session = new Session(engine, new Mock<ISessionState>().Object);

            var authors = new List<User> { new User { Username = "Bob", IsEnabled = true }, new User { Username = "Mark", IsEnabled = true }, new User { Username = "James", IsEnabled = false } };
            session.Insert(authors);
            
            var blogs = new List<Blog> { new Blog { Owner = authors[0], Title = "Bob's Blog" } };
            session.Insert(blogs);

            var posts = new List<Post> {
                                           new Post { Author = authors[0], Blog = blogs[0], Title = "My First Post" },
                                           new Post { Author = authors[0], Blog = blogs[0], Title = "My Second Post" },
                                           new Post { Author = authors[1], Title = "A post without a blog!" } 
                                       };
            session.Insert(posts);

            var comments = new List<Comment> {
                                                 new Comment { User = authors[1], Post = posts[0], Content = "This is marks comment on the first post" },
                                                 new Comment {
                                                                 User = authors[1],
                                                                 Post = posts[1],
                                                                 Content = "This is marks comment on the second post"
                                                             },
                                                 new Comment {
                                                                 User = authors[2],
                                                                 Post = posts[0],
                                                                 Content = "This is james' comment on the first post"
                                                             },
                                                 new Comment {
                                                                 User = authors[2],
                                                                 Post = posts[1],
                                                                 Content = "This is james' comment on the second post"
                                                             },
                                                 new Comment { User = authors[0], Post = posts[0], Content = "This is bob's comment on the first post" },
                                                 new Comment {
                                                                 User = authors[0],
                                                                 Post = posts[1],
                                                                 Content = "This is bob's comment on the second post"
                                                             },
                                             };
            session.Insert(comments);
            return session;
        }
    }
}