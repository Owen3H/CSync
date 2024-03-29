using System.Runtime.Serialization;
using BepInEx.Configuration;
using CSync.Lib;
using Unity.Netcode;

namespace CSync.Util;

/// <summary>
/// Contains helpful extension methods to aid with synchronization and reduce code duplication.
/// </summary>
public static class Extensions {
    #region ConfigFile bind overloads
    /// <summary>Binds an entry to this file and returns the converted synced entry.</summary>
    /// <typeparam name="V">The value type of this entry. Must be supported by BepInEx.</typeparam>
    /// <param name="cfg">The current config file we are attempting to bind to.</param>
    /// <param name="definition">The key and section defining how this entry is categorised.</param>
    /// <param name="defaultVal">The value which this entry has unless the user changes it.</param>
    /// <param name="desc">
    /// The description indicating what this entry does.<br></br>
    /// It can also hold metadata such as a value range to control its functionality.
    /// <para>See the BepInEx <see href="https://docs.bepinex.dev/api/BepInEx.Configuration.ConfigDescription.html">documentation</see> for more info.</para>
    /// </param>
    /// <returns></returns>
    public static SyncedEntry<V> BindSyncedEntry<V>(this ConfigFile cfg, 
        ConfigDefinition definition, V defaultVal, ConfigDescription desc = null
    ) {
       return cfg.Bind(definition, defaultVal, desc).ToSyncedEntry();
    }

    public static SyncedEntry<V> BindSyncedEntry<V>(this ConfigFile cfg,
        ConfigDefinition definition, V defaultValue, string desc
    ) {
        return cfg.Bind(definition, defaultValue, new(desc)).ToSyncedEntry();
    }

    public static SyncedEntry<V> BindSyncedEntry<V>(this ConfigFile cfg,
        string section, string key, V defaultValue, ConfigDescription desc = null
    ) {
        return cfg.Bind(new(section, key), defaultValue, desc).ToSyncedEntry();
    }

    public static SyncedEntry<V> BindSyncedEntry<V>(this ConfigFile cfg,
        string section, string key, V defaultValue, string desc
    ) {
        return cfg.Bind(new(section, key), defaultValue, new(desc)).ToSyncedEntry();
    }
    #endregion

    #region Utils to help serialize a ConfigEntry
    /// <summary>Converts this entry into a serializable alternative, allowing it to be synced.</summary>
    public static SyncedEntry<V> ToSyncedEntry<V>(this ConfigEntry<V> entry) => new(entry);

    /// <summary>Helper method to grab a value from SerializationInfo and cast it to the specified type.</summary>
    public static T GetObject<T>(this SerializationInfo info, string key) => (T) info.GetValue(key, typeof(T));
    
    /// <summary>Binds and returns a new ConfigEntry created from the serialization info.</summary>
    internal static ConfigEntry<V> Reconstruct<V>(this ConfigFile cfg, SerializationInfo info) {
        ConfigDefinition definition = new(info.GetString("Section"), info.GetString("Key"));
        ConfigDescription description = new(info.GetString("Description"));

        return cfg.Bind(definition, info.GetObject<V>("DefaultValue"), description);
    }
    #endregion

    /// <summary>
    /// Sends a message using this stream to a client via their ID.<br></br>
    /// If no ID is specified, the message will be sent to the host by default.
    /// </summary>
    /// <param name="stream">The byte stream containing data to be synced.</param>
    /// <param name="guid">The mod GUID that this message was invoked from.</param>
    /// <param name="label">The name of the message to invoke.</param>
    /// <param name="clientId"></param>
    internal static void SendMessage(this FastBufferWriter stream, string guid, string label, ulong clientId = 0uL) {
        bool fragment = stream.Capacity > 1300;

        //if (fragment) Plugin.Logger.LogDebug(
        //    $"{guid} - Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
        //    "Config instance will be sent in fragments to avoid overflowing the buffer."
        //);

        var msgManager = NetworkManager.Singleton.CustomMessagingManager;
        msgManager.SendNamedMessage($"{guid}_{label}", clientId, stream, fragment 
            ? NetworkDelivery.ReliableFragmentedSequenced 
            : NetworkDelivery.Reliable
        );
    }
}