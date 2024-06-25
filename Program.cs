//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Threading.Tasks;
//using Amazon;
//using Amazon.CostExplorer;
//using Amazon.CostExplorer.Model;
//using ClosedXML.Excel;

//namespace AwsCostReportConsole
//{
//    class Program
//    {
//        static async Task Main(string[] args)
//        {
//            Console.Write("Enter AWS Account ID: ");
//            string awsAccountId = Console.ReadLine();

//            Console.Write("Enter AWS Access Key: ");
//            string awsAccessKey = Console.ReadLine();

//            Console.Write("Enter AWS Secret Key: ");
//            string awsSecretKey = Console.ReadLine();

//            try
//            {

//                var costData = await GetCostData(awsAccessKey, awsSecretKey); 
//                Displaytotalcost(costData);
//                ExportToExcel(costData, "aws_cost_report.xlsx");

//                Console.WriteLine("Report generation completed successfully. File saved as aws_cost_report.xlsx");
//            }
//            catch (DataUnavailableException ex)
//            {
//                Console.WriteLine("Data is not available. Please try again later or adjust the time period.");
//                Console.WriteLine($"Error: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"An error occurred: {ex.Message}");
//            }
//        }

//        private static async Task<GetCostAndUsageResponse> GetCostData(string accessKey, string secretKey)
//        {
//            //specifies the AWS region where the Cost Explorer service will be accessed. USEast1 refers to the US East (N. Virginia) region.
//            var client = new AmazonCostExplorerClient(accessKey, secretKey, RegionEndpoint.USEast1);

//            var startDate = new DateTime(DateTime.Now.Year, 4, 1).ToString("yyyy-MM-dd");
//            var endDate = new DateTime(DateTime.Now.Year, 5, 22).ToString("yyyy-MM-dd");

//            var request = new GetCostAndUsageRequest
//            {
//                TimePeriod = new DateInterval
//                {
//                    Start = startDate,
//                    End = endDate
//                },
//                Granularity = Granularity.DAILY,
//                Metrics = new List<string> { "UnblendedCost" },

//                GroupBy = new List<GroupDefinition>
//                {
//                    new GroupDefinition
//                    {
//                        Type = GroupDefinitionType.DIMENSION,
//                        Key = "SERVICE"
//                    }
//                }
//                //Filter = new Amazon.CostExplorer.Model.Expression
//                //{
//                //    Dimensions = new Amazon.CostExplorer.Model.DimensionValues
//                //    {
//                //        Key = "LINKED_ACCOUNT",
//                //        Values = new List<string> { accountId }
//                //    }
//                //}
//            };
//            return await client.GetCostAndUsageAsync(request);
//        }

//        private static void ExportToExcel(GetCostAndUsageResponse costData, string fileName)
//        {
//            using var workbook = new XLWorkbook();
//            var worksheet = workbook.Worksheets.Add("Cost Report");

//            worksheet.Cell(1, 1).Value = "Date";
//            worksheet.Cell(1, 2).Value = "Cost";
//            worksheet.Cell(1, 3).Value = "Service";

//            int row = 2;
//            foreach (var result in costData.ResultsByTime)
//            {
//                var date = result.TimePeriod.Start;

//                foreach( var group in result.Groups)
//                {
//                    var service = group.Keys[0];
//                    var amount = group.Metrics["UnblendedCost"].Amount;

//                    worksheet.Cell(row, 1).Value = date;
//                    worksheet.Cell(row, 2).Value = double.Parse(amount);
//                    worksheet.Cell(row, 3).Value = service;
//                    row++;

//                }
//            }
//            workbook.SaveAs(fileName);
//        }

//        private static void Displaytotalcost(GetCostAndUsageResponse costData)
//        {
//            double totalcost = 0;   

//            foreach(var result in costData.ResultsByTime)
//            {
//                foreach( var group in result.Groups)
//                {
//                    var amount = group.Metrics["UnblendedCost"].Amount;
//                    totalcost += double.Parse(amount);
//                }
//            }
//            Console.WriteLine($"Total cost for the specified period is : ${totalcost}");
//        }

//    }
//}

//--------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Amazon;
using Amazon.CostExplorer;
using Amazon.CostExplorer.Model;
using ClosedXML.Excel;

namespace AwsCostReportConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Enter AWS Account ID: ");
            string awsAccountId = Console.ReadLine();

            Console.Write("Enter AWS Access Key: ");
            string awsAccessKey = Console.ReadLine();

            Console.Write("Enter AWS Secret Key: ");
            string awsSecretKey = Console.ReadLine();

            try
            {
                var costData = await GetCostData(awsAccessKey, awsSecretKey);
                Displaytotalcost(costData);
                ExportToExcel(costData, awsAccountId, "aws_cost_report.xlsx");

                Console.WriteLine("Report generation completed successfully. File saved as aws_cost_report.xlsx");
            }
            catch (DataUnavailableException ex)
            {
                Console.WriteLine("Data is not available. Please try again later or adjust the time period.");
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static async Task<GetCostAndUsageResponse> GetCostData(string accessKey, string secretKey)
        {
            var client = new AmazonCostExplorerClient(accessKey, secretKey, RegionEndpoint.USEast1);  //authenticated request

            var startDate = new DateTime(DateTime.Now.Year, 4, 1).ToString("yyyy-MM-dd");
            var endDate = new DateTime(DateTime.Now.Year, 5, 22).ToString("yyyy-MM-dd");

            var request = new GetCostAndUsageRequest
            {
                TimePeriod = new DateInterval
                {
                    Start = startDate,
                    End = endDate
                },
                Granularity = Granularity.DAILY,
                Metrics = new List<string> { "UnblendedCost" },
                GroupBy = new List<GroupDefinition>
                {
                    new GroupDefinition
                    {
                        Type = GroupDefinitionType.DIMENSION,
                        Key = "SERVICE"
                    }
                }
            };

            return await client.GetCostAndUsageAsync(request);
        }

        private static void ExportToExcel(GetCostAndUsageResponse costData, string awsAccountId, string fileName)
        {
            using var workbook = new XLWorkbook();

            var costReportSheet = workbook.Worksheets.Add("Cost Report");
            costReportSheet.Cell(1, 1).Value = "Date";
            costReportSheet.Cell(1, 2).Value = "Service";
            costReportSheet.Cell(1, 3).Value = "Cost";
          

            int row = 2;
            foreach (var result in costData.ResultsByTime)
            {
                var date = result.TimePeriod.Start;

                foreach (var group in result.Groups)
                {
                    var service = group.Keys[0];
                    var amount = group.Metrics["UnblendedCost"].Amount;

                    costReportSheet.Cell(row, 1).Value = date;
                    costReportSheet.Cell(row, 2).Value = service;
                    costReportSheet.Cell(row, 3).Value = double.Parse(amount);
                    row++;
                }
            }

            var summarySheet = workbook.Worksheets.Add("Summary");
            summarySheet.Cell(1, 1).Value = "Account ID";
            summarySheet.Cell(1, 2).Value = "Total Cost";

            double totalCost = CalculateTotalCost(costData);
            summarySheet.Cell(2, 1).Value = awsAccountId;
            summarySheet.Cell(2, 2).Value = totalCost;

            workbook.SaveAs(fileName);
        }

        private static double CalculateTotalCost(GetCostAndUsageResponse costData)
        {
            double totalCost = 0;

            foreach (var result in costData.ResultsByTime)
            {
                foreach (var group in result.Groups)
                {
                    var amount = group.Metrics["UnblendedCost"].Amount;
                    totalCost += double.Parse(amount);
                }
            }

            return totalCost;
        }

        private static void Displaytotalcost(GetCostAndUsageResponse costData)
        {
            double totalCost = CalculateTotalCost(costData);
            Console.WriteLine($"Total cost for the specified period is : ${totalCost}");
        }
    }
}
