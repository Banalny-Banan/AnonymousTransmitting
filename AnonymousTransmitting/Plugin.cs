using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using MEC;
using Handlers = Exiled.Events.Handlers;

namespace AnonymousTransmitting;

public class Plugin : Plugin<Config>
{
    public override string Prefix => "AnonymousTransmitting";
    public override string Name => Prefix;
    public override string Author => "Banalny_Banan";
    public override Version Version { get; } = new(1, 2, 0);
    public override Version RequiredExiledVersion { get; } = new(8, 8, 0);

    private bool xpSystemPresent = false;
    
    public override void OnEnabled()
    {
        if (Config.AnonymizeRadio)
            Handlers.Player.Transmitting += OnTransmitting;
        if (Config.AnonymizeIntercom)
            Handlers.Player.IntercomSpeaking += OnTransmitting;
        if(File.Exists(Path.Combine(Paths.Plugins, "XPSystem-EXILED.dll")))
            xpSystemPresent = true;
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Handlers.Player.Transmitting -= OnTransmitting;
        Handlers.Player.IntercomSpeaking -= OnTransmitting;
        base.OnDisabled();
    }

    private readonly Dictionary<Player, string> replacedNicknames = new();

    private IEnumerator<float> OnTransmitting(IPlayerEvent ev)
    {
        if (!(ev.Player.IsTransmitting || Intercom.Speaker == ev.Player) || replacedNicknames.ContainsKey(ev.Player)) yield break;

        if (xpSystemPresent)
            replacedNicknames[ev.Player] = Regex.Replace(ev.Player.Nickname, @"^\[Lv\.\d+\]", "");
        else
            replacedNicknames[ev.Player] = ev.Player.CustomName;

        Log.Debug($"Saved {ev.Player.Nickname}'s nickname: {replacedNicknames[ev.Player]}");
        
        string frequency;
        string range;
        
        if (Intercom.Speaker == ev.Player)
        {
            frequency = "835.82MHz";
            range = "Intercom";
        }
        else if (ev.Player.IsTransmitting && ev.Player.Items.FirstOrDefault(item => item.Type is ItemType.Radio) is Radio radio)
        {
            frequency = Frequency(radio.Range);
            range = radio.Range.ToString();
        }
        else yield break;

        ev.Player.CustomName = Config.NicknameReplacer
            .Replace("%frequency%", frequency)
            .Replace("%range%", range)
            .Replace("%side%", ev.Player.Role.Side.ToString())
            .Replace("%team%", ev.Player.Role.Team.ToString());

        yield return Timing.WaitUntilTrue(() => !ev.Player.IsTransmitting && Intercom.Speaker != ev.Player);
        
        if (replacedNicknames[ev.Player] == ev.Player.Nickname) ev.Player.CustomName = null;
        else ev.Player.DisplayNickname = replacedNicknames[ev.Player];
        
        Log.Debug($"Restored {ev.Player.Nickname}'s nickname: {replacedNicknames[ev.Player]}");
        
        replacedNicknames.Remove(ev.Player);
    }

    private static string Frequency(RadioRange range)
    {
        return range switch
        {
            RadioRange.Short => "145.82MHz",
            RadioRange.Medium => "151.25MHz",
            RadioRange.Long => "164.97MHz",
            RadioRange.Ultra => "172.68MHz",
            _ => "835.82MHz",
        };
    }
}