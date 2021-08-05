using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;

namespace CowinChecker
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // IHttpManager manager = new RequestHttpManager();
            // manager.Get("https://cdn-api.co-vin.in/api/v2/appointment/sessions/calendarByDistrict?district_id=650&date=06-05-2021");

            StartLoop().GetAwaiter().GetResult();
        }

        private static async Task StartLoop()
        {
            // IHttpManager manager = new WebdriverHttpManager();
            var manager = new RequestHttpManager();
            IMessageService messageService = new WhatsappMessageService();
            while (!messageService.IsReady)
            {
                await Task.Delay(100);
            }

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            while (true)
            {
                var config = JsonConvert.DeserializeObject<Configuration>(await File.ReadAllTextAsync("config.json"));
                var formattedDate = DateTime.Now.ToString("dd/MM_HH-mm-ss");
                var fileName = $"{formattedDate}.json";
                var filePath = Path.Join("Data", fileName);

                foreach (var person in config.PersonData)
                {
                    if (person.Districts.Length > 0)
                    {
                        foreach (var district in person.Districts)
                        {
                            var uri =
                                @$"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByDistrict?district_id={district}&date={DateTime.Today:dd/MM/yyyy}";
                            // var result = manager.Get(uri);
                            var result = await manager.GetAsync(uri);
                            await ProcessData(result, filePath, messageService, person, DataType.District);

                            await Task.Delay(200);
                        }
                    }

                    if (person.PinCodes.Length > 0)
                    {
                        foreach (var pincode in person.PinCodes)
                        {
                            var uri =
                                @$"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/calendarByPin?pincode={pincode}&date={DateTime.Today:dd/MM/yyyy}";
                            // var result = manager.Get(uri);
                            var result = await manager.GetAsync(uri);
                            await ProcessData(result, filePath, messageService, person, DataType.Pincode);

                            await Task.Delay(50);
                        }
                    }
                }

                Console.WriteLine($"\nChecked at {formattedDate}");
                await Task.Delay(Math.Max(config.Interval, 2000));
            }
        }

        private static async Task ProcessData(string result, string filePath, IMessageService messageService,
            PersonData personData, DataType dataType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(result)) return;

                var centers = JsonConvert.DeserializeObject<RootData>(result)?.centers;
                if (centers is not {Count: > 0}) return;

                Console.Write($"{centers.Count} ");

                var possibleSessions = new Dictionary<Center, List<Session>>();

                foreach (var center in centers)
                {
                    if (personData.CenterKeywords is {Length: > 0})
                    {
                        var valid = personData.CenterKeywords.Count(x => center.name.ToLower().Contains(x.ToLower())) > 0;
                        if (!valid) continue;
                    }
                    foreach (var session in center.sessions.Where(session =>
                        session.available_capacity >= personData.MinimumSeats && session.min_age_limit < 45))
                    {
                        if (!string.IsNullOrWhiteSpace(personData.VaccineType) &&
                            !string.Equals(session.vaccine, personData.VaccineType, StringComparison.CurrentCultureIgnoreCase)) continue;
                        
                        if (!possibleSessions.ContainsKey(center))
                        {
                            possibleSessions.Add(center, new List<Session>());
                        }

                        possibleSessions[center].Add(session);
                    }
                }

                if (possibleSessions.Count > 0)
                {
                    Console.WriteLine("found something");

                    var enter = $"{Keys.LeftShift}{Keys.Enter}{Keys.LeftShift}";

                    IEnumerable<string> formattedCenters = Array.Empty<string>();

                    switch (dataType)
                    {
                        case DataType.District:
                            formattedCenters = from centerData in possibleSessions
                                from session in centerData.Value
                                select
                                    $"*{session.date}*{enter}{centerData.Key.state_name}{enter}{centerData.Key.district_name}{enter}{centerData.Key.name}{enter}*{session.available_capacity} seat(s) left!*";
                            break;
                        case DataType.Pincode:
                            formattedCenters = from centerData in possibleSessions
                                from session in centerData.Value
                                select
                                    $"*{session.date}*{enter}{centerData.Key.state_name}{enter}Pin code:{centerData.Key.pincode}{enter}{centerData.Key.name}{enter}*{session.available_capacity} seat(s) left!*";
                            break;
                    }

                    var jsonData = JsonConvert.SerializeObject(possibleSessions, Formatting.Indented);
                    await File.WriteAllTextAsync(filePath, jsonData);

                    messageService.Send(personData.Name, formattedCenters);
                }
            }
            catch (JsonReaderException)
            {
                Console.WriteLine("Not working temporarily");
            }
        }

        private enum DataType
        {
            District = 0,
            Pincode = 1
        }
    }
}