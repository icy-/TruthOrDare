using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TruthOrDare;

public enum SpecialChannel : ushort
{
    RandomYou = 2122,
    Random = 8266,
    TeleportReadyYou = 2219,
    TeleportUseYou = 2091,
    SprintUse = 8235,
    SprintGainEffect = 8750,
    SprintUseYou = 2091,
    SprintGainEffectYou = 2222,  //Jog too
    SprintLoseEfectYou = 2224,
    FreeCompanyLog = 8774, // log in or out
    ItemPlacedArmoury = 2105,  // also when you assign task to retainer
    ItemObtain = 2110,
    RetainerPayment = 3129,  // e.g. with 2 ventures
}

public partial class ChatHandler
{
    private Plugin plugin;

    public ChatHandler(Plugin plugin)
    {
        this.plugin = plugin;
    }

    // Static regex method should hopefully keep it compiled
    private Roll ParseRollMessage(string message)
    {
        string pattern = @"^Random! (.*) rolls?.*?(\d+)\.$";
        Match m = Regex.Match(message, pattern);
        if (m.Success)
        {
            string m1 = m.Groups[1].Value, m2 = m.Groups[2].Value;
            Service.Logger.Debug($" successful regex.  Group1: {m1}; Group2: {m2}");
            return new Roll(m1, ushort.Parse(m2));
        }

        return new Roll();
    }

    public void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString sestring, ref bool isHandled)
    {
        // TODO: check config to see if we're set to listen.  Exit early if we're not.


        // There are many, many channels that are not defined enums.  So if we want a special one, have to jump through hoops
        ushort number = (ushort)type;
        var message = sestring.ToString();
        //string prefix;
        if (Enum.IsDefined(typeof(XivChatType), number))
        {
            Service.Logger.Debug($"  type[{number}] is fine.  Here is its string: {number}.  {message}");
            if (type is XivChatType.Say)
            {                
                if (message == "!td" && Service.configuration.ReactToExclamTd)                
                {
                    Service.Logger.Debug($"  !td detected! Performing a Start()!");
                    plugin.Start();
                }
                else if (message == "!tod" && Service.configuration.ReactToExclamTod)
                { 
                    Service.Logger.Debug($"  !tod detected! Performing a Start()!");
                    plugin.Start();
                }
                else if (message == "!truth" && Service.configuration.ReactToExclamTruth)
                {
                    Service.Logger.Debug($"  !truth detected! Performing a RandomTruth()!");
                    plugin.RandomTruth();
                }
                else if (message == "!dare" && Service.configuration.ReactToExclamDare)
                {
                    Service.Logger.Debug($"  !dare detected! Performing a RandomDare()!");
                    plugin.RandomDare();
                }
            }

        }
        else if (Enum.IsDefined(typeof(SpecialChannel), number))
        {
            var special = (SpecialChannel)number;
            Service.Logger.Debug($"  type[{number}] is special.  Here is its string: {special}.  {message}");

            // Only check rolls if game is running
            if (plugin.IsRunning && (special is SpecialChannel.RandomYou || special is SpecialChannel.Random))
            {
                Roll roll = ParseRollMessage(message);
                if (roll.IsEmpty())
                {
                    Service.Logger.Debug($" Random roll parse failed and is blank; skipping (likely a deathroll)");
                    return;
                }
                if (special is SpecialChannel.RandomYou)
                {
                    // Convert "You" to character name                    
                    roll.Name = Plugin.PlayerState.CharacterName;
                }
                
                Service.configuration.Rolls.Add(roll);                
                if (roll.Value < Service.configuration.LowRoll.Value)
                    Service.configuration.LowRoll = roll;                
                if (roll.Value > Service.configuration.HighRoll.Value)
                    Service.configuration.HighRoll = roll;                
            }           
            else
            {
                // other type of special we don't care about rn
            }           
            
        }
        else // Missing another player readying and using teleport, but maybe other fun stuff
        {
            Service.Logger.Debug($"  chat type[{number}] undefined. {message}");
        }
    }
}
