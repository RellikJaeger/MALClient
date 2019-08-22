﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Android.Runtime;
using MALClient.Models.Enums;
using MALClient.Models.Models.Anime;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using Newtonsoft.Json;

namespace MALClient.XShared.Comm.Anime
{
    public class AnimeSearchQuery : Query
    {
        private readonly string _query;

        public AnimeSearchQuery(string query, ApiType? apiOverride = null)
        {
            _query = query;
        }

        public async Task<List<AnimeGeneralDetailsData>> GetSearchResults()
        {
            var output = new List<AnimeGeneralDetailsData>();

            try
            {
                var client = new HttpClient(ResourceLocator.MalHttpContextProvider.GetHandler());
                var response = await client.GetAsync($"https://api.jikan.moe/v3/search/anime?q={_query}");

                if (!response.IsSuccessStatusCode)
                    return output;

                var results = JsonConvert.DeserializeObject<RootObject>(await response.Content.ReadAsStringAsync());

                foreach (var result in results.results)
                {
                    result.image_url =
                        Regex.Replace(result.image_url, @"\/r\/\d+x\d+", "");
                    result.image_url =
                        result.image_url.Substring(0, result.image_url.IndexOf('?'));

                    output.Add(new AnimeGeneralDetailsData
                    {
                        Id = result.mal_id,
                        AllEpisodes = result.episodes,
                        Title = WebUtility.HtmlDecode(result.title),
                        ImgUrl = result.image_url,
                        Type = result.type,
                        Synopsis = WebUtility.HtmlDecode(result.synopsis),
                        MalId = result.mal_id,
                        GlobalScore = (float)result.score,      
                        Status =  result.airing ? "Currently Airing" : "Unknown"                       
                    });
                }

            }
            catch (Exception e)
            {
                return output;
            }

            return output;
            //switch (CurrentApiType)
            //{
            //    case ApiType.Mal:
            //        try
            //        {
            //            var parsed = XElement.Parse(raw.Replace("&", "")); //due to unparasable stuff returned by mal
            //            foreach (var element in parsed.Elements("entry"))
            //            {
            //                var item = new AnimeGeneralDetailsData();
            //                item.ParseXElement(element, true, Settings.PreferEnglishTitles);
            //                output.Add(item);
            //            }
            //        }
            //        catch (Exception)
            //        {
            //            //mal can throw html in synopisis and xml cannot do much
            //        }

            //        break;
            //    case ApiType.Hummingbird:
            //        dynamic jsonObj = JsonConvert.DeserializeObject(raw);
            //        foreach (var entry in jsonObj)
            //        {
            //            try
            //            {
            //                var allEps = 0;
            //                if (entry.episode_count != null)
            //                    allEps = Convert.ToInt32(entry.episode_count.ToString());
            //                output.Add(new AnimeGeneralDetailsData
            //                {
            //                    Title = entry.title.ToString(),
            //                    ImgUrl = entry.cover_image.ToString(),
            //                    Type = entry.show_type.ToString(),
            //                    Id = Convert.ToInt32(entry.id.ToString()),
            //                    MalId = Convert.ToInt32(entry.mal_id.ToString()),
            //                    AllEpisodes = allEps,
            //                    StartDate = "0000-00-00", //TODO : Do sth
            //                    EndDate = "0000-00-00",
            //                    Status = entry.status.ToString(),
            //                    Synopsis = entry.synopsis.ToString(),
            //                    GlobalScore = float.Parse(entry.community_rating.ToString()),
            //                    Synonyms = new List<string>()
            //                });
            //            }
            //            catch (Exception e)
            //            {
            //            }
            //        }

            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}

            //return output;
        }

        [Preserve(AllMembers = true)]
        class Result
        {
            public int mal_id { get; set; }
            public string url { get; set; }
            public string image_url { get; set; }
            public string title { get; set; }
            public string synopsis { get; set; }
            public string type { get; set; }
            public double score { get; set; }
            public int episodes { get; set; }
            public bool airing { get; set; }
            public int members { get; set; }
        }

        [Preserve(AllMembers = true)]
        class RootObject
        {
            public string request_hash { get; set; }
            public bool request_cached { get; set; }
            public List<Result> results { get; set; }
            public int result_last_page { get; set; }
        }
    }
}