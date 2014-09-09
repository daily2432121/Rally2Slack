using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rally.RestApi;
using Rally.RestApi.Response;

namespace Rally2Slack.Core
{
    

    public class RallyService
    {
        private static RallyRestApi _rallyRestApi = new RallyRestApi(@"cheng.huang@finoconsulting.com", "570124yaya");

        public QueryResult GetItem(string itemId)
        {
            RallyRestApi restApi = new RallyRestApi(@"cheng.huang@finoconsulting.com","570124yaya");
            Request request = new Request("hierarchicalrequirement");
            //Request request = new Request();
            request.Fetch = new List<string>() {"Name","Description","FormattedID"};

            request.Query = new Query("FormattedID", Query.Operator.Equals, itemId);

            QueryResult queryResult = restApi.Query(request);
            return queryResult;


        }
    }
}
