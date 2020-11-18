using Elasticsearch.Net;
using Nest;

namespace ESApplication
{
    class Program
    {
        private const string DefaultIndexName = "default_blog";
        private const string MyIndexName = "my_new_blog";


        static void Main(string[] args)
        {
            IConnectionPool pool = ESHelper.GetConnectionPool();

            using (var settings = new ConnectionSettings(pool).DefaultIndex(DefaultIndexName))
            {
                //Working with NEST 7.10 client without authentication
                ElasticClient Client = new ElasticClient(settings);

                //1. Create an index with a name different from default index name
                ESHelper.CreateIndex(MyIndexName, typeof(Post), Client);
                // 2. Add documents to the index (one by one or in bulk)
                ESHelper.AddToIndex(MyIndexName, Client);
                // Search from the index
                ESHelper.SearchFromIndex(MyIndexName, Client);

            }

        }
    }

}
