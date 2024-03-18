# Local Server
A simple and dynamic api generation and consumption app, which can be hosted locally for faster frontend development. It uses `Bogus` library to randomly generate some data.

## How to use
1. Clone repo on your machine.
1. Run the app using `dotnet run`, which requires **`.NET`** installed on your machine.
1. Once the app is running, start creating and consuming the apis.

## Example
The following example demonstrates, a complete **`CRUD`** operations for `Users` endpoints using REST Client extension in VS Code. The api is running on port 5217, which can be different on your machine.

#### Note:
- Always use camelCase for content.
- Always include "id" property.

---

1. Creating a user. There are some placeholders, for eg: `$GUID$`, `$USERNAME$`, etc., which will be dynamically generated while creating the user. Check out list of all placeholders.
    ```
    POST http://localhost:5217/api/users
    Content-Type: application/json

    {
        "id": "$GUID$",
        "username": "$USERNAME$",    
        "imageUrl": "$PROFILE$",    
        "name": "$FULLNAME$",
        "gender": "$GENDER$",
        "role": "$ROLES$",
        "createdOn": "$DATETIME$",
        "isActive": "$BOOL$"
    }
    ```

1. Get all users
    ```
    GET http://localhost:5217/api/users    
    ```

1. Get user by Id - Guid is the id of the user.
    ```
    GET http://localhost:5217/api/users/09d9cd58-d154-4d06-98a8-bc5442abbe72
    ```

1. Update user by Id - Guid here will be different and there are some placeholders which will be dynamically generated while updating.
    ```
    PUT http://localhost:5217/api/users/09d9cd58-d154-4d06-98a8-bc5442abbe72
    Content-Type: application/json

    {
        "id": "09d9cd58-d154-4d06-98a8-bc5442abbe72",
        "username": "some.user",    
        "imageUrl": "$PROFILE$",    
        "name": "Some User",
        "gender": "$GENDER$",
        "role": "$ROLES$",
        "createdOn": "$DATETIME$",
        "isActive": true
    }
    ```

1. Delete user by Id - Guid is the id of the user.
    ```
    DELETE http://localhost:5217/api/users/09d9cd58-d154-4d06-98a8-bc5442abbe72
    ```

1. Creating bulk users. This will come handy when you want to generate many users to be used in listings to create a datagrid or table. In the following example, you are going to create 100 users. `http://localhost:5217/api/generate/{entityName}/{numberofEntitiesToGenerate}`
    ```
    POST http://localhost:5217/api/generate/users/100
    Content-Type: application/json

    {
        "id": "$GUID$",
        "username": "$USERNAME$",    
        "imageUrl": "$PROFILE$",    
        "name": "$FULLNAME$",
        "gender": "$GENDER$",
        "role": "$ROLES$",
        "createdOn": "$DATETIME$",
        "isActive": "$BOOL$"
    }
    ```

1. Delete all users. To remove all users you need to execute DELETE method on the entity without any id.
    ```
    DELETE http://localhost:5217/api/users
    ```

## List of Placeholders

In order to create realistic looking data, you can use placeholders, which will be replaced with appropriate values while generating. Here are the list of placeholders:

-----------------------------------------------
| Placeholder           | Description          
|-----------------------|----------------------
| `$GUID$`              | Replaces with a new unique identifier. For eg: `09d9cd58-d154-4d06-98a8-bc5442abbe72`
| `$I$`                 | Replaces with current count
| `$ID$`                | Replaces with current Id
| `$USERNAME$`          | Replaces with a randomly generated Username
| `$FIRSTNAME$`         | Replaces with a randomly generated First Name
| `$LASTNAME$`          | Replaces with a randomly generated Last Name
| `$FULLNAME$`          | Replaces with a randomly generated Full Name
| `$GENDER$`            | Replaces with a randomly generated Gender - `Male or Female`
| `$COMPANY$`           | Replaces with a randomly generated Company Name
| `$ADDRESS$`           | Replaces with a randomly generated Address
| `$SUITE$`             | Replaces with a randomly generated Suite which is part of the Address
| `$STREET$`            | Replaces with a randomly generated State which is part of the Address
| `$CITY$`              | Replaces with a randomly generated City which is part of the Address
| `$STATE$`             | Replaces with a randomly generated State which is part of the Address
| `$PHONE$`             | Replaces with a randomly generated Phone number
| `$EMAIL$`             | Replaces with a randomly generated Email address
| `$WEBSITE$`           | Replaces with a randomly generated Website
| `$ROLES$`             | Replaces with a randomly generated Role - `Superadmin, Admin, User, Guest, etc.,`
| `$PROFILE$`           | Replaces with a randomly generated Image Profile Url based on Gender
| `$LOREM$`             | Replaces with a randomly generated a paragraph of text.
| `$SHORT$`             | Replaces with a randomly generated number between 0 and 100.
| `$PERCENT$`           | Replaces with a randomly generated floating number between 0.00 and 100.00
| `$BIGINT$`            | Replaces with a randomly generated number between 10000 and 1000000.
| `$INT$`               | Replaces with a randomly generated number between 100 and 10000.
| `$DATETIME$`          | Replaces with a randomly generated Date Time starting from January 1, 1900.
| `$DATE$`              | Replaces with a randomly generated Date starting from January 1, 1900.
| `$TIME$`              | Replaces with a randomly generated Time between 00:00:00 and 23:59:59.
| `$BOOL$`              | Replaces with a randomly generated `true` or `false`.
| `$DOUBLE$`            | Replaces with a randomly generated value with 2 decimals upto 10000.
