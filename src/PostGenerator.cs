using System;
using System.Collections.Generic;

namespace ESApplication
{
    static class PostGenerator
    {
        public static List<Post> GeneratePosts(int quantity)
        {
            List<Post> posts = new List<Post>();
            var random = new Random();
            for (int i = 0; i < quantity; i++)
            {
                var post = new Post
                {
                    UserId = random.Next(),
                    PostDate = new DateTime(random.Next(2015, 2020), random.Next(1, 12), random.Next(1, 28))
                };
                post.PostText = $"post is created at: {post.PostDate}";
                posts.Add(post);
            }
            return posts;
        }
    }
}
