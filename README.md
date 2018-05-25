# AppInsightTest

The main purpose of this app is to log telemetry of API calls and the surrounding data for how many successful calls and the duration of these calls per api. But this is also a bit of a helper and demo into how you can use telemetry in you app and get some information back programatically. 

Once this app has run a few times, you would want to query [app insights](https://azure.microsoft.com/en-us/services/application-insights/). This app will also allow you to run some queries and have Json returned to you, please see the .cs file called __ConstQueryString__ for examples. 

Here are a few queries to view the responses from your api calls in Application Insights (The queries for c# api call looks same as below). 

### Dependencies
```
//All
dependencies

//With Filter
dependencies
| where resultCode == "200" 

```

![image](https://user-images.githubusercontent.com/17876815/40541151-ec459b14-601a-11e8-87e4-60403f27edf2.png)

### Count

```
//All Count
dependencies | count 

//Count where success call
dependencies
| where resultCode == "200" | count 

//Count where non success call
dependencies
| where resultCode != "200" | count 

```

![image](https://user-images.githubusercontent.com/17876815/40541226-399fed92-601b-11e8-96a9-750646b96480.png)


### Average

```
//Avg duration all
dependencies
| summarize avg(duration) 

//Average resonse times
dependencies
| summarize avg(duration) by target

```

![image](https://user-images.githubusercontent.com/17876815/40541397-e0f5ce72-601b-11e8-916b-e2c09e18eeec.png)


### Columns

```
//Diplay only certain columns
dependencies
| project duration, resultCode , target, client_City , client_CountryOrRegion   

//Add/Rename columns
dependencies
| extend Api_Name=target , Success=resultCode  
| project Api_Name , Success
```

![image](https://user-images.githubusercontent.com/17876815/40541467-250a29fa-601c-11e8-973d-b7fa8b943cf7.png)

### Union

```
//Union
let T=dependencies
| extend Api_Name=target , Result_Code=resultCode  
| project Api_Name , Result_Code;
union
(T | where Result_Code == "200" and Api_Name contains "fixer" | summarize count(Result_Code) | extend ResultCode_Count = "Success" ),
(T | where Result_Code != "200" and Api_Name contains "fixer" | summarize count(Result_Code) | extend ResultCode_Count = "Error")
```

![image](https://user-images.githubusercontent.com/17876815/40541514-54ca406c-601c-11e8-8a17-8adcf1ac06b6.png)


### Pivot

```
//Basic Pivot
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

//Counts of response codes success or error/ Rename columns of Pivot - this is changed from True and False to Success and Error
dependencies
| extend OutCome=iff(success == true, "Success", "Error")
| project  target, name, OutCome
| evaluate pivot(OutCome)

//Join counts with avg duration and Data Table. More complex, where we give names of expected API's in the datatable. If calls were made 
//against one of the API's it will contain data requested. If not, the name of API will still display but no accompanying data will be present
//Basically, API name without data is api that was never called
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

![image](https://user-images.githubusercontent.com/17876815/40541610-b9ca6dca-601c-11e8-95cb-6ebb7c843705.png)
