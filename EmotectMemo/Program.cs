using EmotectMemo.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                          policy =>
                          {
                              policy.WithOrigins("http://localhost:9090",
                                                  "http://emotect.microj.ir")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                          });
});

builder.Services.AddSingleton<IMongoClient>(provider =>
{
    MongoClientSettings mongoClientSettings = MongoClientSettings.FromUrl(
        new MongoUrl(
            System.Environment.GetEnvironmentVariable("MONGODB_URL")));

    return new MongoClient(mongoClientSettings);
});

builder.Services.AddSingleton<IMongoDatabase>(provider =>
{
    var mongoClient = provider.GetService<IMongoClient>();
    return mongoClient!.GetDatabase(
        System.Environment.GetEnvironmentVariable("DATABASE_NAME"));
});



var app = builder.Build();
app.UseCors();

//todo: mongo index, refactor needed
var mongodb = app.Services.GetService<IMongoDatabase>()!;
var indexOptions = new CreateIndexOptions() { Unique = true };
var indexKeys = Builders<Memo>.IndexKeys.Ascending("Key");
var indexModel = new CreateIndexModel<Memo>(indexKeys, indexOptions);

await mongodb.GetCollection<Memo>("memo").Indexes.CreateOneAsync(indexModel);
///

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

app.MapGet("/", (IMongoDatabase db) =>
{
    var memos = db.GetCollection<Memo>("memo");
    return memos.Find(_ => true).ToList();
});

app.MapGet("/{key}", async (IMongoDatabase db, string key) =>
{
    var memos = db.GetCollection<Memo>("memo");
    return await (await memos.FindAsync(x => x.Key == key)).FirstOrDefaultAsync();
});


//var todosApi = app.MapGroup("/todos");
//todosApi.MapGet("/", () => sampleTodos);
//todosApi.MapGet("/{id}", (int id) =>
//    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
//        ? Results.Ok(todo)
//        : Results.NotFound());

app.Run();
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Memo))]
[JsonSerializable(typeof(IList<Memo>))]
[JsonSerializable(typeof(List<Memo>))]

internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
