using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
// ReSharper disable All

namespace DisneySjov
{
    class DisneySjovDay
    {
        private const string RomanceStandardTime = "Romance Standard Time";
        public readonly string DisneySjovTitle = "disney sjov";

        public DateTime Date { get; private set; }
        public bool IsFriday { get; private set; }
        public bool IsDisneySjovOnDR1 { get; private set; }
        public bool IsDisneySjovOnDR1After6pmDanishTime { get; private set; }

        private DisneySjovDay()
        {}

        public static DisneySjovDay ReadDRSchedule(DateTime date, out string error)
        {
            var returnInstance = new DisneySjovDay();
            returnInstance.Date = date.Date;
            error = string.Empty;
            try
            {
                var request = WebRequest.Create(string.Format("http://www.dr.dk/mu/Schedule/{0}%40dr.dk/mas/whatson/channel/DR1", returnInstance.Date.ToString("yyyy-MM-dd")));
                var response = request.GetResponse();
                var data = response.GetResponseStream();
                if (data == null)
                {
                    error = "Response is empty";
                    return null;
                }
                string json;
                using (var sr = new StreamReader(data))
                {
                    json = sr.ReadToEnd();
                }
                var broadcasts = JObject.Parse(json).SelectToken("Data[0].Broadcasts").ToObject<List<Broadcast>>();
                returnInstance.SetResult(broadcasts);
                return returnInstance;
            }
            catch (Exception ex)
            {
                error = string.Format("Exception occurred reading schedule:\r\n{0}", ex.Message);
                return null;
            }
        }

        private void SetResult(List<Broadcast> broadcasts)
        {
            IsFriday = Date.DayOfWeek == DayOfWeek.Friday;
            var disneySjovBroadcasts = broadcasts.Where(b => b.Title.ToLower().Contains(DisneySjovTitle)).ToList();
            IsDisneySjovOnDR1 = disneySjovBroadcasts.Any();
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(RomanceStandardTime);
            var disneySjovBroadcastsLocalTime = disneySjovBroadcasts.Select(b => b.StartTime.Kind == DateTimeKind.Utc ? new Broadcast {StartTime = TimeZoneInfo.ConvertTimeFromUtc(b.StartTime, timeZone), Title = b.Title} : b);
            IsDisneySjovOnDR1After6pmDanishTime = disneySjovBroadcastsLocalTime.Any(b => b.StartTime.Date.Equals(Date) && b.StartTime.TimeOfDay.TotalHours >= 18);
        }

        private class Broadcast
        {
            public DateTime StartTime { get; set; }
            public string Title { get; set; }
        }
    }
}
