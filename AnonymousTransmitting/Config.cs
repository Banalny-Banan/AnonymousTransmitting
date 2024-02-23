using Exiled.API.Interfaces;
using System.ComponentModel;

namespace AnonymousTransmitting;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;

    [Description("For intercom, frequency will be \"835.82MHz\" and range - \"Intercom\"")]
    public bool AnonymizeIntercom { get; set; } = true;

    public bool AnonymizeRadio { get; set; } = true;

    [Description("String that will replace the nickname of the player when they are transmitting. Available placeholders:" +
        "\n%frequency% - cool number that represents radio range, like \"145.82MHz\"" +
        "\n%range% - radio range. Can be \"Short\", \"Medium\", \"Long\", or \"Ultra\"" +
        "\n%side% - transmitting player's side. Can be \"Scp\", \"Mtf\", \"ChaosInsurgency\", \"Tutorial\", or \"None\"" +
        "\n%team% - transmitting player's team. Can be \"SCPs\", \"FoundationForces\", \"ChaosInsurgency\", \"Scientists\", \"ClassD\", \"Dead\", or \"OtherAlive\""
    )]

    public string NicknameReplacer { get; set; } = "%frequency% (%range%)";
    
    [Description("Will force the plugin to enable XPSystem fix, even if it hasn't detected the XPSystem plugin")]
    public bool ForceXpSystemFix { get; set; } = false;
}