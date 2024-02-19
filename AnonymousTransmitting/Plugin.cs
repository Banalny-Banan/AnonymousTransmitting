using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public override Version Version { get; } = new(1, 0, 0);
    public override Version RequiredExiledVersion { get; } = new(8, 8, 0);

    public override void OnEnabled()
    {
        if (Config.AnonymizeRadio)
            Handlers.Player.Transmitting += OnTransmitting;
        if (Config.AnonymizeIntercom)
            Handlers.Player.IntercomSpeaking += OnTransmitting;
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
        replacedNicknames[ev.Player] = ev.Player.CustomName;

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

        ev.Player.CustomName = replacedNicknames[ev.Player] == ev.Player.Nickname ? null : replacedNicknames[ev.Player];
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