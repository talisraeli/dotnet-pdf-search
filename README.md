# .NET PDF Search

1. Run the following commands in your terminal in the root directory:
   
   ```batch
   docker build -t pdf-search .
   docker run -d -p 5000:8080 --name pdf-search-container
   ```

2. Send a HTTP request to the web server with the following details:
   
      - **Method:** `POST`
   
      - **URL:** `http://localhost:5000/search-pdf`
   
      - **Content-Type:** `multipart/form-data`
   
      - **Body:**
        
        | Key            | Value | Description                        |
        | -------------- | ----- | ---------------------------------- |
        | `pdf`          | File  | PDF file                           |
        | `searchedText` | Text  | The text to search in the PDF file |

3. Receive one of the following results:
   
      - Status code 500:
        
           - Wrong file type.
        
           - No content in the `searchedText` field.
        
           - Other internal server errors.
   
      - Status code 404:
        
           - Not found the searched text in the PDF.
   
      - Status code 200:
        
           - Found the searched text in one or more pages.
        
           - If found in one page, the response `Content-Type` will be `image/png` with the page.
        
           - If found in more than one page, the response `Content-Type` will be `application/json` with an object which represents the pages as `image/png` in base64 encoding. (Provided a URL to decode base64 string to PNG).




