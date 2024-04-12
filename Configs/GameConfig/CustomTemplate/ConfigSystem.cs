using Luban;
using GameBase;
using GameConfig;
using TEngine;
using UnityEngine;
using TEngine.Localization.SimpleJSON;

/// <summary>
/// 配置加载器。
/// </summary>
public class ConfigSystem : Singleton<ConfigSystem>
{
    private bool _init = false;

    private Tables _tables;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                Load();
            }

            return _tables;
        }
    }

    /// <summary>
    /// 加载配置。
    /// </summary>
    public void Load()
    {
        var tablesCtor = typeof(Tables).GetConstructors()[0];
        var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
        System.Delegate loader = loaderReturnType == typeof(ByteBuf) ? 
            new System.Func<string, ByteBuf>(LoadByteBuf) : 
            (System.Delegate)new System.Func<string, JSONNode>(LoadJson);
        _tables = (Tables)tablesCtor.Invoke(new object[] {loader});
        _init = true;
    }

    /// <summary>
    /// 加载二进制配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>ByteBuf</returns>
    private ByteBuf LoadByteBuf(string file)
    {
        TextAsset textAsset = GameModule.Resource.LoadAsset<TextAsset>(file);
        byte[] bytes = textAsset.bytes;
        GameModule.Resource.UnloadAsset(textAsset);
        return new ByteBuf(bytes);
    }
    
    /// <summary>
    /// 加载json配置
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private JSONNode LoadJson(string file)
    {
        TextAsset textAsset = GameModule.Resource.LoadAsset<TextAsset>(file);
        string json = textAsset.text;
        GameModule.Resource.UnloadAsset(textAsset);
        return JSON.Parse(json);
    }
}