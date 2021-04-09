using AIService.Entities;
using AIService.Logs;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace aiservice.Services
{
    public class YoutubeService
    {
        private static string label = "Services";
        private static string className = "YoutubeService";
        public static async Task<dynamic> Search(AppSettings appSettings, JObject requestBody)
        {
            string methodName = "Search";
            dynamic result = new ExpandoObject();
            try
            {
                // https://developers.google.com/youtube/v3/docs/search/list
                // https://developers.google.com/youtube/v3/code_samples/dotnet#search_by_keyword
                YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = requestBody["apikey"]?.ToString()
                });
                SearchResource.ListRequest listRequest = youtubeService.Search.List("snippet");
                listRequest.Q = requestBody["q"]?.ToString();
                listRequest.MaxResults = 50;
                listRequest.Type = requestBody["type"]?.ToString() ?? "video"; // Accepted values: channel, playlist, video
                if (requestBody["channelId"] != null)
                {
                    listRequest.ChannelId = requestBody["channelId"].ToString();
                }
                if (requestBody["order"] != null)
                {
                    //Accepted values: date, rating, relevance, title, videoCount, viewCount
                    listRequest.Order = Enum.Parse<SearchResource.ListRequest.OrderEnum>(requestBody["order"].ToString());
                }
                if (requestBody["publishedAfter"] != null)
                {
                    listRequest.PublishedAfter = DateTime.ParseExact(requestBody["publishedAfter"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString();
                }
                if (requestBody["publishedBefore"] != null)
                {
                    listRequest.PublishedBefore = DateTime.ParseExact(requestBody["publishedBefore"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString();
                }
                SearchListResponse listResponse = await listRequest.ExecuteAsync();
                return listResponse;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }

        public static async Task<dynamic> CommentThreads(AppSettings appSettings, JObject requestBody)
        {
            string methodName = "CommentThreads";
            dynamic result = new ExpandoObject();
            try
            {
                // https://developers.google.com/youtube/v3/docs/commentThreads/list
                YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = requestBody["apikey"]?.ToString()
                });
                CommentThreadsResource.ListRequest listRequest = youtubeService.CommentThreads.List("id,replies,snippet");
                if (requestBody["pagetoken"] != null)
                {
                    listRequest.PageToken = requestBody["pagetoken"].ToString();
                }
                switch (requestBody["type"]?.ToString())
                {
                    case "channel":
                        listRequest.ChannelId = requestBody["id"].ToString();
                        break;
                    case "video":
                        listRequest.VideoId = requestBody["id"].ToString();
                        break;
                    default:
                        listRequest.AllThreadsRelatedToChannelId = requestBody["id"].ToString();
                        break;
                }
                listRequest.MaxResults = 100;
                if (requestBody["order"] != null)
                {
                    //Accepted values: time, relevance
                    listRequest.Order = Enum.Parse<CommentThreadsResource.ListRequest.OrderEnum>(requestBody["order"].ToString());
                }
                if (requestBody["q"] != null)
                {
                    listRequest.SearchTerms = requestBody["q"].ToString();
                }
                listRequest.TextFormat = Enum.Parse<CommentThreadsResource.ListRequest.TextFormatEnum>(requestBody["textFormat"]?.ToString() ?? "PlainText");
                CommentThreadListResponse listResponse = await listRequest.ExecuteAsync();

                Dictionary<string, object> response = new Dictionary<string, object>();
                response["totalresults"] = listResponse.PageInfo.TotalResults;
                response["pagetoken"] = listResponse.NextPageToken;
                
                List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
                listResponse.Items.ToList().ForEach(i =>
                {
                    Dictionary<string, object> item = new Dictionary<string, object>();
                    item["commentid"] = i.Id;
                    item["channelid"] = i.Snippet.ChannelId;
                    item["videoid"] = i.Snippet.VideoId;
                    item["likecount"] = i.Snippet.TopLevelComment.Snippet.LikeCount;
                    item["viewerrating"] = i.Snippet.TopLevelComment.Snippet.ViewerRating;
                    item["userchannelid"] = i.Snippet.TopLevelComment.Snippet.AuthorChannelId.Value;
                    item["userchannelurl"] = i.Snippet.TopLevelComment.Snippet.AuthorChannelUrl;
                    item["username"] = i.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
                    item["userprofileimageurl"] = i.Snippet.TopLevelComment.Snippet.AuthorProfileImageUrl;
                    item["text"] = i.Snippet.TopLevelComment.Snippet.TextDisplay;
                    item["publisheddate"] = i.Snippet.TopLevelComment.Snippet.PublishedAt;
                    item["updateddate"] = i.Snippet.TopLevelComment.Snippet.UpdatedAt;
                    if (i.Replies != null)
                    {
                        List<Dictionary<string, object>> replies = new List<Dictionary<string, object>>();
                        i.Replies.Comments.ToList().ForEach(c =>
                        {
                            Dictionary<string, object> reply = new Dictionary<string, object>();
                            reply["replyid"] = c.Id;
                            reply["userchannelid"] = c.Snippet.AuthorChannelId.Value;
                            reply["userchannelurl"] = c.Snippet.AuthorChannelUrl;
                            reply["username"] = c.Snippet.AuthorDisplayName;
                            reply["userprofileimageurl"] = c.Snippet.AuthorProfileImageUrl;
                            reply["text"] = c.Snippet.TextDisplay;
                            reply["publisheddate"] = c.Snippet.PublishedAt;
                            reply["updateddate"] = c.Snippet.UpdatedAt;
                            replies.Add(reply);
                        });
                        item["replies"] = replies;
                    }
                    items.Add(item);
                });
                response["items"] = items;
                
                return response;
            }
            catch (Exception e)
            {
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {JsonConvert.SerializeObject(requestBody)}");
                Log.Write(appSettings, LogEnum.ERROR.ToString(), label, className, methodName, $"ERROR: {e.Source + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace}");
                throw e;
            }
        }
    }
}
