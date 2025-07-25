using Newtonsoft.Json;

namespace DiscordBotV3
{
    class JsonClassHelper
    {
        //GelBooru
        public partial class GelBooru
        {
            [JsonProperty("@attributes")]
            public Attributes Attributes { get; set; }

            [JsonProperty("post")]
            public List<Post> Post { get; set; }
        }
        public partial class Attributes
        {
            [JsonProperty("limit")]
            public long Limit { get; set; }

            [JsonProperty("offset")]
            public long Offset { get; set; }

            [JsonProperty("count")]
            public long Count { get; set; }
        }

        public partial class Post
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }

            [JsonProperty("score")]
            public long Score { get; set; }

            [JsonProperty("width")]
            public long Width { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("md5")]
            public string Md5 { get; set; }

            [JsonProperty("directory")]
            public string Directory { get; set; }

            [JsonProperty("image")]
            public string Image { get; set; }

            [JsonProperty("rating")]
            public string Rating { get; set; }

            [JsonProperty("source")]
            public Uri Source { get; set; }

            [JsonProperty("change")]
            public long Change { get; set; }

            [JsonProperty("owner")]
            public string Owner { get; set; }

            [JsonProperty("creator_id")]
            public long CreatorId { get; set; }

            [JsonProperty("parent_id")]
            public long ParentId { get; set; }

            [JsonProperty("sample")]
            public long Sample { get; set; }

            [JsonProperty("preview_height")]
            public long PreviewHeight { get; set; }

            [JsonProperty("preview_width")]
            public long PreviewWidth { get; set; }

            [JsonProperty("tags")]
            public string Tags { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("has_notes")]
            public bool HasNotes { get; set; }

            [JsonProperty("has_comments")]
            public bool HasComments { get; set; }

            [JsonProperty("file_url")]
            public Uri FileUrl { get; set; }

            [JsonProperty("preview_url")]
            public Uri PreviewUrl { get; set; }

            [JsonProperty("sample_url")]
            public Uri SampleUrl { get; set; }

            [JsonProperty("sample_height")]
            public long SampleHeight { get; set; }

            [JsonProperty("sample_width")]
            public long SampleWidth { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("post_locked")]
            public long PostLocked { get; set; }

            [JsonProperty("has_children")]
            public bool HasChildren { get; set; }
        }
    }
}
