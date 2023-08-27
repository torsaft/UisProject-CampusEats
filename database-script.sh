#!/bin/sh
 
echo "Removing docker containers and recreating them" 
docker stop postgres adminer && docker rm postgres adminer 
docker compose up -d

STRING="from-script"
echo "Removing old migrations"

for file in "./src/CampusEats/Migrations/CampusEats"/*; do
    echo "Deleting file $file"; rm -rf "$file"
done

for file in "./src/CampusEats/Migrations/Identity"/; do
    echo "Deleting file $file"; rm -rf "$file"
done

#rm -rf ./src/CampusEats/Migrations/CampusEats/CampusEatsContextModelSnapshot.cs
#rm -rf ./src/CampusEats/Migrations/Identity/IdentityContextModelSnapshot.cs

# rm -rf ./Migrations/*

# echo "Making new migration folders"
# mkdir ./Migrations/CampusEats
# mkdir ./Migrations/Identity

echo "Making migrations for each context"
dotnet ef migrations add $STRING --project ./src/CampusEats/ -o ./Migrations/CampusEats/ --context CampusEatsContext 
dotnet ef migrations add $STRING --project ./src/CampusEats/ -o ./Migrations/Identity/ --context IdentityContext 

echo "Updating database from the new migrations"
dotnet ef database update --project ./src/CampusEats/ --context CampusEatsContext
dotnet ef database update --project ./src/CampusEats/ --context IdentityContext

echo "Done done."
