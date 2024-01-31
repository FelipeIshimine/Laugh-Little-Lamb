using System.Collections.Generic;
using Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayLevelWindow : UnityEditor.EditorWindow
{
	private LevelListModel listModel;
	private Editor cacheEditor;
	private List<string> labels;

	[MenuItem("Window/Levels")]
	public static void Open()
	{
		GetWindow<PlayLevelWindow>().Show();
	}
	private void CreateGUI()
	{
		var guids= AssetDatabase.FindAssets($"t:{nameof(LevelListModel)}");
		
		if(guids.Length > 0)
		{
			listModel = AssetDatabase.LoadAssetAtPath<LevelListModel>(AssetDatabase.GUIDToAssetPath(guids[0]));
		}
Debug.Log(listModel == null);
		if (listModel)
		{
			var serializedObject = new SerializedObject(listModel);
			Editor.CreateCachedEditor(listModel, null, ref cacheEditor);
			var inspector = cacheEditor.CreateInspectorGUI();
			if (inspector != null)
			{
				inspector.Bind(serializedObject);
				rootVisualElement.Add(inspector);
			}
			else
			{
				Debug.Log("Inspector is null");
			}
			
			labels = listModel.levelPrefabs.ConvertAll(x => x.editorAsset.name);
			var dropdown = new DropdownField(labels,-1, OnSelection);
			rootVisualElement.Add(dropdown);
		}
	}

	private string OnSelection(string arg)
	{
		int index = listModel.levelPrefabs.FindIndex(x => x.editorAsset.name == arg);
		if (index != -1)
		{
			EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
			listModel.quickLoadLevelIndex = index;
			EditorApplication.EnterPlaymode();
		}

		return arg;
	}
}
