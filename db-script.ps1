
echo "Removing docker containers and recreating them" 
docker stop postgres adminer
docker rm postgres adminer
docker-compose up -d

echo "Removing old migrations"

ForEach ($file in (Get-ChildItem "./src/CampusEats/Migrations/CampusEats/")){
    if ($file.Name -Match 'from-script'){
       Write-Verbose "found the string, deleting" -Verbose
       Remove-Item $file.Name -WhatIf
 }
}
 ForEach ($file in (Get-ChildItem "./src/CampusEats/Migrations/Identity/")){
    if ($file.Name -Match 'from-script'){
       Write-Verbose "found the string, deleting" -Verbose
       Remove-Item $file.Name -WhatIf
 }
}

# rm -force .\Migrations\*

# echo "Making new migration folders"
# mkdir .\Migrations\CampusEats
# mkdir .\Migrations\Identity

echo "Making migrations for each context"
dotnet ef migrations add from-script --project ./src/CampusEats/ -o ./src/CampusEats/Migrations/CampusEats/ --context CampusEatsContext 
dotnet ef migrations add from-script --project ./src/CampusEats/ -o ./src/CampusEats/Migrations/Identity/ --context IdentityContext 

echo "Updating databse from the new migrations"
dotnet ef database update --project ./src/CampusEats/ --context CampusEatsContext 
dotnet ef database update --project ./src/CampusEats/ --context IdentityContext 

echo "Done done."
