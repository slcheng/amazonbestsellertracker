
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Data;
using System.Text;

int int_bestsellerQty = 100; // top 100 items of Best Seller

List<string> bestsellerurls = new List<string>(); // List of Amazon Best Seller URLs to track
bestsellerurls.Add("https://www.amazon.com/Best-Sellers-Electronics-External-Solid-State-Drives/zgbs/electronics/3015429011?language=en_US&currency=USD");

ChromeOptions options = new ChromeOptions();
options.AddArguments("--disable-popup-blocking");
options.AddArguments("--disable-infobars");
options.AddArguments("--disable-extensions");
options.AddArguments("--window-size=1920,1080");
options.AddArguments("--ignore-certificate-errors");
options.AddArguments("--ignore-ssl-errors");
options.AddArguments("--log-level=3"); // restrict Chromium's log level to 3, only fatal errors are logged. 

IWebDriver driver = new ChromeDriver(options);
if (driver == null)
{
    Console.WriteLine(" Not found ChromeDriver. Please install Chrome browser first.");
    return;
}

GetBestSeller(bestsellerurls);

driver.Quit();
void GetBestSeller(List<string> sellerurls)
{
    foreach (string sellerurlitem in sellerurls)
    {
        int sellerurlIndex = sellerurlitem.IndexOf(sellerurlitem);

        try
        {
            driver.Navigate().GoToUrl(sellerurlitem);
            Thread.Sleep(2000);

            DateTime visitstarttime = DateTime.Now;
            Console.WriteLine("\n" + visitstarttime.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ----- Visit Amazon.com BestSeller Start ----- " + "\n\n" + sellerurlitem);
            driver.Navigate().Refresh();

            string str_title = driver.Title;

            IWebElement body = driver.FindElement(By.CssSelector("body"));
            for (int keytimes = 0; keytimes < 10; keytimes++) // Move down several times and breaks as human behavior
            {
                body.SendKeys(Keys.PageDown);
                Thread.Sleep(300);
            }
            body.SendKeys(Keys.PageUp);

            // Create DataTable for BestSeller
            DataTable dt_bestseller = new DataTable("BestSeller");
            dt_bestseller.Columns.Add("#", typeof(int));
            dt_bestseller.Columns.Add("Product", typeof(string));
            dt_bestseller.Columns.Add("Brand", typeof(string));
            dt_bestseller.Columns.Add("Price", typeof(string));
            dt_bestseller.Columns.Add("URL", typeof(string));

            bool isPage2 = false;

            for (int item = 0; item < int_bestsellerQty; item++)
            {
                int itemindex = 0;

                if (isPage2)
                {
                    itemindex = item - 50;
                }
                else
                {
                    itemindex = item;
                }
                string str_item = itemindex.ToString();

                try
                {   // Get the BestSeller item data attribute
                    IWebElement bestsellitem_rank = driver.FindElement(By.XPath("//div[@id=\"p13n-asin-index-" + str_item + "\"]//span[@class=\"zg-bdg-text\"]"));
                    IWebElement bestsellitem_asin = driver.FindElement(By.XPath("//div[@id=\"p13n-asin-index-" + str_item + "\"]//div[@data-asin]"));
                    IWebElement bestsellitem_alt = driver.FindElement(By.XPath("//div[@id=\"p13n-asin-index-" + str_item + "\"]//img[@alt]"));
                    string str_rank = bestsellitem_rank.Text;
                    string str_asin = bestsellitem_asin.GetAttribute("data-asin");
                    string str_alt = bestsellitem_alt.GetAttribute("alt");
                    string str_price = string.Empty;
                    string str_brand = string.Empty;
                    string str_type = string.Empty;
                    int length_stralt = str_alt.Length;

                    string str_xpath = "//div[@id=\"p13n-asin-index-" + str_item + "\"] //span[@class=\"_cDEzb_p13n-sc-price_3mJ9Z\" or @class=\"p13n-sc-price\"]";
                    bool isPriceExist = driver.FindElements(By.XPath(str_xpath)).Count > 0;
                    if (isPriceExist)
                    {
                        IWebElement bestsellitem_price = driver.FindElement(By.XPath("//div[@id=\"p13n-asin-index-" + str_item + "\"]//span[@class=\"_cDEzb_p13n-sc-price_3mJ9Z\" or @class=\"p13n-sc-price\"]"));
                        str_price = bestsellitem_price.Text;
                    }
                    else
                        str_price = "N/A";

                    int firstSpaceIndex = str_alt.IndexOf(' ');
                    str_brand = str_alt.Substring(0, firstSpaceIndex);

                    //Console.WriteLine(" SellerRank : " + str_rank + " " + str_brand + " " + str_type + " " + str_capacity + " " + str_price + " " + str_alt[..length_stralt] + " " + str_asin);
                    AddProductToDataTable(dt_bestseller, item + 1, str_alt[..50], str_price, "https://www.amazon.com/dp/" + str_asin, str_brand);

                    if (item == 49) // click next page 51-100 items then move
                    {
                        IWebElement nextButton = driver.FindElement(By.LinkText("Next page"));
                        nextButton.Click();
                        IWebElement nextpagebody = driver.FindElement(By.CssSelector("body"));
                        for (int movetimes = 0; movetimes < 10; movetimes++) // Move down several times and breaks as human behavior
                        {
                            nextpagebody.SendKeys(Keys.PageDown);
                            Thread.Sleep(300);
                        }
                        nextpagebody.SendKeys(Keys.PageUp);
                        isPage2 = true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    continue;
                }
            }

            PrintFormatted(dt_bestseller);
            
            // export CSV file
            string csvFilePath = $"d:\\AmazonBestSeller_{DateTime.Now:yyyyMMdd}.csv";
            ExportDataTableToCsv(dt_bestseller, csvFilePath);
            Console.WriteLine($"CSV exported: {csvFilePath}");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing the seller URL: {ex.Message}");
            continue;
        }

        DateTime visitsellerendtime = DateTime.Now;
        Console.WriteLine(" ----- Visit Amazon.com BestSeller End ----- " + visitsellerendtime.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }
}

static void AddProductToDataTable(DataTable dt, int number, string name, string price, string productURL, string brand = "", string capacity = "", string type = "")
{
    DataRow row = dt.NewRow();
    row["#"] = number;
    row["Brand"] = brand;
    row["Product"] = name;
    row["Price"] = price;
    row["URL"] = productURL;
    dt.Rows.Add(row);
}

static void PrintFormatted(DataTable dt)
{
    string str_temp = string.Empty;
    // Calculate column widths based on content
    var columnWidths = new int[dt.Columns.Count];

    // Get max width for each column based on column name and data
    for (int i = 0; i < dt.Columns.Count; i++)
    {
        columnWidths[i] = dt.Columns[i].ColumnName.Length;
        foreach (DataRow row in dt.Rows)
        {
            int length = row[i]?.ToString()?.Length ?? 0;
            if (length > columnWidths[i])
                columnWidths[i] = length;
        }
        // Add padding
        columnWidths[i] += 2;
    }

    PrintLineSeparator(columnWidths);

    // Print column headers
    for (int i = 0; i < dt.Columns.Count; i++)
    {
        Console.Write($"| {dt.Columns[i].ColumnName.PadRight(columnWidths[i] - 2)} ");
    }
    Console.WriteLine("|");

    PrintLineSeparator(columnWidths);

    // Print data rows
    foreach (DataRow row in dt.Rows)
    {
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            string value = row[i]?.ToString() ?? "";
            if (dt.Columns[i].DataType == typeof(decimal))
            {
                // Format decimal numbers with 2 decimal places
                value = string.Format("{0:F2}", row[i]);
            }
            Console.Write($"| {value.PadRight(columnWidths[i] - 2)} ");
        }
        Console.WriteLine("|");
    }

    PrintLineSeparator(columnWidths);
    Console.WriteLine("\n");
}

static void PrintLineSeparator(int[] columnWidths)
{
    Console.Write("+");
    foreach (int width in columnWidths)
    {
        Console.Write(new string('-', width));
        Console.Write("+");
    }
    Console.WriteLine();
}

static void ExportDataTableToCsv(DataTable dt, string filePath)
{
    var sb = new StringBuilder();

    // Header
    for (int i = 0; i < dt.Columns.Count; i++)
    {
        sb.Append(dt.Columns[i].ColumnName);
        if (i < dt.Columns.Count - 1)
            sb.Append(",");
    }
    sb.AppendLine();

    // Rows
    foreach (DataRow row in dt.Rows)
    {
        for (int i = 0; i < dt.Columns.Count; i++)
        {
            string value = row[i]?.ToString()?.Replace("\"", "\"\"") ?? "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                value = $"\"{value}\"";
            sb.Append(value);
            if (i < dt.Columns.Count - 1)
                sb.Append(",");
        }
        sb.AppendLine();
    }

    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
}