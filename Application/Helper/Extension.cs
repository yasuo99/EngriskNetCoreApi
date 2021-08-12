using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Application.DTOs.Pagination;
using Domain.Models;
using Application.Services.Core;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using System.Linq.Expressions;
using Application.Services;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Application.Helper
{
    public static class Extension
    {
        public static List<string> TempImagePath = new List<string>();
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, int pageSize, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, pageSize, totalItems, totalPages);
            var camelPropertyName = new JsonSerializerSettings();
            camelPropertyName.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Append("Pagination", JsonConvert.SerializeObject(paginationHeader, camelPropertyName));
            response.Headers.Append("Access-Control-Expose-Headers", "Pagination");
        }
        public static PaginateDTO<T> CreatePaginate<T>(this PagingList<T> paginList)
        {
            return new PaginateDTO<T>(paginList.CurrentPage, paginList.PageSize, paginList.TotalPages, paginList.TotalItems, paginList.Count, paginList);
        }
        public static bool CompareDate(this DateTime date, DateTime inputDate)
        {
            if (date.Year != inputDate.Year || date.Month != inputDate.Month || date.Day != inputDate.Day)
            {
                return false;
            }
            return true;
        }
        public static bool CompareProperties<T>(this T subject, Dictionary<dynamic, dynamic> properties) where T : class
        {
            int samePropertiesCount = 0;
            foreach (var property in subject.GetType().GetProperties())
            {
                if (properties.CompareKey(property.Name))
                {
                    if (property.GetValue(subject).Equals(properties[property.Name]))
                    {
                        samePropertiesCount++;
                    }
                }
            }
            if (samePropertiesCount == properties.Count)
            {
                return true;
            }
            return false;
        }
        public static bool CompareKey(this Dictionary<dynamic, dynamic> properties, dynamic key)
        {
            foreach (var propKey in properties.Keys)
            {
                if (propKey.Equals(key))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool ExistsValue<T>(this T subject, dynamic value) where T : class
        {
            foreach (var property in subject.GetType().GetProperties())
            {
                if (property.GetValue(subject).Equals(value))
                {
                    return true;
                }
            }
            return false;
        }
        public static int CalculateAge(this DateTime dob)
        {
            var age = DateTime.Now.Year - dob.Year;
            if (dob.AddYears(age) > DateTime.Now)
            {
                age--;
            }
            return age;
        }
        public static double ConvertToTimestamp(this DateTime date)
        {
            DateTime epoch = DateTime.UnixEpoch;
            TimeSpan result = date.Subtract(epoch);

            double seconds = result.TotalMilliseconds;
            return seconds;
        }
        public static double MinusDate(this DateTime endDate, DateTime startDate)
        {
            var timestamp = (endDate.Subtract(startDate)).TotalSeconds;
            return timestamp;
        }
        public static async Task<IEnumerable<JToken>> DeserializeJson(this IFormFile file)
        {
            try
            {
                using (var fileStream = file.OpenReadStream())
                {
                    using (var result = new StreamReader(fileStream))
                    {
                        using (var json = new JsonTextReader(result))
                        {
                            var jResult = await JToken.ReadFromAsync(json);
                            return jResult.Children().ToList();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        ///<summary>
        ///<para>Đọc file từ excel chỉ nhận các file *.csv hoặc *.xlsx</para>
        ///</summary>
        public static async Task<DataSet> ReadExcel(this IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = true,
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                    return result;
                }
            }
        }
        ///<summary>
        ///<p>Thay đổi một property của một list được phân trang thành null sử dụng reflection</p>
        ///</summary>
        public static PagingList<T> SetNullProperty<T>(this PagingList<T> source, string property)
        {
            foreach (var item in source)
            {
                PropertyInfo prop = item.GetType().GetProperty(property);
                if (prop != null)
                {
                    prop.SetValue(item, null);
                }
            }
            return source;
        }
        ///<summary><para>Hàm lấy random một phần tử từ danh sách</para></summary>
        public static T GetOneRandomFromList<T>(this IEnumerable<T> source)
        {
            var random = new Random();
            if (source.Count() > 0)
            {
                var index = random.Next(0, source.Count());
                return source.ElementAt(index);
            }
            return default(T);
        }
        public static IEnumerable<T> GetAmountRandomFromAList<T>(this IEnumerable<T> source, int amount)
        {
            var random = new Random();
            HashSet<T> returnList = new HashSet<T>();
            if (source.Count() > amount)
            {
                while (returnList.Count < amount)
                {
                    var index = random.Next(0, source.Count());
                    var item = source.ElementAt(index);
                    returnList.Add(item);
                }
            }
            else{
                foreach(var item in source){
                    returnList.Add(item);
                }
            }
            return returnList;
        }
        public static bool CompareStringPropWithList(this string property, List<string> input)
        {
            foreach (var comparer in input)
            {
                if (!property.ToLower().Contains(comparer.ToLower()))
                {
                    return false;
                }
            }
            return true;
        }
        public static string ReplaceStringPropWithList(this string property, List<string> input)
        {
            foreach (var replace in input)
            {
                property = property.Replace(replace, "______");
            }
            return property;
        }
        ///<summary>
        ///<para>Hàm convert string thành slug link</para>
        ///</summary>
        public static string ConvertToSlug(this string link)
        {
            foreach (var character in link.ToCharArray())
            {
                if (character.Equals(" "))
                {
                    link = link.Replace(character, '-');
                }
            }
            return link;
        }
        public static void ConfigurationServices(this IServiceCollection services)
        {
            services.AddScoped<ITranslator, Translator>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<IWordService, WordService>();
            services.AddScoped<IWordCategoryService, WordCategoryService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IExamOnlineScheduleService, ExamOnlineScheduleService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<IAppService,AppService>();
            services.AddScoped<IRouteService,RouteService>();
            services.AddScoped<IAdminService,AdminService>();
            services.AddScoped<IPostService,PostService>();
            services.AddScoped<ICertificateService,CertificateService>();
            services.AddScoped<ICategoryTagService,CategoryTagService>();
        }
        public static string CamelCaseSerialize(object subject)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(subject, jsonSerializerSettings);
        }
        public static string CamelcaseSerialize(this object subject)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(subject, jsonSerializerSettings);
        }
        public static Dictionary<TKey, object> RemoveRange<TKey>(this Dictionary<TKey, object> source, IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                source.Remove(key);
            }
            return source;
        }
        public static string FirstLetterUppercase(this string source)
        {
            source = char.ToUpper(source[0]) + source.Substring(1);
            return source;
        }
        public static List<DateTime> GetNearestSevenDay(this DateTime now)
        {
            var datetimes = new List<DateTime>();
            var start = now.Date.AddDays((int)now.Date.DayOfWeek * -1);
            var end = start.AddDays(6);
            var range = end.Subtract(start).Days;
            for (var i = 0; i <= range; i++)
            {
                datetimes.Add(start.Date.AddDays(i));
            }
            return datetimes;
        }
        public static async Task<PaginateDTO<TModel>> PaginateAsync<TModel>(
            this IQueryable<TModel> query,
            int page,
            int limit,
            CancellationToken cancellationToken)
            where TModel : class
        {

            var paged = new PaginateDTO<TModel>();

            page = (page < 0) ? 1 : page;

            paged.CurrentPage = page;
            paged.PageSize = limit;

            var totalItemsCountTask = await query.CountAsync(cancellationToken);

            var startRow = (page - 1) * limit;
            paged.Items = await query
                       .Skip(startRow)
                       .Take(limit)
                       .ToListAsync(cancellationToken);

            paged.TotalItems =  totalItemsCountTask;
            paged.TotalPages = (int)Math.Ceiling(paged.TotalItems / (double)limit);

            return paged;
        }
    }
}