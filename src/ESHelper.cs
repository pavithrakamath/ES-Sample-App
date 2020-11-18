using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;

namespace ESApplication
{
    class ESHelper
    {
        /// <summary>
        /// Defines the list ES nodes available for the program
        /// </summary>
        /// <returns>List of nodes where ES is running</returns>
        public static List<Uri> getNodes()
        {
            return new List<Uri>
            {
                new Uri("http://localhost:9200")
            };
        }

        /// <summary>
        /// Get a connection pool to which ES Client can connect. Can be a SingleNodeConnectionPool or StaticConnectionPool depending on the number of nodes available.
        /// </summary>
        /// <returns>connection pool containing nodes</returns>
        public static IConnectionPool GetConnectionPool()
        {
            List<Uri> Nodes = getNodes();

            if (Nodes.Count == 1)
            {
               return new SingleNodeConnectionPool(Nodes[0]);
            }
            else
            {
                var uris = Enumerable.Range(9200, 5).Select(port => new Uri($"http://localhost:{port}"));
                return new StaticConnectionPool(uris);
            }
        }
        /// <summary>
        /// Creates and returns static index settings with given number of replicas and given number of shards. Default is 1.
        /// </summary>
        /// <param name="numOfReplicas" optional></param>
        /// <param name="numOfShards" optional></param>
        /// <returns></returns>
        public static IndexSettings GetIndexSettings(int numOfReplicas=1, int numOfShards=1)
        {
            var indexSettings = new IndexSettings();
            indexSettings.NumberOfReplicas = numOfReplicas;
            indexSettings.NumberOfShards = numOfShards;
            return indexSettings;
        }

        /// <summary>
        /// Search a given index using the given client
        /// <![CDATA[ TODO take the query also in the input]]>
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="esClient"></param>
        /// <param name="indexSettings" optional></param>

        public static void SearchFromIndex(string indexName, ElasticClient esClient, IndexSettings indexSettings = null)
        {
            var searchRequest = new SearchInputRequest();

            //term query
            var searchResponse = esClient.Search<Post>(s => s.Index(indexName).Query(q => q.Term(t => t.PostText, "blog")));

            //MathAll query
            searchResponse = esClient.Search<Post>(s => s.Index(indexName).Query(q => q.MatchAll()));

            //Find all posts created after 01-Jan-2019
            searchResponse = esClient.Search<Post>(s => s
                                                        .Index(indexName)
                                                            .Query(q => q
                                                                .Bool(b => b
                                                                    .Filter(bf => bf
                                                                        .DateRange(s => s
                                                                            .Field(f => f.PostDate)
                                                                                .GreaterThan(new DateTime(2019, 01, 01))
                                                                                .LessThan(DateTime.Today)
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        );

            foreach (var post in searchResponse.Documents)
            {
                if (post != null)
                {
                    Console.WriteLine($"{post.UserId} {post.PostText} {post.PostDate}");
                }
            }
        }
        /// <summary>
        /// 
        ///
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="esClient"></param>
        /// <param name="indexSettings" optional></param>
        public static async void AddToIndex(string indexName, ElasticClient esClient, IndexSettings indexSettings = null)
        {
            var posts = PostGenerator.GeneratePosts(5);
            //Index document one by one
            foreach (var post in posts)
            {
                // single index
                var indexResponse = await esClient.IndexAsync<Post>(post, s => s.Index(indexName));
                if (!indexResponse.IsValid)
                {
                    throw new InvalidOperationException();
                }
            }

            posts = PostGenerator.GeneratePosts(5);
            //bulk index
            var response = await esClient.IndexManyAsync(posts, indexName);
            if (!response.IsValid)
            {
                throw new InvalidOperationException();
            }

        }
        /// <summary>
        /// creates index for a given name and automaps the type
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="type"></param>
        /// <param name="esClient"></param>
        /// <param name="indexSettings"optional></param>
        /// <param name="forceCreate" optional></param>
        public static void CreateIndex(String indexName, Type type, ElasticClient esClient, IndexSettings indexSettings = null, bool forceCreate = false)
        {
            var response = esClient.Indices.Get(indexName);
            if (response.IsValid)
            {
                if (response.Indices.Count > 0)
                {
                    if (forceCreate)
                    {
                        foreach (var name in response.Indices.Keys)
                            esClient.Indices.Delete(name);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            //TODO: Genralize this instead of using Post directly.
            var createIndexResponse = esClient.Indices.Create(indexName, c => c.Map<Post>(m => m.AutoMap(type)));
            if (createIndexResponse.IsValid)
            {
                Console.WriteLine($"Created :: {createIndexResponse.Index}");
            }
        }
    }

}
