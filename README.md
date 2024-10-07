# Transaction History API Client

This project is a C# console application that interacts with the EPİAŞ Transparency API to fetch transaction history data. The application authenticates a user with their credentials to retrieve a TGT and then uses that ticket to request transaction history data for a specified date range.

## Features

- User authentication to obtain a TGT.
- Fetch transaction history based on specified start and end dates.
- Calculation of total quantities and weighted average prices based on transaction data.
- Group transaction data by contract name and calculate totals and averages.
- Display results in a formatted table.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or higher)
- A registered account on the EPİAŞ Transparency Platform. You can sign up from [here](https://kayit.epias.com.tr/epias-transparency-platform-registration-form).

## Usage

Open Program.cs and replace the `username` and `password` variables with your EPİAŞ account credentials.

Once the application is running, it will authenticate and retrieve the transaction history for the specified date range (set in the Main method). 

The results will be displayed in the console with the following columns:

- Contract Name
- Date
- Total Transaction Quantity
- Total Transaction Amount
- Weighted Average Price

