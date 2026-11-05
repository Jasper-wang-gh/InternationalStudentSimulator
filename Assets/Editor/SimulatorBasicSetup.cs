using System.IO;
using UnityEditor;
using UnityEngine;

// Place this file in Assets/Editor, then use Tools > International Student Simulator > Apply Basic Setup.
public static class SimulatorBasicSetup
{
    [MenuItem("Tools/International Student Simulator/Apply Basic Setup")]
    public static void Apply()
    {
        string[] folders = {
            "Assets/Scenes", "Assets/Prefabs/Player", "Assets/Prefabs/UI",
            "Assets/Prefabs/Interactions", "Assets/Scripts/Core",
            "Assets/Scripts/Player", "Assets/Scripts/Systems",
            "Assets/Scripts/UI", "Assets/Scripts/Gameplay",
            "Assets/Art/Sprites", "Assets/Art/Tilemaps", "Assets/Art/UI",
            "Assets/Audio/Music", "Assets/Audio/SFX", "Assets/Data/Dialogue",
            "Assets/Data/Questions", "Assets/Data/Jobs", "Assets/Editor"
        };
        foreach (string folder in folders) EnsureFolder(folder);

        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        EditorSettings.serializationMode = SerializationMode.ForceText;
        VersionControlSettings.mode = "Visible Meta Files";
        PlayerSettings.productName = "InternationalStudentSimulator";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("InternationalStudentSimulator: basic setup complete. 2D mode, text serialization, visible meta files, 1280x720 window, and project folders configured. Input System and URP settings preserved.");
        EditorUtility.DisplayDialog("基础配置完成", "已建立 Assets 目录结构，设置 2D 编辑模式、文本序列化、可见 Meta 文件和 1280×720 窗口。\n\n保留现有 URP、输入系统和场景设置。", "确定");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
