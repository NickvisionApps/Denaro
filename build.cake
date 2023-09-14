const string appId = "org.nickvision.money";
const string projectName = "NickvisionMoney";
const string shortName = "denaro";
readonly string[] projectsToBuild = new string[] { "GNOME" };

if (FileExists("CakeScripts/main.cake"))
{
    #load local:?path=CakeScripts/main.cake
}
else
{
    throw new CakeException("Failed to load main script.");
}