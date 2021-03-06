﻿using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    [NuwaFramework]
    public class InvalidQueryTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<InvalidQueryCustomer> invalidQueryCustomers = builder.EntitySet<InvalidQueryCustomer>("InvalidQueryCustomers");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/InvalidQueryCustomers?$filter=id eq 5")]
        [InlineData("/odata/InvalidQueryCustomers(5)?$filter=id eq 5")]
        [InlineData("/odata/InvalidQueryCustomers?$orderby=id")]
        [InlineData("/odata/InvalidQueryCustomers(5)?$orderby=id asc")]
        [InlineData("/odata/InvalidQueryCustomers?$orderby=id desc")]
        public void ParseErrorsProduceMeaningfulMessages(string query)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = Client.SendAsync(request).Result;
            dynamic error = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("The query specified in the URI is not valid. Could not find a property named 'id' on type 'WebStack.QA.Test.OData.QueryComposition.InvalidQueryCustomer'.",
                         error["odata.error"].message.value.Value);
        }
    }

    public class InvalidQueryCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 1).Select(i => new InvalidQueryCustomer { }).AsQueryable());
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            return Ok(SingleResult.Create(Enumerable.Range(0, 1).Select(i => new InvalidQueryCustomer { }).AsQueryable()));
        }
    }

    public class InvalidQueryCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
