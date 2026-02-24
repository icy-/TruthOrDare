using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;  //SeString
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.VertexShader;

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
    // Make the LoggerFactory static and settable
    //public static ILoggerFactory? LoggerFactory { get; set; }

    //// Helper method to create loggers for specific categories (classes)
    //public static ILogger? CreateLogger<T>() => LoggerFactory?.CreateLogger<T>();

    //public static ILogger? CreateLogger(string categoryName) => LoggerFactory?.CreateLogger(categoryName);
    //private static ILogger Logger;
        
    public ChatHandler()
    {        
    }

    public static async Task RunMacroAsync(string[] lines, int index)
    {

//        Service.semaphore.WaitOne();
//        var reaction = Service.configuration!.Reactions[index];
//        Service.semaphore.Release();

//        foreach (var line in lines)
//        {
//            var textCommand = Service.FormatCommand(line);
//            if (!string.IsNullOrEmpty(textCommand.Main))
//            {
//                // Process emote
//                var isEmote = Service.Emotes.Contains(textCommand.Main);
//                if (isEmote)
//                {
//                    if ((textCommand.Main == "/sit" || textCommand.Main == "/groundsit" || textCommand.Main == "/lounge") && !reaction.AllowSit)
//                        textCommand.Main = "/no";
//                    if (reaction.MotionOnly)
//                        textCommand.Args = "motion";
//                }

//                if (!reaction.CommandBlacklist.Contains(textCommand.Main))
//                {
//                    // Execute command
//                    if (reaction.AllowAllCommands || isEmote || reaction.CommandWhitelist.Contains(textCommand.Main))
//                    {
//                        if (textCommand.Main == "/wait" && float.TryParse(textCommand.Args, out var seconds))
//                            await Task.Delay((int)(Math.Clamp(seconds, 0.0, 60.0) * 1000.0));
//                        else
//                            Chat.SendMessage($"{textCommand}");
//                    }
//                }
//#if DEBUG
//                else
//                {
//                    Service.ChatGui.Print($"{textCommand.Main} in CommandBlacklist");
//                    return;
//                }
//#endif
//            }
//        }
    }

    public static async Task DoCommandAsync(int index, XivChatType type, String message)
    {
        //        // Check if part of enabled channels
        //        if (!Service.configuration!.Reactions[index].EnabledChannels.Contains((int)type)) return;

        //        var usingRegex = (Service.configuration.Reactions[index].UseRegex && Service.configuration.Reactions[index].CustomRx != null);

        //        // Guard against whitespace regex
        //        if ((usingRegex && Service.configuration.Reactions[index].CustomRx!.ToString().IsNullOrWhitespace()) ||
        //            (!usingRegex && Service.configuration.Reactions[index].Rx!.ToString().IsNullOrWhitespace()))
        //        {
        //#if DEBUG
        //            Service.ChatGui.PrintError($"[PuppetMasster][ERR] Empty RegEx [{message}]");
        //#endif
        //            return;
        //        }

        //        // Find command in message
        //        var matches = usingRegex ? Service.configuration.Reactions[index].CustomRx!.Matches(message) : Service.configuration.Reactions[index].Rx!.Matches(message);
        //        if (matches.Count == 0) return;
        //        var command = string.Empty;
        //        try
        //        {
        //            command = usingRegex ?
        //                Service.configuration.Reactions[index].CustomRx!.Replace(matches[0].Value, Service.configuration.Reactions[index].ReplaceMatch) :
        //                Service.configuration.Reactions[index].Rx!.Replace(matches[0].Value, Service.GetDefaultReplaceMatch());
        //        }
        //        catch (Exception) { }


        var command = String.Empty;
        var lines = MyRegex().Split(command.ToString());
//        await RunMacroAsync(lines, index);
    }

    // Static regex method should hopefully keep it compiled
    public static Roll ParseRollMessage(string message)
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

    public static void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        // TODO: check config to see if we're set to listen.  Exit early if we're not.


        // There are many, many channels that are not defined enums.  So if we want a special one, have to jump through hoops
        ushort number = (ushort)type;
        //string prefix;
        if (Enum.IsDefined(typeof(XivChatType), number))
        {
            Service.Logger.Debug($"  type[{number}] is fine.  Here is its string: {number}.  {message}");
            if (type is XivChatType.Say && message.ToString()=="!tod")
            {
                Service.Logger.Debug($"  !tod detected! Performing a clear of the table!");
                Service.configuration.ClearRolls();
            }
        }
        else if (Enum.IsDefined(typeof(SpecialChannel), number))
        {
            var special = (SpecialChannel)number;
            Service.Logger.Debug($"  type[{number}] is special.  Here is its string: {special}.  {message}");

            // todo regex to shorten the message
            if (special is SpecialChannel.RandomYou)
            {

                Roll roll = ParseRollMessage(message.ToString());
                if (roll.IsEmpty())
                {
                    Service.Logger.Debug($" RandomYou roll parse failed and is blank; skipping (likely a deathroll)");
                    return;
                }
                // Convert "You" to character name                    
                roll.Name = Plugin.PlayerState.CharacterName;
                Service.configuration.Rolls.Add(roll);
                // Calculate high and low, which should update in those other spots?!
                if (roll.Value < Service.configuration.LowRoll.Value)
                {
                    Service.configuration.LowRoll = roll;
                }
                if (roll.Value > Service.configuration.HighRoll.Value)
                {
                    Service.configuration.HighRoll = roll;
                }
            }
            else if (special is SpecialChannel.Random)
            {
                Roll roll = ParseRollMessage(message.ToString());
                if (roll.IsEmpty())
                {
                    Service.Logger.Debug($" Random roll parse failed and is blank; skipping (likely a deathroll)");
                    return;
                }
                Service.configuration.Rolls.Add(roll);
                // Calculate high and low, which should update in those other spots?!
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


        //if (Service.configuration!.DebugLogTypes && type != XivChatType.Debug)
        //{
        //var prefix = int.TryParse(type.ToString(), out var number) ? "[" + number + "]" : "[" + ((int)type) + "][" + type + "]";
        //    prefix += (sender.ToString().IsNullOrEmpty() ? "" : "<" + sender + "> ");
        //    //Service.ChatGui.Print(prefix + " " + message);
        //    Service.Logger.Debug($"{prefix} {message}");
        //}
        

        if (isHandled) return;

        string messageStr = message.ToString();

        //_ = Task.Run(async () =>
        //{
        //    var tasks = new List<Task>();
        //    for (var index = 0; index < Service.configuration.Reactions.Count; index++)
        //    {
        //        if (Service.configuration.Reactions[index].Enabled)
        //            tasks.Add(DoCommandAsync(index, type, messageStr));
        //    }
        //    await Task.WhenAll(tasks);
        //});
    }

    [GeneratedRegex("\r\n|\r|\n")]
    private static partial Regex MyRegex();
}
