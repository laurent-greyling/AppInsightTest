namespace AppInsightTest
{
    /// <summary>
    /// Example of how queries should look and be passed in
    /// You can choose how you want to send this to your app
    /// </summary>
    public class ConstQueryString
    {
        //Get all information logged in the depencies table for appinsights
        public const string AllDependencies = "dependencies";

        //Get all information logged in the exceptions table for appinsights
        public const string AllExceptions = "exceptions";

        //Get all information logged in the requests table for appinsights
        public const string AllRequests = "requests";

        //Counts of the depency records
        public const string Count = " | count ";

        //Get the records where resultcode of api call is success
        public const string SuccessFul = " | where resultCode == '200'";

        //Get the records where resultcode of api call is non-success
        public const string Unsuccesful = " | where resultCode != '200'";

        //Get the counts for resultcode of api call is success
        public const string SuccesfulCount = " | where resultCode == '200' | count ";

        //Get the counts for resultcode of api call is non-success
        public const string UnsuccesfulCount = " | where resultCode != '200' | count ";

        //Get average duration for an API call
        public const string AverageDuration = " | summarize avg(duration) ";

        //Project will get only the columns you specify, so here you should pass columns you want to display
        public const string SpecifiedColumns = " | project duration, resultCode , target, client_City , client_CountryOrRegion";

        //Add/Rename Columns
        public const string AddColumns = " | extend Api_Name=target , Success=resultCode | project Api_Name , Success";

        //Union
        public const string UnionDependencies = @" let T=dependencies
| extend Api_Name=target , Result_Code=resultCode  
| project Api_Name, Result_Code;
        union
        (T | where Result_Code == '200' and Api_Name contains 'fixer' | summarize count(Result_Code) | extend ResultCode_Count = 'Success'),
(T | where Result_Code != '200' and Api_Name contains 'fixer' | summarize count(Result_Code) | extend ResultCode_Count = 'Error')";

        //Simple Pivot Table with a filter where api name contains some value
        public const string PivotWithFilter = @" | project  success, target, name, resultCode 
| where target contains 'openweather' | evaluate pivot(resultCode)";

        //Simple Pivot Table 
        public const string Pivot = @" | project  success, target, name, resultCode
| evaluate pivot(success)";

        //Pivot where names of pivot columns are renamed to more meaningful values
        public const string PivotRenameColumns = @" | extend OutCome=iff(success == true, 'Success', 'Error')
| project target, name, OutCome
| evaluate pivot(OutCome)";

        //average response time by a specific column (basically a distinct on the by columnName)
        public const string AverageDurationBy = " | summarize avg(duration) by target";

        //basic join of pivot and average
        public const string BasicJoin = @" | extend OutCome=iff(success == true, 'Success', 'Error')
| project target, name, OutCome
| evaluate pivot(OutCome)
| join(
   dependencies
   | summarize avg(duration) by target
) on $left.target == $right.target 
| project target, name, Success, Error, avg_duration";

        //Join counts with avg duration and Data Table. More complex, where we give names of expected API's in the datatable. If calls were made 
        //against one of the API's it will contain data requested. If not, the name of API will still display but no accompanying data will be present
        // Basically, API name without data is api that was never called
        public const string JoinWithDataTable = @"let T = datatable(ApiName:string)
[
    'api.openweathermap.org',
    'api.fixer.io',
    'api.idonotexist.nl',
    'api.somerandomapi.com',
];
T
| join kind=leftouter (
    dependencies
        | extend OutCome=iff(success == true, 'Success', 'Error')
        | project target, OutCome
        | evaluate pivot(OutCome)
        | join(
    dependencies
        | summarize avg(duration) by target
    ) on $left.target == $right.target
) on $left.ApiName == $right.target
| project ApiName, Success, Error, avg_duration";
    }
}
