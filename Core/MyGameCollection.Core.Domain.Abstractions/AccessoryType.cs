namespace MyGameCollection.Core.Domain.Abstractions;

/// <summary>What kind of accessory it is.</summary>
public enum AccessoryType
{
    /// <summary>A gamepad, joystick, arcade stick or motion controller.</summary>
    Controller,

    /// <summary>A light gun, racing wheel, dance mat, instrument or other game-specific peripheral.</summary>
    Peripheral,

    /// <summary>A memory card, hard drive or other storage.</summary>
    Storage,

    /// <summary>A headset, microphone or camera.</summary>
    AudioVideo,

    /// <summary>A cable, adapter, power supply, charger or dock.</summary>
    CableOrPower,

    /// <summary>An amiibo, Skylander or other toys-to-life figure.</summary>
    Figure,

    /// <summary>A case, bag, stand or anything else.</summary>
    Other
}
