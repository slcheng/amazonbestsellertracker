# Amazon Best Seller Price Tracker

This application is an automation tool that uses Selenium to automatically browse Amazon Best Seller pages, capturing the top 100 products and outputs the results as a table in the console.
The results can also be exported as a CSV file for further analysis.

![image](https://github.com/slcheng/amazonbestsellertracker/blob/main/bestsellerpricetracker.png)

## Environment

- .NET 9 / Windows OS
- Google Chrome browser installed
- NuGet packages: `Selenium.WebDriver`, `Selenium.WebDriver.ChromeDriver`

## Installation Steps

1. Download or clone the project source code.
2. Open the project in Visual Studio 2022.
3. Use NuGet Package Manager to install the following packages:
   - `Selenium.WebDriver`
   - `Selenium.WebDriver.ChromeDriver`
4. Make sure Google Chrome browser is installed.
5. Edit the `bestsellerurls` list in `Program.cs` to add the Amazon Best Seller category URLs you want to track.




