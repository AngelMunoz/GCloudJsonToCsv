# GCloud Function with F#

this is a small sample of how easy is to write F# Functions for Google Cloud Platform

# Deploy
- Create a gcloud project
- Run `gcloud init`
- Set your project where you want to deploy your functions
- Run `gcloud functions deploy json-to-csv-fn  --entry-point JsonToCsv.Function --runtime dotnet3 --trigger-http --allow-unauthenticated`
    > Note: this will allow anyone to hit your function so be careful for potential abuses


# Run

> ***Note: do not forget to put your cloud function endpoint in the index file line 11 (index.fsx:11)***

once your function is deployed
- Run `dotnet fsi index.fsx`

this will make a request your endpoint with a sample data set and convert that json into an excel file which will be downloaded into `file.xlsx` you can delete the current one to see it's working

originally it was going to be a csv file, but I decided to spice it a little bit more and convert that into a full Excel file thanks to [this project](https://github.com/Zaid-Ajaj/ClosedXML.SimpleSheets) so feel free to go and support Zaid's wonderfull OSS work any time you can 😁
