using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.Nodes;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ImageBasedSearch.Models;
using System.Collections.Immutable;

namespace ImageBasedSearch.Services
{
	public class ElasticService
	{
		private readonly ElasticsearchClient _client;

		public ElasticService()
		{
			_client = GetElasticClient();
		}

		public async Task<List<ResponseDocument>> SearchSimilar(float[] queryVector)
		{
			var esResponse = await _client.SearchAsync<ResponseDocument>(new SearchRequest
			{
				Sort = [
					SortOptions.Score(
						new ScoreSort
						{
							Order = SortOrder.Asc
						}
					)
				],
				Size = 50,
				Query = Query.ScriptScore(new ScriptScoreQuery
				{
					Query = Query.MatchAll(new MatchAllQuery()),
					Script = new Script
					{
						Source = "l2norm(params.query_vector, 'Embeddings')",
						Params = new Dictionary<string, object>
						{
							{ "query_vector", queryVector }
						}
					}
				}),
				Source = new SourceConfig(
					new SourceFilter
					{
						Excludes = Field.FromString(nameof(ImageDocument.Embeddings))
					}
				)
			});
			/*
			something like 'user_id_album' - index name for each user
			TODO - each user has to have a separate API key
			TODO - add UserId or IndexNAme or something to HttpContext and set it in middleware,
				so you can use it everywhere in the app for every request. if needed, create your own context
				for more complex stuff. 
			 
			 */
			var elements = esResponse.Hits
				.Where(h => h.Score < 0.8)
				.Select(h => new ResponseDocument
					{
						ImagePath = h.Source?.ImagePath,
						Score = h.Score,
						Id = h.Id
					}
				).ToList();

			return elements;
		}

		public async Task InitIndex()
		{
			var response = await _client.Indices.GetAsync(Constants.MyIndexName);

			if (!response.IsValidResponse)
			{
				await CreateIndex();
			}
			else
			{
				await _client.Indices.DeleteAsync(Constants.MyIndexName);
				await CreateIndex();
			}

			//var tc = new TestClass();
			//var res = await client.IndexAsync(tc, s => s.Index(MyIndexName));
		}

		public async Task InsertBulk(IEnumerable<ImageDocument> imageDocuments)
		{
			await _client.IndexManyAsync(imageDocuments, Constants.MyIndexName);
		}

		private async Task CreateIndex()
		{
			await _client.Indices.CreateAsync(Constants.MyIndexName, c => c
				.Mappings(m => m
					.Properties<IndexMapping>(p => p
						.DenseVector(nameof(IndexMapping.Embeddings), v => v.Dims(512))
						.Keyword(nameof(IndexMapping.ImagePath))
					)
				)
			);
		}

		private ElasticsearchClient GetElasticClient()
		{
			var settings = new ElasticsearchClientSettings()
				.EnableDebugMode()
				.PrettyJson()
				.RequestTimeout(TimeSpan.FromMinutes(2));

			return new ElasticsearchClient(settings);
		}
	}
}
