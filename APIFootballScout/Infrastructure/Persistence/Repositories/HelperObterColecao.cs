using APIFootballScout.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace APIFootballScout.Infrastructure.Persistence.Repositories
{
    public static class HelperObterColecao
    {
        public static IMongoCollection<T> ObterColecao<T>(IMongoClient mongoClient, string collectionName)
        => mongoClient.GetDatabase("scoutdb").GetCollection<T>(collectionName);
    }
}
