using EmotectMemo.Helper;
using EmotectMemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddLogging(config => {config.SetMinimumLevel(LogLevel.Information);});
builder.Logging.AddConsole();


using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
ILogger logger = factory.CreateLogger("Program");

logger.LogInformation("Hello World! Logging is {Description}.", "fun");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                          policy =>
                          {
                            policy.WithOrigins("http://localhost:9090")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                            if (System.Environment.GetEnvironmentVariable("FRONTEND_URL") != null){
                                policy.WithOrigins(System.Environment.GetEnvironmentVariable("FRONTEND_URL")!)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                            }
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

builder.Services.AddExceptionHandler<ExceptionLogger>();


var app = builder.Build();

app.UseCors();

//todo: mongo index, refactor needed
var mongodb = app.Services.GetService<IMongoDatabase>()!;
var indexOptions = new CreateIndexOptions() { Unique = true };
var indexKeys = Builders<Memo>.IndexKeys.Ascending("Key");
var indexModel = new CreateIndexModel<Memo>(indexKeys, indexOptions);

await mongodb.GetCollection<Memo>("memo").Indexes.CreateOneAsync(indexModel);

app.MapGet("/", () =>
{
    logger.LogInformation("test of direct information log");
    logger.LogCritical("test of direct critical log");
    throw new Exception("test exception handler log");
});


app.MapGet("/{key}/exists", async (IMongoDatabase db, string key) =>
{
    var memoCollection = db.GetCollection<Memo>("memo");
    return new {
        Key = key,
        Exists = await memoCollection.CountDocumentsAsync(x => x.Key == key) > 0
        };
});


app.MapGet("/{key}", async (IMongoDatabase db, string key, [FromHeader(Name = "Secret-Key")] string secretKey) =>
{
    var memoCollection = db.GetCollection<Memo>("memo");
    var memo = await (await memoCollection.FindAsync(x => x.Key == key)).FirstOrDefaultAsync();
    return memo.SecretKey != secretKey?
            Results.Unauthorized(): 
            Results.Ok(memo);
    
});

app.MapDelete("/{key}/{id}", async (IMongoDatabase db, string key, string id,
     [FromHeader(Name = "Secret-Key")] string secretKey) =>
{
    var memoCollection = db.GetCollection<Memo>("memo");
    var memo = await (await memoCollection.FindAsync(x => x.Key == key)).FirstOrDefaultAsync();
    if (memo.SecretKey != secretKey)
    {
        return Results.Unauthorized();
    }
    //else    
    var update = Builders<Memo>.Update.PullFilter(m => m.Content,
                                                f => f.Id == id);
    var result = await memoCollection
        .FindOneAndUpdateAsync(x=>x.Key == key, update);
    return Results.Ok();
});

app.MapPut("/{key}/{id}", async (IMongoDatabase db, string key, string id,
     [FromHeader(Name = "Secret-Key")] string secretKey, MemoPostDto memoDto) =>
{
    var memoCollection = db.GetCollection<Memo>("memo");
    var memo = await (await memoCollection.FindAsync(x => x.Key == key)).FirstOrDefaultAsync();
    if (memo.SecretKey != secretKey)
    {
        return Results.Unauthorized();
    }
    //else    

    var update = Builders<Memo>.Update.Push(m => m.Content, 
        new MemoContent(){Body = memoDto.Body});
    var result = await memoCollection
        .FindOneAndUpdateAsync(x=>x.Key == key, update);
    return Results.Ok();
});

app.MapPost("/{key}", async (IMongoDatabase db, 
    string key, 
    [FromHeader(Name = "Secret-Key")] string secretKey,
    [FromBody]MemoPostDto memoDto) =>
{
    var memoCollection = db.GetCollection<Memo>("memo");
    var memo = await (await memoCollection.FindAsync(x => x.Key == key)).FirstOrDefaultAsync();
    MemoContent memoContent = new MemoContent(){ Body = memoDto.Body};
    try
    {
        if (memo is null)
        {
                await memoCollection.InsertOneAsync(new Memo() { 
                    Key = key, SecretKey = secretKey, Content = 
                    [memoContent] });
                return Results.Ok(new {id = memoContent.Id});
        }
        else
        {
            if (memo.SecretKey != secretKey)
            {
                return Results.Unauthorized();
            }
            var update = Builders<Memo>.Update.Push<MemoContent>(m => m.Content,
                        new MemoContent() { Body = memoDto.Body });
            var result = await memoCollection
                .FindOneAndUpdateAsync(x => x.Key == key, update);
                return Results.Ok(new {id = memoContent.Id});
        }
    }
    catch(Exception ex)
    {
        logger.LogError(ex, $"Post called key:{key}");
    }
    return Results.BadRequest();
});


app.Run();

record MemoPostDto (string Body);

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Memo))]
[JsonSerializable(typeof(IList<Memo>))]
[JsonSerializable(typeof(List<Memo>))]

internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
