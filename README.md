# AppInsightTest

(will add more details in time)

Once this app has run a few times, you would want to query app insights.

Here are a few queries to view the responses from your api calls in Application Insights

```
//All
dependencies

//All Count
dependencies | count 

//With Filter
dependencies
| where resultCode == "Faulted" 

//Count where success call
dependencies
| where resultCode == "200" | count 

//Count where non success call
dependencies
| where resultCode != "200" | count 

//Avg duration
dependencies
| summarize avg(duration) 

//Diplay only certain columns
dependencies
| project duration, resultCode , target, client_City , client_CountryOrRegion   

//Add/Rename columns
dependencies
| extend Api_Name=target , Success=resultCode  
| project Api_Name , Success

//Union
let T=dependencies
| extend Api_Name=target , Result_Code=resultCode  
| project Api_Name , Result_Code;
union
(T | where Result_Code == "200" and Api_Name contains "fixer" | summarize count(Result_Code) | extend ResultCode_Count = "Success" ),
(T | where Result_Code != "200" and Api_Name contains "fixer" | summarize count(Result_Code) | extend ResultCode_Count = "Error")

//Pivot
dependencies
| project  success, target, name, resultCode 
| where target contains "openweather"
| evaluate pivot(resultCode)

dependencies
| project  success, target, name, resultCode
| evaluate pivot(success)

dependencies
| project  success, target, name, resultCode
| evaluate pivot(target)

//Counts of response codes success or error
dependencies
| extend OutCome=iff(success == true, "Success", "Error")
| project  target, name, OutCome
| evaluate pivot(OutCome)

//Average resonse times
dependencies
| summarize avg(duration) by target

//Join counts with avg duration and Data Table 
let T = datatable(ApiName:string)
[
    'api.openweathermap.org',
    'api.fixer.io',
    'api.idonotexist.nl',
    'api.somerandomapi.com',
];
T
| join kind=leftouter (
    dependencies
        | extend OutCome=iff(success == true, "Success", "Error")
        | project  target, OutCome
        | evaluate pivot(OutCome)
        | join (
    dependencies
        | summarize avg(duration) by target  
    ) on $left.target == $right.target
) on $left.ApiName == $right.target
| project ApiName , Success, Error, avg_duration


//basic join of pivot and average
dependencies
| extend OutCome=iff(success == true, "Success", "Error")
| project  target, name, OutCome
| evaluate pivot(OutCome)
| join (
   dependencies
   | summarize avg(duration) by target  
) on $left.target == $right.target 
| project target , name , Success, Error, avg_duration

```
